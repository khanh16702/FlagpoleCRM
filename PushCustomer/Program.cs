using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.EntityFrameworkCore;
using PushCustomers;
using StackExchange.Redis;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        var connectionString = configuration["ConnectionString"];

        services.AddDbContext<FlagpoleCRMContext>(x => x.UseSqlServer(connectionString));
        services.AddScoped<DbContext, FlagpoleCRMContext>();
        services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(WebsiteService)));
        services.AddTransient<IWebsiteService, WebsiteService>();
        services.AddTransient<ICustomerService, CustomerService>();
        services.AddSingleton<IConnectionMultiplexer>(options => ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { $"{configuration["RedisConnection:Host"]}:{configuration["RedisConnection:Port"]}" }
        }));
        services.AddHostedService<PushCustomersWorker>();
    })
    .Build();

await host.RunAsync();
