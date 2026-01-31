using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Model
{
    public class Shipment
    {
        public object job_id { get; set; }  //can use this to group together shipments
        public bool collectionOnDelivery { get; set; }  //generally this will be false for us
        public object invoice { get; set; } //only needed for international shipments
        public string collectionDate { get; set; }
        public bool consolidate { get; set; }
    }
}
