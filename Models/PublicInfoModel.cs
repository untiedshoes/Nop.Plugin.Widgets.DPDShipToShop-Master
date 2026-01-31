using Nop.Plugin.Widgets.DPDShipToShop.Response;
using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.DPDShipToShop.Models
{
    public record PublicInfoModel : BaseNopModel
    {
        public string ShippingMethodName { get; set; }

        public string GoogleMapsApiKey { get; set; }

        public bool DisplayPickupPointsOnMap { get; set; }

        public bool UseGoogleAutoComplete { get; set; }
        
        public string ResponseData { get; set; }

        public IEnumerable<PickupPoints> PickupPoints { get; set; }

        public string PostalCode { get; set; }

        public string CountryCode { get; set; }

        public string ErrorMessage { get; set; }

    }

    public class PickupPoints
    {
        public string pickupLocation { get; set; }
    }
}
