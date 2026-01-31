using System;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Widgets.DPDShipToShop.Models
{
    public record DPDShipmentModel : ShipmentModel
    {
        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.CollectionDate")]
        public DateTime? CollectionDate { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.NumberOfParcels")]
        public int NumberOfParcels { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.ParcelTotalWeight")]
        public decimal? ParcelsTotalWeight { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.ShippingRef1")]
        public string ShippingRef1 { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.ShippingRef2")]
        public string ShippingRef2 { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.ShippingRef3")]
        public string ShippingRef3 { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.CustomsValue")]
        public object CustomsValue { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.DeliveryInstructions")]
        public string DeliveryInstructions { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.ParcelDescription")]
        public string ParcelDescription { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.Liability")]
        public bool Liability { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.DPDShipToShop.Fields.LiabilityValue")]
        public object LiabilityValue { get; set; }
    }
}
