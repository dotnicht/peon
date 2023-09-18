using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using Quiiiz.Peon.Works;
using RecurrentTasks;

var builder = Host.CreateApplicationBuilder();

builder.Services.Configure<Blockchain>(builder.Configuration.GetSection(nameof(Blockchain)));
builder.Services.Configure<Database>(builder.Configuration.GetSection(nameof(Database)));

builder.Services.AddTransient<IRepository<User>, MongoRepository<User>>();

var works = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableFrom(typeof(IRunnable)));

//builder.Services.AddTask<CheckUsers>(x => x.AutoStart(TimeSpan.FromDays(1), TimeSpan.FromDays(0)));
//builder.Services.AddTask<FillGas>(x => x.AutoStart(TimeSpan.FromDays(1), TimeSpan.FromMinutes(0)));
//builder.Services.AddTask<ApproveSpend>(x => x.AutoStart(TimeSpan.FromDays(0), TimeSpan.FromMinutes(0)));
builder.Services.AddTask<CheckUsers>(x => x.AutoStart(TimeSpan.FromDays(0), TimeSpan.FromMinutes(0)));

var host = builder.Build();
var source = new CancellationTokenSource();
await host.RunAsync(source.Token);