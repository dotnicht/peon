using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace Quiiiz.Peon;

internal class Worker : IHostedService
{
    private readonly ILogger<Worker> logger;
    private readonly IRepository<Address> repository;
    private readonly IOptions<Configuration.Wallet> options;

    public Worker(ILogger<Worker> logger, IRepository<Address> repository, IOptions<Configuration.Wallet> options)
    {
        this.logger = logger;
        this.repository = repository;
        this.options = options;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const int offset = 1000000;

        var wallet = new Wallet(options.Value.Seed, options.Value.Password);
        var data = SHA256.HashData(Encoding.UTF8.GetBytes(options.Value.Seed + options.Value.Password));
        var hash = BitConverter.ToString(data).Replace("-", string.Empty);

        for (var i = offset; i <= offset + 5000; i++)
        {
            var account = wallet.GetAccount(i);
            var address = repository.Content.SingleOrDefault(x => x.Public == account.Address);
            if (address == null)
            {
                address = new Address { Id = i, Public = account.Address, Hash = hash };
                await repository.Add(address);
                logger.LogInformation("Address {Address} for ID {ID} generated.", account.Address, i);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}
