using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.RPC.Eth.DTOs;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon.Works;

internal class Fill : IWork
{
    private readonly ILogger<Fill> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly IRepository<User> repository;
    private readonly IChain chain;

    public Fill(ILogger<Fill> logger, IOptions<Blockchain> blockchain, IRepository<User> repository, IOptions<Configuration> options, IChain chain)
    {
        this.logger = logger;
        this.blockchain = blockchain;
        this.repository = repository;
        this.options = options;
        this.chain = chain;
    }

    public async Task Work(CancellationToken cancellationToken)
    {
        var users = repository.Content.Where(x => x.Gas == 0);
        await chain.FillGas(users.Select(x => x.Address).ToArray(), options.Value.Amount);
        foreach (var user in users) 
        {
            var gas = await chain.GetGasBalance(user.Id);
            await repository.Update(user with { Gas = gas, Updated = DateTime.UtcNow });
            logger.LogInformation("User {User} updated with gas {Gas}.", user, gas);
        }
    }

    public sealed record class Configuration
    {
        public required decimal Amount { get; init; }
    }
}
