using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using Nop.Plugin.Widgets.DPDShipToShop.Services;
using Nop.Plugin.Widgets.DPDShipToShop.Domain;
using Nop.Plugin.Widgets.DPDShipToShop.Model;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services
{
    public class DPDShipToShopAutomationManager : IDPDShipToShopAutomationManager
    {
        #region Fields
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IDPDShipToShopService _dPDShipToShopService;
        #endregion

        #region Ctor
        public DPDShipToShopAutomationManager(IOrderService orderService,
            IStoreContext storeContext,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            ILogger logger,
            IDPDShipToShopService dPDShipToShopService)
        {

            this._orderService = orderService;
            this._storeContext = storeContext;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._workContext = workContext;
            this._logger = logger;
            this._dPDShipToShopService = dPDShipToShopService;
        }

        #endregion

        /// <summary>
        /// Handle order placed event
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleOrderPlacedEventAsync(Order orderEvent)
        {
            var order = orderEvent;

            var customOrderModel = new CustomOrder
            {
                Id = order.Id
            };

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
                    string Latitude = string.Empty;
                    string Longitude = string.Empty;
                    string LocationAddress = string.Empty;

                    if (dpdShipToShopLocation != null)
                    {

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

                        LocationAddress = string.Format("Address:<br/>{0}{1}{2}{3}{4}{5}{6}", Organisation, Property, Street, Locality, Town, County, Postcode);

                        var shipToShopLocation = string.Format("{0}{1}", PickupLocationCode, LocationAddress);

                        //Add the selected DPD Pickup Location to the order.
                        customOrderModel.DPDShipToShopLocation = shipToShopLocation;

                        //Add the selected DPD Pickup Location to the order notes.
                        await _orderService.InsertOrderNoteAsync(new OrderNote()
                        {
                            OrderId = order.Id,
                            Note = string.Format("DPD Ship to Shop Location: {0}", shipToShopLocation),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });


                        await _orderService.UpdateOrderAsync(customOrderModel);
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
        }
    }
}
