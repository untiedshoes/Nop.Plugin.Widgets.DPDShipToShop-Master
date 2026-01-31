using Nop.Plugin.Widgets.DPDShipToShop.Model;
using Nop.Plugin.Widgets.DPDShipToShop.Request;
using Nop.Plugin.Widgets.DPDShipToShop.Request.Base;
using Nop.Plugin.Widgets.DPDShipToShop.Response;
using System;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.DPDShipToShop
{
    public class DpdShipToShopClient
    {
        public DpdShipToShopClient(string dpdUserName, string dpdPassword, string dpdAccountNumber, string dpdSessionId)
        {
            ServiceModelConfig.Create(dpdUserName, dpdPassword, dpdAccountNumber, dpdSessionId);
        }

        public async Task<LoginResponse> LoginAsync()
        {
            return await SendRequestAsync(new LoginRequest(new Model.Login()));
        }

        public LoginResponse Login()
        {
            return SendRequest(new LoginRequest(new Model.Login()));
        }

        //*********************************************************************************************************

        //Create Ship to Shop Async

        public async Task<ShipToShopResponse> CreateShipToShopAsync(Address Address, string DpdUserName, string DpdPassword, string dpdAccountNumber)
        {

            //CR Logger
            var logger = EngineContext.Current.Resolve<ILogger>();
            //logger.Information("DPDShipToShopClient Task<ShipToShopResponse> CreateShipToShopAsync has been hit");

            ShipToShopResponse ShipToShopResponse = new ShipToShopResponse();

            //Call Login as we need the login session token here on the header to continue
            LoginResponse loginResponse = await LoginAsync();

            if (!string.IsNullOrEmpty(loginResponse.data.geoSession))
            {
                //Create a new instance of the class and add login session value to the HTTP header
                var loggedInClient = new DpdShipToShopClient(DpdUserName, DpdPassword, dpdAccountNumber, loginResponse.data.geoSession);

                ShipToShopResponse = await SendRequestAsync(new ShipToShopRequest(Address));
            }

            //logger.Information("DPDShipToShopClient Task<ShipToShopResponse> CreateShipToShopAsync - " + Address + " - " + DpdUserName + " - " + " - " + DpdPassword + " - " + dpdAccountNumber);
            //logger.Information("DPDShipToShopClient Task<ShipToShopResponse> CreateShipToShopAsync ShipToShopResponse - " + ShipToShopResponse.ShipToShopData);
            return ShipToShopResponse;
        }


        //Create Shipment
        public async Task<ShipmentResponse> CreateShipmentAsync(Shipment shipment, string DpdUserName, string DpdPassword, string DpdAccountNumber)
        {
            ShipmentResponse shipmentResponse = new ShipmentResponse();

            //Call Login as we need the login session token here on the header to continue
            LoginResponse loginResponse = await LoginAsync();

            if (!string.IsNullOrEmpty(loginResponse.data.geoSession))
            {
                //Create a new instance of the class and add login session value to the HTTP header
                var loggedInClient = new DpdShipToShopClient(DpdUserName, DpdPassword, DpdAccountNumber, loginResponse.data.geoSession);

                shipmentResponse = await SendRequestAsync(new ShipmentRequest(shipment));
            }

            return shipmentResponse;
        }

        public ShipmentResponse CreateShipment(Shipment shipment, string DpdUserName, string DpdPassword, string DpdAccountNumber)
        {
            ShipmentResponse shipmentResponse = new ShipmentResponse();

            //Call Login as we need the login session token here on the header to continue
            LoginResponse loginResponse = Login();

            if (!string.IsNullOrEmpty(loginResponse.data.geoSession))
            {
                //Create a new instance of the class and add login session value to the HTTP header
                var loggedInClient = new DpdShipToShopClient(DpdUserName, DpdPassword, DpdAccountNumber, loginResponse.data.geoSession);

                shipmentResponse = SendRequest(new ShipmentRequest(shipment));
            }

            return shipmentResponse;
        }

        //*********************************************************************************************************

        private static async Task<TResponse> SendRequestAsync<TResponse>(BaseRequest<TResponse> request) where TResponse : BaseResponse, new()
        {
            try
            {
                return await request.ExecuteAsync();
            }
            catch (Exception e)
            {
                //Log.Error(request.GetType().FullName, e);

                return new TResponse()
                {
                    Error = e.InnerException != null ? e.InnerException.Message : e.Message,
                    Ok = false
                };
            }
        }

        private static TResponse SendRequest<TResponse>(BaseRequest<TResponse> request) where TResponse : BaseResponse, new()
        {
            try
            {
                return request.Execute();

            }
            catch (Exception e)
            {
                //Log.Error(request.GetType().FullName, e);

                return new TResponse()
                {
                    Error = e.InnerException != null ? e.InnerException.Message : e.Message,
                    Ok = false
                };
            }
        }


    }
}
