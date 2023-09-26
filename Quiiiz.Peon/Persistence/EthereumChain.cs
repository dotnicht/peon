using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quiiiz.Peon.Configuration;

namespace Quiiiz.Peon.Persistence;

internal class EthereumChain : IChain
{
    private readonly ILogger<EthereumChain> logger;
    private readonly IOptions<Blockchain> options;

    public Task<string> ApproveSpend(long index, string address, BigInteger amount)
    {
        throw new NotImplementedException();
    }

    public Task<string> ExtractGas(long index, string address)
    {
        throw new NotImplementedException();
    }

    public Task<string> ExtractToken(long index, string address)
    {
        throw new NotImplementedException();
    }

    public Task FillGas(string[] addresses, BigInteger amount)
    {
        throw new NotImplementedException();
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
}
