using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Widgets.DPDShipToShop.Models
{
    public record CustomCheckoutShippingMethodModel : CheckoutShippingAddressModel
    {
        public int? DPDShipToShopLocationCode { get; set; }
    }
}
