using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

var options = (services.GetRequiredService<IOptions<CheckUsers.Configuration>>().Value as WorkConfigurationBase)!;
builder.Services.AddTask<CheckUsers>(x => x.AutoStart(options.Interval, options.FirstRunDelay));

options = services.GetRequiredService<IOptions<ApproveSpend.Configuration>>().Value;
builder.Services.AddTask<ApproveSpend>(x => x.AutoStart(options.Interval, options.FirstRunDelay));

options = services.GetRequiredService<IOptions<FillGas.Configuration>>().Value;
builder.Services.AddTask<FillGas>(x => x.AutoStart(options.Interval, options.FirstRunDelay));

options = services.GetRequiredService<IOptions<SyncNumbers.Configuration>>().Value;
builder.Services.AddTask<SyncNumbers>(x => x.AutoStart(options.Interval, options.FirstRunDelay));

options = services.GetRequiredService<IOptions<ExtractStuff.Configuration>>().Value;
builder.Services.AddTask<ExtractStuff>(x => x.AutoStart(options.Interval, options.FirstRunDelay));

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);