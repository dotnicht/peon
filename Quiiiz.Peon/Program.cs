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
builder.Services.Configure<Check.Configuration>(section.GetSection(nameof(Check)));
builder.Services.Configure<Fill.Configuration>(section.GetSection(nameof(Fill)));
builder.Services.Configure<Allow.Configuration>(section.GetSection(nameof(Allow)));
builder.Services.Configure<Sync.Configuration>(section.GetSection(nameof(Sync)));
builder.Services.Configure<Extract.Configuration>(section.GetSection(nameof(Extract)));

builder.Services.AddScoped<IRepository<User>, MongoRepository<User>>();

builder.Services.AddTransient<IWork, Check>();
builder.Services.AddTransient<IWork, Fill>();
builder.Services.AddTransient<IWork, Allow>();
builder.Services.AddTransient<IWork, Sync>();
builder.Services.AddTransient<IWork, Extract>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);