using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Nop.Services.Configuration;
using Nop.Services; 
using Nop.Core;
using Nop.Plugin.Widgets.DPDShipToShop.Model;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services
{
    /// <summary>
    /// Centralized DPD session manager: performs login, caches geoSession and refreshes when needed.
    /// </summary>
    public class DPDSessionService : IDPDSessionService
    {
        private readonly IDistributedCache _cache;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILogger<DPDSessionService> _logger;

        private const int CacheExpirationInDays = 1;

        public DPDSessionService(IDistributedCache cache,
            IStoreContext storeContext,
            ISettingService settingService,
            ILogger<DPDSessionService> logger)
        {
            _cache = cache;
            _storeContext = storeContext;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<string> GetGeoSessionAsync(string cacheKey = "DPD_GEOSESSION")
        {
            var item = await GetAccessTokenFromCacheAsync(cacheKey).ConfigureAwait(false);
            if (item != null && item.ExpiresIn > DateTime.UtcNow && !string.IsNullOrEmpty(item.AccessToken))
                return item.AccessToken;

            var refreshed = await RefreshGeoSessionAsync(cacheKey).ConfigureAwait(false);
            return refreshed;
        }

        public async Task<string> RefreshGeoSessionAsync(string cacheKey = "DPD_GEOSESSION")
        {
            try
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync().ConfigureAwait(false);
                var dpdSettings = await _settingService.LoadSettingAsync<DPDShipToShopSettings>(currentStore.Id).ConfigureAwait(false);

                var client = new DpdShipToShopClient(dpdSettings.UserName, dpdSettings.Password, dpdSettings.AccountNumber, "");
                var loginResponse = await client.LoginAsync().ConfigureAwait(false);

                if (loginResponse?.data?.geoSession == null)
                {
                    _logger.LogWarning("DPD login returned null geoSession");
                    return string.Empty;
                }

                var tokenItem = new AccessTokenItem
                {
                    AccessToken = loginResponse.data.geoSession,
                    ExpiresIn = DateTime.UtcNow.AddDays(CacheExpirationInDays)
                };

                await AddAccessTokenToCacheAsync(cacheKey, tokenItem).ConfigureAwait(false);

                return tokenItem.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing DPD geoSession");
                throw;
            }
        }

        private async Task<AccessTokenItem> GetAccessTokenFromCacheAsync(string key)
        {
            var item = await _cache.GetStringAsync(key).ConfigureAwait(false);
            if (item == null)
                return null;

            return await Task.Run(() => JsonConvert.DeserializeObject<AccessTokenItem>(item)).ConfigureAwait(false);
        }

        private async Task AddAccessTokenToCacheAsync(string key, AccessTokenItem accessTokenItem)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(CacheExpirationInDays));
            await _cache.SetStringAsync(key, JsonConvert.SerializeObject(accessTokenItem), options).ConfigureAwait(false);
        }
    }
}
