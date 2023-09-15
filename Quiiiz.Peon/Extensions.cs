using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using Quiiiz.Peon.Provider;

namespace Quiiiz.Peon;

public static class Extensions
{
    public static IServiceCollection AddAddressProvider(this IServiceCollection services, string connection, string database)
    {
        services.AddSingleton(Options.Create(new Configuration.Database { Connection = connection, Name = database }));
        return services.AddTransient<IAddressProvider, Address>().AddTransient<IRepository<User>, MongoRepository<User>>();
    }
}
