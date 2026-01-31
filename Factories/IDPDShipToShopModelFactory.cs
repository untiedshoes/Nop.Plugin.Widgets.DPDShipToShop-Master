using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Factories
{
    /// <summary>
    /// Represents the DPD ship to shop models factory
    /// </summary>
    public interface IDPDShipToShopModelFactory
    {

        /// <summary>
        /// Prepare DPD shipment model
        /// </summary>
        /// <param name="model">DPD Shipment model</param>
        /// <param name="shipment">Shipment</param>
        /// <param name="order">Order</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Shipment model</returns>
        Task<DPDShipmentModel> PrepareDPDShipmentModel(DPDShipmentModel model, Shipment shipment, Order order, bool excludeProperties = false);

    }
}
