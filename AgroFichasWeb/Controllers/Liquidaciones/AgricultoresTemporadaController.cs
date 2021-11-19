using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class AgricultoresTemporadaController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public AgricultoresTemporadaController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }, 
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "soloACubrir", Request.QueryString["soloACubrir"] ?? "" },
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Index(int? pageIndex, int? idTemporada, int? idEmpresa, bool? soloACubrir, string key = "")
        {
            CheckPermisoAndRedirect(34);

            //Temporadas
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out Temporada temporada);

            //Items
            var items =  dc.AgricultoresTemporada(temporada.IdTemporada, idEmpresa ?? 0, key, soloACubrir ?? false).ToList();

            var model = new PaginatedList<AgricultoresTemporadaResult>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key }, 
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "soloACubrir", Request.QueryString["soloACubrir"] ?? "" },
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["soloACubrir"] = soloACubrir ?? false;
            ViewData["intencionesSiembra"] = dc.CRM_Objetivos.Where(X => X.IdTemporada == temporada.IdTemporada).ToList();
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult IndexExport(int idTemporada, int? idEmpresa, bool? soloACubrir, string key = "")
        {
            CheckPermisoAndRedirect(34);

            var items = dc.AgricultoresTemporada(idTemporada, idEmpresa ?? 0, key, soloACubrir ?? false).ToList();

            return new CsvActionResult<AgricultoresTemporadaResult>(items.ToList(), "AgricultoresTemporada.csv", 1, ';', null);
        }

    }
}
