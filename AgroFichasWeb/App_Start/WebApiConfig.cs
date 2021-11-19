using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;

namespace AgroFichasWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
