using Nop.Plugin.Widgets.DPDShipToShop.Model;
using Nop.Plugin.Widgets.DPDShipToShop.Request.Base;
using Nop.Plugin.Widgets.DPDShipToShop.Response;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Request
{
    public class LoginRequest : PostRequest<LoginResponse, Login>
    {
        public LoginRequest(Login login)
            : base(login)
        {
            RequestUrl = "user/?action=login";
        }
    }
}

