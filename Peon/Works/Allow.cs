using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peon.Configuration;
using Peon.Domain;
using Peon.Persistence;

namespace Peon.Works;

internal class Allow : IWork
{
    private readonly ILogger<Allow> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly IRepository<User> repository;
    private readonly IChain chain;

    public Allow(ILogger<Allow> logger, IOptions<Blockchain> blockchain, IRepository<User> repository, IOptions<Configuration> options, IChain chain)
    {
        this.logger = logger;
        this.blockchain = blockchain;
        this.repository = repository;
        this.options = options;
        this.chain = chain;
    }

    public async Task Work(CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content)
        {
            if (user.Approved == 0)
            {
                await chain.ApproveSpend(user.Id, blockchain.Value.SpenderAddress, options.Value.Amount);
                if (options.Value.Refresh)
                {
                    var approved = await chain.GetAllowance(user.Id, blockchain.Value.SpenderAddress);
                    await repository.Update(user with { Approved = approved, Updated = DateTime.UtcNow });
                    logger.LogInformation("User {User} updated with approved {Approved}.", user, approved);
                }
            }
        }
    }

    public sealed record class Configuration
    {
        public long Amount { get; init; }
        public required bool Refresh { get; init; }
    }
}
