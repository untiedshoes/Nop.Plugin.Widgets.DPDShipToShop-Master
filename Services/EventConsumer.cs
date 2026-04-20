using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Directory;
using System;
using System.Collections.Generic;
using System.Text;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using Nop.Plugin.Widgets.DPDShipToShop.Services;
using Nop.Plugin.Widgets.DPDShipToShop.Domain;
using Nop.Plugin.Widgets.DPDShipToShop.Model;
using Nop.Services.Localization;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services
{
    public partial class EventConsumer : IConsumer<OrderPlacedEvent>
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IDPDShipToShopService _dPDShipToShopService;
        private readonly DPDShipToShopAutomationManager _dPDShipToShopAutomationManager;
        private readonly ICountryService _countryService;


        #endregion

        #region Ctor

        public EventConsumer(IOrderService orderService,
            IStoreContext storeContext,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ILogger logger,
            IDPDShipToShopService dPDShipToShopService,
            DPDShipToShopAutomationManager dPDShipToShopAutomationManager,
            ICountryService countryService)
        {

            this._orderService = orderService;
            this._storeContext = storeContext;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._logger = logger;
            this._dPDShipToShopService = dPDShipToShopService;
            this._dPDShipToShopAutomationManager = dPDShipToShopAutomationManager;
            this._countryService = countryService;
        }

        #endregion

        /// <summary>
        /// Handles the order placed event and persists the selected pickup location against the order.
        /// </summary>
        /// <param name="eventMessage">The order placed event payload.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;
            if (order == null)
                throw new ArgumentNullException("Widgets.DPDShipToShop (OrderPlacedEvent): Order is null");


            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order.ShippingMethod == "DPD Ship to Shop")
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var storeId = store.Id;
                var storeDPDShipToShopLocationId = await _genericAttributeService.GetAttributeAsync<string>(customer, CustomNopCustomerDefaults.DPDShopToShopLocationId, storeId);
                int.TryParse(storeDPDShipToShopLocationId, out int dpdShipToShopLocationId);

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
                    string LocationAddressForNote = string.Empty;

                    if (dpdShipToShopLocation != null)
                    {

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.PickupLocationCode))
                        {
                            PickupLocationCode = string.Format("Location Code: {0}, ", dpdShipToShopLocation.PickupLocationCode);
                        }

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.Organisation))
                        {
                            Organisation = string.Format("{0}, ", dpdShipToShopLocation.Organisation);
                        }

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.Property))
                        {
                            Property = string.Format("{0}, ", dpdShipToShopLocation.Property);
                        }

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.Street))
                        {
                            Street = string.Format("{0}, ", dpdShipToShopLocation.Street);
                        }

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.Locality))
                        {
                            Locality = string.Format("{0}, ", dpdShipToShopLocation.Locality);
                        }

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.Town))
                        {
                            Town = string.Format("{0}, ", dpdShipToShopLocation.Town);
                        }

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.County))
                        {
                            County = string.Format("{0}, ", dpdShipToShopLocation.County);
                        }

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.Postcode))
                        {
                            Postcode = string.Format("{0}, ", dpdShipToShopLocation.Postcode);
                        }

                        if (!String.IsNullOrEmpty(dpdShipToShopLocation.CountryCode))
                        {
                            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(dpdShipToShopLocation.CountryCode);
                            Country = string.Format("{0}", country?.Name ?? string.Empty);
                        }

                        LocationAddress = string.Format("Address:<br/>{0}{1}{2}{3}{4}{5}{6}{7}", Organisation, Property, Street, Locality, Town, County, Postcode, Country);
                        LocationAddressForNote = string.Format("Address: {0}{1}{2}{3}{4}{5}{6}{7}", Organisation, Property, Street, Locality, Town, County, Postcode, Country);

                        var shipToShopLocation = string.Format("{0}{1}", PickupLocationCode, LocationAddress);
                        var shipToShopLocationForNote = string.Format("{0}{1}", PickupLocationCode, LocationAddressForNote);


                        var locationModel = new DPDShipToShopLocation
                        {
                            OrderId = order.Id,
                            DPDShipToShopLocationAddress = shipToShopLocation,
                            PickupLocationCode = dpdShipToShopLocation.PickupLocationCode,
                            Latitude = dpdShipToShopLocation.Latitude,
                            Longitude = dpdShipToShopLocation.Longitude,
                            OpeningHours = dpdShipToShopLocation.OpeningHours

                        };

                        //Add the selected DPD Pickup Location to the order notes.
                        await _orderService.InsertOrderNoteAsync(new OrderNote()
                        {
                            OrderId = order.Id,
                            Note = string.Format("DPD Ship to Shop: {0}", shipToShopLocationForNote),
                            DisplayToCustomer = true,
                            CreatedOnUtc = DateTime.UtcNow
                        });

                        //_logger.Information(string.Format("DPD Ship to Shop Location: {0}", shipToShopLocation));

                        await _dPDShipToShopService.InsertDPDShipToShopLocation(locationModel);

                    }
                    else
                    {
                        await _logger.ErrorAsync("Widgets.DPDShipToShop (OrderPlacedEvent): shipToShopLocation is null");
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(ex.Message, ex);
                }
            }

            //_dPDShipToShopAutomationManager.HandleOrderPlacedEventAsync(order);
        }
    }
}
