using System;

namespace Nop.Plugin.Widgets.DPDShipToShop.Model
{
    public class AccessTokenItem
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresIn { get; set; }
    }
}
