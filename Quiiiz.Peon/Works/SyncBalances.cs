using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class SyncBalances(IRepository<User> repository, IOptions<Configuration.Blockchain> options) 
    : IRunnable
{
    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        var web3 = new Web3(new Wallet(options.Value.Master.Seed, options.Value.Master.Password)
            .GetAccount(default, options.Value.ChainId), options.Value.Node.ToString());

        //web3.Eth.TransactionManager.UseLegacyAsDefault = true;

        foreach (var user in repository.Content)
        {
            var balance = await web3.Eth.GetBalance.SendRequestAsync(user.Address);
            if (balance != user.Balance)
            {
                await repository.Update(user with { Balance = balance });
            }
        }
    }
}
