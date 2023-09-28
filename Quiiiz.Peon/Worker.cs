﻿using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quiiiz.Peon.Works;

namespace Quiiiz.Peon;

public sealed class Worker : IHostedService
{
    private readonly ILogger<Worker> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IOptions<Configuration.Works> options;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IOptions<Configuration.Works> options)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.options = options;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var mapping = serviceProvider.GetRequiredService<IDictionary<string, Type>>();

        if (Environment.GetCommandLineArgs().Length < 2)
        {
            logger.LogWarning("No commands were passed as command line arguments. Available commands: {CommandList}.", string.Join(", ", mapping.Keys));
            return;
        }

        var commands = Environment.GetCommandLineArgs().Skip(1);

        do
        {
            var timer = Stopwatch.StartNew();

            foreach (var cmd in commands)
            {
                if (!mapping.TryGetValue(cmd, out Type? value))
                {
                    throw new InvalidOperationException($"Unknown command '{cmd}' supplied.");
                }

                if (serviceProvider.GetRequiredService(value) is IWork work)
                {
                    using var scope = serviceProvider.CreateScope();
                    logger.LogInformation("Running work {WorkType}.", value);
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        await work.Work(cancellationToken);
                        logger.LogInformation("Execution {WorkType} finished in {Elapsed}.", value, sw.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while executing {WorkType} at {Elapsed}.", value, sw.Elapsed);
                        if (options.Value.Exceptions) throw;
                    }
                }
            }

            logger.LogInformation("All works execution finished in {Elapsed}. Timeout {Timeout}.", timer.Elapsed, options.Value.Timeout);
            await Task.Delay(options.Value.Timeout, cancellationToken);

        } while (options.Value.Loop && !cancellationToken.IsCancellationRequested);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping worker.");
        await Task.CompletedTask;
    }
}
