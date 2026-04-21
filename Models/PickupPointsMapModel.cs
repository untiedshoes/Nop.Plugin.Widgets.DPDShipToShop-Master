using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.DPDShipToShop.Models
{
    public class PickupPointsMapModel
    {
        //GET THE SHIPPING METHOD NAME FROM THE CONFIG
        public string ShippingMethodName { get; set; }

        //GET THE GOOGLE MAPS API KEY NAME FROM THE CONFIG
        public string GoogleMapsApiKey { get; set; }

        //SHOULD WE DISPLAY THE PICKUP POINS ON THE MAP
        public bool DisplayPickupPointsOnMap { get; set; }

        //GETS THE CURRENT CUSTOMERS POSTA CODE
        public string PostalCode { get; set; }

        //GETS THE CURRENT CUSTOMERS COUNTRY CODE
        public string CountryCode { get; set; }

        public string ResponseData { get; set; }

        //GETS THE API RESULT OBJECT
        public DpdPickupPointsApiResponse ApiResponse { get; set; }
    }

    public class DpdPickupPointGroup
    {
        public List<PickupLocation> pickupLocation { get; set; }
    }

    public class PickupPointsData
    {
        public int ShipToShopId { get; set; }
        public List<DpdPickupPointGroup> results { get; set; }
    }

    public class PickupLocation
    {
        public string pickupLocationCode { get; set; }
    }


    //*----------------------------------------


    public class DpdPickupPointsApiResponse
    {

        public DpdPickupPointsApiError[] error { get; set; }
        public DpdPickupPointsApiData data { get; set; }

        //GET THE SHIPPING METHOD NAME FROM THE CONFIG
        public string ShippingMethodName { get; set; }

        //GET THE GOOGLE MAPS API KEY NAME FROM THE CONFIG
        public string GoogleMapsApiKey { get; set; }

        //SHOULD WE DISPLAY THE PICKUP POINS ON THE MAP
        public bool DisplayPickupPointsOnMap { get; set; }

        //GETS THE CURRENT CUSTOMERS POSTA CODE
        public string PostalCode { get; set; }

        //GETS THE CURRENT CUSTOMERS COUNTRY CODE
        public string CountryCode { get; set; }

        public string ResponseData { get; set; }
    }

    public class DpdPickupPointsApiData
    {
        public DpdPickupPointsApiResult[] results { get; set; }
        public int totalResults { get; set; }
    }

    public class DpdPickupPointsApiResult
    {
        public DpdPickupLocation pickupLocation { get; set; }
        public float distance { get; set; }
        public DpdPickupPointAddressPoint addressPoint { get; set; }
    }

    public class DpdPickupLocation
    {
        public string pickupLocationCode { get; set; }
        public Address address { get; set; }
        public string pickupLocationType { get; set; }
        public DpdPickupLocationAddressPoint addressPoint { get; set; }
        public string languageSpoken { get; set; }
        public bool disabledAccess { get; set; }
        public bool parkingAvailable { get; set; }
        public string[] geoServiceCode { get; set; }
        public string pickupLocationDirections { get; set; }
        public DpdPickupLocationAvailability pickupLocationAvailability { get; set; }
        public bool openLate { get; set; }
        public bool openSaturday { get; set; }
        public bool openSunday { get; set; }
        public object shortName { get; set; }
        public object pickupLocationImageUrl { get; set; }
        public object pickupLocationLogoUrl { get; set; }
        public DpdPickupLocationDriverWindow[] pickupLocationDriverWindow { get; set; }
    }

    public class Address
    {
        public string organisation { get; set; }
        public string property { get; set; }
        public string street { get; set; }
        public string locality { get; set; }
        public string town { get; set; }
        public string county { get; set; }
        public string postcode { get; set; }
        public string countryCode { get; set; }
    }

    public class DpdPickupLocationAddressPoint
    {
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
    }

    public class DpdPickupLocationAvailability
    {
        public string pickupLocationActiveStart { get; set; }
        public string pickupLocationActiveEnd { get; set; }
        public string pickupLocationDeliveryStart { get; set; }
        public string pickupLocationDeliveryClosed { get; set; }
        public string pickupLocationOpeningExceptions { get; set; }
        public DpdPickupLocationOpenWindow[] pickupLocationOpenWindow { get; set; }
        public object[] pickupLocationDownTime { get; set; }
    }

    public class DpdPickupLocationOpenWindow
    {
        public string pickupLocationOpenWindowStartTime { get; set; }
        public string pickupLocationOpenWindowEndTime { get; set; }
        public int pickupLocationOpenWindowDay { get; set; }
    }

    public class DpdPickupLocationDriverWindow
    {
        public string pickupLocationDriverWindowStartTime { get; set; }
        public string pickupLocationDriverWindowEndTime { get; set; }
        public int pickupLocationDriverWindowDay { get; set; }
    }

    public class DpdPickupPointAddressPoint
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
    }

    public class DpdPickupPointsApiError
    {
        public string errorObj { get; set; }
        public string errorMessage { get; set; }
        public string errorCode { get; set; }
        public string errorType { get; set; }
    }


}
