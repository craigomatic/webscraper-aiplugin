using System.Net;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Playwright;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Text;

public class PluginEndpoint
{
    private readonly ILogger _logger;
    private readonly IKernel _kernel;
    private readonly ISKFunction _summaryFunction;

    public PluginEndpoint(
        ILoggerFactory loggerFactory, 
        IKernel kernel)
    {
        _logger = loggerFactory.CreateLogger<PluginEndpoint>();
        _kernel = kernel;
        _summaryFunction = _kernel.CreateSemanticFunction(
            Assembly.GetExecutingAssembly().LoadEmbeddedResource("webscraper_aiplugin.Functions.Summary.skprompt.txt"));            
    }
    
    [Function("WellKnownAIPlugin")]
    public async Task<HttpResponseData> WellKnownAIPlugin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route=".well-known/ai-plugin.json")] HttpRequestData req)
    {
        var toReturn = new AIPlugin();
        toReturn.Api.Url = $"{req.Url.Scheme}://{req.Url.Host}:{req.Url.Port}/swagger.json";

        var r = req.CreateResponse(HttpStatusCode.OK);
        await r.WriteAsJsonAsync(toReturn);
        return r;
    }    

    [OpenApiOperation(operationId: "Scrape", tags: new[] { "ScrapeWebsiteFunction" }, Description = "Scrapes the given website to retrieve information based on the query.")]
    [OpenApiParameter(name: "URL", Description = "The URL of the website to scrape", Required = true, In = ParameterLocation.Query)]
    [OpenApiParameter(name: "Summarise", Description = "If true, the returned result will be the summary of the page, when false it will be the entire contents of the page", Required = false, In = ParameterLocation.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Returns the information that was scraped from the website that is relevant to the query")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
    [Function("ScrapeWebsiteWithQuery")]
    public async Task<HttpResponseData> ScrapeWebsiteWithQuery([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="scrape")] HttpRequestData req)
    {
        var urlToScrape = req.Query("URL").FirstOrDefault();

        if (urlToScrape == null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var summaryRequested = req.Query("Summarise").FirstOrDefault() == null ? false :
            bool.Parse(req.Query("Summarise").First());

        if (!urlToScrape.StartsWith("http") && !urlToScrape.StartsWith("https"))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        _logger.LogInformation($"Starting to scrape {urlToScrape}");

        var content = null as string;

        using var playwright = await Playwright.CreateAsync();
        {
            try
            {
                await using var browser = await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions 
                    { 
                        Headless = true, 
                        ExecutablePath = PlaywrightBootstrapper.ChromiumExecutablePath
                    });

                var maxRetry = 5;

                for (var i = 0; i < maxRetry; i++)
                {
                    try
                    {
                        content = await _ScrapePage(browser, urlToScrape);
                        break; //exit the loop if we are successful in scraping the page
                    }
                    catch (System.TimeoutException) { }
                }

                if (content == null)
                {
                    _logger.LogInformation($"Failed to scrape {urlToScrape}");

                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                if (summaryRequested)
                {
                    _logger.LogInformation($"Starting to summarise {urlToScrape}");

                    var maxTokens = 2000;

                    List<string> lines = TextChunker.SplitPlainTextLines(content, maxTokens);
                    List<string> paragraphs = TextChunker.SplitPlainTextParagraphs(lines, maxTokens);

                    var context = _kernel.CreateNewContext();
                    var result = await this._summaryFunction.AggregatePartitionedResultsAsync(paragraphs, context);

                    content = result.Result;
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"Failed to scrape {urlToScrape}");

                //most likely we have not finished downloading the chromium dependencies
                return req.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }
            
            _logger.LogInformation($"Scrape completed for {urlToScrape}");

            var r = req.CreateResponse(HttpStatusCode.OK);
            r.Headers.Add("Content-Type", "text/plain");
            await r.WriteStringAsync(content);
            return r;
        }        
    }

    private async Task<string> _ScrapePage(IBrowser browser, string urlToScrape)
    {
        var page = await browser.NewPageAsync(
                new BrowserNewPageOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.39"
                });

        await page.GotoAsync(urlToScrape);

        var locator = page.GetByRole(AriaRole.Main).First;
        return await locator.InnerTextAsync();
    }
}
