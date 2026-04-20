using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Utility
{
    public static class StringUtility
    {
        /// <summary>
        /// Retrieve the description on the enum, e.g.
        /// [Description("Bright Pink")]
        /// BrightPink = 2,
        /// Then when you pass in the enum, it will retrieve the description
        /// </summary>
        /// <param name="en">The Enumeration</param>
        /// <returns>A string representing the friendly name</returns>
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }

        /// <summary>
        /// Formats pickup-point opening and closing times into a single display string.
        /// </summary>
        /// <param name="openTime">The opening time to display.</param>
        /// <param name="closedTime">The closing time to display.</param>
        /// <returns>A formatted opening-hours string or a closed indicator.</returns>
        public static Task<string> CleanHours(string openTime, string closedTime)
        {
            if (!String.IsNullOrEmpty(openTime) || !String.IsNullOrEmpty(closedTime))
            {
                return Task.FromResult(openTime + " - " + closedTime);
            }
            else
            {
                return Task.FromResult("Closed");
            }
        }

    }
}
