using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using Quiiiz.Peon.Provider;

namespace Quiiiz.Peon;

public static class Extensions
{
    public static IServiceCollection AddAddressProvider(this IServiceCollection services, string connection, string database)
        => services
            .AddSingleton(Options.Create(new Database { Connection = connection, Name = database }))
            .AddTransient<IAddressProvider, Address>()
            .AddTransient<IRepository<User>, MongoRepository<User>>();

    public static Web3 CreateUser(this Blockchain blockchain, int index)
        => blockchain.Create(index, blockchain.Users);

    public static Web3 CreateMaster(this Blockchain blockchain)
        => blockchain.Create(blockchain.MasterIndex, blockchain.Master);

    private static Web3 Create(this Blockchain blockchain, int index, Blockchain.Credentials credentials)
    {
        var web3 = new Web3(new Wallet(credentials.Seed, credentials.Password)
            .GetAccount(index, blockchain.ChainId), blockchain.Node.ToString());
        web3.Eth.TransactionManager.UseLegacyAsDefault = true;
        return web3;
    }
}
