using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class CreateUsers(ILogger<CreateUsers> logger, IRepository<User> repository, IOptions<Blockchain> options)
    : IRunnable
{
    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        const int offset = 1000000;
        var wallet = new Wallet(options.Value.Users.Seed, options.Value.Users.Password);

        for (var i = offset; i <= offset + 3; i++)
        {
            var account = wallet.GetAccount(i);

            var existing = repository.Content.SingleOrDefault(x => x.Id == i);

            if (existing == null)
            {
                await repository.Add(new User { Id = i, Address = account.Address, Balance = 0, Approved = 0 });
                logger.LogInformation("Address {Address} for user {UserId} generated.", account.Address, i);
            }
            else if (existing.Address != account.Address)
            {
                logger.LogWarning("Wallet credentials changed. Address {Address} for user {UserId} is no longer valid.", existing.Address, existing.Id);
            }
        }
    }
}
