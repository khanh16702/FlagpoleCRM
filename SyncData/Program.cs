using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Core.Configuration;
using SyncData;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        var connectionString = configuration["ConnectionString"];

        services.AddDbContext<FlagpoleCRMContext>(x => x.UseSqlServer(connectionString));
        services.AddScoped<DbContext, FlagpoleCRMContext>();
        services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(WebsiteService)));
        services.AddTransient<IWebsiteService, WebsiteService>();
        services.AddHostedService<SyncDataWorker>();
    })
    .Build();

await host.RunAsync();
