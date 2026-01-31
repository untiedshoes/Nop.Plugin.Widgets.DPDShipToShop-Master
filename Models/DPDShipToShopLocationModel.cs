using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Widgets.DPDShipToShop.Models
{
    public partial class DPDShipToShopLocationModel
    {
        public int OrderId { get; set; }
        public string PickupLocationCode { get; set; }
        public string DPDShipToShopLocationAddress { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string OpeningHours { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
    }
}
