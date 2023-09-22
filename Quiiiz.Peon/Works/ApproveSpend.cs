using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon.Works;

internal class ApproveSpend : IWork
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

    public async Task Work(CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content)
        {
            if (user.Approved == 0)
            {
                var web3 = blockchain.Value.CreateUser((int)user.Id);

                var receipt = await web3.Eth.ERC20
                    .GetContractService(blockchain.Value.TokenAddress)
                    .ApproveRequestAndWaitForReceiptAsync(new ApproveFunction
                    {
                        Spender = blockchain.Value.SpenderAddress,
                        Value = options.Value.Amount
                    }, cancellationToken);

                logger.LogInformation("Approve transaction {Hash} by user {User}.", receipt.TransactionHash, user);

                if (!receipt.Succeeded()) logger.LogError("Approve transaction failed {Hash}.", receipt.TransactionHash);

                var approve = await web3.Eth.ERC20
                    .GetContractService(blockchain.Value.TokenAddress)
                    .AllowanceQueryAsync(new AllowanceFunction
                    {
                        Spender = blockchain.Value.SpenderAddress,
                        Owner = user.Address
                    });

                if (approve == 0) throw new InvalidOperationException("Zero allowance detected. ");

                await repository.Update(user with { Approved = approve });
            }
        }
    }

    public sealed record class Configuration
    {
        public long Amount { get; init; }
    }
}
