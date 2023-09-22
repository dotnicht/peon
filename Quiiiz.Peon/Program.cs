using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using Quiiiz.Peon.Works;

var builder = Host.CreateApplicationBuilder();

builder.Services.Configure<Blockchain>(builder.Configuration.GetSection(nameof(Blockchain)));
builder.Services.Configure<Database>(builder.Configuration.GetSection(nameof(Database)));

var section = builder.Configuration.GetSection("Works");
builder.Services.Configure<CheckUsers.Configuration>(section.GetSection(nameof(CheckUsers)));
builder.Services.Configure<ApproveSpend.Configuration>(section.GetSection(nameof(ApproveSpend)));
builder.Services.Configure<FillGas.Configuration>(section.GetSection(nameof(FillGas)));
builder.Services.Configure<SyncNumbers.Configuration>(section.GetSection(nameof(SyncNumbers)));
builder.Services.Configure<ExtractStuff.Configuration>(section.GetSection(nameof(ExtractStuff)));

builder.Services.AddTransient<IRepository<User>, MongoRepository<User>>();

var services = builder.Services.BuildServiceProvider();

//builder.Services.AddTask<CheckUsers>();
builder.Services.AddTask<ApproveSpend>();
//builder.Services.AddTask<FillGas>();
//builder.Services.AddTask<SyncNumbers>();
//builder.Services.AddTask<ExtractStuff>();

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);