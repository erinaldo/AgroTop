using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer.Extensions
{
    public static class DateExtensions
    {
        public static string ToString(this DateTime? value, string format, string defaultstring="")
        {
            if (value.HasValue)
                return value.Value.ToString(format);
            else
                return defaultstring;
        }
    }
}