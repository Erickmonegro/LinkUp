using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Settings;
using LinkUpPro.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Shared
{
    public static class ServicesRegistration
    {
        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IImageStorageService, ImageStorageService>();
            #endregion
        }

    }
}
