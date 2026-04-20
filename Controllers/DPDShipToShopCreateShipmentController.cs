using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Widgets.DPDShipToShop.Factories;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.DPDShipToShop.Controllers
{
    
    public class DPDShipToShopCreateShipmentController : BasePluginController
    {
        #region Fields

        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly IEncryptionService _encryptionService;
        private readonly IExportManager _exportManager;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly OrderSettings _orderSettings;
        private readonly IDPDShipToShopModelFactory _DPDShipToShopModelFactory;
        private readonly ILogger _logger;
        #endregion

        #region Ctor

        public DPDShipToShopCreateShipmentController(IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IEncryptionService encryptionService,
            IExportManager exportManager,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            OrderSettings orderSettings,
            IDPDShipToShopModelFactory DPDShipToShopModelFactory,
            ILogger logger)
        {
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _encryptionService = encryptionService;
            _exportManager = exportManager;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _orderSettings = orderSettings;
            _DPDShipToShopModelFactory = DPDShipToShopModelFactory;
            _logger = logger;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Determines whether the current user can access the specified order.
        /// </summary>
        /// <param name="order">The order to check.</param>
        /// <returns>True when the current user can access the order; otherwise false.</returns>
        protected virtual async Task<bool> HasAccessToOrder(Order order)
        {
            return order != null && await HasAccessToOrder(order.Id);
        }

        /// <summary>
        /// Determines whether the current user can access the specified order identifier.
        /// </summary>
        /// <param name="orderId">The order identifier to check.</param>
        /// <returns>True when the current user can access the order; otherwise false.</returns>
        protected virtual async Task<bool> HasAccessToOrder(int orderId)
        {
            if (orderId == 0)
                return false;

            var vendor = await _workContext.GetCurrentVendorAsync();
            if (vendor == null)
                return true;

            var hasVendorProducts = (await _orderService.GetOrderItemsAsync(orderId, vendorId: vendor.Id)).Any();

            return hasVendorProducts;
        }

        /// <summary>
        /// Determines whether the current user can access the specified order item product.
        /// </summary>
        /// <param name="orderItem">The order item to check.</param>
        /// <returns>True when the current user can access the product; otherwise false.</returns>
        protected virtual async Task<bool> HasAccessToProduct(OrderItem orderItem)
        {
            if (orderItem == null || orderItem.ProductId == 0)
                return false;

            var vendor = await _workContext.GetCurrentVendorAsync();
            if (vendor == null)
                return true;

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            return product?.VendorId == vendor.Id;
        }

        /// <summary>
        /// Determines whether the current user can access the specified shipment.
        /// </summary>
        /// <param name="shipment">The shipment to check.</param>
        /// <returns>True when the current user can access the shipment; otherwise false.</returns>
        protected virtual async Task<bool> HasAccessToShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var vendor = await _workContext.GetCurrentVendorAsync();
            if (vendor == null)
                return true;

            return await HasAccessToOrder(shipment.OrderId);
        }

        /// <summary>
        /// Writes an activity log entry for an edited order.
        /// </summary>
        /// <param name="orderId">The identifier of the edited order.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async virtual Task LogEditOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            await _customerActivityService.InsertActivityAsync("EditOrder",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }

        #endregion

        #region Shipments

        /// <summary>
        /// Displays the admin page used to create a DPD shipment for an order.
        /// </summary>
        /// <param name="id">The order identifier.</param>
        /// <returns>The DPD shipment creation view.</returns>
        [Area(AreaNames.Admin)]
        public async virtual Task<IActionResult> AddDPDShipment(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            await _logger.InformationAsync("Order ID:" + id);

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && !await HasAccessToOrder(order))
                return RedirectToAction("List");

            //prepare model
            var model = await _DPDShipToShopModelFactory.PrepareDPDShipmentModel(new DPDShipmentModel(), null, order);

            return View("~/Plugins/Widgets.DPDShipToShop/Views/DPDAddShipment.cshtml", model);
        }

        #endregion
    }
}
