using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.RPC.Eth.DTOs;
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
        var web3 = blockchain.Value.CreateMaster();

        var hashes = new List<string>();

        foreach (var user in repository.Content)
        {
            if (user.Balance == default)
            {
                var balance = await web3.Eth.GetBalance.SendRequestAsync(user.Address);

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
