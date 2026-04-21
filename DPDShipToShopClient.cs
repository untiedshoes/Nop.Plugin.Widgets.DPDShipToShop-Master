using Nop.Plugin.Widgets.DPDShipToShop.Model;
using Nop.Plugin.Widgets.DPDShipToShop.Request;
using Nop.Plugin.Widgets.DPDShipToShop.Request.Base;
using Nop.Plugin.Widgets.DPDShipToShop.Response;
using System;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop
{
    public class DpdShipToShopClient
    {
        /// <summary>
        /// Initializes a new DPD API client and configures the shared HTTP headers.
        /// </summary>
        /// <param name="dpdUserName">The DPD API user name.</param>
        /// <param name="dpdPassword">The DPD API password.</param>
        /// <param name="dpdAccountNumber">The DPD account number.</param>
        /// <param name="dpdSessionId">The optional DPD session identifier.</param>
        public DpdShipToShopClient(string dpdUserName, string dpdPassword, string dpdAccountNumber, string dpdSessionId)
        {
            ServiceModelConfig.Create(dpdUserName, dpdPassword, dpdAccountNumber, dpdSessionId);
        }

        /// <summary>
        /// Sends an asynchronous login request to the DPD API.
        /// </summary>
        /// <returns>The login response containing the session details.</returns>
        public async Task<LoginResponse> LoginAsync()
        {
            return await SendRequestAsync(new LoginRequest(new Model.Login()));
        }

        /// <summary>
        /// Sends a synchronous login request to the DPD API.
        /// </summary>
        /// <returns>The login response containing the session details.</returns>
        public LoginResponse Login()
        {
            return SendRequest(new LoginRequest(new Model.Login()));
        }

        //*********************************************************************************************************

        //Create Ship to Shop Async

        /// <summary>
        /// Creates a ship-to-shop request for the specified address.
        /// </summary>
        /// <param name="Address">The address to use for the ship-to-shop lookup.</param>
        /// <returns>The DPD ship-to-shop response.</returns>
        public async Task<ShipToShopResponse> CreateShipToShopAsync(Address Address)
        {
            return await SendRequestAsync(new ShipToShopRequest(Address));
        }


        //Create Shipment
        /// <summary>
        /// Creates a shipment asynchronously using the supplied DPD credentials.
        /// </summary>
        /// <param name="shipment">The shipment payload to send to DPD.</param>
        /// <returns>The shipment creation response.</returns>
        public async Task<ShipmentResponse> CreateShipmentAsync(Shipment shipment)
        {
            return await SendRequestAsync(new ShipmentRequest(shipment));
        }

        /// <summary>
        /// Creates a shipment synchronously using the supplied DPD credentials.
        /// </summary>
        /// <param name="shipment">The shipment payload to send to DPD.</param>
        /// <returns>The shipment creation response.</returns>
        public ShipmentResponse CreateShipment(Shipment shipment)
        {
            return SendRequest(new ShipmentRequest(shipment));
        }

        //*********************************************************************************************************

        /// <summary>
        /// Executes the specified request asynchronously and maps any exception to a failed response.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="request">The request to execute.</param>
        /// <returns>The deserialized response instance.</returns>
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

        /// <summary>
        /// Executes the specified request synchronously and maps any exception to a failed response.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="request">The request to execute.</param>
        /// <returns>The deserialized response instance.</returns>
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
