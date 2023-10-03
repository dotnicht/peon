using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Peon;
using Peon.Configuration;
using Peon.Domain;
using Peon.Persistence;

var builder = Host.CreateApplicationBuilder();

builder.Services.Configure<Blockchain>(builder.Configuration.GetSection(nameof(Blockchain)));
builder.Services.Configure<Database>(builder.Configuration.GetSection(nameof(Database)));

builder.Services.AddScoped<IRepository<User>, MongoRepository<User>>();
builder.Services.AddSingleton<IChain, EthereumChain>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddWorks(builder.Configuration);

await builder.Build().RunAsync();