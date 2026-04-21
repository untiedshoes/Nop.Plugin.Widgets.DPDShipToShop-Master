using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
using Nop.Plugin.Widgets.DPDShipToShop.Services;
using Nop.Plugin.Widgets.DPDShipToShop.Factories;
using Nop.Services.Messages;
using Nop.Plugin.Widgets.DPDShipToShop.Services.Messages;

namespace Nop.Plugin.Widgets.DPDShipToShop.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDPDShipToShopService, DPDShipToShopService>();
            services.AddScoped<IDPDShipToShopModelFactory, DPDShipToShopModelFactory>();
            services.AddScoped<IDPDShipToShopAutomationManager, DPDShipToShopAutomationManager>();
            services.AddScoped<IDPDSessionService, DPDSessionService>();
            services.AddScoped<LicenseService>();

            //override services
            services.AddScoped<IMessageTokenProvider, CustomMessageTokenProvider>();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 3000;
    }
}
