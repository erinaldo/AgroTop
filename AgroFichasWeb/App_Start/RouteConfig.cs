using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Zen.Barcode.Web;

namespace AgroFichasWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "AgroFichasWeb.Controllers" }
            );

            // Allow extensionless handling of barcode image URIs
            routes.Add(
                "BarcodeImaging",
                new Route("agrotop/generador/Barcode/{id}", new BarcodeImageRouteHandler()));

        }
    }
}