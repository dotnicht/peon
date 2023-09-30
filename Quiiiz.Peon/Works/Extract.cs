using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon.Works;

internal class Extract : IWork
{
    private readonly IRepository<User> repository;
    private readonly ILogger<Extract> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly IChain chain;

    public Extract(IRepository<User> repository, ILogger<Extract> logger, IOptions<Blockchain> blockchain, IOptions<Configuration> options, IChain chain)
    {
        this.repository = repository;
        this.logger = logger;
        this.blockchain = blockchain;
        this.options = options;
        this.chain = chain;
    }

    public async Task Work(CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content.OrderBy(x => x.Id))
        {
            var web3 = blockchain.Value.CreateUser(user);

            if (options.Value.Token.Extract && user.Token > 0)
            {
                await chain.ExtractToken(user.Id, options.Value.Token.Address, false);
                if (options.Value.Token.Refresh)
                {
                    var token = await chain.GetGasBalance(user.Id);
                    await repository.Update(user with { Token = token, Updated = DateTime.UtcNow });
                    logger.LogInformation("User {User} updated with token {Token}.", user, token);
                }
            }

            if (options.Value.Gas.Extract && user.Gas > 0)
            {
                await chain.ExtractGas(user.Id, options.Value.Gas.Address);
                if (options.Value.Gas.Refresh)
                {
                    var gas = await chain.GetGasBalance(user.Id);
                    await repository.Update(user with { Gas = gas, Updated = DateTime.UtcNow });
                    logger.LogInformation("User {User} updated with gas {Gas}.", user, gas);
                }
            }
        }
    }

    public sealed record class Configuration
    {
        public required Asset Token { get; init; }
        public required Asset Gas { get; init; }

        public sealed record Asset
        {
            public required bool Extract { get; init; }
            public required string Address { get; init; }
            public required bool Refresh { get; init; }
        }
    }
}
