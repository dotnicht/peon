using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.HdWallet;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class ApproveSpend : IRunnable
{
    private readonly ILogger<ApproveSpend> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly IRepository<User> repository;

    public ApproveSpend(ILogger<ApproveSpend> logger, IOptions<Blockchain> blockchain, IRepository<User> repository, IOptions<Configuration> options)
    {
        this.logger = logger;
        this.blockchain = blockchain;
        this.repository = repository;
        this.options = options;
    }

    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content)
        {
            if (user.Approved == 0)
            {
                var account = new Wallet(blockchain.Value.Users.Seed, blockchain.Value.Users.Password)
                    .GetAccount((int)user.Id, blockchain.Value.ChainId);

                var web3 = new Web3(account, blockchain.Value.Node.ToString());

                web3.Eth.TransactionManager.UseLegacyAsDefault = true;

                var receipt = await web3.Eth.ERC20
                    .GetContractService(blockchain.Value.TokenAddress)
                    .ApproveRequestAndWaitForReceiptAsync(new ApproveFunction
                    {
                        Spender = blockchain.Value.SpenderAddress,
                        Value = options.Value.Amount
                    }, cancellationToken);

                logger.LogInformation("Approve transaction {Hash}.", receipt.TransactionHash);

                if (!receipt.Succeeded()) logger.LogError("Approve transaction failed {Hash}.", receipt.TransactionHash);
            }
        }
    }

    public sealed record class Configuration : WorkConfigurationBase
    {
        public ulong Amount { get; init; }
    }
}
