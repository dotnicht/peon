using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.RPC.Eth.DTOs;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon.Works;

internal class Fill : IWork
{
    private readonly ILogger<Fill> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly IRepository<User> repository;

    public Fill(ILogger<Fill> logger, IOptions<Blockchain> blockchain, IRepository<User> repository, IOptions<Configuration> options)
    {
        this.logger = logger;
        this.blockchain = blockchain;
        this.repository = repository;
        this.options = options;
    }

    public async Task Work(CancellationToken cancellationToken)
    {
        var web3 = blockchain.Value.CreateMaster();

        var hashes = new List<string>();

        foreach (var user in repository.Content)
        {
            if (user.Gas == 0)
            {
                logger.LogInformation("User {UserId} with empty balance at address {Address} detected.", user.Id, user.Address);

                var hash = await web3.Eth.GetEtherTransferService().TransferEtherAsync(user.Address, options.Value.Amount);

                logger.LogInformation("Sending transaction {Hash} with gas for user {User}.", hash, user);

                hashes.Add(hash);
            }
        }

        if (hashes.Count == 0)
        {
            return;
        }

        var receipts = await web3.Eth.Transactions.GetTransactionReceipt.SendBatchRequestAsync(hashes.ToArray());

        foreach (var receipt in receipts.Where(x => !x.Succeeded()))
        {
            logger.LogError("Transaction {Hash} failed.", receipt.TransactionHash);
        }

        foreach (var user in repository.Content)
        {
            await user.UpdateGas(repository, blockchain.Value);
        }
    }

    public sealed record class Configuration
    {
        public required long Amount { get; init; }
    }
}
