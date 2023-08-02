using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
