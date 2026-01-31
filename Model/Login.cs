using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Model
{
    public class Login
    {
        public string AccountNumber { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresIn { get; set; }
    }
}
