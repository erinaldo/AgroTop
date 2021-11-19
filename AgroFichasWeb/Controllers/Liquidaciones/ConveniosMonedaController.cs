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
    public class ConveniosMonedaController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ConveniosMonedaController()
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
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        private void SetLookupLists()
        {
            ViewData["monedas"] = dc.Moneda.ToList();
        }

        public ActionResult Index(int? pageIndex, int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(19);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);
            
            //Items
            var items = from cp in dc.ConvenioCambioMoneda
                        join co in dc.Contrato on cp.IdContrato equals co.IdContrato
                        join ag in dc.Agricultor on co.IdAgricultor equals ag.IdAgricultor
                        where co.IdTemporada == temporada.IdTemporada
                           && (key == "" || ag.Nombre.Contains(key))
                        orderby co.NumeroContrato, cp.Prioridad
                        select cp;

            var model = new PaginatedList<ConvenioCambioMoneda>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key }, 
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" }
            };

            //ViewData
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;
            
            return View(model);
        }

        public ActionResult IndexExport(int idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(19);

            var items = from cp in dc.ConvenioCambioMoneda
                        join co in dc.Contrato on cp.IdContrato equals co.IdContrato
                        join ag in dc.Agricultor on co.IdAgricultor equals ag.IdAgricultor
                        where co.IdTemporada == idTemporada
                           && (key == "" || ag.Nombre.Contains(key))
                        orderby co.NumeroContrato, cp.Prioridad
                        select ConveniosMonedaExportModel.FromConvenioCambio(cp);

            return new CsvActionResult<ConveniosMonedaExportModel>(items.ToList(), "ConveniosCambioMoneda.csv", 1, ';', null);
        }

        public ActionResult Crear(int? idContrato)
        {
            CheckPermisoAndRedirect(20);

            var convenio = new ConvenioCambioMoneda
            {
                IdContrato = idContrato ?? 0,
                Habilitado = true,
                IdMonedaOrigen = 2, //USD
                IdMonedaDestino = 1 //CLP
            };

            if (convenio.IdContrato > 0 && convenio.Contrato == null)
                convenio.Contrato = dc.Contrato.SingleOrDefault(c => c.IdContrato == convenio.IdContrato);

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("ConvenioCambioMoneda", convenio);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(ConvenioCambioMoneda convenio)
        {
            CheckPermisoAndRedirect(20);
            if (ModelState.IsValid)
            {
                try
                {
                    if (convenio.Comentarios == null)
                        convenio.Comentarios = "";

                    convenio.Prioridad = ConvenioCambioMoneda.NextPrioridad(dc, convenio.IdContrato);
                    convenio.UserIns = User.Identity.Name;
                    convenio.FechaHoraIns = DateTime.Now;
                    convenio.IpIns = RemoteAddr();

                    dc.ConvenioCambioMoneda.InsertOnSubmit(convenio);
                    dc.SubmitChanges();

                    if (!String.IsNullOrEmpty(Request["backto"]))
                        return Redirect(Request["backto"]);
                    else
                        return RedirectToAction("Index");
                }
                catch
                {
                    var rv = convenio.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                if (convenio.Contrato == null)
                {
                    convenio.Contrato = dc.Contrato.SingleOrDefault(c => c.IdContrato == convenio.IdContrato);
                }
            }

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("ConvenioCambioMoneda", convenio);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(21);

            var convenio = dc.ConvenioCambioMoneda.SingleOrDefault(p => p.IdConvenioCambioMoneda == id);
            if (convenio == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "ConvenioCambioMoneda Not Found");

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("ConvenioCambioMoneda", convenio);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(21);
            var convenio = dc.ConvenioCambioMoneda.SingleOrDefault(p => p.IdConvenioCambioMoneda == id);
            var idContratoOrig = convenio.IdContrato;

            var fields = new string[] { "IdContrato", "IdMonedaDestino", "IdMonedaOrigen", "Cantidad", "PrecioUnidad", "Habilitado", "Comentarios" };
            TryUpdateModel(convenio, fields);

            if (ModelState.IsValid)
            {
                try
                {
                    if (convenio.Comentarios == null)
                        convenio.Comentarios = "";

                    if (convenio.IdContrato != idContratoOrig)
                        convenio.Prioridad = ConvenioCambioMoneda.NextPrioridad(dc, convenio.IdContrato);

                    convenio.UserUpd = User.Identity.Name;
                    convenio.FechaHoraUpd = DateTime.Now;
                    convenio.IpUpd = RemoteAddr();

                    dc.SubmitChanges();

                    if (!String.IsNullOrEmpty(Request["backto"]))
                        return Redirect(Request["backto"]);
                    else
                        return RedirectToAction("Index", IndexRouteValues(null));
                }
                catch
                {
                    var rv = convenio.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                if (convenio.Contrato == null)
                {
                    convenio.Contrato = dc.Contrato.SingleOrDefault(c => c.IdContrato == convenio.IdContrato);
                }
            }
            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("ConvenioCambioMoneda", convenio);
        }

        public ActionResult Subir(int id)
        {
            return Mover(id, true);
        }

        public ActionResult Bajar(int id)
        {
            return Mover(id, false);
        }

        public ActionResult Mover(int id, bool up)
        {
            CheckPermisoAndRedirect(21);
            var convenio = dc.ConvenioCambioMoneda.SingleOrDefault(c => c.IdConvenioCambioMoneda == id);
            if (convenio != null)
            {
                dc.ConvenioCambioMoneda_MovePrioridad(convenio.IdConvenioCambioMoneda, (up) ? -1 : 1);
            }

            if (!String.IsNullOrEmpty(Request["backto"]))
                return Redirect(Request["backto"]);
            else
                return RedirectToAction("Index", IndexRouteValues(null));
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(22);
            var convenio = dc.ConvenioCambioMoneda.SingleOrDefault(p => p.IdConvenioCambioMoneda == id);

            var routeValues = new RouteValueDictionary();
            try
            {
                if (convenio != null)
                {
                    dc.ConvenioCambioMoneda.DeleteOnSubmit(convenio);
                    dc.SubmitChanges();
                }
                routeValues.Add("msgok", "El convenio de precio fue eliminado con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible eliminar el convenio de precio";
                if (ex.Message.Contains("***"))
                    msgerr += " porque tiene al menos una ** asociada";
                else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            if (!String.IsNullOrEmpty(Request["backto"]))
                return Redirect(Request["backto"] + "&msgok=El convenio de precio fue eliminado con éxito");
            else
                return RedirectToAction("Index", IndexRouteValues(routeValues));
        }
    }
}
