using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public class ApiRealUrl
    {
        public static string ProductionUrl = "http://agrotopapi.empresasagrotop.cl/api";

        public static string DevUrl = "http://localhost:54843/api";

        public static string GetApiUrl()
        {
            if (HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority).Contains("localhost"))
            {
                return DevUrl;
            }
            else
            {
                return ProductionUrl;
            }
        }
    }
}