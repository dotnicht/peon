using Peon.Domain;
using Peon.Persistence;

namespace Peon.Provider;

internal class Address : IAddressProvider
{
    private readonly IRepository<User> repository;

    public Address(IRepository<User> repository) => this.repository = repository;

    public async Task<string> GetDepositAddress(long userId)
        => await Task.FromResult((repository.Content.SingleOrDefault(x => x.Id == userId)
            ?? throw new ArgumentException($"User not found with id {userId}.", nameof(userId))).Address);
}
