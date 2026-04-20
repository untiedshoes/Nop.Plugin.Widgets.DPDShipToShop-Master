using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.DPDShipToShop.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        /// <summary>
        /// Populates values used when expanding view search locations.
        /// </summary>
        /// <param name="context">The view location expander context.</param>
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <summary>
        /// Adds plugin-specific Razor view search locations for checkout pages.
        /// </summary>
        /// <param name="context">The view location expander context.</param>
        /// <param name="viewLocations">The existing view location sequence.</param>
        /// <returns>The updated collection of view search locations.</returns>
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            //if (context.AreaName == "Admin")
            //{
            //    viewLocations = new[] { $"/Plugins/Widgets.DPDShipToShop/Areas/Admin/Views/{context.ControllerName}/{context.ViewName}.cshtml" }.Concat(viewLocations);
            //}
            //else
            //{
            //    viewLocations = new[] { $"/Plugins/Widgets.DPDShipToShop/Views/{context.ControllerName}/{context.ViewName}.cshtml" }.Concat(viewLocations);
            //}

            if (context.AreaName == null && context.ControllerName == "Checkout" && context.ViewName == "OpcShippingMethods")
            {
                viewLocations = new string[] { $"/Plugins/Widgets.DPDShipToShop/Views/checkout/{{0}}.cshtml" }.Concat(viewLocations);
            }

            if (context.AreaName == null && context.ControllerName == "Checkout" && context.ViewName == "OnePageCheckout")
            {
                viewLocations = new string[] { $"/Plugins/Widgets.DPDShipToShop/Views/checkout/{{0}}.cshtml" }.Concat(viewLocations);
            }

            return viewLocations;
        }
    }
}
