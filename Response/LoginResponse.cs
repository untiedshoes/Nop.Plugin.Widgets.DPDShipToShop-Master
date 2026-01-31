using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Response
{
    public class LoginResponse : BaseResponse
    {
        public data data { get; set; }
        public string error { get; set; }
        public DateTime ExpiresIn { get; set; }
        public string AccountNumber { get; set; }
        public string ClientId { get; set; }
    }

    public class data
    {
        public string flag { get; set; }
        public string geoSession { get; set; }
        
    }
}
