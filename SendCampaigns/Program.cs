using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.EntityFrameworkCore;
using SendCampaigns;
using StackExchange.Redis;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        var connectionString = configuration["ConnectionString"];

        services.AddDbContext<FlagpoleCRMContext>(x => x.UseSqlServer(connectionString));
        services.AddScoped<DbContext, FlagpoleCRMContext>();
        services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(CampaignService)));
        services.AddTransient<ICampaignService, CampaignService>();
        services.AddTransient<IWebsiteService, WebsiteService>();
        services.AddTransient<ICustomerService, CustomerService>();
        services.AddTransient<ITemplateService, TemplateService>();
        services.AddTransient<IUnsubscribeService, UnsubscribeService>();
        services.AddSingleton<IConnectionMultiplexer>(options => ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { $"{configuration["RedisConnection:Host"]}:{configuration["RedisConnection:Port"]}" }
        }));
        services.AddHostedService<SendCampaignWorker>();
    })
    .Build();

await host.RunAsync();
