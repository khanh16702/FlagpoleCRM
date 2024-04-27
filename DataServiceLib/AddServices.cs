using FlagpoleCRM.Models;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DataServiceLib
{
    public static class AddServices
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddScoped<DbContext, FlagpoleCRMContext>();
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(AccountService)));
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(WebsiteService)));
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IWebsiteService, WebsiteService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<ICustomerFieldService, CustomerFieldService>();
            services.AddTransient<ITemplateService, TemplateService>();
            services.AddTransient<ICampaignService, CampaignService>();
            services.AddTransient<IUnsubscribeService, UnsubscribeService>();
            return services;
        }
    }
}
