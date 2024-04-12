using FlagpoleCRM.Areas.Login.Controllers;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace FlagpoleCRM
{
    public static class AddServices
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddSingleton<ILog>(_ => LogManager.GetLogger(typeof(AuthenticationController)));
            return services;
        }
    }
}
