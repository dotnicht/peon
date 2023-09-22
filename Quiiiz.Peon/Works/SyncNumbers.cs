using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon.Works;

internal class SyncNumbers : IWork
{
    private readonly IRepository<User> repository;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly ILogger<SyncNumbers> logger;

    public SyncNumbers(IRepository<User> repository, IOptions<Blockchain> blockchain, ILogger<SyncNumbers> logger, IOptions<Configuration> options)
    {
        this.repository = repository;
        this.blockchain = blockchain;
        this.logger = logger;
        this.options = options;
    }

    public async Task Work(CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content)
        {
            if (options.Value.Gas)
            {
                logger.LogInformation("Updating gas amount for user {User}.", await user.UpdateGas(repository, blockchain.Value));
            }

            if (options.Value.Token)
            {
                logger.LogInformation("Updating token amount for user {User}.", await user.UpdateToken(repository, blockchain.Value));
            }

            if (options.Value.Approved)
            {
                logger.LogInformation("Updating approved amount for user {User}.", await user.UpdateApproved(repository, blockchain.Value));
            }
        }
    }

    public sealed record class Configuration
    {
        public required bool Gas { get; init; }
        public required bool Token { get; init; }
        public required bool Approved { get; init; }
    }
}
