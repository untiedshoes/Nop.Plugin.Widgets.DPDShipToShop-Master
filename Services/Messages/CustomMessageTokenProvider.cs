using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using Nop.Services.Blogs;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Forums;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services.Messages
{

    /// <summary>
    /// Represents overridden messae token provider
    /// </summary>
    public class CustomMessageTokenProvider : MessageTokenProvider
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressService _addressService;
        private readonly IBlogService _blogService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsService _newsService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShipmentService _shipmentService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeFormatter _vendorAttributeFormatter;
        private readonly IWorkContext _workContext;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IDPDShipToShopService _dPDShipToShopService;

        #endregion

        #region Ctor

        public CustomMessageTokenProvider(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressService addressService,
            IBlogService blogService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerAttributeFormatter customerAttributeFormatter,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            INewsService newsService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IVendorAttributeFormatter vendorAttributeFormatter,
            IWorkContext workContext,
            MessageTemplatesSettings templatesSettings,
            PaymentSettings paymentSettings,
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings,
            IDPDShipToShopService dPDShipToShopService) : base(catalogSettings,
                currencySettings,
                actionContextAccessor,
                addressAttributeFormatter,
                addressService,
                blogService,
                countryService,
                currencyService,
                customerAttributeFormatter,
                customerService,
                dateTimeHelper,
                eventPublisher,
                genericAttributeService,
                giftCardService,
                htmlFormatter,
                languageService,
                localizationService,
                newsService,
                orderService,
                paymentPluginManager,
                paymentService,
                priceFormatter,
                productService,
                rewardPointService,
                shipmentService,
                stateProvinceService,
                storeContext,
                storeService,
                urlHelperFactory,
                urlRecordService,
                vendorAttributeFormatter,
                workContext,
                templatesSettings,
                paymentSettings,
                storeInformationSettings,
                taxSettings
            )
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressService = addressService;
            _blogService = blogService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerAttributeFormatter = customerAttributeFormatter;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _languageService = languageService;
            _localizationService = localizationService;
            _newsService = newsService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _shipmentService = shipmentService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _vendorAttributeFormatter = vendorAttributeFormatter;
            _workContext = workContext;
            _templatesSettings = templatesSettings;
            _paymentSettings = paymentSettings;
            _storeInformationSettings = storeInformationSettings;
            _taxSettings = taxSettings;
            _dPDShipToShopService = dPDShipToShopService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Implement our own AddOrderTokens to the MessageTokenProvider class
        /// This allows to create own Tokens for the message templates
        /// We're going to add a token for replacing the Call for info field
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <param name="vendorId"></param>
        public async override Task AddOrderTokensAsync(IList<Token> tokens, Order order, int languageId, int vendorId = 0)
        {

            var customer = await _workContext.GetCurrentCustomerAsync();
            var shipToShopLocation = string.Empty;

            if (order.ShippingMethod == "DPD Ship to Shop")
            {
                //var location = _dPDShipToShopService.GetDPDPickupPointByCustomerId(order.CustomerId);

                var store = await _storeContext.GetCurrentStoreAsync();
                var storeId = store.Id;
                var location = await _genericAttributeService.GetAttributeAsync<string>(customer, CustomNopCustomerDefaults.DPDShopToShopLocationId, storeId);
                int.TryParse(location, out int dpdShipToShopLocationId);

                try
                {
                    var dpdShipToShopLocation = await _dPDShipToShopService.GetDPDPickupPointByIdAsync(dpdShipToShopLocationId);

                    string PickupLocationCode = string.Empty;
                    string Organisation = string.Empty;
                    string Property = string.Empty;
                    string Street = string.Empty;
                    string Locality = string.Empty;
                    string Town = string.Empty;
                    string County = string.Empty;
                    string Postcode = string.Empty;
                    string Country = string.Empty;
                    string Latitude = string.Empty;
                    string Longitude = string.Empty;
                    string LocationAddress = string.Empty;

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.PickupLocationCode))
                    {
                        PickupLocationCode = string.Format("Location Code: {0}<br />", dpdShipToShopLocation.PickupLocationCode);
                    }

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.Organisation))
                    {
                        Organisation = string.Format("{0}<br />", dpdShipToShopLocation.Organisation);
                    }

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.Property))
                    {
                        Property = string.Format("{0}<br />", dpdShipToShopLocation.Property);
                    }

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.Street))
                    {
                        Street = string.Format("{0}<br />", dpdShipToShopLocation.Street);
                    }

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.Locality))
                    {
                        Locality = string.Format("{0}<br />", dpdShipToShopLocation.Locality);
                    }

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.Town))
                    {
                        Town = string.Format("{0}<br />", dpdShipToShopLocation.Town);
                    }

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.County))
                    {
                        County = string.Format("{0}<br />", dpdShipToShopLocation.County);
                    }

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.Postcode))
                    {
                        Postcode = string.Format("{0}<br />", dpdShipToShopLocation.Postcode);
                    }

                    if (!String.IsNullOrEmpty(dpdShipToShopLocation.CountryCode))
                    {
                        var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(dpdShipToShopLocation.CountryCode);
                        Country = string.Format("{0}", country?.Name ?? string.Empty);
                    }

                    LocationAddress = string.Format("Address:<br/>{0}{1}{2}{3}{4}{5}{6}{7}", Organisation, Property, Street, Locality, Town, County, Postcode, Country);

                    shipToShopLocation = string.Format("{0}{1}", PickupLocationCode, LocationAddress);

                } catch (Exception ex)
                {
                    
                }


            };
            
            tokens.Add(new Token("Order.DPDShipToShopLocation", shipToShopLocation, true));


            await base.AddOrderTokensAsync(tokens, order, languageId, vendorId);
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>Collection of allowed message tokens</returns>
        public async override Task<IEnumerable<string>> GetListOfAllowedTokensAsync(IEnumerable<string> tokenGroups = null)
        {
            var additionalTokens = new AdditionalTokensAddedEvent();
            await _eventPublisher.PublishAsync(additionalTokens);

            var allowedTokens = AllowedTokens.Where(x => tokenGroups == null || tokenGroups.Contains(x.Key))
                .SelectMany(x => x.Value).ToList();

            allowedTokens.AddRange(additionalTokens.AdditionalTokens);

            // add our custom token
            allowedTokens.Add("%Order.DPDShipToShopLocation%");

            return allowedTokens.Distinct();
        }

        #endregion
    }
}
