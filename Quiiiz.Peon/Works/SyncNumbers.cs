using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class SyncNumbers : IRunnable
{
    private readonly IRepository<User> repository;
    private readonly IOptions<Blockchain> options;
    private readonly ILogger<SyncNumbers> logger;

    public SyncNumbers(IRepository<User> repository, IOptions<Blockchain> options, ILogger<SyncNumbers> logger)
    {
        this.repository = repository;
        this.options = options;
        this.logger = logger;
    }

    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        var web3 = new Web3(new Wallet(options.Value.Master.Seed, options.Value.Master.Password)
            .GetAccount(default, options.Value.ChainId), options.Value.Node.ToString());

        web3.Eth.TransactionManager.UseLegacyAsDefault = true;

        foreach (var user in repository.Content)
        {
            var balance = await web3.Eth.GetBalance.SendRequestAsync(user.Address);

            var approved = await web3.Eth.ERC20
                .GetContractService(options.Value.TokenAddress)
                .AllowanceQueryAsync(new AllowanceFunction
                {
                    Spender = options.Value.SpenderAddress,
                    Owner = user.Address
                });

            if (balance != user.Balance || approved != user.Approved)
            {
                logger.LogInformation("Updating user {User}.", user);
                await repository.Update(user with { Balance = balance, Approved = approved });
            }
        }
    }
}
