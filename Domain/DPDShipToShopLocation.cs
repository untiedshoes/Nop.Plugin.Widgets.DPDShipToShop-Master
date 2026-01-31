using Nop.Core;
using System;

namespace Nop.Plugin.Widgets.DPDShipToShop.Domain
{
    public class DPDShipToShopLocation : BaseEntity
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
