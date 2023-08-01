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

//check playwright has been installed
if (!Directory.Exists(Path.Combine(Environment.ExpandEnvironmentVariables("%localappdata%"), "ms-playwright")))
{
    var loggerFactory = host.Services.GetService<ILoggerFactory>();
    var depLogger = loggerFactory!.CreateLogger("Dependencies");
    depLogger.Log(LogLevel.Information, "Installing Playwright");

    _ = Task.Run(async () =>
    {
        var dir = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;
        var p = System.Diagnostics.Process.Start("pwsh", $"{dir}{Path.DirectorySeparatorChar}playwright.ps1 install");
        await p.WaitForExitAsync();

        depLogger.Log(LogLevel.Information, "Playwright installation completed");
    });    
}

host.Run();
