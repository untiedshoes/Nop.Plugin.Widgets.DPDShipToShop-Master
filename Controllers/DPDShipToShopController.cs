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
using Microsoft.Extensions.Caching.Distributed;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Plugin.Widgets.DPDShipToShop.Request.Base;
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
            IDistributedCache cache,
            ICustomerService customerService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IGenericAttributeService genericAttributeService,
            IStaticCacheManager staticCacheManager,
            IStaticCacheManager cacheKeyService)
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
            _cache = cache;
            _customerService = customerService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _genericAttributeService = genericAttributeService;
            _staticCacheManager = staticCacheManager;
            _cacheKeyService = cacheKeyService;

        }

        #endregion

        private static readonly Object _lock = new Object();
        private IDistributedCache _cache;

        private const int cacheExpirationInDays = 1;

        private class AccessTokenItem
        {
            public string AccessToken { get; set; } = string.Empty;
            public DateTime ExpiresIn { get; set; }
        }

        public class Result
        {
            public int PickupLocationId { get; set; }
            public string PickupLocationCode { get; set; }
            public string PickupLocationOrganisation { get; set; }
            public string PickupLocationProperty { get; set; }
            public string PickupLocationStreet { get; set; }
            public string PickupLocationLocality { get; set; }
            public string PickupLocationTown { get; set; }
            public string PickupLocationCounty { get; set; }
            public string PickupLocationPostcode { get; set; }
            public string PickupLocationCountryCode { get; set; }
            public float PickupLocationDistance { get; set; }
            public bool PickupLocationDisabledAccess { get; set; }
            public bool PickupLocationOpenLate { get; set; }
            public bool PickupLocationOpenSaturday { get; set; }
            public bool PickupLocationOpenSunday { get; set; }
            public bool PickupLocationParkingAvailable { get; set; }
            public decimal? PickupLocationLatitude { get; set; }
            public decimal? PickupLocationLongitude { get; set; }
            public string PickupLocationWeekdayTime { get; set; }
            public string PickupLocationSaturdayTime { get; set; }
            public string PickupLocationSundayTime { get; set; }
            public int CustomerId { get; set; }
            public virtual DateTime CreatedOnUtc { get; set; }
        }

        public class Error
        {
            public string errorCode { get; set; }
            public string errorMessage { get; set; }
            public string obj { get; set; }
            public string errorType { get; set; }
        }

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

        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {

            if (!_permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings).Result)
                return AccessDeniedView();

            bool IsRegisted = await _staticCacheManager.GetAsync(_cacheKeyService.PrepareKeyForDefaultCache(DPDShipToShopDefaults.LicenseCacheKey), () =>
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

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //whether user has the authority to manage configuration
            if (!_permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings).Result)
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

            if (!IsRegisted)
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
                model.CountryCode = _countryService.GetCountryByIdAsync(_billingAddress.CountryId ?? 0).Result?.TwoLetterIsoCode
                    ?? _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0).Result?.TwoLetterIsoCode;

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
                    var NewPickUpPointsModel = JsonConvert.DeserializeObject<Rootobject>(stringData);

                    if (NewPickUpPointsModel.data == null)
                    {

                        List<Error> errors = new List<Error>();

                        foreach (var errorItem in NewPickUpPointsModel.error)
                        {
                            errors.Add(new Error
                            {
                                errorCode = errorItem.errorCode,
                                errorMessage = errorItem.errorMessage,
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


                    List<Result> Results = new List<Result>();


                    foreach (var pickUpPoint in NewPickUpPointsModel.data.results)
                    {
                        Results.Add(new Result
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
                model.CountryCode = _countryService.GetCountryByIdAsync(_billingAddress.CountryId ?? 0).Result?.TwoLetterIsoCode
                    ?? _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0).Result?.TwoLetterIsoCode;
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

                    var NewPickUpPointsModel = JsonConvert.DeserializeObject<Rootobject>(stringData);


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
                        NewPickUpPointsModel.CountryCode = _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0).Result?.TwoLetterIsoCode
                            ?? _countryService.GetCountryByIdAsync(_shippingAddress.CountryId ?? 0).Result?.TwoLetterIsoCode;
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

            List<Result> getAllDPDPickupPointsResult = new List<Result>();

            //Check if we have the pickupPoint in the DB
            foreach (var dpdpoint in await _dpdShipToShopService.GetAllDPDPickupPointsByLocationCodeCustomerIDAsync(dpdPickupPointModel.CustomerId))
            {
                getAllDPDPickupPointsResult.Add(new Result
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

        private string FilterWeekdayOpeningTimes(string stringData, int day, string pickupLocationCode)
        {
            var jsonString = JsonConvert.DeserializeObject<Rootobject>(stringData);
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

        private string FilterWeekdayClosingTimes(string stringData, int day, string pickupLocationCode)
        {
            var jsonString = JsonConvert.DeserializeObject<Rootobject>(stringData);
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

        private string FilterWeekendOpeningTimes(string stringData, int day, string pickupLocationCode)
        {
            var jsonString = JsonConvert.DeserializeObject<Rootobject>(stringData);
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

        private string FilterWeekendClosingTimes(string stringData, int day, string pickupLocationCode)
        {
            var jsonString = JsonConvert.DeserializeObject<Rootobject>(stringData);
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

        private async Task<string> GetLoginToken(string apiName)
        {
            //var logger = EngineContext.Current.Resolve<ILogger>();
            var accessToken = await GetAccessTokenFromCacheAsync(apiName);

            if (accessToken != null)
            {
                if (accessToken.ExpiresIn > DateTime.UtcNow)
                {

                    return accessToken.AccessToken;
                }
                else
                {
                    // remove  => NOT Needed for this cache type
                }
            }

            // add
            var newAccessToken = await GetLoginAsyncToken(apiName);
            await AddAccessTokenToCacheAsync(apiName, newAccessToken);

            return newAccessToken.AccessToken;
        }

        private async Task<AccessTokenItem> GetLoginAsyncToken(string apiName)
        {
            try
            {
                var dpdSettings = await _settingService.LoadSettingAsync<DPDShipToShopSettings>(_storeContext.GetCurrentStoreAsync().Result.Id);
                //var logger = EngineContext.Current.Resolve<ILogger>();
                var DpdClient = new DpdShipToShopClient(dpdSettings.UserName, dpdSettings.Password, dpdSettings.AccountNumber, "");
                //Call Login as we need the login session token here on the header to continue
                LoginResponse loginResponse = await DpdClient.LoginAsync();

                //logger.Information("getLogin - AccessToken: " + loginResponse.data.geoSession);
                return new AccessTokenItem
                {
                    ExpiresIn = DateTime.UtcNow.AddSeconds(60 * 60 * 24),
                    AccessToken = loginResponse.data.geoSession
                };


            }
            catch (Exception e)
            {
                throw new ApplicationException($"Exception {e}");
            }
        }

        private async Task<AccessTokenItem> GetAccessTokenFromCacheAsync(string key)
        {
            var item = await _cache.GetStringAsync(key);
            if (item != null)
            {
                var result = await Task.Run(() => JsonConvert.DeserializeObject<AccessTokenItem>(item));
                return result;
            }

            return null;
        }

        private async Task AddAccessTokenToCacheAsync(string key, AccessTokenItem accessTokenItem)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(cacheExpirationInDays));

            await _cache.SetStringAsync(key, JsonConvert.SerializeObject(accessTokenItem), options);

        }

        protected virtual async Task DeleteDPDPickupPointAsync(int pickupPointId)
        {
            var pickupPoint = await _dpdShipToShopService.GetDPDPickupPointByIdAsync(pickupPointId);

            if (pickupPoint != null)
            {
                await _dpdShipToShopService.DeleteDPDPickupPoint(pickupPoint);
            }
        }

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
