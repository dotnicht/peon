namespace Quiiiz.Peon.Provider;

public interface IAddressProvider
{
    Task<string> GetDepositAddress(long userId);
}
