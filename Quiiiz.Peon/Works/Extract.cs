using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon.Works;

internal class Extract : IWork
{
    private readonly IRepository<User> repository;
    private readonly ILogger<Extract> logger;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;

    public Extract(IRepository<User> repository, ILogger<Extract> logger, IOptions<Blockchain> blockchain, IOptions<Configuration> options)
    {
        this.repository = repository;
        this.logger = logger;
        this.blockchain = blockchain;
        this.options = options;
    }

    public async Task Work(CancellationToken cancellationToken)
    {
        foreach (var user in repository.Content.OrderBy(x => x.Id).Take(20))
        {
            var web3 = blockchain.Value.CreateUser(user);

            if (options.Value.Token.Extract && user.Token > 0)
            {
                var receipt = await web3.Eth.ERC20
                    .GetContractService(blockchain.Value.TokenAddress)
                    .TransferRequestAndWaitForReceiptAsync(new TransferFunction
                    {
                        AmountToSend = user.Token,
                        To = options.Value.Token.Address,
                        GasPrice = await web3.Eth.GasPrice.SendRequestAsync(),
                        Gas = 210000
                    }, cancellationToken);

                if (receipt.Succeeded())
                {
                    logger.LogInformation("Extracted token from user {User}. Transaction {Hash}.",
                        await user.UpdateToken(repository, blockchain.Value),
                        receipt.TransactionHash);
                }
                else
                {
                    logger.LogError("Token transfer transaction failed {Hash}.", receipt.TransactionHash);
                }
            }

            if (options.Value.Gas.Extract && user.Gas > 0)
            {
                var amount = await web3.Eth
                    .GetEtherTransferService()
                    .CalculateTotalAmountToTransferWholeBalanceInEtherAsync(user.Address, await web3.Eth.GasPrice.SendRequestAsync());
                var receipt = await web3.Eth
                    .GetEtherTransferService()
                    .TransferEtherAndWaitForReceiptAsync(options.Value.Gas.Address, amount, cancellationToken: cancellationToken);

                if (receipt.Succeeded())
                {
                    logger.LogInformation("Extracted gas from user {User}. Transaction {Hash}.",
                        await user.UpdateGas(repository, blockchain.Value),
                        receipt.TransactionHash);
                }
                else
                {
                    logger.LogError("Gas transfer transaction failed {Hash}.", receipt.TransactionHash);
                }
            }
        }
    }

    public sealed record class Configuration
    {
        public required Asset Token { get; init; }
        public required Asset Gas { get; init; }

        public sealed record Asset
        {
            public required bool Extract { get; init; }
            public required string Address { get; init; }
        }
    }
}
