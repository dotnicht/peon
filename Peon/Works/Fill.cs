using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peon.Domain;
using Peon.Persistence;

namespace Peon.Works;

internal class Fill(ILogger<Fill> logger, IRepository<User> repository, IOptions<Fill.Configuration> options, IChain chain) 
    : IWork, IConfig<Fill.Configuration>
{
    public async Task Work(CancellationToken cancellationToken)
    {
        var users = repository.Content.Where(x => x.Gas == BigInteger.Zero);
        await chain.FillGas([.. users.Select(x => x.Address)], options.Value.Amount);
        
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
