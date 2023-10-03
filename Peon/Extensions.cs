using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peon.Works;

namespace Peon;

public static class Extensions
{
    // TODO: refactor to use IConfig.
    public static IServiceCollection AddWorks(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Works");
        services.Configure<Configuration.Works>(section);

        var mi = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod(
                nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                [typeof(IServiceCollection), typeof(IConfiguration)])!;

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
