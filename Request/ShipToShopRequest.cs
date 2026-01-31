using Nop.Plugin.Widgets.DPDShipToShop.Model;
using Nop.Plugin.Widgets.DPDShipToShop.Request.Base;
using Nop.Plugin.Widgets.DPDShipToShop.Response;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Widgets.DPDShipToShop.Request
{
    public class ShipToShopRequest : PostRequest<ShipToShopResponse, Address>
    {
        public ShipToShopRequest(Address Address)
            : base(Address)
        {
            RequestUrl = "https://api.dpdgroup.co.uk/organisation/pickuplocation/filter=nearAddress&countryCode=GB&searchPageSize=10&searchPage=1&searchCriteria=&maxDistance=10&searchAddress=DH34AG";
        }

        public async Task<ShipToShopResponse> ExecuteShipToShopAsync()
        {
            //CR Logger
            var logger = EngineContext.Current.Resolve<ILogger>();
            await logger.InformationAsync("ShipToShopRequest PostRequest has been hit");

            var json = JsonConvert.SerializeObject(Content, SerializerSettings);

            //var response = await HttpClient.PostAsync(Host + RequestUrl, new StringContent(json, Encoding, MediaType));
            var response = await HttpClient.GetAsync(RequestUrl);

            RestResponse<ShipToShopResponse> r = new RestResponse<ShipToShopResponse>();

            await logger.InformationAsync("ShipToShopRequest ShipToShopResponse - " + r.DeserializeResponseAsync(response));

            return await r.DeserializeResponseAsync(response);
        }
    }
}
