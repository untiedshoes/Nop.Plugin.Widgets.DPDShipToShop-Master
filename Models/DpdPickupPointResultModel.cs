using System;

namespace Nop.Plugin.Widgets.DPDShipToShop.Models
{
    /// <summary>
    /// Represents the flattened pickup-point data returned by the controller.
    /// </summary>
    public class DpdPickupPointResultModel
    {
        public int PickupLocationId { get; set; }

        public string PickupLocationCode { get; set; }

        public string PickupLocationOrganisation { get; set; }

        public string PickupLocationProperty { get; set; }

        public string PickupLocationStreet { get; set; }

        public string PickupLocationLocality { get; set; }

        public string PickupLocationTown { get; set; }

        public string PickupLocationCounty { get; set; }

        public string PickupLocationPostcode { get; set; }

        public string PickupLocationCountryCode { get; set; }

        public float PickupLocationDistance { get; set; }

        public bool PickupLocationDisabledAccess { get; set; }

        public bool PickupLocationOpenLate { get; set; }

        public bool PickupLocationOpenSaturday { get; set; }

        public bool PickupLocationOpenSunday { get; set; }

        public bool PickupLocationParkingAvailable { get; set; }

        public decimal? PickupLocationLatitude { get; set; }

        public decimal? PickupLocationLongitude { get; set; }

        public string PickupLocationWeekdayTime { get; set; }

        public string PickupLocationSaturdayTime { get; set; }

        public string PickupLocationSundayTime { get; set; }

        public int CustomerId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}