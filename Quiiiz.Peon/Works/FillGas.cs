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

internal class FillGas : IRunnable
{
    private readonly ILogger<FillGas> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly IRepository<User> repository;

    public FillGas(ILogger<FillGas> logger, IOptions<Blockchain> blockchain, IRepository<User> repository, IOptions<Configuration> options)
    {
        this.logger = logger;
        this.blockchain = blockchain;
        this.repository = repository;
        this.options = options;
    }

    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        var account = new Wallet(blockchain.Value.Master.Seed, blockchain.Value.Master.Password)
            .GetAccount(blockchain.Value.MasterIndex, blockchain.Value.ChainId);

        var web3 = new Web3(account, blockchain.Value.Node.ToString());

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

                    var hash = await web3.Eth.GetEtherTransferService().TransferEtherAsync(user.Address, options.Value.Amount);

                    logger.LogInformation("Sending transaction {Hash}.", hash);

                    hashes.Add(hash);
                }

                await repository.Update(user with { Balance = balance.Value });
            }
        }

        if (hashes.Count == 0) return;

        var receipts = await web3.Eth.Transactions.GetTransactionReceipt.SendBatchRequestAsync(hashes.ToArray());

        foreach (var receipt in receipts.Where(x => !x.Succeeded()))
        {
            logger.LogError("Transaction {Hash} failed.", receipt.TransactionHash);
        }
    }

    public sealed record class Configuration : WorkConfigurationBase
    {
        public required long Amount { get; init; }
    }
}
