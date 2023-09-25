using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quiiiz.Peon;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using Quiiiz.Peon.Works;

var builder = Host.CreateApplicationBuilder();

builder.Services.Configure<Blockchain>(builder.Configuration.GetSection(nameof(Blockchain)));
builder.Services.Configure<Database>(builder.Configuration.GetSection(nameof(Database)));

var mapping = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(x => x.IsAssignableTo(typeof(IWork)) && !x.IsInterface && !x.IsAbstract)
    .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

var section = builder.Configuration.GetSection("Works");

var mi = typeof(OptionsConfigurationServiceCollectionExtensions)
    .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure), new[] { typeof(IServiceCollection), typeof(IConfiguration) })!;

builder.Services.AddScoped<IRepository<User>, MongoRepository<User>>();

foreach (var work in mapping)
{
    mi.MakeGenericMethod(work.Value.GetNestedType("Configuration")!)
        .Invoke(null, new object[] { builder.Services, section.GetSection(work.Key) });
    builder.Services.AddTransient(typeof(IWork), work.Value);
}

builder.Services.AddSingleton<IDictionary<string, Type>>(mapping);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);