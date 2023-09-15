using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class FillGas(ILogger<FillGas> logger, IRepository<User> repository, IOptions<Blockchain> options) 
    : IRunnable
{
    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        var account = new Wallet(options.Value.Master.Seed, options.Value.Master.Password).GetAccount(default, options.Value.ChainId);
        var web3 = new Web3(account, options.Value.Node.ToString());

        web3.Eth.TransactionManager.UseLegacyAsDefault = true;

        var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);

        if (balance.Value.IsZero) throw new InvalidOperationException("Empty root account balance.");

        var hashes = new List<string>();

        foreach (var user in repository.Content)
        {
            if (user.Balance == default)
            {
                balance = await web3.Eth.GetBalance.SendRequestAsync(user.Address);

                if (balance.Value == default)
                {
                    logger.LogInformation("User {UserId} with empty balance at address {Address} detected.", user.Id, user.Address);

                    var hash = await web3.Eth.GetEtherTransferService().TransferEtherAsync(user.Address, 1m);

                    logger.LogInformation("Sending transaction {Hash}.", hash);

                    hashes.Add(hash);
                }

                await repository.Update(user with { Balance = balance.Value });
            }
        }

        var receipts = await web3.Eth.Transactions.GetTransactionReceipt.SendBatchRequestAsync([.. hashes]);

        foreach (var receipt in receipts.Where(x => !x.Succeeded()))
        {
            logger.LogError("Transaction {Hash} failed.", receipt.TransactionHash);
        }
    }
}
