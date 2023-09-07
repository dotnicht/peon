using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works
{
    internal class CreateUsers : IRunnable
    {
        private readonly ILogger logger;
        private readonly IRepository<User> repository;
        private readonly IOptions<Credentials> options;

        public CreateUsers(ILogger<CreateUsers> logger, IRepository<User> repository, IOptions<Credentials> options)
        {
            this.logger = logger;
            this.repository = repository;
            this.options = options;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            const int offset = 1000000;
            var wallet = new Wallet(options.Value.Seed, options.Value.Password);

            for (var i = offset; i <= offset + 50; i++)
            {
                var account = wallet.GetAccount(i);

                var existing = repository.Content.SingleOrDefault(x => x.Id == i);

                if (existing == null)
                {
                    await repository.Add(new User { Id = i, Address = account.Address, Balance = 0 });
                    logger.LogInformation("Public key {Address} for ID {ID} generated.", account.Address, i);
                }
                else if (existing.Address != account.Address)
                {
                    logger.LogWarning("Wallet credentials changed. Address {Address} for ID {ID} is no longer valid.", existing.Address, existing.Id);
                }
            }
        }
    }
}
