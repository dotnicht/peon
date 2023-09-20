using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class CheckUsers : IRunnable
{
    private readonly ILogger<CheckUsers> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly IRepository<User> repository;

    public CheckUsers(ILogger<CheckUsers> logger, IOptions<Blockchain> blockchain, IRepository<User> repository, IOptions<Configuration> options)
    {
        this.logger = logger;
        this.blockchain = blockchain;
        this.options = options;
        this.repository = repository;
    }

    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        var wallet = new Wallet(blockchain.Value.Users.Seed, blockchain.Value.Users.Password);

        for (var i = options.Value.Offset; i <= options.Value.Offset + options.Value.UsersNumber; i++)
        {
            var account = wallet.GetAccount(i);

            var existing = repository.Content.SingleOrDefault(x => x.Id == i);

            if (existing == null)
            {
                await repository.Add(new User { Id = i, Address = account.Address, Gas = 0, Token = 0, Approved = 0 });
                logger.LogInformation("Address {Address} for user {UserId} generated.", account.Address, i);
            }
            else if (existing.Address != account.Address)
            {
                logger.LogWarning("Wallet credentials changed. Address {Address} for user {UserId} is no longer valid.", existing.Address, existing.Id);
            }
        }
    }

    public sealed record class Configuration : WorkConfigurationBase 
    { 
        public int Offset { get; init; }
        public int UsersNumber { get; init; }
    }
}
