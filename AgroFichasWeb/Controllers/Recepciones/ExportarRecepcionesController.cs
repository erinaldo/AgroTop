using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Recepciones;
using AgroFichasWeb.AppLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers.Recepciones
{
    [WebsiteAuthorize]
    public class ExportarRecepcionesController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ExportarRecepcionesController()
        {
            SetCurrentModulo(3); //Recepciones
        }

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Index(int? pageIndex, int? idTemporada, int? idEmpresa)
        {
            CheckPermisoAndRedirect(78);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from eb in dc.ExportBatch
                        where eb.IdTemporada == temporada.IdTemporada
                           && (idEmpresaSelect == 0 || eb.IdEmpresa == idEmpresaSelect)
                        orderby eb.IdExportBatch descending
                        select eb;



            var model = new PaginatedList<ExportBatch>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;

            return View(model);
        }

        public ActionResult Crear(int idEmpresa, int? idTemporada)
        {
            CheckPermisoAndRedirect(79);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var model = new ExportarRecepcionesViewModel(dc, temporada.IdTemporada, idEmpresa);

            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporada"] = model.Temporada;
            ViewData["temporadas"] = temporadas;
            return View(model);
        }

        [HttpPost]
        public ActionResult Crear(ExportarRecepcionesViewModel model)
        {
            CheckPermisoAndRedirect(79);
            model.Validate(dc, ModelState);
            if (ModelState.IsValid)
            {
                var batch = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("crearfin", new { id = batch.IdExportBatch });
            }
            else
            {
                model.LoadLookups(dc);
                ViewData["empresas"] = dc.Empresa.ToList();

                Temporada temporada;
                List<Temporada> temporadas = ResolveTemporadas(dc, model.IdTemporada, out temporada);
                ViewData["temporada"] = temporada;
                ViewData["temporadas"] = temporadas;

                return View(model);
            }
        }

        public ActionResult CrearFin(int id)
        {
            CheckPermisoAndRedirect(79);
            var model = dc.ExportBatch.Single(eb => eb.IdExportBatch == id);
            ViewData["empresas"] = dc.Empresa.ToList();
            return View(model);
        }

        public ActionResult Detalle(int id)
        {
            CheckPermisoAndRedirect(78);
            var model = dc.ExportBatch.Single(eb => eb.IdExportBatch == id);
            ViewData["empresas"] = dc.Empresa.ToList();
            return View(model);
        }

        public ActionResult Anular(int id)
        {
            CheckPermisoAndRedirect(80);

            var routeValues = new RouteValueDictionary();
            try
            {
                dc.AnularExportBatch(id, SYS_User.Current().UserName, RemoteAddr());
                routeValues.Add("msgok", "El lote de exportación fue anulado con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible anular el lote de exportación";
                if (ex.Message.Contains("FK_"))
                    msgerr += " porque tiene al menos un *** asociado";
                else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            return RedirectToAction("Index", IndexRouteValues(routeValues));
        }

        public ActionResult ExportEncabezado(int id)
        {
            CheckPermisoAndRedirect(78);
            var model = dc.ExportBatch.Single(eb => eb.IdExportBatch == id);

            //Para Granotop y Saprosem los parámetros de exportación son distintos
            if (model.IdEmpresa == 3 || model.IdEmpresa == 4)
            {
                var items = CsvExportBatchRecepcionesEncabezadoEspecial.FromExportBatch(dc, model);
                return new CsvActionResult<CsvExportBatchRecepcionesEncabezadoEspecial>(items, String.Format("Encabezado_{1}_{0}.txt", model.IdExportBatch, model.Empresa.Nombre), 2, '\t');
            }
            else
            {
                var items = CsvExportBatchRecepcionesEncabezado.FromExportBatch(dc, model);
                return new CsvActionResult<CsvExportBatchRecepcionesEncabezado>(items, String.Format("Encabezado_{1}_{0}.txt", model.IdExportBatch, model.Empresa.Nombre), 2, '\t');
            }
        }

        public ActionResult ExportDetalle(int id)
        {
            CheckPermisoAndRedirect(78);
            var model = dc.ExportBatch.Single(eb => eb.IdExportBatch == id);

            //Para Granotop y Saprosem los parámetros de exportación son distintos
            if (model.IdEmpresa == 3 || model.IdEmpresa == 4)
            {
                var items = CsvExportBatchRecepcionesDetalleEspecial.FromExportBatch(dc, model);
                return new CsvActionResult<CsvExportBatchRecepcionesDetalleEspecial>(items, String.Format("Detalle_{1}_{0}.txt", model.IdExportBatch, model.Empresa.Nombre), 2, '\t');
            }
            else
            {
                var items = CsvExportBatchRecepcionesDetalle.FromExportBatch(dc, model);
                return new CsvActionResult<CsvExportBatchRecepcionesDetalle>(items, String.Format("Detalle_{1}_{0}.txt", model.IdExportBatch, model.Empresa.Nombre), 2, '\t');
            }
        }

        public JsonResult BatchRecepcionesSAP(int id)
        {
            CheckPermisoAndRedirect(78);
            var model = dc.ExportBatch.Single(eb => eb.IdExportBatch == id);

            //Para Granotop y Saprosem los parámetros de exportación son distintos
            if (model.IdEmpresa == 3 || model.IdEmpresa == 4)
            {
                var items = ExportBatchRecepcionesSAP.FromExportBatchSAP(dc, model);
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var items = ExportBatchRecepcionesSAP.FromExportBatchSAP(dc, model);
                return Json(items, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
