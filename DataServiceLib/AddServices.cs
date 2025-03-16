using DataAccessLib;
using DnsClient;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RepositoriesLib;
using System;

namespace DataServiceLib
{
    public static class AddServices
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddScoped<DbContext, FlagpoleCRMContext>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IWebsiteService, WebsiteService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<ICustomerFieldService, CustomerFieldService>();
            services.AddTransient<ITemplateService, TemplateService>();
            services.AddTransient<ICampaignService, CampaignService>();
            services.AddTransient<IUnsubscribeService, UnsubscribeService>();

            services.AddTransient<ICustomerRepository, CustomerRepository>();
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<IWebsiteRepository, WebsiteRepository>();
            services.AddTransient<ICustomerFieldRepository, CustomerFieldRepository>();
            services.AddTransient<ITemplateRepository, TemplateRepository>();
            services.AddTransient<ICampaignRepository, CampaignRepository>();
            services.AddTransient<IUnsubscribeRepository, UnsubscribeRepository>();

            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(CustomerRepository)));
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(AccountRepository)));
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(WebsiteRepository)));
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(CustomerFieldRepository)));
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(TemplateRepository)));
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(CampaignRepository)));
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(UnsubscribeRepository)));

            return services;
        }
    }
}
