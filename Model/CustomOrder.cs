using System;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;

namespace Nop.Plugin.Widgets.DPDShipToShop.Model
{
    public partial class CustomOrder : Order
    {
        //[CR]
        /// <summary>
        /// Gets or sets the custom order dpd Ship to Shop Location
        /// </summary>
        public string DPDShipToShopLocation { get; set; }
    }
}
