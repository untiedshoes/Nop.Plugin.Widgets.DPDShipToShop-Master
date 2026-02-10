using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Plugin.Widgets.DPDShipToShop.Data;
using Nop.Plugin.Widgets.DPDShipToShop.Domain;
using Nop.Plugin.Widgets.DPDShipToShop.Model;
using Nop.Services.Events;
using Nop.Core.Events;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services
{
    /// <summary>
    /// DPD pickup point service
    /// </summary>
    class DPDShipToShopService : IDPDShipToShopService
    {
        #region Constants

        /// <summary>
        /// Cache key for pickup points
        /// </summary>
        /// <remarks>
        /// {0} : page index
        /// {1} : page size
        /// {2} : current store ID
        /// </remarks>
        private const string DPDPICKUP_POINT_ALL_KEY = "Nop.dpdpickuppoint.all-{0}-{1}-{2}-{3}";
        private const string DPDPICKUP_POINT_PATTERN_KEY = "Nop.dpdpickuppoint.";

        #endregion

        #region Fields

        private readonly IStaticCacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<DPDShipToShopLocations> _dpdPickupPointRepository;
        private readonly IRepository<DPDShipToShopLocation> _dpdShipToShopLocationsRepository;
        private readonly IRepository<Order> _orderRepository;


        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="dpdPickupPointRepository">DPD pickup point repository</param>
        public DPDShipToShopService(IEventPublisher eventPublisher,
            IStaticCacheManager cacheManager,
            IRepository<DPDShipToShopLocations> dpdPickupPointRepository,
            IRepository<DPDShipToShopLocation> dpdShipToShopLocationsRepository,
            IRepository<Order> orderRepository)
        {
            _eventPublisher = eventPublisher;
            _cacheManager = cacheManager;
            _dpdPickupPointRepository = dpdPickupPointRepository;
            _dpdShipToShopLocationsRepository = dpdShipToShopLocationsRepository;
            _orderRepository = orderRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Pickup points</returns>
        public virtual async Task<IPagedList<DPDShipToShopLocations>> GetAllDPDPickupPointsByLocationCodeCustomerIDAsync(int customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (customerId == 0)
                return null;

            var query = _dpdPickupPointRepository.Table
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .OrderBy(o => o.CreatedOnUtc)
                .ThenBy(o => o.Organisation);

            var items = await query.ToListAsync();

            return new PagedList<DPDShipToShopLocations>(items, pageIndex, pageSize);
        }


        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointId">Pickup point identifier</param>
        /// <returns>Pickup point</returns>
        public virtual async Task<DPDShipToShopLocations> GetDPDPickupPointByIdAsync(int pickupPointId)
        {
            if (pickupPointId == 0)
                return null;

            return await _dpdPickupPointRepository.GetByIdAsync(pickupPointId);
        }

        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointCustomerId">Pickup point customer identifier</param>
        /// <returns>Pickup point</returns>
        public virtual async Task<DPDShipToShopLocations> GetDPDPickupPointByCustomerIdAsync(int pickupPointCustomerId)
        {
            if (pickupPointCustomerId == 0)
                return null;

            var query = _dpdPickupPointRepository.Table
                         .Where(o => o.CustomerId == pickupPointCustomerId);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public async virtual Task InsertDPDPickupPoint(DPDShipToShopLocations pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException(nameof(pickupPoint));

            await _dpdPickupPointRepository.InsertAsync(pickupPoint);
            //_cacheManager.RemoveByPrefix(DPDPICKUP_POINT_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public async virtual Task UpdateDPDPickupPoint(DPDShipToShopLocations pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException(nameof(pickupPoint));

            await _dpdPickupPointRepository.UpdateAsync(pickupPoint);
            //_cacheManager.RemoveByPrefix(DPDPICKUP_POINT_PATTERN_KEY);
        }

        /// <summary>
        /// Deletes a pickup point
        /// </summary>
        /// <param name="pickupPoint">Pickup point</param>
        public async virtual Task DeleteDPDPickupPoint(DPDShipToShopLocations pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException(nameof(pickupPoint));

            await _dpdPickupPointRepository.DeleteAsync(pickupPoint);
            //_cacheManager.RemoveByPrefix(DPDPICKUP_POINT_PATTERN_KEY);
        }


        public virtual async Task<DPDShipToShopLocation> GetDPDShipToShopLocationByOrderIdAsync(int orderId)
        {
            if (orderId == 0)
                return null;

            var query = _dpdShipToShopLocationsRepository.Table
                         .Where(o => o.OrderId == orderId);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public virtual async Task<DPDShipToShopLocation> GetDPDShipToShopLocationByIdAsync(int locationId)
        {
            if (locationId == 0)
                return null;

            return await _dpdShipToShopLocationsRepository.GetByIdAsync(locationId);
        }

        public async virtual Task InsertDPDShipToShopLocation(DPDShipToShopLocation dpdShipToShopLocation)
        {
            if (dpdShipToShopLocation == null)
                throw new ArgumentNullException(nameof(dpdShipToShopLocation));

            await _dpdShipToShopLocationsRepository.InsertAsync(dpdShipToShopLocation);
        }

        public async virtual Task UpdateDPDShipToShopLocation(DPDShipToShopLocation dpdShipToShopLocation)
        {
            if (dpdShipToShopLocation == null)
                throw new ArgumentNullException(nameof(dpdShipToShopLocation));

            await _dpdShipToShopLocationsRepository.UpdateAsync(dpdShipToShopLocation);
        }

        public async virtual Task DeleteDPDShipToShopLocation(DPDShipToShopLocation dpdShipToShopLocation)
        {
            if (dpdShipToShopLocation == null)
                throw new ArgumentNullException(nameof(dpdShipToShopLocation));

            await _dpdShipToShopLocationsRepository.DeleteAsync(dpdShipToShopLocation);
        }


        #endregion
    }
}
