using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using Quiiiz.Peon.Works;

var builder = Host.CreateApplicationBuilder();

builder.Services.Configure<Blockchain>(builder.Configuration.GetSection(nameof(Blockchain)));
builder.Services.Configure<Database>(builder.Configuration.GetSection(nameof(Database)));

builder.Services.AddTransient<IRepository<User>, MongoRepository<User>>();

builder.Services.AddTask<CreateUsers>(x => x.AutoStart(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
builder.Services.AddTask<SendCurrency>(x => x.AutoStart(TimeSpan.FromDays(1)));

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);