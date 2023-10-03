using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Peon.Configuration;
using Peon.Domain;
using Peon.Persistence;
using Peon.Provider;
using Peon.Works;

namespace Peon;

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
            services.AddTransient(work.Value);
            var cfg = work.Value.GetNestedType("Configuration");
            if (cfg != null)
            {
                mi.MakeGenericMethod(cfg).Invoke(null, new object[] { services, section.GetSection(work.Key) });
            }
        }

        return services.AddSingleton<IDictionary<string, Type>>(mapping);
    }
}
