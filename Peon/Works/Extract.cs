using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peon.Domain;
using Peon.Persistence;

namespace Peon.Works;

internal class Extract(
    IRepository<User> repository,
    ILogger<Extract> logger,
    IOptions<Extract.Configuration> options,
    IChain chain) 
        : IWork, IConfig<Extract.Configuration>
{
    public async Task Work(CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content.OrderBy(x => x.Id))
        {
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

        public sealed record class Asset
        {
            public required bool Extract { get; init; }
            public required string Address { get; init; }
            public required bool Refresh { get; init; }
        }
    }
}