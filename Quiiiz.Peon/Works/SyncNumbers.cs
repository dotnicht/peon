using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class SyncNumbers(IRepository<User> repository, IOptions<Configuration.Blockchain> options)
    : IRunnable
{
    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        var cfg = options.Value;

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
                    Owner = web3.TransactionManager.Account.Address
                });

            //var approved = await web3.Eth.ERC20
            //    .GetContractService(options.Value.TokenAddress)
            //    .AllowedQueryAsync(new AllowedFunction
            //    {
            //        Spender = options.Value.SpenderAddress,
            //        Owner = web3.TransactionManager.Account.Address
            //    });

            if (balance != user.Balance || approved != user.Approved)
            {
                await repository.Update(user with { Balance = balance, Approved = approved });
            }
        }
    }
}
