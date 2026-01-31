using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Response
{
    public class ShipToShopResponse : BaseResponse
    {
        [JsonProperty("error")]  //needed so doesnt clash with login response which also returns 'error' object
        public List<ShipToShopError> ShipToShopErrors { get; set; }
        [JsonProperty("data")]  //needed so doesnt clash with login response which also returns 'data' object
        //public ShipToShopData ShipToShopData { get; set; }
        public object ShipToShopData { get; set; }
    }


    public class ShipToShopData
    {
        public int ShipToShopId { get; set; }
        public bool consolidated { get; set; }
        public List<ConsignmentDetail> consignmentDetail { get; set; }
    }

    public class ShipToShopError
    {
        public string errorCode { get; set; }
        public string obj { get; set; }
        public string errorType { get; set; }
        public string errorMessage { get; set; }
        public object errorAction { get; set; }
    }
}
