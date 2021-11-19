using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer
{
    public static class DateParser
    {
        public static DateTime? DateFromRequest(string field)
        {
            DateTime dt;
            var formats = new string[] {"dd/MM/yyyy", "dd-MM-yyyy"};
            if (DateTime.TryParseExact(HttpContext.Current.Request[field], formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            else
                return null;
        }

        public static DateTime? DateFromString(string value)
        {
            DateTime dt;
            var formats = new string[] { "dd/MM/yyyy", "dd-MM-yyyy" };
            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            else
                return null;
        }

    }
}