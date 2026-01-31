using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.DPDShipToShop.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor

        public ConfigurationModel()
        {

        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.PluginEnabled")]
        public bool PluginEnabled { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.UserName")]
        public string UserName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.DPDBaseUrl")]
        public string DPDBaseURL { get; set; }  

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.ShippingMethodName")]
        public string ShippingMethodName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.GoogleMapsApiKey")]
        public string GoogleMapsApiKey { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.DisplayPickupPointsOnMap")]
        public bool DisplayPickupPointsOnMap { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.UseGoogleAutoComplete")]
        public bool UseGoogleAutoComplete { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.StoreUrl")]
        public string StoreUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.SerialNumber")]
        public string SerialNumber { get; set; }

        public bool IsRegisted { get; set; }


        #endregion
    }
}
