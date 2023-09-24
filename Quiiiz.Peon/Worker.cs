using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quiiiz.Peon.Works;

namespace Quiiiz.Peon;

public sealed class Worker : IHostedService
{
    private readonly ILogger<Worker> logger;
    private readonly ServiceProvider serviceProvider;

    public Worker(ILogger<Worker> logger, ServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var mapping = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "check", typeof(CheckUsers) },
            { "fill", typeof(FillGas) },
            { "approve", typeof(ApproveSpend) },
            { "sync", typeof(SyncNumbers) },
            { "extract", typeof(ExtractStuff) }
        };

        foreach (var cmd in Environment.GetCommandLineArgs())
        {
            if (!mapping.TryGetValue(cmd, out Type? value))
            {
                throw new InvalidOperationException($"Unknown command '{cmd}' supplied.");
            }

            if (serviceProvider.GetRequiredService(value) is IWork work)
            {
                logger.LogInformation("Running work {WorkType}.", value);
                await work.Work(cancellationToken);
            }
        }

        logger.LogInformation("All works executed.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping worker.");
        await Task.CompletedTask;
    }
}
