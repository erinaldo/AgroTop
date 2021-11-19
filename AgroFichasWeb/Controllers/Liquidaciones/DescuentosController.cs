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

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class DescuentosController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public DescuentosController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTipoDescuento", Request.QueryString["idTipoDescuento"] ?? "" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        private void SetLookupLists()
        {
            ViewData["tiposDescuento"] = dc.TipoDescuento.OrderBy(td => td.Prioridad).ToList();
        }

        public ActionResult Index(int? pageIndex, int? idTipoDescuento, int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(36);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var idTipoDescuentoSelect = idTipoDescuento ?? 0;

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<Descuento> items;
            if (adminTodos)
            {
                items = from de in dc.Descuento
                        join ag in dc.Agricultor on de.IdAgricultor equals ag.IdAgricultor
                        where de.IdTemporada == temporada.IdTemporada
                           && (idTipoDescuentoSelect == 0 || de.IdTipoDescuento == idTipoDescuentoSelect)
                           && (key == "" || ag.Nombre.Contains(key))
                        orderby de.IdDescuento
                        select de;
            }
            else
            {
                items = from de in dc.Descuento
                        join ag in dc.Agricultor on de.IdAgricultor equals ag.IdAgricultor
                        join ua in dc.UsuarioAgricultor on ag.IdAgricultor equals ua.IdAgricultor
                        join su in dc.SYS_User on ua.UserID equals su.UserID
                        where de.IdTemporada == idTemporada
                           && (idTipoDescuentoSelect == 0 || de.IdTipoDescuento == idTipoDescuentoSelect)
                           && (key == "" || ag.Nombre.Contains(key))
                           && ua.UserID == CurrentUser.UserID
                        orderby de.IdDescuento
                        select de;
            }

            var model = new PaginatedList<Descuento>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTipoDescuento", Request.QueryString["idTipoDescuento"] ?? "" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" }
            };

            //ViewData
            ViewData["idTipoDescuento"] = idTipoDescuentoSelect;
            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            SetLookupLists();

            return View(model);
        }

        public ActionResult IndexExport(int? idTipoDescuento, int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(36);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var idTipoDescuentoSelect = idTipoDescuento ?? 0;

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<DescuentosExportModel> items;
            if (adminTodos)
            {
                items = from de in dc.Descuento
                        join ag in dc.Agricultor on de.IdAgricultor equals ag.IdAgricultor
                        where de.IdTemporada == idTemporada
                           && (idTipoDescuentoSelect == 0 || de.IdTipoDescuento == idTipoDescuentoSelect)
                           && (key == "" || ag.Nombre.Contains(key))
                        orderby de.IdDescuento
                        select DescuentosExportModel.FromDescuento(de);
            }
            else
            {
                items = from de in dc.Descuento
                        join ag in dc.Agricultor on de.IdAgricultor equals ag.IdAgricultor
                        join ua in dc.UsuarioAgricultor on ag.IdAgricultor equals ua.IdAgricultor
                        join su in dc.SYS_User on ua.UserID equals su.UserID
                        where de.IdTemporada == idTemporada
                           && (idTipoDescuentoSelect == 0 || de.IdTipoDescuento == idTipoDescuentoSelect)
                           && (key == "" || ag.Nombre.Contains(key))
                           && ua.UserID == CurrentUser.UserID
                        orderby de.IdDescuento
                        select DescuentosExportModel.FromDescuento(de);
            }

            return new CsvActionResult<DescuentosExportModel>(items.ToList(), "Descuentos.csv", 1, ';', null);
        }

        public ActionResult Crear(int idTipoDescuento, int? idTemporada)
        {
            CheckPermisoAndRedirect(37);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, null, out temporada);

            var descuento = new Descuento
            {
                IdTipoDescuento = idTipoDescuento,
                IdTemporada = temporada.IdTemporada,
                IdMoneda = 1,
                Fecha = DateTime.Today,
                FechaVencimiento = DateTime.Today
            };

            if (descuento.TipoDescuento == null)
                descuento.TipoDescuento = dc.TipoDescuento.SingleOrDefault(td => td.IdTipoDescuento == descuento.IdTipoDescuento);

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            SetLookupLists();

            return View("Descuento", descuento);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Crear([Bind(Exclude = "Fecha, FechaVencimiento")] Descuento descuento)
        {
            CheckPermisoAndRedirect(37);

            descuento.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();
            descuento.FechaVencimiento = DateParser.DateFromRequest("FechaVencimiento").GetValueOrDefault();

            if (ModelState.IsValid)
            {
                try
                {
                    if (descuento.Comentarios == null)
                        descuento.Comentarios = "";

                    if (descuento.Institucion == null)
                        descuento.Institucion = "";

                    if (descuento.PrecioUnidad.HasValue && descuento.Cantidad.HasValue)
                    {
                        var moneda = dc.Moneda.Single(m => m.IdMoneda == descuento.IdMoneda);
                        descuento.TotalNeto = Math.Round(descuento.PrecioUnidad.Value * descuento.Cantidad.Value, moneda.Decimales);
                    }
                    else
                    {
                        descuento.TotalNeto = null;
                    }

                    descuento.UserIns = User.Identity.Name;
                    descuento.FechaHoraIns = DateTime.Now;
                    descuento.IpIns = RemoteAddr();

                    dc.Descuento.InsertOnSubmit(descuento);

                    dc.SubmitChanges();

                    if (!String.IsNullOrEmpty(Request["backto"]))
                        return Redirect(Request["backto"]);
                    else
                        return RedirectToAction("Index");
                }
                catch
                {
                    var rv = descuento.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                if (descuento.TipoDescuento == null)
                    descuento.TipoDescuento = dc.TipoDescuento.SingleOrDefault(td => td.IdTipoDescuento == descuento.IdTipoDescuento);



            }

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, descuento.IdTemporada, out temporada);
            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            SetLookupLists();

            return View("Descuento", descuento);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(38);

            var descuento = dc.Descuento.SingleOrDefault(p => p.IdDescuento == id);
            if (descuento == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Descuento Not Found");

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("Descuento", descuento);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(38);
            var descuento = dc.Descuento.SingleOrDefault(p => p.IdDescuento == id);

            var fields = new string[] { "IdAgricultor", "IdMoneda", "Monto", "Institucion", "NumeroDocumento", "Comentarios", "PrecioUnidad", "Cantidad" };
            TryUpdateModel(descuento, fields);

            descuento.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();
            descuento.FechaVencimiento = DateParser.DateFromRequest("FechaVencimiento").GetValueOrDefault();

            if (ModelState.IsValid)
            {
                try
                {
                    if (descuento.Comentarios == null)
                        descuento.Comentarios = "";

                    if (descuento.PrecioUnidad.HasValue && descuento.Cantidad.HasValue)
                    {
                        var moneda = dc.Moneda.Single(m => m.IdMoneda == descuento.IdMoneda);
                        descuento.TotalNeto = Math.Round(descuento.PrecioUnidad.Value * descuento.Cantidad.Value, moneda.Decimales);
                    }
                    else
                    {
                        descuento.TotalNeto = null;
                    }

                    descuento.UserUpd = User.Identity.Name;
                    descuento.FechaHoraUpd = DateTime.Now;
                    descuento.IpUpd = RemoteAddr();

                    dc.SubmitChanges();

                    if (!String.IsNullOrEmpty(Request["backto"]))
                        return Redirect(Request["backto"]);
                    else
                        return RedirectToAction("Index", IndexRouteValues(null));
                }
                catch
                {
                    var rv = descuento.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                if (descuento.TipoDescuento == null)
                    descuento.TipoDescuento = dc.TipoDescuento.SingleOrDefault(td => td.IdTipoDescuento == descuento.IdTipoDescuento);
            }

            ViewData["indexRouteValues"] = IndexRouteValues(null);
            SetLookupLists();

            return View("Descuento", descuento);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(39);
            var descuento = dc.Descuento.SingleOrDefault(p => p.IdDescuento == id);

            var routeValues = new RouteValueDictionary();
            try
            {
                if (descuento != null)
                {
                    dc.Descuento.DeleteOnSubmit(descuento);
                    dc.SubmitChanges();
                }
                routeValues.Add("msgok", "El descuento fue eliminado con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible eliminar el descuento";
                if (ex.Message.Contains("***"))
                    msgerr += " porque tiene al menos un ... asociado";
                else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            return RedirectToAction("Index", IndexRouteValues(routeValues));
        }
    }
}
