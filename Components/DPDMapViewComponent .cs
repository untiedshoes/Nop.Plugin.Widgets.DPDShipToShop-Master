using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using Nop.Plugin.Widgets.DPDShipToShop.Model;
using Nop.Plugin.Widgets.DPDShipToShop.Services;
using Nop.Plugin.Widgets.DPDShipToShop.Response;
using Nop.Plugin.Widgets.DPDShipToShop.Utility;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;
using System.Net;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Customers;
using Newtonsoft.Json;
using Nop.Services.Directory;
using Nop.Services.Caching;

namespace Nop.Plugin.Widgets.DPDShipToShop.Components
{
    [ViewComponent(Name = DPDShipToShopDefaults.DPD_MAP_VIEW_COMPONENT_NAME)]
    public class DPDMapViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IDPDShipToShopService _DPDShipToShopService;
        private readonly DPDShipToShopSettings _DPDShipToShopSettings;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStaticCacheManager _cacheKeyService;
        private readonly LicenseService _licenseService;
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

        private const string DpdUserName = "DPD_USERNAME_REMOVED";
        private const string DpdPassword = "DPD_PASSWORD_REMOVED";
        private const string DpdAccountNumber = "DPD_ACCOUNT_REMOVED";

        public DPDMapViewComponent(IStoreContext storeContext,
            ISettingService settingService, IWebHelper webHelper,
            IWorkContext workContext,
            IDPDShipToShopService DPDShipToShopService,
            DPDShipToShopSettings DPDShipToShopSettings,
            LicenseService licenseService,
            ICustomerService customerService,
            ICountryService countryService,
            IStaticCacheManager staticCacheManager,
            IStaticCacheManager cacheKeyService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _webHelper = webHelper;
            _workContext = workContext;
            _customerService = customerService;
            _countryService = countryService;
            _DPDShipToShopService = DPDShipToShopService;
            _DPDShipToShopSettings = DPDShipToShopSettings;
            _staticCacheManager = staticCacheManager;
            _cacheKeyService = cacheKeyService;
            _licenseService = licenseService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {

            if (!_DPDShipToShopSettings.PluginEnabled)
            {
                return Content(string.Empty);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync().Result;
            //var dpdSettings = _settingService.LoadSetting<DPDShipToShopSettings>(storeScope);

            var _shippingAddress = await _customerService.GetCustomerBillingAddressAsync(_workContext.GetCurrentCustomerAsync().Result).Result;
            var _billingAddress = await _customerService.GetCustomerBillingAddressAsync(_workContext.GetCurrentCustomerAsync().Result).Result;
            string ShippingMethodName = _DPDShipToShopSettings.ShippingMethodName;
            string GoogleMapsApiKey = _DPDShipToShopSettings.GoogleMapsApiKey;
            string ErrorMessage = null;

            //Check for VerifyLicense
            //bool IsRegisted = await _staticCacheManager.GetAsync(_cacheKeyService.PrepareKeyForDefaultCache(DPDShipToShopDefaults.LicenseCacheKey), () =>
            //{
            //    return await _licenseService.VerifyLicense(_DPDShipToShopSettings.SerialNumber);
            //}).Result;

            //IsRegisted = true;

            //if(!IsRegisted)
            //{
            //    ShippingMethodName = null;
            //    GoogleMapsApiKey = null;
            //    ErrorMessage = "DPD Ship to Shop Plugin is not authorised, please enter a valid Serial Number.";
            //}

            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(DPDShipToShopDefaults.LicenseCacheKey);

            var isRegistered = await _staticCacheManager.GetAsync(
                cacheKey,
                () => _licenseService.VerifyLicense(_DPDShipToShopSettings.SerialNumber)
            );

            if (!isRegistered)
            {
                ShippingMethodName = null;
                GoogleMapsApiKey = null;
                ErrorMessage = "DPD Ship to Shop Plugin is not authorised, please enter a valid Serial Number.";
            }

            var model = new PublicInfoModel
            {

                ShippingMethodName = ShippingMethodName,
                GoogleMapsApiKey = GoogleMapsApiKey,
                UseGoogleAutoComplete = _DPDShipToShopSettings.UseGoogleAutoComplete,
                //get postal code from the billing address or from the shipping one
                PostalCode = _billingAddress?.ZipPostalCode
                    ?? _shippingAddress?.ZipPostalCode,
                CountryCode = _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0).Result?.TwoLetterIsoCode
                    ?? _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0).Result?.TwoLetterIsoCode,
                //ResponseData = JsonConvert.SerializeObject(GetPickupPoints(_DPDShipToShopSettings.UserName, _DPDShipToShopSettings.Password, _DPDShipToShopSettings.AccountNumber), SerializerSettings),
                ErrorMessage = ErrorMessage

            };

            //ModelState.AddModelError("ErrorMessage", "You are using a trial version of the DPD Ship to Shop Plugin");
            return View("~/Plugins/Widgets.DPDShipToShop/Views/DPDMap.cshtml", model);

        }


        //Get Pickup Points
        public async Task<string> GetPickupPoints(string DpdUserName, string DpdPassword, string dpdAccountNumber)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var shippingAddress = await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync());
            var billingAddress = await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync());
            var postalCode = billingAddress?.ZipPostalCode
                    ?? shippingAddress?.ZipPostalCode;
            
            var DpdClient = new DpdShipToShopClient(DpdUserName, DpdPassword, dpdAccountNumber, "");


            //Call Login as we need the login session token here on the header to continue
            LoginResponse loginResponse = await DpdClient.LoginAsync();
            //ShipToShopResponse ShipToShopResponse = new ShipToShopResponse();

            var response = string.Empty;

            if (!string.IsNullOrEmpty(loginResponse.data.geoSession))
            {
                string dpdUrl = string.Format("{0}organisation/pickuplocation/?filter=nearAddress&countryCode=GB&searchPageSize=10&searchPage=1&searchCriteria=&maxDistance=10&searchAddress={1}", _DPDShipToShopSettings.DPDBaseURL, postalCode.Replace(" ", ""));

                await GetURI(new Uri(dpdUrl), DpdUserName, DpdPassword, dpdAccountNumber, loginResponse.data.geoSession);

                return response;
            };

            return response;

        }

        //Get The URI
        public async Task<string> GetURI(Uri u, String dpdUserName, String dpdPassword, string dpdAccountNumber, string dpdSessionId)
        {

            var response = string.Empty;
            var auth = String.Format("{0}:{1}", dpdUserName, dpdPassword);
            auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                client.DefaultRequestHeaders.Add("GEOClient", "account/" + dpdAccountNumber);
                client.DefaultRequestHeaders.Add("GEOSession", dpdSessionId);

                HttpResponseMessage result = await client.GetAsync(u);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
                response = await result.Content.ReadAsStringAsync();

            }

            return response;
        }

        /// <summary>
        /// Creates a <see cref="JsonResult"/> object that serializes the specified <paramref name="data"/> object
        /// to JSON.
        /// </summary>
        /// <param name="data">The object to serialize.</param>
        /// <returns>The created <see cref="JsonResult"/> that serializes the specified <paramref name="data"/>
        /// to JSON format for the response.</returns>
        [NonAction]
        public virtual JsonResult Json(object data)
        {
            return new JsonResult(data);
        }

        /// <summary>
        /// Creates a <see cref="JsonResult"/> object that serializes the specified <paramref name="data"/> object
        /// to JSON.
        /// </summary>
        /// <param name="data">The object to serialize.</param>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings"/> to be used by
        /// the formatter.</param>
        /// <returns>The created <see cref="JsonResult"/> that serializes the specified <paramref name="data"/>
        /// as JSON format for the response.</returns>
        /// <remarks>Callers should cache an instance of <see cref="JsonSerializerSettings"/> to avoid
        /// recreating cached data with each call.</remarks>
        [NonAction]
        public virtual JsonResult Json(object data, JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings == null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            return new JsonResult(data, serializerSettings);
        }

    }
}
