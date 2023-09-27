using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Quiiiz.Peon.Configuration;

namespace Quiiiz.Peon.Persistence;

internal class EthereumChain : IChain
{
    private readonly ILogger<EthereumChain> logger;
    private readonly IOptions<Blockchain> options;

    public EthereumChain(ILogger<EthereumChain> logger, IOptions<Blockchain> options)
    {
        this.logger = logger;
        this.options = options;
    }

    public async Task<string> ApproveSpend(long index, string address, BigInteger amount)
    {
        var receipt = await User((int)index).Eth.ERC20
            .GetContractService(options.Value.TokenAddress)
            .ApproveRequestAndWaitForReceiptAsync(address, amount);

        logger.LogInformation("Approve transaction {Hash} by account index {Index}.", receipt.TransactionHash, index);

        if (!receipt.Succeeded())
        {
            logger.LogError("Approve transaction failed {Hash}.", receipt.TransactionHash);
        }

        return receipt.TransactionHash;
    }

    public async Task<string> ExtractGas(long index, string address)
    {
        var web3 = User(index);

        // TODO: recheck gas price.
        var amount = await web3.Eth
            .GetEtherTransferService()
            .CalculateTotalAmountToTransferWholeBalanceInEtherAsync(web3.TransactionManager.Account.Address, await web3.Eth.GasPrice.SendRequestAsync());

        var receipt = await web3.Eth
            .GetEtherTransferService()
            .TransferEtherAndWaitForReceiptAsync(address, amount);

        if (receipt.Succeeded())
        {
            logger.LogInformation("Extracted gas from account index {Index}. Transaction {Hash}.", index, receipt.TransactionHash);
        }
        else
        {
            logger.LogError("Gas transfer transaction failed {Hash}.", receipt.TransactionHash);
        }

        return receipt.TransactionHash;
    }

    public Task<string> ExtractToken(long index, string address)
    {
        throw new NotImplementedException();
    }

    public async Task FillGas(string[] addresses, decimal amount)
    {
        var hashes = new List<string>();
        var web3 = Master();

        foreach (var address in addresses)
        {
            hashes.Add(await web3.Eth.GetEtherTransferService().TransferEtherAsync(address, amount));
        }

        if (hashes.Count != 0)
        {
            var receipts = await web3.Eth.Transactions.GetTransactionReceipt.SendBatchRequestAsync(hashes.ToArray());

            foreach (var receipt in receipts.Where(x => !x.Succeeded()))
            {
                logger.LogError("Transaction {Hash} failed.", receipt.TransactionHash);
            }
        }
    }

    public Task<BigInteger> GetAllowance(long index, string address)
    {
        throw new NotImplementedException();
    }

    public Task<BigInteger> GetGasBalance(long index)
    {
        throw new NotImplementedException();
    }

    public Task<BigInteger> GetTokenBalance(long index)
    {
        throw new NotImplementedException();
    }

    private Web3 User(long index)
        => Create((int)index, options.Value.Users);

    private Web3 Master()
        => Create(options.Value.MasterIndex, options.Value.Master);

    private Web3 Create(int index, Blockchain.Credentials credentials)
    {
        var web3 = new Web3(new Wallet(credentials.Seed, credentials.Password)
            .GetAccount(index, options.Value.ChainId), options.Value.Node.ToString());
        web3.Eth.TransactionManager.UseLegacyAsDefault = true;
        return web3;
    }
}
