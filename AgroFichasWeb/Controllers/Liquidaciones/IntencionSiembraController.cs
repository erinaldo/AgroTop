using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers.Liquidaciones
{
    [WebsiteAuthorize]
    public class IntencionSiembraController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public IntencionSiembraController()
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
                { "idCultivo", Request.QueryString["idCultivo"] ?? "" },
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        private void SetLookupLists()
        {
            //ViewData["cultivos"] = Cultivo.CultivosParaIntencionSiembra(dc).ToList();
            //ViewData["comunas"] = dc.Comuna.OrderBy(c => c.Nombre).ToList();
        }


        public ActionResult Index(int? pageIndex, int? idTemporada, int? idCultivo, string key = "")
        {
            CheckPermisoAndRedirect(1002);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Items
            var idCultivoSelect = idCultivo ?? 0;
            var items = from c in dc.IntencionSiembra
                        where c.IdTemporada == temporada.IdTemporada
                           && (idCultivoSelect == 0 || c.IdCultivo == idCultivoSelect)
                           && (key == "" ||
                               c.Agricultor.Nombre.Contains(key) ||
                               c.Cultivo.Nombre.Contains(key) ||
                               c.Comuna.Nombre.Contains(key) ||
                               c.Comuna.Provincia.Nombre.Contains(key))
                        orderby c.Agricultor.Nombre, c.Cultivo.Nombre descending
                        select c;

            var model = new PaginatedList<IntencionSiembra>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idCultivo", Request.QueryString["idCultivo"] ?? "" }
            };

            //ViewData
            ViewData["idCultivo"] = idCultivo ?? 0;
            ViewData["cultivos"] = (from cc in dc.CultivoContrato
                                    join cu in dc.Cultivo on cc.IdCultivo equals cu.IdCultivo
                                    select cu).Distinct().ToList();


            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult IndexExport(int idTemporada, int? idCultivo, string key = "")
        {
            CheckPermisoAndRedirect(1002);

            var idCultivoSelect = idCultivo ?? 0;
            var items = from c in dc.IntencionSiembra
                        where c.IdTemporada == idTemporada
                           && (idCultivoSelect == 0 || c.IdCultivo == idCultivoSelect)
                           && (key == "" ||
                               c.Agricultor.Nombre.Contains(key) ||
                               c.Cultivo.Nombre.Contains(key) ||
                               c.Comuna.Nombre.Contains(key) ||
                               c.Comuna.Provincia.Nombre.Contains(key))
                        orderby c.Agricultor.Nombre, c.Cultivo.Nombre descending
                        select IntencionSiembraExportModel.FromIntencionSiembra(c);

            return new CsvActionResult<IntencionSiembraExportModel>(items.ToList(), "IntencionSiembra.csv", 1, ';', null);
        }

        public ActionResult Crear(int? idTemporada)
        {
            CheckPermisoAndRedirect(1003);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, null, out temporada);

            var intecionSiembra = new IntencionSiembra
            {
                IdTemporada = temporada.IdTemporada
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("IntencionSiembra", intecionSiembra);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(IntencionSiembra intencionSiembra, FormCollection formValues)
        {
            CheckPermisoAndRedirect(1003);

            if (ModelState.IsValid)
            {
                try
                {
                    intencionSiembra.SetDefaults();

                    
                    intencionSiembra.UserIns = User.Identity.Name;
                    intencionSiembra.FechaHoraIns = DateTime.Now;
                    intencionSiembra.IpIns = RemoteAddr();
                    intencionSiembra.MobileTag = "";

                    dc.IntencionSiembra.InsertOnSubmit(intencionSiembra);

                    dc.SubmitChanges();
                    return RedirectToAction("Index", new { idTemporada = intencionSiembra.IdTemporada });
                }
                catch
                {
                    var rv = intencionSiembra.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                intencionSiembra.Agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == intencionSiembra.IdAgricultor);
            }

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, intencionSiembra.IdTemporada, out temporada);

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("IntencionSiembra", intencionSiembra);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(1004);

            var intencionSiembra = dc.IntencionSiembra.SingleOrDefault(p => p.IdIntencionSiembra == id);
            if (intencionSiembra == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "IntencionSiembra Not Found");

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("IntencionSiembra", intencionSiembra);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(1004);

            var intencionSiembra = dc.IntencionSiembra.SingleOrDefault(p => p.IdIntencionSiembra == id);
            var fields = new string[] { "IdAgricultor", "IdCultivo", "IdComuna", "Cantidad", "Superficie", "PuntoEntrega", "Observaciones" };

            if (TryUpdateModel(intencionSiembra, fields))
            {
                try
                {
                    intencionSiembra.SetDefaults();

                    intencionSiembra.UserUpd = User.Identity.Name;
                    intencionSiembra.FechaHoraUpd = DateTime.Now;
                    intencionSiembra.IpUpd = RemoteAddr();

                    dc.SubmitChanges();
                    return RedirectToAction("Index", IndexRouteValues(null));
                }
                catch
                {
                    var rv = intencionSiembra.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                if (intencionSiembra.Agricultor == null)
                {
                    intencionSiembra.Agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == intencionSiembra.IdAgricultor);
                }
            }


            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);

            return View("IntencionSiembra", intencionSiembra);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(1005);
            var intencionSiembra = dc.IntencionSiembra.SingleOrDefault(p => p.IdIntencionSiembra == id);

            var routeValues = new RouteValueDictionary();
            try
            {
                if (intencionSiembra != null)
                {
                    dc.IntencionSiembra.DeleteOnSubmit(intencionSiembra);
                    dc.SubmitChanges();
                }
                routeValues.Add("msgok", "La intención de siembra fue eliminada con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible eliminar la intención de siembra";
                msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            return RedirectToAction("Index", IndexRouteValues(routeValues));
        }
    }
}
