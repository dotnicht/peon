using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon.Works;

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
                var tx = await chain.ApproveSpend(user.Id, blockchain.Value.SpenderAddress, options.Value.Amount);

                var updated = await user.UpdateApproved(repository, blockchain.Value);

                if (updated.Approved == 0)
                {
                    logger.LogError("Zero allowance detected for user {User}.", updated);
                }
            }
        }
    }

    public sealed record class Configuration
    {
        public long Amount { get; init; }
    }
}
