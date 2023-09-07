using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quiiiz.Peon;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<IRepository<Account>, MongoRepository<Account>>();
builder.Services.Configure<Wallet>(builder.Configuration.GetSection(nameof(Wallet)));
builder.Services.Configure<Database>(builder.Configuration.GetSection(nameof(Database)));

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);