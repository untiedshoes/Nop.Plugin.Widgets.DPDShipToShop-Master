using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using Nop.Plugin.Widgets.DPDShipToShop.Services;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Components
{
    [ViewComponent(Name = DPDShipToShopDefaults.DPD_CREATE_SHIPMENT_BUTTON_COMPONENT_NAME)]
    public class DPDCreateShipmentButtonViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IDPDShipToShopService _DPDShipToShopService;
        private readonly DPDShipToShopSettings _DPDShipToShopSettings;

        #endregion

        #region Ctor

        public DPDCreateShipmentButtonViewComponent(IOrderService orderService,
            IDPDShipToShopService DPDShipToShopService,
            DPDShipToShopSettings DPDShipToShopSettings)
        {
            _orderService = orderService;
            _DPDShipToShopService = DPDShipToShopService;
            _DPDShipToShopSettings = DPDShipToShopSettings;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            
            if (!_DPDShipToShopSettings.PluginEnabled)
            {
                return Content(string.Empty);
            }

            if (string.IsNullOrEmpty(_DPDShipToShopSettings.AccountNumber) || string.IsNullOrEmpty(_DPDShipToShopSettings.Password) || string.IsNullOrEmpty(_DPDShipToShopSettings.UserName))
                return Content(string.Empty);

            if (!(additionalData is OrderModel orderModel))
                return Content(string.Empty);

            //try to get data to fill model
            var order = await _orderService.GetOrderByIdAsync(orderModel.Id).Result;

            //if (!string.IsNullOrEmpty(order.ShippingMethod) && order.PaymentStatus == PaymentStatus.Paid && order.ShippingStatus == ShippingStatus.NotYetShipped)
            if (!string.IsNullOrEmpty(order.ShippingMethod) && order.PaymentStatus == PaymentStatus.Paid)
            {
                if (order.ShippingMethod == "DPD Ship to Shop")
                {
                    var OrderId = order.Id;
                    return View("~/Plugins/Widgets.DPDShipToShop/Views/DPDCreateShipmentButton.cshtml", (widgetZone, OrderId));
                }
            }

            return Content(string.Empty);
        }

        #endregion
    }
}
