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
    private readonly IOptions<Blockchain> options;
    private readonly IRepository<User> repository;

    public ApproveSpend(ILogger<ApproveSpend> logger, IOptions<Blockchain> options, IRepository<User> repository)
    {
        this.logger = logger;
        this.options = options;
        this.repository = repository;
    }

    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content)
        {
            if (user.Approved == 0)
            {
                var account = new Wallet(options.Value.Users.Seed, options.Value.Users.Password)
                    .GetAccount((int)user.Id, options.Value.ChainId);

                var web3 = new Web3(account, options.Value.Node.ToString());

                web3.Eth.TransactionManager.UseLegacyAsDefault = true;

                var receipt = await web3.Eth.ERC20
                    .GetContractService(options.Value.TokenAddress)
                    .ApproveRequestAndWaitForReceiptAsync(new ApproveFunction
                    {
                        Spender = options.Value.SpenderAddress,
                        Value = long.MaxValue
                    }, cancellationToken);

                logger.LogInformation("Approve transaction {Hash}.", receipt.TransactionHash);

                if (!receipt.Succeeded()) logger.LogError("Approve transaction failed {Hash}.", receipt.TransactionHash);
            }
        }
    }
}
