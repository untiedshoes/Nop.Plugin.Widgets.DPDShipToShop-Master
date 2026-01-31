using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Request.Base
{
    public class RestRequest<T>
    {
        public virtual async Task<T> ExecuteAsync()
        {
            return default(T);
        }

    }
}
