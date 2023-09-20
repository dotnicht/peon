using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class ExtractStuff : IRunnable
{
    private readonly IRepository<User> repository;
    private readonly ILogger<ExtractStuff> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;

    public ExtractStuff(IRepository<User> repository, ILogger<ExtractStuff> logger, IOptions<Blockchain> blockchain, IOptions<Configuration> options)
    {
        this.repository = repository;
        this.logger = logger;
        this.blockchain = blockchain;
        this.options = options;
    }

    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content)
        {
            if (options.Value.Token.Extract && user.TokenBalance > 0)
            {
                throw new NotImplementedException();
            }

            if (options.Value.Gas.Extract && user.Balance > 0)
            {
                throw new NotImplementedException();
            }
        }
    }

    public sealed record class Configuration : WorkConfigurationBase
    {
        public required Asset Token { get; init; }
        public required Asset Gas { get; init; }

        public sealed record Asset
        {
            public required bool Extract { get; init; }
            public required string Address { get; init; }
        }
    }
}
