using System;

namespace Nop.Plugin.Widgets.DPDShipToShop.Models
{
    /// <summary>
    /// Represents a simplified DPD API error returned to the client-side map workflow.
    /// </summary>
    public class DpdApiErrorModel
    {
        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ObjectName { get; set; }

        public string ErrorType { get; set; }
    }
}