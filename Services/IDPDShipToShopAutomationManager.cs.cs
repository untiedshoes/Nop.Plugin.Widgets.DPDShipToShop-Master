using Nop.Core.Domain.Orders;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services
{
    public interface IDPDShipToShopAutomationManager
    {
        /// <summary>
        /// Handle order placed event
        /// </summary>
        /// <param name="orderEvent">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task HandleOrderPlacedEventAsync(Order orderEvent);
    }
}