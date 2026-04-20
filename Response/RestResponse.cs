using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Response
{
    public class RestResponse<T>
    {
        /// <summary>
        /// Deserializes the supplied HTTP response into the target type.
        /// </summary>
        /// <param name="response">The HTTP response to deserialize.</param>
        /// <returns>The deserialized response instance.</returns>
        public async Task<T> DeserializeResponseAsync(HttpResponseMessage response)
        {
            var contentJson = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<T>(contentJson);

            //result.Message = response.ReasonPhrase;
            //result.HttpResponseCode = response.StatusCode;

            return result;
        }
    }
}
