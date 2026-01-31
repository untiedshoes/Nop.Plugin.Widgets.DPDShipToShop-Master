using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.DPDShipToShop
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class DPDShipToShopSettings : ISettings
    {

        
        /// <summary>
        /// Gets or sets if the plugin is enabled
        /// </summary>
        public bool PluginEnabled { get; set; }
        /// <summary>
        /// Gets or sets the UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the Account Number
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the DPD Base Url
        /// </summary>
        public string DPDBaseURL { get; set; }

        /// <summary>
        /// Gets or sets the Shipping Method Name
        /// </summary>
        public string ShippingMethodName { get; set; }

        /// <summary>
        /// Gets or sets the Google Maps API Key
        /// </summary>
        public string GoogleMapsApiKey { get; set; }

        /// <summary>
        /// Gets or sets the Use Google Auto Complete
        /// </summary>
        public bool UseGoogleAutoComplete { get; set; }

        /// <summary>
        /// Gets or sets the DisplayPickupPointsOnMap
        /// </summary>
        public bool DisplayPickupPointsOnMap { get; set; }

        /// <summary>
        /// Gets or sets the store url
        /// </summary>
        public string StoreUrl { get; set; }

        /// <summary>
        /// Gets or sets the Serial Number
        /// </summary>
        public string SerialNumber { get; set; }

    }
}
