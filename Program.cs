using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using System.Text.Json;

var builtConfig = null as IConfigurationRoot;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
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

        // return JSON with expected lowercase naming
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    })
    .Build();

//check playwright has been installed
if (!Directory.Exists(Path.Combine(Environment.ExpandEnvironmentVariables("%localappdata%"), "ms-playwright")))
{
    //TODO: pwsh bin/Debug/netX/playwright.ps1 install
}

host.Run();
