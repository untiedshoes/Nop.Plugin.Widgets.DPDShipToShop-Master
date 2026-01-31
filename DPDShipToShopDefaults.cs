using Nop.Core;
using Nop.Core.Caching;

namespace Nop.Plugin.Widgets.DPDShipToShop
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class DPDShipToShopDefaults
    {
        /// <summary>
        /// Gets a name of the route to the import contacts callback
        /// </summary>
        public static string GetShipToShopLocations => "Plugin.Widgets.DPDShipToShop.GetShipToShopLocations";

        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string SystemName = "Widgets.DPDShipToShop";

        /// <summary>
        /// Gets a name of the view component to display payment info in public store
        /// </summary>
        public const string DPD_MAP_VIEW_COMPONENT_NAME = "DPDMap";

        /// <summary>
        /// Gets a name of the view component to display create shipment button
        /// </summary>
        public const string DPD_CREATE_SHIPMENT_BUTTON_COMPONENT_NAME = "DPDCreateShipmentButton";

        #region Caching

        /// <summary>
        /// Gets the cache key for configuration
        /// </summary>
        /// <remarks>
        /// {0} : configuration identifier
        /// </remarks>
        public static CacheKey LicenseCacheKey => new CacheKey("Plugin.Widgets.DPDShipToShop.LicenseCacheKey-{0}", LicensePrefixCacheKey);

        /// <summary>
        /// Gets the prefix key to clear cache
        /// </summary>
        public static string LicensePrefixCacheKey => "Plugin.Widgets.DPDShipToShop.LicenseKey";

        #endregion
    }
}
