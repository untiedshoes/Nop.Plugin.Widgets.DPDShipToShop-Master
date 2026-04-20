using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using Nop.Plugin.Widgets.DPDShipToShop.Services;
using Nop.Web.Areas.Admin.Factories;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Factories
{
    /// <summary>
    /// Represents DPD ship to shop models factory implementation
    /// </summary>
    public class DPDShipToShopModelFactory : IDPDShipToShopModelFactory
    {

        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IAddressService _addressService;
        private readonly IAffiliateService _affiliateService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IEncryptionService _encryptionService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IUrlRecordService _urlRecordService;
        private readonly TaxSettings _taxSettings;
        private readonly IDPDShipToShopService _dpdShipToShopService;

        #endregion

        #region Ctor

        public DPDShipToShopModelFactory(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressAttributeModelFactory addressAttributeModelFactory,
            IAddressService addressService,
            IAffiliateService affiliateService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDownloadService downloadService,
            IEncryptionService encryptionService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            IOrderProcessingService orderProcessingService,
            IOrderReportService orderReportService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            OrderSettings orderSettings,
            ShippingSettings shippingSettings,
            IUrlRecordService urlRecordService,
            TaxSettings taxSettings,
            IDPDShipToShopService dpdShipToShopService)
        {
            _addressSettings = addressSettings;
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressAttributeModelFactory = addressAttributeModelFactory;
            _addressService = addressService;
            _affiliateService = affiliateService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _downloadService = downloadService;
            _encryptionService = encryptionService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _measureService = measureService;
            _orderProcessingService = orderProcessingService;
            _orderReportService = orderReportService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _rewardPointService = rewardPointService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _taxService = taxService;
            _urlHelperFactory = urlHelperFactory;
            _vendorService = vendorService;
            _workContext = workContext;
            _measureSettings = measureSettings;
            _orderSettings = orderSettings;
            _shippingSettings = shippingSettings;
            _urlRecordService = urlRecordService;
            _taxSettings = taxSettings;
            _dpdShipToShopService = dpdShipToShopService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare shipment model
        /// </summary>
        /// <param name="model">Shipment model</param>
        /// <param name="shipment">Shipment</param>
        /// <param name="order">Order</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Shipment model</returns>
        public async Task<DPDShipmentModel> PrepareDPDShipmentModel(DPDShipmentModel model, Shipment shipment, Order order,
            bool excludeProperties = false)
        {
            if (shipment != null)
            {
                //fill in model values from the entity
                model ??= shipment.ToModel<DPDShipmentModel>();

                model.CanShip = !shipment.ShippedDateUtc.HasValue;
                model.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;

                var shipmentOrder = await _orderService.GetOrderByIdAsync(shipment.OrderId);

                model.CustomOrderNumber = shipmentOrder.CustomOrderNumber;

                model.ShippedDate = shipment.ShippedDateUtc.HasValue
                    ? (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc)).ToString()
                    : await _localizationService.GetResourceAsync("Admin.Orders.Shipments.ShippedDate.NotYet");
                model.DeliveryDate = shipment.DeliveryDateUtc.HasValue
                    ? (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc)).ToString()
                    : await _localizationService.GetResourceAsync("Admin.Orders.Shipments.DeliveryDate.NotYet");

                if (shipment.TotalWeight.HasValue)
                    model.TotalWeight =
                        $"{shipment.TotalWeight:F2} [{(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name}]";

                //prepare shipment items
                var shipmentItems = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);

                foreach (var item in shipmentItems)
                {
                    var orderItem = await _orderService.GetOrderItemByIdAsync(item.OrderItemId);

                    if (orderItem == null)
                        continue;

                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                    var warehouse = await _shippingService.GetWarehouseByIdAsync(item.WarehouseId);

                    var shipmentItemModel = new ShipmentItemModel
                    {
                        Id = item.Id,
                        QuantityInThisShipment = item.Quantity,
                        ShippedFromWarehouse = warehouse?.Name
                    };

                    await PrepareShipmentItemModelAsync(shipmentItemModel, orderItem, product);

                    model.Items.Add(shipmentItemModel);
                }

                //prepare shipment events
                if (!string.IsNullOrEmpty(shipment.TrackingNumber))
                {
                    var shipmentTracker = await _shipmentService.GetShipmentTrackerAsync(shipment);
                    if (shipmentTracker != null)
                    {
                        model.TrackingNumberUrl = await shipmentTracker.GetUrlAsync(shipment.TrackingNumber);
                        if (_shippingSettings.DisplayShipmentEventsToStoreOwner)
                            await PrepareShipmentStatusEventModels(model.ShipmentStatusEvents, shipment);
                    }
                }
            }

            if (shipment != null)
                return model;

            model.OrderId = order.Id;
            model.CustomOrderNumber = order.CustomOrderNumber;

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isShipEnabled: true, vendorId: currentVendor?.Id ?? 0);
            //get the number of parcels for DPD
            model.NumberOfParcels = orderItems.Count();

            decimal? totalWeight = 0;

            foreach (var orderItem in orderItems)
            {

                var shipmentItemModel = new ShipmentItemModel();

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                await PrepareShipmentItemModelAsync(shipmentItemModel, orderItem, product);

                //ensure that this product can be added to a shipment
                if (shipmentItemModel.QuantityToAdd <= 0)
                    continue;

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    shipmentItemModel.AllowToChooseWarehouse = true;
                    var inventoryRecords = await _productService.GetAllProductWarehouseInventoryRecordsAsync(orderItem.ProductId);
                    foreach (var pwi in inventoryRecords.OrderBy(w => w.WarehouseId).ToList())
                    {
                        var warehouse = await _productService.GetWarehousesByIdAsync(pwi.WarehouseId);
                        if (warehouse != null)
                        {
                            shipmentItemModel.AvailableWarehouses.Add(new ShipmentItemModel.WarehouseInfo
                            {
                                WarehouseId = warehouse.Id,
                                WarehouseName = warehouse.Name,
                                StockQuantity = pwi.StockQuantity,
                                ReservedQuantity = pwi.ReservedQuantity,
                                PlannedQuantity =
                                    await _shipmentService.GetQuantityInShipmentsAsync(product, warehouse.Id, true, true)
                            });
                        }
                    }
                }
                else
                {
                    //multiple warehouses are not supported
                    var warehouse = await _shippingService.GetWarehouseByIdAsync(product.WarehouseId);
                    if (warehouse != null)
                    {
                        shipmentItemModel.AvailableWarehouses.Add(new ShipmentItemModel.WarehouseInfo
                        {
                            WarehouseId = warehouse.Id,
                            WarehouseName = warehouse.Name,
                            StockQuantity = product.StockQuantity
                        });
                    }
                }

                //var usedWeight = _measureService.GetMeasureWeightBySystemKeyword("lb");
                var targetWeight = await _measureService.GetMeasureWeightBySystemKeywordAsync("kg");
                var orderItemTotalWeight = orderItem.ItemWeight * orderItem.Quantity;

                if (orderItemTotalWeight.HasValue)
                    //totalWeight += _measureService.ConvertWeight((decimal)orderItemTotalWeight, usedWeight, targetWeight, true);
                    totalWeight += await _measureService.ConvertFromPrimaryMeasureWeightAsync((decimal)orderItemTotalWeight, targetWeight);

                model.Items.Add(shipmentItemModel);
            }

            model.ParcelsTotalWeight = Math.Round((decimal)totalWeight, 2);

            return model;
        }

        #endregion

        /// <summary>
        /// Prepare shipment item model
        /// </summary>
        /// <param name="model">Shipment item model</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="product">Product item</param>
        protected virtual async Task PrepareShipmentItemModelAsync(ShipmentItemModel model,OrderItem orderItem,Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (orderItem is null)
                throw new ArgumentNullException(nameof(orderItem));

            if (product is null)
                throw new ArgumentNullException(nameof(product));

            if (orderItem.ProductId != product.Id)
                throw new ArgumentException($"{nameof(orderItem.ProductId)} != {nameof(product.Id)}");

            model.OrderItemId = orderItem.Id;
            model.ProductId = orderItem.ProductId;
            model.ProductName = product.Name;
            model.AttributeInfo = orderItem.AttributeDescription;
            model.ShipSeparately = product.ShipSeparately;
            model.QuantityOrdered = orderItem.Quantity;

            model.Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);

            model.QuantityInAllShipments =
                await _orderService.GetTotalNumberOfItemsInAllShipmentsAsync(orderItem);

            model.QuantityToAdd =
                await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);

            var baseWeightEntity =
                await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId);

            var baseDimensionEntity =
                await _measureService.GetMeasureDimensionByIdAsync(_measureSettings.BaseDimensionId);

            var baseWeight = baseWeightEntity?.Name;
            var baseDimension = baseDimensionEntity?.Name;

            if (orderItem.ItemWeight.HasValue)
                model.ItemWeight = $"{orderItem.ItemWeight:F2} [{baseWeight}]";

            model.ItemDimensions =
                $"{product.Length:F2} x {product.Width:F2} x {product.Height:F2} [{baseDimension}]";

            if (!product.IsRental)
                return;

            var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value)
                : string.Empty;

            var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value)
                : string.Empty;

            var resource =
                await _localizationService.GetResourceAsync("Order.Rental.FormattedDate");

            model.RentalInfo = string.Format(resource, rentalStartDate, rentalEndDate);
        }

        /// <summary>
        /// Prepare shipment status event models
        /// </summary>
        /// <param name="models">List of shipment status event models</param>
        /// <param name="shipment">Shipment</param>
        protected async virtual Task PrepareShipmentStatusEventModels(IList<ShipmentStatusEventModel> models, Shipment shipment)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            var shipmentTracker = await _shipmentService.GetShipmentTrackerAsync(shipment);
            var shipmentEvents = await shipmentTracker.GetShipmentEventsAsync(shipment.TrackingNumber);
            if (shipmentEvents == null)
                return;

            foreach (var shipmentEvent in shipmentEvents)
            {
                var shipmentStatusEventModel = new ShipmentStatusEventModel
                {
                    Date = shipmentEvent.Date,
                    EventName = shipmentEvent.EventName,
                    Location = shipmentEvent.Location
                };
                var shipmentEventCountry = await _countryService.GetCountryByTwoLetterIsoCodeAsync(shipmentEvent.CountryCode);
                shipmentStatusEventModel.Country = shipmentEventCountry != null
                    ? await _localizationService.GetLocalizedAsync(shipmentEventCountry, x => x.Name) : shipmentEvent.CountryCode;
                models.Add(shipmentStatusEventModel);
            }
        }

        protected class Weight
        {
            public static string Units => "ounce";

            public int Value { get; set; }
        }
    }
}
