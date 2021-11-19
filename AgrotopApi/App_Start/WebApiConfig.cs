using AgrotopApi.Models;
using AgrotopApi.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AgrotopApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuración y servicios de API web

            EnableCorsAttribute enableCors = new EnableCorsAttribute(RequestAllowedSites.AllowedSites, "*", "*");
            config.EnableCors(enableCors);

            // Rutas de API web
            config.MapHttpAttributeRoutes();

            config.MessageHandlers.Add(new TokenValidationHandler());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Liquidaciones",
                routeTemplate: "api/{controller}/{action}/{privateKey}",
                defaults: new { privateKey = RouteParameter.Optional }
            );
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }
    }
}
