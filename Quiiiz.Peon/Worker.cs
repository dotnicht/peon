using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon;

internal class Worker : IHostedService
{
    private readonly ILogger<Worker> logger;
    private readonly IRepository<Account> repository;
    private readonly IOptions<Configuration.Wallet> options;

    public Worker(ILogger<Worker> logger, IRepository<Account> repository, IOptions<Configuration.Wallet> options)
    {
        this.logger = logger;
        this.repository = repository;
        this.options = options;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const int offset = 1000000;

        var wallet = new Wallet(options.Value.Seed, options.Value.Password);

        for (var i = offset; i <= offset + 50; i++)
        {
            var account = wallet.GetAccount(i);

            var existing = repository.Content.SingleOrDefault(x => x.Id == i);

            if (existing == null)
            {
                await repository.Add(new Account { Id = i, Public = account.Address });
                logger.LogInformation("Public key {Address} for ID {ID} generated.", account.Address, i);
            }
            else if (existing.Public != account.Address)
            {
                logger.LogWarning("Wallet credentials changed. Address {Address} for ID {ID} is no longer valid.", existing.Public, existing.Id);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}
