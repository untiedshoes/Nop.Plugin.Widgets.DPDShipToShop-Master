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
        public virtual IPagedList<DPDShipToShopLocations> GetAllDPDPickupPointsByLocationCodeCustomerID(int customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _dpdPickupPointRepository.Table;
            if (customerId == 0)
            {
                return null;
            }
            else
            {
                query = query.Where(o => o.CustomerId == customerId);
                query = query.OrderBy(o => o.CreatedOnUtc).ThenBy(o => o.Organisation);

                return new PagedList<DPDShipToShopLocations>(query.ToList(), pageIndex, pageSize);
            }
        }


        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointId">Pickup point identifier</param>
        /// <returns>Pickup point</returns>
        public virtual DPDShipToShopLocations GetDPDPickupPointById(int pickupPointId)
        {
            if (pickupPointId == 0)
                return null;

            return _dpdPickupPointRepository.GetByIdAsync(pickupPointId).Result;
        }

        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointCustomerId">Pickup point customer identifier</param>
        /// <returns>Pickup point</returns>
        public virtual DPDShipToShopLocations GetDPDPickupPointByCustomerId(int pickupPointCustomerId)
        {
            if (pickupPointCustomerId == 0)
                return null;

            var query = _dpdPickupPointRepository.Table;
            query = query.Where(o => o.CustomerId == pickupPointCustomerId);
            var item = query.FirstOrDefault();

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


        public virtual DPDShipToShopLocation GetDPDShipToShopLocationByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;

            var query = _dpdShipToShopLocationsRepository.Table;
                query = query.Where(o => o.OrderId == orderId);
            var item = query.FirstOrDefault();

            return item;

            //return _dpdShipToShopLocationsRepository.GetById(locationId);
        }

        public virtual DPDShipToShopLocation GetDPDShipToShopLocationById(int locationId)
        {
            if (locationId == 0)
                return null;

            return _dpdShipToShopLocationsRepository.GetByIdAsync(locationId).Result;
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
