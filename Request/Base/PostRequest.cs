using Nop.Plugin.Widgets.DPDShipToShop.Response;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Widgets.DPDShipToShop.Request.Base
{


    /// <summary>
	/// Used to form a VALIDATE request
	/// Accept the same range of payloads as /api/push, but parse and validate only, without sending any pushes
	/// 
	/// http://docs.urbanairship.com/reference/api/v3/push.html
	/// </summary>
	public class PostRequest<TResponse, TContent> : BaseRequest<TResponse> where TResponse : BaseResponse, new()
    {
        //TODO: PostRequest shouldn't declate this - should be more abstract
        public readonly Encoding Encoding = Encoding.UTF8;
        public const String MediaType = "application/json";

        protected TContent Content;

        public PostRequest(TContent content)
            : base(ServiceModelConfig.Host, ServiceModelConfig.HttpClient, ServiceModelConfig.SerializerSettings)
        {
            RequestMethod = HttpMethod.Post;
            Content = content;
        }

        /// <summary>
        /// Executes the POST request asynchronously.
        /// </summary>
        /// <returns>The deserialized response payload.</returns>
        public override async Task<TResponse> ExecuteAsync()
        {
            //CR Logger
            //var logger = EngineContext.Current.Resolve<ILogger>();
            //logger.Information("PostRequest ExecuteAsync has been hit");
            //logger.Information("PostRequest ExecuteAsync RequestMethod - " + RequestMethod + " - " + Host + RequestUrl);

            var json = JsonConvert.SerializeObject(Content, SerializerSettings);

            //logger.Information("PostRequest ExecuteAsync Payload - " + json);

            var response = await HttpClient.PostAsync(Host + RequestUrl, new StringContent(json, Encoding, MediaType));

            //logger.Information("PostRequest ExecuteAsync HttpClient.PostAsync - " + Host + RequestUrl + Content.ToString());

            return await DeserializeResponseAsync(response);
        }

        /// <summary>
        /// Executes the POST request synchronously.
        /// </summary>
        /// <returns>The deserialized response payload.</returns>
        public override TResponse Execute()
        {

            //CR Logger
            //var logger = EngineContext.Current.Resolve<ILogger>();
            //logger.Information("PostRequest Execute has been hit");
            //logger.Information("PostRequest Execute RequestMethod - " + RequestMethod + " - " + Host + RequestUrl);

            var json = JsonConvert.SerializeObject(Content, SerializerSettings);

            //logger.Information("PostRequest Execute Payload - " + json);

            var response = HttpClient.PostAsync(Host + RequestUrl, new StringContent(json, Encoding, MediaType));

            //logger.Information("PostRequest Execute HttpClient.PostAsync - " + Host + RequestUrl + Content.ToString());

            return DeserializeResponse(response.Result);
        }
    }
}
