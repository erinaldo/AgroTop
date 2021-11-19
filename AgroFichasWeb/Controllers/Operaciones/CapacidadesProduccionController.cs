using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class CapacidadesProduccionController : BaseApplicationController
    {
        //
        // GET: /CapacidadesProduccion/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public CapacidadesProduccionController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(174);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OPR_CapacidadLineaProduccion> items = dc.OPR_CapacidadLineaProduccion.Where(x => x.Habilitado == true).OrderBy(a => a.IdCapacidadLineaProduccion);

            var pagina = new PaginatedList<OPR_CapacidadLineaProduccion>(items, pageIndex, pageSize);

            ViewData["MensajeError"] = Request["MensajeError"];
            ViewData["MensajeExito"] = Request["MensajeExito"];
            ViewData["PuedeCrear"] = CheckPermiso(175);
            ViewData["PuedeEditar"] = CheckPermiso(176);
            ViewData["PuedeEliminar"] = CheckPermiso(177);

            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(175);
            var capacidadLineaProduccion = new OPR_CapacidadLineaProduccion();
            ViewData["lineasProduccionList"] = SetLineasProduccion(null);
            return View("crear", capacidadLineaProduccion);
        }

        [HttpPost]
        public ActionResult Crear(OPR_CapacidadLineaProduccion capacidadLineaProduccion)
        {
            CheckPermisoAndRedirect(175);
            if (ModelState.IsValid)
            {
                try
                {
                    var version = dc.OPR_Version.Single(X => X.Activa == true);
                    capacidadLineaProduccion.Habilitado = true;
                    capacidadLineaProduccion.FechaHoraIns = DateTime.Now;
                    capacidadLineaProduccion.IpIns = RemoteAddr();
                    capacidadLineaProduccion.UserIns = User.Identity.Name;
                    capacidadLineaProduccion.IdVersion = version.IdVersion;
                    dc.OPR_CapacidadLineaProduccion.InsertOnSubmit(capacidadLineaProduccion);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = capacidadLineaProduccion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["lineasProduccionList"] = SetLineasProduccion(capacidadLineaProduccion.IdLineaProduccion);
            return View("crear", capacidadLineaProduccion);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(176);
            var capacidadLineaProduccion = dc.OPR_CapacidadLineaProduccion.SingleOrDefault(x => x.IdCapacidadLineaProduccion == id && x.Habilitado == true);
            if (capacidadLineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la capacidad de la línea de producción"); }
            ViewData["lineasProduccionList"] = SetLineasProduccion(capacidadLineaProduccion.IdLineaProduccion);
            return View("crear", capacidadLineaProduccion);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(176);
            var capacidadLineaProduccion = dc.OPR_CapacidadLineaProduccion.SingleOrDefault(x => x.IdCapacidadLineaProduccion == id && x.Habilitado == true);
            if (capacidadLineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la capacidad de la línea de producción"); }

            try
            {
                UpdateModel(capacidadLineaProduccion, new string[] { "IdLineaProduccion", "Valor" });

                capacidadLineaProduccion.UserUpd = User.Identity.Name;
                capacidadLineaProduccion.FechaHoraUpd = DateTime.Now;
                capacidadLineaProduccion.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = capacidadLineaProduccion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["lineasProduccionList"] = SetLineasProduccion(capacidadLineaProduccion.IdLineaProduccion);
            return View("crear", capacidadLineaProduccion);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(177);
            var capacidadLineaProduccion = dc.OPR_CapacidadLineaProduccion.SingleOrDefault(x => x.IdCapacidadLineaProduccion == id && x.Habilitado == true);
            if (capacidadLineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la capacidad de la línea de producción"); }

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                capacidadLineaProduccion.Habilitado = false;
                capacidadLineaProduccion.UserUpd = User.Identity.Name;
                capacidadLineaProduccion.FechaHoraUpd = DateTime.Now;
                capacidadLineaProduccion.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("La capacidad de producción de la línea {0} ha sido eliminada", capacidadLineaProduccion.OPR_LineaProduccion.Descripcion);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }

        private IEnumerable<SelectListItem> SetLineasProduccion(int? IdLineaProduccion)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OPR_LineaProduccion
                                                     where X.Habilitado == true
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdLineaProduccion == IdLineaProduccion && IdLineaProduccion != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdLineaProduccion.ToString()
                                                     };
            return selectList;
        }
    }
}
