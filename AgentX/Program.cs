using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using AgentX;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddHostedService<Worker>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddEventLog();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

await host.RunAsync();