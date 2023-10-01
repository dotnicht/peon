using System.Numerics;

namespace Quiiiz.Peon.Persistence;

public interface IChain
{
    Task<string> GenerateAddress(long index);
    Task FillGas(string[] addresses, decimal amount);

    Task<string> ExtractGas(long index, string address);
    Task<string> ExtractToken(long index, string address, bool prefill);
    Task<string> ApproveSpend(long index, string address, BigInteger amount);
    Task<BigInteger> GetGasBalance(long index);
    Task<BigInteger> GetTokenBalance(long index);
    Task<BigInteger> GetAllowance(long index, string address);
}
