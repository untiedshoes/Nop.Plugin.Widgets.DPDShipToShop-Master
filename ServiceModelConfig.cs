using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Nop.Core.Configuration;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Nop.Plugin.Widgets.DPDShipToShop.Services;

namespace Nop.Plugin.Widgets.DPDShipToShop
{
    public static class ServiceModelConfig
    {

        //public static readonly String Host = "https://api.dpdgroup.co.uk";
        //public static readonly String Host = "https://api.interlinkexpress.com";
        public static readonly String Host = "https://api.dpdlocal.co.uk/";
        public static readonly HttpClient HttpClient = new HttpClient();
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

        /// <summary>
        /// Configures the shared HTTP client with the headers required for DPD API requests.
        /// </summary>
        /// <param name="dpdUserName">The DPD API user name.</param>
        /// <param name="dpdPassword">The DPD API password.</param>
        /// <param name="dpdAccountNumber">The DPD account number.</param>
        /// <param name="dpdSessionId">The optional DPD session identifier.</param>
        public static void Create(String dpdUserName, String dpdPassword, string dpdAccountNumber, string dpdSessionId = "")
        {
            //CR Logger
            //var logger = EngineContext.Current.Resolve<ILogger>();
            //logger.Information("ServiceModelConfig Create has been hit");

            var auth = String.Format("{0}:{1}", dpdUserName, dpdPassword);
            auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));


            //Flush any existing headers
            HttpClient.DefaultRequestHeaders.Remove("GEOClient");
            HttpClient.DefaultRequestHeaders.Remove("GEOSession");

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            HttpClient.DefaultRequestHeaders.Add("GEOClient", "account/" + dpdAccountNumber);


            if (!string.IsNullOrEmpty(dpdSessionId))
            {
                HttpClient.DefaultRequestHeaders.Add("GEOSession", dpdSessionId);
            }

            //logger.Information("ServiceModelConfig Create - " + dpdUserName + " - " + dpdPassword + " - " + dpdAccountNumber + " - " + dpdSessionId);
        }


    }
}
