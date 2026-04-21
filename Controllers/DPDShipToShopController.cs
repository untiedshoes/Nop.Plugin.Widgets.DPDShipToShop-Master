using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Plugin.Widgets.DPDShipToShop;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using Nop.Plugin.Widgets.DPDShipToShop.Services;
using Nop.Plugin.Widgets.DPDShipToShop.Utility;
using Nop.Plugin.Widgets.DPDShipToShop.Model;
using Nop.Plugin.Widgets.DPDShipToShop.Response;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Threading.Tasks;
using Nop.Core;
using System.Text;
using System.Net;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Plugin.Widgets.DPDShipToShop.Domain;
using Microsoft.AspNetCore.Http;
using Nop.Core.Http.Extensions;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Nop.Core.Caching;
using Nop.Services.Caching;

namespace Nop.Plugin.Widgets.DPDShipToShop.Controllers
{
    public class DPDShipToShopController : BasePluginController
    {

        #region Fields
        private readonly IHttpContextFactory _httpContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IDPDShipToShopService _dpdShipToShopService;
        private readonly DPDShipToShopSettings _DPDShipToShopSettings;
        private readonly LicenseService _licenseService;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStaticCacheManager _cacheKeyService;
        private readonly IDPDSessionService _dpdSessionService;
        #endregion

        #region Ctor

        public DPDShipToShopController(IHttpContextFactory httpContext, IStoreContext storeContext,
            ISettingService settingService, IWebHelper webHelper,
            IWorkContext workContext, ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IDPDShipToShopService dpdShipToShopService,
            DPDShipToShopSettings DPDShipToShopSettings,
            LicenseService licenseService,
            ICustomerService customerService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IGenericAttributeService genericAttributeService,
            IStaticCacheManager staticCacheManager,
            IStaticCacheManager cacheKeyService,
            IDPDSessionService dpdSessionService)
        {
            _httpContext = httpContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _webHelper = webHelper;
            _workContext = workContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _dpdShipToShopService = dpdShipToShopService;
            _DPDShipToShopSettings = DPDShipToShopSettings;
            _licenseService = licenseService;
            _customerService = customerService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _genericAttributeService = genericAttributeService;
            _staticCacheManager = staticCacheManager;
            _cacheKeyService = cacheKeyService;
            _dpdSessionService = dpdSessionService;

        }

        #endregion
        internal class PickupLocationOpenWindow
        {
            public string pickupLocationOpenWindowStartTime { get; set; }
            public string pickupLocationOpenWindowEndTime { get; set; }
            public int pickupLocationOpenWindowDay { get; set; }
        }

        internal class TimeList
        {
            public string time { get; set; }
        }


        #region Methods

        /// <summary>
        /// Displays the plugin configuration page in the admin area.
        /// </summary>
        /// <returns>The configuration view for the DPD Ship to Shop plugin.</returns>
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            bool IsRegisted = await _staticCacheManager.GetAsync(_cacheKeyService.PrepareKeyForDefaultCache(DPDShipToShopDefaults.LicenseCacheKey), async () =>
            {
                return await _licenseService.VerifyLicense(_DPDShipToShopSettings.SerialNumber);
            });

            IsRegisted = true;

            var model = new ConfigurationModel
            {
                PluginEnabled = _DPDShipToShopSettings.PluginEnabled,
                UserName = _DPDShipToShopSettings.UserName,
                Password = _DPDShipToShopSettings.Password,
                AccountNumber = _DPDShipToShopSettings.AccountNumber,
                DPDBaseURL = _DPDShipToShopSettings.DPDBaseURL,
                ShippingMethodName = _DPDShipToShopSettings.ShippingMethodName,
                GoogleMapsApiKey = _DPDShipToShopSettings.GoogleMapsApiKey,
                UseGoogleAutoComplete = _DPDShipToShopSettings.UseGoogleAutoComplete,
                StoreUrl = HttpContext.Request.Host.Host.ToLower(),
                SerialNumber = _DPDShipToShopSettings.SerialNumber,
                IsRegisted = IsRegisted
            };
            if (!IsRegisted)
            {
                _notificationService.ErrorNotification("Unregistered version.");
            }

            //if running on local host, validate the licenseKey
            var webHostEnvironment = EngineContext.Current.Resolve<IWebHostEnvironment>();
            var isLocal = webHostEnvironment.IsDevelopment();
            if (isLocal && IsRegisted)
            {
                _notificationService.SuccessNotification("You are using a registered version for 'localhost' only, although this version has one limitation; it will only work 30 days from the day the License was issued.");
            }

            return View("~/Plugins/Widgets.DPDShipToShop/Views/Configure.cshtml", model);
        }

        /// <summary>
        /// Saves the plugin configuration values posted from the admin area.
        /// </summary>
        /// <param name="model">The configuration model containing the updated settings.</param>
        /// <returns>The refreshed configuration view.</returns>
        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //whether user has the authority to manage configuration
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            _DPDShipToShopSettings.PluginEnabled = model.PluginEnabled;
            _DPDShipToShopSettings.UserName = model.UserName;
            _DPDShipToShopSettings.Password = model.Password;
            _DPDShipToShopSettings.AccountNumber = model.AccountNumber;
            _DPDShipToShopSettings.ShippingMethodName = model.ShippingMethodName;
            _DPDShipToShopSettings.DPDBaseURL = model.DPDBaseURL;
            _DPDShipToShopSettings.GoogleMapsApiKey = model.GoogleMapsApiKey;
            _DPDShipToShopSettings.UseGoogleAutoComplete = model.UseGoogleAutoComplete;
            _DPDShipToShopSettings.StoreUrl = HttpContext.Request.Host.Host.ToLower();
            _DPDShipToShopSettings.SerialNumber = model.SerialNumber;

            await _staticCacheManager.RemoveByPrefixAsync(DPDShipToShopDefaults.LicensePrefixCacheKey);

            await _settingService.SaveSettingAsync(_DPDShipToShopSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        /// <summary>
        /// Returns pickup-point search results as JSON for the supplied postcode and country.
        /// </summary>
        /// <param name="postalCode">The postcode used to search nearby pickup locations.</param>
        /// <param name="countryCode">The two-letter country code for the search.</param>
        /// <returns>A JSON result containing the pickup-point data or an error response.</returns>
        [HttpPost]
        public async Task<ActionResult> PickupPointsMapJson(string postalCode, string countryCode)
        {

            if (string.IsNullOrEmpty(postalCode) || string.IsNullOrEmpty(countryCode))
            {
                return Json(new
                {
                    success = false
                });
            }

            //bool IsRegisted = await _staticCacheManager.GetAsync(_cacheKeyService.PrepareKeyForDefaultCache(DPDShipToShopDefaults.LicenseCacheKey), () =>
            //{
            //    return await _licenseService.VerifyLicense(_DPDShipToShopSettings.SerialNumber);
            //});

            //IsRegisted = true;

            //if (!IsRegisted)
            //{
            //    return Json(new
            //    {
            //        success = false
            //    });
            //}

            //Updated the way we check if the plugin is regsitered.
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(DPDShipToShopDefaults.LicenseCacheKey);

            var isRegistered = await _staticCacheManager.GetAsync(
                cacheKey,
                () => _licenseService.VerifyLicense(_DPDShipToShopSettings.SerialNumber)
            );

            if (!isRegistered)
            {
                return Json(new
                {
                    success = false
                });
            }

            //var logger = EngineContext.Current.Resolve<ILogger>();
            //var dpdSettings = _settingService.LoadSetting<DPDShipToShopSettings>(_storeContext.CurrentStore.Id);

            var response = string.Empty;
            var responseData = string.Empty;
            string jsonResponse = string.Empty;
            string PostalCode = postalCode;
            string CountryCode = countryCode;
            string apiName = "DPD";

            var model = new PickupPointsMapModel();

            model.GoogleMapsApiKey = _DPDShipToShopSettings.GoogleMapsApiKey;

            var _shippingAddress = await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync());
            var _billingAddress = await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync());

            if (!string.IsNullOrEmpty(PostalCode))
            {
                model.PostalCode = PostalCode;
            }
            else
            {
                model.PostalCode = _billingAddress?.ZipPostalCode
                    ?? _shippingAddress?.ZipPostalCode;
            }

            if (!string.IsNullOrEmpty(CountryCode))
            {
                model.CountryCode = CountryCode;
            }
            else
            {
                model.CountryCode = (await _countryService.GetCountryByIdAsync(_billingAddress.CountryId ?? 0))?.TwoLetterIsoCode
                    ?? (await _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0))?.TwoLetterIsoCode;

            }

            // Call Login as we need the login geoSession (AccessToken) here to continue
            var AccessToken = await GetLoginToken(apiName);

            if (!string.IsNullOrEmpty(AccessToken))
            {
                var uri = String.Format("{0}organisation/pickuplocation/?filter=nearAddress&countryCode={1}&searchPageSize=50&searchPage=1&searchCriteria=&maxDistance=10&searchAddress={2}", _DPDShipToShopSettings.DPDBaseURL, model.CountryCode, model.PostalCode.Replace(" ", ""));
                //Convert the DPD Username and Password to Base64
                var auth = String.Format("{0}:{1}", _DPDShipToShopSettings.UserName, _DPDShipToShopSettings.Password);
                auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                    client.DefaultRequestHeaders.Add("GEOClient", "account/" + _DPDShipToShopSettings.AccountNumber);
                    client.DefaultRequestHeaders.Add("GEOSession", AccessToken);

                    HttpResponseMessage result = await client.GetAsync(uri);
                    if (result.IsSuccessStatusCode)
                    {
                        response = await result.Content.ReadAsStringAsync();
                    }

                    //response = await result.Content.ReadAsStringAsync();
                    string stringData = response;
                    var NewPickUpPointsModel = JsonConvert.DeserializeObject<DpdPickupPointsApiResponse>(stringData);

                    if (NewPickUpPointsModel.data == null)
                    {

                        List<Nop.Plugin.Widgets.DPDShipToShop.Models.DpdApiErrorModel> errors = new List<Nop.Plugin.Widgets.DPDShipToShop.Models.DpdApiErrorModel>();

                        foreach (var errorItem in NewPickUpPointsModel.error)
                        {
                            errors.Add(new Nop.Plugin.Widgets.DPDShipToShop.Models.DpdApiErrorModel
                            {
                                ErrorCode = errorItem.errorCode,
                                ErrorMessage = errorItem.errorMessage,
                                ObjectName = errorItem.errorObj,
                                ErrorType = errorItem.errorType,
                            });
                        }
                        ;

                        var jsonError = JsonConvert.SerializeObject(errors);
                        return Json(new
                        {
                            success = false,
                            error = jsonError
                        }, new JsonSerializerSettings());
                    }


                    List<Nop.Plugin.Widgets.DPDShipToShop.Models.DpdPickupPointResultModel> Results = new List<Nop.Plugin.Widgets.DPDShipToShop.Models.DpdPickupPointResultModel>();


                    foreach (var pickUpPoint in NewPickUpPointsModel.data.results)
                    {
                        Results.Add(new Nop.Plugin.Widgets.DPDShipToShop.Models.DpdPickupPointResultModel
                        {
                            PickupLocationCode = pickUpPoint.pickupLocation.pickupLocationCode,
                            PickupLocationOrganisation = pickUpPoint.pickupLocation.address.organisation,
                            PickupLocationProperty = pickUpPoint.pickupLocation.address.property,
                            PickupLocationStreet = pickUpPoint.pickupLocation.address.street,
                            PickupLocationLocality = pickUpPoint.pickupLocation.address.locality,
                            PickupLocationTown = pickUpPoint.pickupLocation.address.town,
                            PickupLocationCounty = pickUpPoint.pickupLocation.address.county,
                            PickupLocationPostcode = pickUpPoint.pickupLocation.address.postcode,
                            PickupLocationCountryCode = pickUpPoint.pickupLocation.address.countryCode,
                            PickupLocationDistance = pickUpPoint.distance,
                            PickupLocationDisabledAccess = pickUpPoint.pickupLocation.disabledAccess,
                            PickupLocationOpenLate = pickUpPoint.pickupLocation.openLate,
                            PickupLocationOpenSaturday = pickUpPoint.pickupLocation.openSaturday,
                            PickupLocationOpenSunday = pickUpPoint.pickupLocation.openSunday,
                            PickupLocationParkingAvailable = pickUpPoint.pickupLocation.parkingAvailable,
                            PickupLocationLatitude = pickUpPoint.pickupLocation.addressPoint.latitude,
                            PickupLocationLongitude = pickUpPoint.pickupLocation.addressPoint.longitude,
                            PickupLocationWeekdayTime = await StringUtility.CleanHours(FilterWeekdayOpeningTimes(stringData, 1, pickUpPoint.pickupLocation.pickupLocationCode), FilterWeekdayClosingTimes(stringData, 1, pickUpPoint.pickupLocation.pickupLocationCode)),
                            PickupLocationSaturdayTime = await StringUtility.CleanHours(FilterWeekendOpeningTimes(stringData, 6, pickUpPoint.pickupLocation.pickupLocationCode), FilterWeekendClosingTimes(stringData, 6, pickUpPoint.pickupLocation.pickupLocationCode)),
                            PickupLocationSundayTime = await StringUtility.CleanHours(FilterWeekendOpeningTimes(stringData, 7, pickUpPoint.pickupLocation.pickupLocationCode), FilterWeekendClosingTimes(stringData, 7, pickUpPoint.pickupLocation.pickupLocationCode))
                        });
                    }
                    ;


                    var json = JsonConvert.SerializeObject(Results);

                    return Json(new
                    {
                        success = true,
                        results = json
                    }, new JsonSerializerSettings());

                }

            }

            return Json(new
            {
                success = false
            });

            //return PartialView("~/Plugins/Widgets.DPD/Views/PickupPointsMap.cshtml", model);
        }

        /// <summary>
        /// Returns the pickup-point map markup for the supplied search details.
        /// </summary>
        /// <param name="postalCode">The postcode used to search nearby pickup locations.</param>
        /// <param name="countryCode">The two-letter country code for the search.</param>
        /// <returns>A JSON result containing the rendered map HTML.</returns>
        [HttpPost]
        public async Task<ActionResult> PickupPointsMap(string postalCode, string countryCode)
        {
            var logger = EngineContext.Current.Resolve<ILogger>();
            //var dpdSettings = _settingService.LoadSetting<DPDShipToShopSettings>(_storeContext.CurrentStore.Id);

            var response = string.Empty;
            var responseData = string.Empty;
            string jsonResponse = string.Empty;
            string PostalCode = postalCode;
            string CountryCode = countryCode;
            string apiName = "DPD";

            var model = new PickupPointsMapModel();

            model.GoogleMapsApiKey = _DPDShipToShopSettings.GoogleMapsApiKey;


            var _shippingAddress = await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync());
            var _billingAddress = await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync());

            if (!string.IsNullOrEmpty(PostalCode))
            {
                model.PostalCode = _billingAddress?.ZipPostalCode
                    ?? _shippingAddress?.ZipPostalCode;
            }
            else
            {
                model.PostalCode = PostalCode;
            }

            if (!string.IsNullOrEmpty(CountryCode))
            {
                model.CountryCode = (await _countryService.GetCountryByIdAsync(_billingAddress.CountryId ?? 0))?.TwoLetterIsoCode
                    ?? (await _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0))?.TwoLetterIsoCode;
            }
            else
            {
                model.CountryCode = CountryCode;
            }

            // Call Login as we need the login geoSession (AccessToken) here to continue
            var AccessToken = await GetLoginToken(apiName);

            //logger.Information("PickupPointsMap - AccessToken: " + AccessToken);

            //var DpdClient = new DpdShippingClient(dpdSettings.UserName, dpdSettings.Password, dpdSettings.AccountNumber, "");
            //Call Login as we need the login session token here on the header to continue
            //LoginResponse loginResponse = DpdClient.Login();

            if (!string.IsNullOrEmpty(AccessToken))
            {
                //Get the post code from teh HTTP post
                //Conver the DPD Username and Password to Base64
                var uri = String.Format("https://api.dpdgroup.co.uk/organisation/pickuplocation/?filter=nearAddress&countryCode={0}&searchPageSize=50&searchPage=1&searchCriteria=&maxDistance=10&searchAddress={1}", model.CountryCode, model.PostalCode);
                var auth = String.Format("{0}:{1}", _DPDShipToShopSettings.UserName, _DPDShipToShopSettings.Password);
                auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                    client.DefaultRequestHeaders.Add("GEOClient", "account/" + _DPDShipToShopSettings.AccountNumber);
                    client.DefaultRequestHeaders.Add("GEOSession", AccessToken);

                    HttpResponseMessage result = await client.GetAsync(uri);
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        response = await result.Content.ReadAsStringAsync();
                    }
                    response = await result.Content.ReadAsStringAsync();
                    string stringData = response;

                    //HttpResponseMessage response = await client.GetAsync(uri).Result;
                    //string stringData = await response.Content.ReadAsStringAsync().Result;

                    var NewPickUpPointsModel = JsonConvert.DeserializeObject<DpdPickupPointsApiResponse>(stringData);


                    if (!string.IsNullOrEmpty(PostalCode))
                    {
                        NewPickUpPointsModel.PostalCode = _billingAddress?.ZipPostalCode
                            ?? _shippingAddress?.ZipPostalCode;
                    }
                    else
                    {
                        NewPickUpPointsModel.PostalCode = PostalCode;
                    }

                    if (!string.IsNullOrEmpty(CountryCode))
                    {
                        NewPickUpPointsModel.CountryCode = (await _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0))?.TwoLetterIsoCode
                            ?? (await _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0))?.TwoLetterIsoCode;
                    }
                    else
                    {
                        NewPickUpPointsModel.CountryCode = CountryCode;
                    }

                    NewPickUpPointsModel.ResponseData = JsonConvert.SerializeObject(stringData);
                    NewPickUpPointsModel.GoogleMapsApiKey = _DPDShipToShopSettings.GoogleMapsApiKey;

                    return Json(new
                    {
                        success = true,
                        html = await this.RenderPartialViewToStringAsync("~/Plugins/Widgets.DPDShipToShop/Views/PickupPointsMap.cshtml", NewPickUpPointsModel),
                    });

                }

            }

            return Json(new
            {
                success = true,
                html = await this.RenderPartialViewToStringAsync("~/Plugins/Widgets.DPDShipToShop/Views/PickupPointsMap.cshtml", model),
            });

            //return PartialView("~/Plugins/Widgets.DPD/Views/PickupPointsMap.cshtml", model);
        }

        /// <summary>
        /// Saves the pickup location selected by the current customer.
        /// </summary>
        /// <param name="shiptoshoplocation">The pickup location selected in the checkout UI.</param>
        /// <returns>A JSON result indicating whether the selection was saved successfully.</returns>
        [HttpPost]
        public async Task<IActionResult> SelectPickupLocation([FromBody] DPDShipToShopLocations shiptoshoplocation)
        {

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    result = "Sorry, we seem to have an error..",
                });

            }

            var store = await _storeContext.GetCurrentStoreAsync();
            var storeId = store.Id;

            var customer = await _workContext.GetCurrentCustomerAsync();

            DPDShipToShopLocations dpdPickupPointModel = new DPDShipToShopLocations
            {
                PickupLocationCode = shiptoshoplocation.PickupLocationCode,
                Organisation = shiptoshoplocation.Organisation,
                Property = shiptoshoplocation.Property,
                Street = shiptoshoplocation.Street,
                Locality = shiptoshoplocation.Locality,
                Town = shiptoshoplocation.Town,
                County = shiptoshoplocation.County,
                Postcode = shiptoshoplocation.Postcode,
                CountryCode = shiptoshoplocation.CountryCode,
                CustomerId = customer.Id,
                Latitude = shiptoshoplocation.Latitude,
                Longitude = shiptoshoplocation.Longitude,
                CreatedOnUtc = DateTime.UtcNow
            };

            List<Nop.Plugin.Widgets.DPDShipToShop.Models.DpdPickupPointResultModel> getAllDPDPickupPointsResult = new List<Nop.Plugin.Widgets.DPDShipToShop.Models.DpdPickupPointResultModel>();

            //Check if we have the pickupPoint in the DB
            foreach (var dpdpoint in await _dpdShipToShopService.GetAllDPDPickupPointsByLocationCodeCustomerIDAsync(dpdPickupPointModel.CustomerId))
            {
                getAllDPDPickupPointsResult.Add(new Nop.Plugin.Widgets.DPDShipToShop.Models.DpdPickupPointResultModel
                {
                    PickupLocationId = dpdpoint.Id,
                    PickupLocationCode = dpdpoint.PickupLocationCode,
                    PickupLocationOrganisation = dpdpoint.Organisation,
                    PickupLocationProperty = dpdpoint.Property,
                    PickupLocationStreet = dpdpoint.Street,
                    PickupLocationLocality = dpdpoint.Locality,
                    PickupLocationTown = dpdpoint.Town,
                    PickupLocationCounty = dpdpoint.County,
                    PickupLocationPostcode = dpdpoint.Postcode,
                    PickupLocationCountryCode = dpdpoint.CountryCode,
                    PickupLocationLatitude = dpdpoint.Latitude,
                    PickupLocationLongitude = dpdpoint.Longitude,
                    CreatedOnUtc = dpdpoint.CreatedOnUtc
                });
            }
            ;

            if (!getAllDPDPickupPointsResult.Any())
            {
                //Add the new DPD Pickuppoint
                await _dpdShipToShopService.InsertDPDPickupPoint(dpdPickupPointModel);
                await _genericAttributeService.SaveAttributeAsync(customer, CustomNopCustomerDefaults.DPDShopToShopLocationId, dpdPickupPointModel.Id, storeId);
                //SaveDPDShipToShopLocationAttribute(dpdPickupPointModel);

                return Json(new
                {
                    success = true,
                    result = "No Results! Added a new DPD Pickuppoint with ID: " + dpdPickupPointModel.Id + " and Location Code:" + dpdPickupPointModel.PickupLocationCode,
                });

            }
            else
            {
                //If we have results (Check if we have the pickupPoint in the DB), remove those DPD Pickuppoints
                foreach (var dpdpointtoremove in getAllDPDPickupPointsResult)
                {
                    await DeleteDPDPickupPointAsync(dpdpointtoremove.PickupLocationId);
                }

                //Add a new DPD Pickuppoint
                await _dpdShipToShopService.InsertDPDPickupPoint(dpdPickupPointModel);
                await _genericAttributeService.SaveAttributeAsync(customer, CustomNopCustomerDefaults.DPDShopToShopLocationId, dpdPickupPointModel.Id, storeId);

                return Json(new
                {
                    success = true,
                    result = "We've got results! Removed the other entries, then added a new DPD Pickuppoint with ID: " + dpdPickupPointModel.Id + " and Location Code:" + dpdPickupPointModel.PickupLocationCode,
                });
            }

        }

        #endregion

        #region Helpers

        /// <summary>
        /// Extracts the earliest weekday opening time for the specified pickup location.
        /// </summary>
        /// <param name="stringData">The raw DPD API JSON payload.</param>
        /// <param name="day">The day value used when filtering opening windows.</param>
        /// <param name="pickupLocationCode">The pickup location code.</param>
        /// <returns>The earliest weekday opening time, or null when unavailable.</returns>
        private string FilterWeekdayOpeningTimes(string stringData, int day, string pickupLocationCode)
        {
            var jsonString = JsonConvert.DeserializeObject<DpdPickupPointsApiResponse>(stringData);
            var results = jsonString.data.results.Where(x => x.pickupLocation.pickupLocationCode == pickupLocationCode).ToList();

            List<TimeList> Time = new List<TimeList>();
            foreach (var result in results)
            {
                foreach (var item in result.pickupLocation.pickupLocationAvailability.pickupLocationOpenWindow.Where(x => x.pickupLocationOpenWindowDay == 1 || x.pickupLocationOpenWindowDay == 2 || x.pickupLocationOpenWindowDay == 3 || x.pickupLocationOpenWindowDay == 4 || x.pickupLocationOpenWindowDay == 5))
                {
                    Time.Add(new TimeList { time = item.pickupLocationOpenWindowStartTime });
                    //return "" + item.pickupLocationOpenWindowStartTime + " - " + item.pickupLocationOpenWindowEndTime;
                }
            }

            if (Time != null && Time.Count == 0)
            {
                return null;
            }
            else
            {
                var returnTime = Time.OrderBy(t => t.time).FirstOrDefault();
                return returnTime.time.ToString();
            }

        }

        /// <summary>
        /// Extracts the latest weekday closing time for the specified pickup location.
        /// </summary>
        /// <param name="stringData">The raw DPD API JSON payload.</param>
        /// <param name="day">The day value used when filtering opening windows.</param>
        /// <param name="pickupLocationCode">The pickup location code.</param>
        /// <returns>The latest weekday closing time, or null when unavailable.</returns>
        private string FilterWeekdayClosingTimes(string stringData, int day, string pickupLocationCode)
        {
            var jsonString = JsonConvert.DeserializeObject<DpdPickupPointsApiResponse>(stringData);
            var results = jsonString.data.results.Where(x => x.pickupLocation.pickupLocationCode == pickupLocationCode).ToList();

            List<TimeList> Time = new List<TimeList>();
            foreach (var result in results)
            {
                foreach (var item in result.pickupLocation.pickupLocationAvailability.pickupLocationOpenWindow.Where(x => x.pickupLocationOpenWindowDay == 1 || x.pickupLocationOpenWindowDay == 2 || x.pickupLocationOpenWindowDay == 3 || x.pickupLocationOpenWindowDay == 4 || x.pickupLocationOpenWindowDay == 5))
                {
                    Time.Add(new TimeList { time = item.pickupLocationOpenWindowEndTime });
                }
            }

            if (Time != null && Time.Count == 0)
            {
                return null;
            }
            else
            {
                var returnTime = Time.OrderByDescending(t => t.time).FirstOrDefault();
                return returnTime.time.ToString();
            }

        }

        /// <summary>
        /// Extracts the weekend opening time for the specified pickup location and day.
        /// </summary>
        /// <param name="stringData">The raw DPD API JSON payload.</param>
        /// <param name="day">The weekend day to filter for.</param>
        /// <param name="pickupLocationCode">The pickup location code.</param>
        /// <returns>The opening time for the requested weekend day, or null when unavailable.</returns>
        private string FilterWeekendOpeningTimes(string stringData, int day, string pickupLocationCode)
        {
            var jsonString = JsonConvert.DeserializeObject<DpdPickupPointsApiResponse>(stringData);
            var results = jsonString.data.results.Where(x => x.pickupLocation.pickupLocationCode == pickupLocationCode).ToList();

            List<TimeList> Time = new List<TimeList>();
            foreach (var result in results)
            {
                foreach (var item in result.pickupLocation.pickupLocationAvailability.pickupLocationOpenWindow.Where(x => x.pickupLocationOpenWindowDay == day))
                {
                    Time.Add(new TimeList { time = item.pickupLocationOpenWindowStartTime });
                }
            }

            if (Time != null && Time.Count == 0)
            {
                return null;
            }
            else
            {
                var returnTime = Time.OrderBy(t => t.time).FirstOrDefault();
                return returnTime.time.ToString();
            }

        }

        /// <summary>
        /// Extracts the weekend closing time for the specified pickup location and day.
        /// </summary>
        /// <param name="stringData">The raw DPD API JSON payload.</param>
        /// <param name="day">The weekend day to filter for.</param>
        /// <param name="pickupLocationCode">The pickup location code.</param>
        /// <returns>The closing time for the requested weekend day, or null when unavailable.</returns>
        private string FilterWeekendClosingTimes(string stringData, int day, string pickupLocationCode)
        {
            var jsonString = JsonConvert.DeserializeObject<DpdPickupPointsApiResponse>(stringData);
            var results = jsonString.data.results.Where(x => x.pickupLocation.pickupLocationCode == pickupLocationCode).ToList();

            List<TimeList> Time = new List<TimeList>();
            foreach (var result in results)
            {
                foreach (var item in result.pickupLocation.pickupLocationAvailability.pickupLocationOpenWindow.Where(x => x.pickupLocationOpenWindowDay == day))
                {
                    Time.Add(new TimeList { time = item.pickupLocationOpenWindowEndTime });
                }
            }

            if (Time != null && Time.Count == 0)
            {
                return null;
            }
            else
            {
                var returnTime = Time.OrderByDescending(i => i.time).FirstOrDefault();
                return returnTime.time.ToString();
            }

        }

        /// <summary>
        /// Gets a cached DPD login token or requests a new one when needed.
        /// </summary>
        /// <param name="apiName">The cache key name for the API token.</param>
        /// <returns>A DPD session token.</returns>
        private async Task<string> GetLoginToken(string apiName)
        {
            return await _dpdSessionService.GetGeoSessionAsync(apiName);
        }

        /// <summary>
        /// Deletes a stored DPD pickup point by its identifier.
        /// </summary>
        /// <param name="pickupPointId">The pickup point identifier.</param>
        protected virtual async Task DeleteDPDPickupPointAsync(int pickupPointId)
        {
            var pickupPoint = await _dpdShipToShopService.GetDPDPickupPointByIdAsync(pickupPointId);

            if (pickupPoint != null)
            {
                await _dpdShipToShopService.DeleteDPDPickupPoint(pickupPoint);
            }
        }

        /// <summary>
        /// Saves the selected ship-to-shop location as a customer attribute.
        /// </summary>
        /// <param name="dpdshiptoshoplocation">The selected pickup location record.</param>
        protected async virtual Task SaveDPDShipToShopLocationAttribute(DPDShipToShopLocations dpdshiptoshoplocation)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var storeId = store.Id;
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _genericAttributeService.SaveAttributeAsync(customer, CustomNopCustomerDefaults.DPDShopToShopLocationId, dpdshiptoshoplocation, storeId);
        }

        #endregion
    }
}
