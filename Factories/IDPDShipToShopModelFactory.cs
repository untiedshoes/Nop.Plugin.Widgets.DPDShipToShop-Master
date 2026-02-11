using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Factories
{
    /// <summary>
    /// Represents the DPD Ship to Shop model factory contract
    /// </summary>
    public interface IDPDShipToShopModelFactory
    {
        /// <summary>
        /// Prepare a DPD shipment model
        /// </summary>
        /// <param name="model">Shipment model</param>
        /// <param name="shipment">Shipment entity</param>
        /// <param name="order">Order entity</param>
        /// <param name="excludeProperties">Whether to exclude some properties</param>
        /// <returns>Prepared DPD shipment model</returns>
        Task<DPDShipmentModel> PrepareDPDShipmentModel(DPDShipmentModel model,Shipment shipment,Order order,bool excludeProperties = false);

        /// <summary>
        /// Prepare a shipment item model
        /// </summary>
        /// <param name="model">Shipment item model</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="product">Product entity</param>
        /// <returns>Task representing the async operation</returns>
        Task PrepareShipmentItemModelAsync(ShipmentItemModel model,OrderItem orderItem,Product product);

        /// <summary>
        /// Prepare shipment status event models
        /// </summary>
        /// <param name="models">List of shipment status event models</param>
        /// <param name="shipment">Shipment entity</param>
        /// <returns>Task representing the async operation</returns>
        Task PrepareShipmentStatusEventModels(IList<ShipmentStatusEventModel> models,Shipment shipment);
    }
}
