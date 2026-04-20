using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Widgets.DPDShipToShop.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        /// <summary>
        /// Registers plugin services and view location expansion rules.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The application configuration.</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
        }

        /// <summary>
        /// Configures the plugin application pipeline.
        /// </summary>
        /// <param name="application">The application builder.</param>
        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}
