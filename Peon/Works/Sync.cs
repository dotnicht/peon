using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peon.Configuration;
using Peon.Domain;
using Peon.Persistence;

namespace Peon.Works;

internal class Sync(IRepository<User> repository, IOptions<Blockchain> blockchain, ILogger<Sync> logger, IOptions<Sync.Configuration> options, IChain chain) 
    : IWork, IConfig<Sync.Configuration>
{
    public async Task Work(CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content)
        {
            if (options.Value.Gas)
            {
                var gas = await chain.GetGasBalance(user.Id);
                if (gas != user.Gas)
                {
                    await repository.Update(user with { Gas = gas, Updated = DateTime.UtcNow });
                    logger.LogInformation("Updated gas amount {Amount} for user {User}.", gas, user);
                }
            }

            if (options.Value.Token)
            {
                var token = await chain.GetTokenBalance(user.Id);
                if (token != user.Token)
                {
                    await repository.Update(user with { Token = token, Updated = DateTime.UtcNow });
                    logger.LogInformation("Updated token amount {Amount} for user {User}.", token, user);
                }
            }

            if (options.Value.Approved)
            {
                var approved = await chain.GetAllowance(user.Id, blockchain.Value.SpenderAddress);
                if (approved != user.Approved)
                {
                    await repository.Update(user with { Approved = approved, Updated = DateTime.UtcNow });
                    logger.LogInformation("Updated approved amount {Amount} for user {User}.", approved, user);
                }
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
