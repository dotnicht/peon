using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peon.Configuration;
using Peon.Domain;
using Peon.Persistence;

namespace Peon.Works;

internal class Check(
    ILogger<Check> logger,
    IOptions<Blockchain> blockchain,
    IRepository<User> repository,
    IOptions<Check.Configuration> options,
    IChain chain) 
        : IWork, IConfig<Check.Configuration>
{
    public async Task Work(CancellationToken cancellationToken)
    {
        for (var i = options.Value.Offset; i <= options.Value.Offset + options.Value.UsersNumber; i++)
        {
            var address = await chain.GenerateAddress(i);
            var existing = repository.Content.SingleOrDefault(x => x.Id == i);

            if (existing == null)
            {
                await repository.Add(new User
                {
                    Id = i,
                    Address = address,
                    Gas = await chain.GetGasBalance(i),
                    Token = await chain.GetTokenBalance(i),
                    Approved = await chain.GetAllowance(i, blockchain.Value.SpenderAddress)
                });

                logger.LogInformation("Address {Address} for user {UserId} generated.", address, i);
            }
            else if (existing.Address != address)
            {
                logger.LogWarning("Wallet credentials changed. Address {Address} for user {UserId} is no longer valid.", existing.Address, existing.Id);
            }
        }
    }

    public sealed record class Configuration
    {
        public int Offset { get; init; }
        public int UsersNumber { get; init; }
    }
}
