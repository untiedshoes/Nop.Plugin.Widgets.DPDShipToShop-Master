using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Plugin.Widgets.DPDShipToShop.Response;
using System.Net;

namespace Nop.Plugin.Widgets.DPDShipToShop.Request.Base
{
    public abstract class BaseRequest<TResponse> where TResponse : BaseResponse, new()
    {
        protected HttpClient HttpClient;
        protected JsonSerializerSettings SerializerSettings;
        protected String Host;
        protected String RequestUrl;
        protected HttpMethod RequestMethod;

        /// <summary>
        /// Initializes a new base request using the supplied host, HTTP client, and serializer settings.
        /// </summary>
        /// <param name="host">The API host base URL.</param>
        /// <param name="httpClient">The HTTP client used to execute the request.</param>
        /// <param name="serializerSettings">The JSON serializer settings.</param>
        protected BaseRequest(String host, HttpClient httpClient, JsonSerializerSettings serializerSettings)
        {
            Host = host;
            HttpClient = httpClient;
            SerializerSettings = serializerSettings;

            if (!Host.EndsWith("/"))
            {
                Host += "/";
            }
        }

        /// <summary>
        /// Executes the request asynchronously.
        /// </summary>
        /// <returns>The response returned by the request.</returns>
        public virtual async Task<TResponse> ExecuteAsync()
        {
            return null;
        }

        /// <summary>
        /// Executes the request synchronously.
        /// </summary>
        /// <returns>The response returned by the request.</returns>
        public virtual TResponse Execute()
        {
            return null;
        }

        /// <summary>
        /// Deserializes an asynchronous HTTP response into the expected response type.
        /// </summary>
        /// <param name="response">The HTTP response to deserialize.</param>
        /// <returns>The deserialized response.</returns>
        protected async Task<TResponse> DeserializeResponseAsync(HttpResponseMessage response)
        {
            var contentJson = await response.Content.ReadAsStringAsync();

            TResponse result;

            try
            {
                result = JsonConvert.DeserializeObject<TResponse>(contentJson);
            }
            catch (Exception e)
            {
                result = new TResponse
                {
                    Message = "The server did not response with proper JSON (" + contentJson + ")"
                };
            }

            if (result == null)
            {
                result = new TResponse();
            }

            if (String.IsNullOrEmpty(result.Message))
            {
                result.Message = response.ReasonPhrase;
            }

            result.HttpResponseCode = response.StatusCode;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted ||
                    response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.NoContent)
            {
                result.Ok = true;
            }

            result.OnDeserialised();

            return result;
        }

        /// <summary>
        /// Deserializes a synchronous HTTP response into the expected response type.
        /// </summary>
        /// <param name="response">The HTTP response to deserialize.</param>
        /// <returns>The deserialized response.</returns>
        protected TResponse DeserializeResponse(HttpResponseMessage response)
        {
            var contentJson = response.Content.ReadAsStringAsync();

            TResponse result;

            try
            {
                result = JsonConvert.DeserializeObject<TResponse>(contentJson.Result);
            }
            catch (Exception e)
            {
                result = new TResponse
                {
                    Message = "The server did not response with proper JSON (" + contentJson + ")"
                };
            }

            if (result == null)
            {
                result = new TResponse();
            }

            if (String.IsNullOrEmpty(result.Message))
            {
                result.Message = response.ReasonPhrase;
            }

            result.HttpResponseCode = response.StatusCode;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted ||
                    response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.NoContent)
            {
                result.Ok = true;
            }

            result.OnDeserialised();

            return result;
        }
    }
}
