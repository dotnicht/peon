using System.Numerics;

namespace Quiiiz.Peon.Persistence;

public interface IChain
{
    Task FillGas(string[] addresses, BigInteger amount);
    Task<string> ExtractGas(long index, string address);
    Task<string> ExtractToken(long index, string address);
    Task<string> ApproveSpend(long index, string address, BigInteger amount);
    Task<BigInteger> GetGasBalance(long index);
    Task<BigInteger> GetTokenBalance(long index);
    Task<BigInteger> GetAllowance(long index, string address);
}
