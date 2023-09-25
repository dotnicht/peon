using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quiiiz.Peon.Works;

namespace Quiiiz.Peon;

public sealed class Worker : IHostedService
{
    private readonly ILogger<Worker> logger;
    private readonly IServiceProvider serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var mapping = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IWork)))
            .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

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
