using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class SyncNumbers : IRunnable
{
    private readonly IRepository<User> repository;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly ILogger<SyncNumbers> logger;

    public SyncNumbers(IRepository<User> repository, IOptions<Blockchain> blockchain, ILogger<SyncNumbers> logger, IOptions<Configuration> options)
    {
        this.repository = repository;
        this.blockchain = blockchain;
        this.logger = logger;
        this.options = options;
    }

    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        var web3 = blockchain.Value.CreateMaster();

        foreach (var user in repository.Content)
        {
            var balance = await web3.Eth.GetBalance.SendRequestAsync(user.Address);

            var approved = await web3.Eth.ERC20
                .GetContractService(blockchain.Value.TokenAddress)
                .AllowanceQueryAsync(new AllowanceFunction
                {
                    Spender = blockchain.Value.SpenderAddress,
                    Owner = user.Address
                });

            var token = await web3.Eth.ERC20
                .GetContractService(blockchain.Value.TokenAddress)
                .BalanceOfQueryAsync(new BalanceOfFunction
                { 
                    Owner = user.Address,
                });

            if (balance != user.Gas || approved != user.Approved || token != user.Token)
            {
                logger.LogInformation("Updating user {User}.", user);
                await repository.Update(user with { Gas = balance, Approved = approved, Token = token });
            }
        }
    }

    public sealed record class Configuration : WorkConfigurationBase
    { 
    
    }
}
