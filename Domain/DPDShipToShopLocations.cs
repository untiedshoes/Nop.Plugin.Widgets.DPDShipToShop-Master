using Nop.Core;
using System;

namespace Nop.Plugin.Widgets.DPDShipToShop.Domain
{
    public partial class DPDShipToShopLocations : BaseEntity
    {
        /// <summary>
        /// Gets or sets the pickup point identifier
        /// </summary>
        public string PickupLocationCode { get; set; }
        public string Organisation { get; set; }
        public string Property { get; set; }
        public string Street { get; set; }
        public string Locality { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string CountryCode { get; set; }
        public int CustomerId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string OpeningHours { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
    }
}
