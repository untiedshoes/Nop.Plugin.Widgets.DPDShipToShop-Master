using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services
{
    public interface IDPDSessionService
    {
        /// <summary>
        /// Gets a valid DPD geoSession token, refreshing and caching it when necessary.
        /// </summary>
        /// <param name="cacheKey">Optional cache key name.</param>
        Task<string> GetGeoSessionAsync(string cacheKey = "DPD_GEOSESSION");

        /// <summary>
        /// Forces a refresh of the geoSession token and returns the new value.
        /// </summary>
        Task<string> RefreshGeoSessionAsync(string cacheKey = "DPD_GEOSESSION");
    }
}
