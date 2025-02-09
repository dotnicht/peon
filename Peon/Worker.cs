using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peon.Works;

namespace Peon;

public sealed class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger, IOptions<Configuration.Works> options) 
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var mapping = serviceProvider.GetRequiredService<IDictionary<string, Type>>();
        var commands = Environment.GetCommandLineArgs();

        if (commands.Length < 2 || commands.Skip(1).Any(x => !mapping.ContainsKey(x)))
        {
            logger.LogWarning("Unable to parse command line string. Available commands: {CommandList}.", string.Join(", ", mapping.Keys));
            return;
        }

        do
        {
            var timer = Stopwatch.StartNew();

            foreach (var cmd in commands)
            {
                if (cmd == Assembly.GetExecutingAssembly().Location) continue;
                
                if (serviceProvider.GetRequiredService(mapping[cmd]) is IWork work)
                {
                    using var scope = serviceProvider.CreateScope();
                    logger.LogInformation("Running work {WorkType}.", mapping[cmd]);
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        await work.Work(cancellationToken);
                        logger.LogInformation("Execution {WorkType} finished in {Elapsed}.", mapping[cmd], sw.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while executing {WorkType} at {Elapsed}.", mapping[cmd], sw.Elapsed);
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
