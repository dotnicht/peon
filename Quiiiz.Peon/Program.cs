using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quiiiz.Peon;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;

var builder = Host.CreateApplicationBuilder();

builder.Services.Configure<Blockchain>(builder.Configuration.GetSection(nameof(Blockchain)));
builder.Services.Configure<Database>(builder.Configuration.GetSection(nameof(Database)));

builder.Services.AddScoped<IRepository<User>, MongoRepository<User>>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddWorks(builder.Configuration);

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);