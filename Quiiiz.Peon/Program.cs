using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddTransient<IRepository<User>, MongoRepository<User>>();
builder.Services.Configure<Credentials>(builder.Configuration.GetSection(nameof(Credentials)));
builder.Services.Configure<Database>(builder.Configuration.GetSection(nameof(Database)));

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);