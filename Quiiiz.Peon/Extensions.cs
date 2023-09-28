using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using Quiiiz.Peon.Provider;
using Quiiiz.Peon.Works;

namespace Quiiiz.Peon;

public static class Extensions
{
    public static IServiceCollection AddAddressProvider(this IServiceCollection services, string connection, string database)
        => services
            .AddSingleton(Options.Create(new Database { Connection = connection, Name = database }))
            .AddTransient<IAddressProvider, Address>()
            .AddTransient<IRepository<User>, MongoRepository<User>>();

    public static IServiceCollection AddWorks(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Works");
        services.Configure<Configuration.Works>(section);

        var mi = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod(
                nameof(OptionsConfigurationServiceCollectionExtensions.Configure), 
                new[] { typeof(IServiceCollection), typeof(IConfiguration) })!;

        var mapping = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IWork)) && !x.IsInterface && !x.IsAbstract)
            .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);


        foreach (var work in mapping)
        {
            mi.MakeGenericMethod(work.Value.GetNestedType("Configuration")!)
                .Invoke(null, new object[] { services, section.GetSection(work.Key) });

            services.AddTransient(work.Value);
        }

        return services.AddSingleton<IDictionary<string, Type>>(mapping);
    }

    // TODO: refactor to use blockchain persistence.
    public static async Task<User> UpdateGas(this User user, IRepository<User> repository, Blockchain blockchain)
    {
        var updated = user with
        {
            Gas = await blockchain
                .CreateUser(user).Eth.GetBalance
                .SendRequestAsync(user.Address),
            Updated = DateTime.UtcNow
        };

        await repository.Update(updated);
        return updated;
    }

    public static async Task<User> UpdateToken(this User user, IRepository<User> repository, Blockchain blockchain)
    {
        var updated = user with
        {
            Token = await blockchain
                .CreateUser(user).Eth.ERC20
                .GetContractService(blockchain.TokenAddress)
                .BalanceOfQueryAsync(new BalanceOfFunction { Owner = user.Address }),
            Updated = DateTime.UtcNow
        };

        await repository.Update(updated);
        return updated;
    }

    public static async Task<User> UpdateApproved(this User user, IRepository<User> repository, Blockchain blockchain)
    {
        var updated = user with
        {
            Approved = await blockchain
                .CreateUser(user).Eth.ERC20
                .GetContractService(blockchain.TokenAddress)
                .AllowanceQueryAsync(new AllowanceFunction { Spender = blockchain.SpenderAddress, Owner = user.Address }),
            Updated = DateTime.UtcNow
        };

        await repository.Update(updated);
        return updated;
    }

    public static Web3 CreateUser(this Blockchain blockchain, User user)
        => Create(blockchain, (int)user.Id, blockchain.Users);

    public static Web3 CreateMaster(this Blockchain blockchain)
        => Create(blockchain, blockchain.MasterIndex, blockchain.Master);

    private static Web3 Create(Blockchain blockchain, int index, Blockchain.Credentials credentials)
    {
        var web3 = new Web3(new Wallet(credentials.Seed, credentials.Password)
            .GetAccount(index, blockchain.ChainId), blockchain.Node.ToString());
        web3.Eth.TransactionManager.UseLegacyAsDefault = true;
        return web3;
    }
}
