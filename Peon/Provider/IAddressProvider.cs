namespace Peon.Provider;

public interface IAddressProvider
{
    Task<string> GetDepositAddress(long userId);
}
