using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

namespace Quiiiz.Peon.Provider;

internal class Address(IRepository<User> repository) : IAddressProvider
{
    public async Task<string> GetDepositAddress(long userId)
        => await Task.FromResult((repository.Content.SingleOrDefault(x => x.Id == userId)
            ?? throw new ArgumentException($"User not found with id {userId}.", nameof(userId))).Address);
}
