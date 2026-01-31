using Nop.Core;
using Nop.Plugin.Widgets.DPDShipToShop.Domain;
using Nop.Plugin.Widgets.DPDShipToShop.Model;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services
{
    /// <summary>
    /// DPD pickup point service interface
    /// </summary>
    public partial interface IDPDShipToShopService
    {

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Pickup points</returns>
        IPagedList<DPDShipToShopLocations> GetAllDPDPickupPointsByLocationCodeCustomerID(int customerId, int pageIndex = 0, int pageSize = int.MaxValue);
        
        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointId">Pickup point identifier</param>
        /// <returns>Pickup point</returns>
        DPDShipToShopLocations GetDPDPickupPointById(int pickupPointId);

        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointCustomerId">Pickup point customer identifier</param>
        /// <returns>Pickup point</returns>
        DPDShipToShopLocations GetDPDPickupPointByCustomerId(int pickupPointCustomerId);

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        Task InsertDPDPickupPoint(DPDShipToShopLocations pickupPoint);

        /// <summary>
        /// Updates a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        Task UpdateDPDPickupPoint(DPDShipToShopLocations pickupPoint);

        /// <summary>
        /// Deletes a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        Task DeleteDPDPickupPoint(DPDShipToShopLocations pickupPoint);

        /// <summary>
        /// Gets a Ship To Shop Location by order id
        /// </summary>
        /// <param name="dpdShipToShopLocationOrderId">Order identifier</param>
        /// <returns>Pickup point</returns>
        DPDShipToShopLocation GetDPDShipToShopLocationByOrderId(int orderId);

        /// <summary>
        /// Gets a Ship To Shop Location by id
        /// </summary>
        /// <param name="dpdShipToShopLocationId">Ship To Shop Location identifier</param>
        /// <returns>Pickup point</returns>
        DPDShipToShopLocation GetDPDShipToShopLocationById(int dpdShipToShopLocationId);

        /// <summary>
        /// Inserts a Ship To Shop Location
        /// </summary>
        /// <param name="dpdShipToShopLocation">Ship To Shop Location</param>
        Task InsertDPDShipToShopLocation(DPDShipToShopLocation dpdShipToShopLocation);

        /// <summary>
        /// Updates a Ship To Shop Location
        /// </summary>
        /// <param name="dpdShipToShopLocation">Ship To Shop Location</param>
        Task UpdateDPDShipToShopLocation(DPDShipToShopLocation dpdShipToShopLocation);

        /// <summary>
        /// Deletes a Ship To Shop Location
        /// </summary>
        /// <param name="dpdShipToShopLocation">Ship To Shop Location</param>
        Task DeleteDPDShipToShopLocation(DPDShipToShopLocation dpdShipToShopLocation);

    }
}