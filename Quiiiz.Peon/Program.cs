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

var section = builder.Configuration.GetSection("Works");
builder.Services.Configure<CheckUsers.Configuration>(section.GetSection(nameof(CheckUsers)));
builder.Services.Configure<FillGas.Configuration>(section.GetSection(nameof(FillGas)));
builder.Services.Configure<ApproveSpend.Configuration>(section.GetSection(nameof(ApproveSpend)));
builder.Services.Configure<SyncNumbers.Configuration>(section.GetSection(nameof(SyncNumbers)));
builder.Services.Configure<ExtractStuff.Configuration>(section.GetSection(nameof(ExtractStuff)));

builder.Services.AddScoped<IRepository<User>, MongoRepository<User>>();

builder.Services.AddTransient<IWork, CheckUsers>();
builder.Services.AddTransient<IWork, FillGas>();
builder.Services.AddTransient<IWork, ApproveSpend>();
builder.Services.AddTransient<IWork, SyncNumbers>();
builder.Services.AddTransient<IWork, ExtractStuff>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);