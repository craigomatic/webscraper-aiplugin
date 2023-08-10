using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using System.Text.Json;

var builtConfig = null as IConfigurationRoot;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(defaults =>
    {
        defaults.Serializer = new Azure.Core.Serialization.JsonObjectSerializer(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
    })
    .ConfigureAppConfiguration(configuration =>
    {
        var config = configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        
        builtConfig = config.Build();
        
    })    
    .ConfigureServices(services =>
    {        
        services.AddScoped(
            _ => Kernel.Builder.Configure(
                embeddingConfig: null,
                completionConfig: builtConfig.GetRequiredSection("CompletionConfig").Get<ModelConfig>()));

        services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
        {
            var options = new OpenApiConfigurationOptions()
            {
                Info = new OpenApiInfo()
                {
                    Version = "1.0.0",
                    Title = "Webscraper Plugin",
                    Description = "This plugin is capable of scraping webpages."
                },
                Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                OpenApiVersion = OpenApiVersionType.V3,
                IncludeRequestingHostName = true,
                ForceHttps = false,
                ForceHttp = false,                
            };

            return options;
        });
    })
    .Build();

//check playwright has been installed.
//we don't need to await as this can take some time and the plugin will return a 503 while in progress
_ = Task.Run(() =>
{
    var loggerFactory = host.Services.GetService<ILoggerFactory>();
    var depLogger = loggerFactory!.CreateLogger("Dependencies");
    depLogger.Log(LogLevel.Information, "Installing Playwright");

    var result = PlaywrightBootstrapper.Run();
    
    switch(result)
    {
        case PlaywrightInstallStatus.AlreadyInstalled:
            depLogger.Log(LogLevel.Information, "Playwright already installed");
            break;
        case PlaywrightInstallStatus.Installed:
            depLogger.Log(LogLevel.Information, "Playwright installed");
            break;
        case PlaywrightInstallStatus.Failed:
            depLogger.Log(LogLevel.Error, "Playwright failed to install");
            break;
    }
});


host.Run();
