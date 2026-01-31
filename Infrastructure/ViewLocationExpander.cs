using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.DPDShipToShop.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

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
