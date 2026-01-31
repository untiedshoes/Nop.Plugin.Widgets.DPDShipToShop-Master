using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Plugin.Widgets.DPDShipToShop.Services;
using Nop.Plugin.Widgets.DPDShipToShop.Data;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Framework.Menu;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using System.Linq;
using Nop.Services.Security;
using System;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop
{
    class DPDShipToShopComputationMethod : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IDPDShipToShopService _DPDShipToShopService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public DPDShipToShopComputationMethod(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IDPDShipToShopService DPDShipToShopService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _DPDShipToShopService = DPDShipToShopService;
            _permissionService = permissionService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        //public IShipmentTracker ShipmentTracker => new DPDShipmentTracker(_DPDService);

        #endregion

        #region Methods

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public async Task<IList<string>> GetWidgetZonesAsync()
        {
            return new List<string> { PublicWidgetZones.OpCheckoutShippingMethodBottom, AdminWidgetZones.OrderDetailsButtons };
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/DPDShipToShop/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));
            if (widgetZone.Equals(PublicWidgetZones.OpCheckoutShippingMethodBottom))
                return DPDShipToShopDefaults.DPD_MAP_VIEW_COMPONENT_NAME;
            if (widgetZone.Equals(AdminWidgetZones.OrderDetailsButtons))
                return DPDShipToShopDefaults.DPD_CREATE_SHIPMENT_BUTTON_COMPONENT_NAME;

            return string.Empty;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public async override Task InstallAsync()
        {

            await _settingService.SaveSettingAsync(new DPDShipToShopSettings());

            ////locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.DPDShipToShop.Blocktitle"] = "<p>Welcome to the DPD Ship to Store Configuration.</p>",
                ["Plugins.Widgets.DPDShipToShop.Instructions"] = "<p>Enhance your customer experience by integrating DPD Pickup into your checkout.</p><p>The plugin adds a new Order Token (%Order.DPDShipToShopLocation%) which you will need to manually add to your message templates.</p>",
                ["Plugins.Widgets.DPDShipToShop.Fields.PluginEnabled"] = "Enable plugin",
                ["Plugins.Widgets.DPDShipToShop.Fields.PluginEnabled.Hint"] = "Enable Plugin",
                ["Plugins.Widgets.DPDShipToShop.Fields.UserName"] = "DPD Username",
                ["Plugins.Widgets.DPDShipToShop.Fields.UserName.Hint"] = "Enter your DPD Username",
                ["Plugins.Widgets.DPDShipToShop.Fields.Password"] = "DPD Password",
                ["Plugins.Widgets.DPDShipToShop.Fields.Password.Hint"] = "Enter your DPD Password",
                ["Plugins.Widgets.DPDShipToShop.Fields.AccountNumber"] = "DPD Account Number",
                ["Plugins.Widgets.DPDShipToShop.Fields.AccountNumber.Hint"] = "Enter your DPD Account Number",
                ["Plugins.Widgets.DPDShipToShop.Fields.DPDBaseUrl"] = "DPD Base Url",
                ["Plugins.Widgets.DPDShipToShop.Fields.DPDBaseUrl.Hint"] = "Enter the DPD Base Url",
                ["Plugins.Widgets.DPDShipToShop.Fields.ShippingMethodName"] = "Shipping Method Name",
                ["Plugins.Widgets.DPDShipToShop.Fields.ShippingMethodName.Hint"] = "Enter the Shipping Method Name - This is used to hook the JS into to view the map",
                ["Plugins.Widgets.DPDShipToShop.Fields.GoogleMapsApiKey"] = "Google Maps Api Key",
                ["Plugins.Widgets.DPDShipToShop.Fields.GoogleMapsApiKey.Hint"] = "Enter your Google Maps Api Key",
                ["Plugins.Widgets.DPDShipToShop.Fields.UseGoogleAutoComplete"] = "Enable Google Auto Complete",
                ["Plugins.Widgets.DPDShipToShop.Fields.UseGoogleAutoComplete.Hint"] = "Check to Enable Google Auto Complete",
                ["Plugins.Widgets.DPDShipToShop.Fields.StoreUrl"] = "Website address (for registration)",
                ["Plugins.Widgets.DPDShipToShop.Fields.SerialNumber"] = "Serial Number",
                ["Plugins.Widgets.DPDShipToShop.SearchPlaceholder"] = "Postal Code",
                ["Plugins.Widgets.DPDShipToShop.SelectButtonText"] = "SELECT",
                ["Plugins.Widgets.DPDShipToShop.OpenTimesHeading"] = "Normal Opening Times",
                ["Plugins.Widgets.DPDShipToShop.OpenTimesMonFri"] = "Mon - Fri",
                ["Plugins.Widgets.DPDShipToShop.OpenTimesSaturday"] = "Sat",
                ["Plugins.Widgets.DPDShipToShop.OpenTimesSunday"] = "Sun",
                ["Plugins.Widgets.DPDShipToShop.LoadingMap"] = "Loading DPD Shop Finder, please wait...",
                ["Plugins.Widgets.DPDShipToShop.Fields.CollectionDate"] = "Collection Date",
                ["Plugins.Widgets.DPDShipToShop.Fields.CollectionDate.Hint"] = "Collection Date",
                ["Plugins.Widgets.DPDShipToShop.Fields.NumberOfParcels"] = "Number of Parcels",
                ["Plugins.Widgets.DPDShipToShop.Fields.NumberOfParcels.Hint"] = "Number of Parcels",
                ["Plugins.Widgets.DPDShipToShop.Fields.ParcelTotalWeight"] = "Total Weight (kg)",
                ["Plugins.Widgets.DPDShipToShop.Fields.ParcelTotalWeight.Hint"] = "Total Weight (kg) - Not required if weight calculated automatically",
                ["Plugins.Widgets.DPDShipToShop.Fields.ShippingRef1"] = "Shipping Ref 1",
                ["Plugins.Widgets.DPDShipToShop.Fields.ShippingRef1.Hint"] = "Shipping Ref 1",
                ["Plugins.Widgets.DPDShipToShop.Fields.ShippingRef2"] = "Shipping Ref 2",
                ["Plugins.Widgets.DPDShipToShop.Fields.ShippingRef2.Hint"] = "Shipping Ref 2",
                ["Plugins.Widgets.DPDShipToShop.Fields.ShippingRef3"] = "Shipping Ref 3",
                ["Plugins.Widgets.DPDShipToShop.Fields.ShippingRef3.Hint"] = "Shipping Ref 3",
                ["Plugins.Widgets.DPDShipToShop.Fields.DeliveryInstructions"] = "Delivery Instructions",
                ["Plugins.Widgets.DPDShipToShop.Fields.DeliveryInstructions.Hint"] = "Delivery Instructions e.g. Leave behind bins",
                ["Plugins.Widgets.DPDShipToShop.Fields.ParcelDescription"] = "Parcel Description",
                ["Plugins.Widgets.DPDShipToShop.Fields.ParcelDescription.Hint"] = "Parcel Description",
                ["Plugins.Widgets.DPDShipToShop.Fields.Liability"] = "Insurance",
                ["Plugins.Widgets.DPDShipToShop.Fields.Liability.Hint"] = "Insurance required?",
                ["Plugins.Widgets.DPDShipToShop.Fields.LiabilityValue"] = "Insurance Value",
                ["Plugins.Widgets.DPDShipToShop.Fields.LiabilityValue.Hint"] = "Insurance Value"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public async override Task UninstallAsync()
        {
            //database objects
            //_objectContext.Uninstall();

            //settings
            await _settingService.DeleteSettingAsync<DPDShipToShopSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.DPDShipToShop");

            await base.UninstallAsync();
        }

        /// <summary>
        /// Add to the menu
        /// </summary>
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!_permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings).Result)
                return;

            var myPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DPDShiptoShop");
            if (myPluginNode == null)
            {
                myPluginNode = new SiteMapNode()
                {
                    SystemName = "DPDShipToShop",
                    Title = "DPD Ship to Shop",
                    Visible = true,
                    IconClass = "fa-cog"
                };
                rootNode.ChildNodes.Add(myPluginNode);
            }

            myPluginNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "DPDShiptoShop.Configure",
                Title = "Configure",
                ControllerName = "DPDShipToShop",
                ActionName = "Configure",
                Visible = true,
                IconClass = "fa-dot-circle-o",
                RouteValues = new RouteValueDictionary() { { "systemName", "Nop.Plugin.Widgets.DPDShiptoShop" } },
            });

            //myPluginNode.ChildNodes.Add(new SiteMapNode()
            //{
            //    SystemName = "MyPlugin.SomeFeatureList",
            //    Title = "Some Feature",
            //    ControllerName = "MyPlugin",
            //    ActionName = "SomeFeatureList",
            //    Visible = true,
            //    IconClass = "fa-dot-circle-o",
            //    RouteValues = new RouteValueDictionary() { { "area", "Admin" } },  //need to register this route since it has /Admin prefix
            //});
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;



        #endregion
    }
}
