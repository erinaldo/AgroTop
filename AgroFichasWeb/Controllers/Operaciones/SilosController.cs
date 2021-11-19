using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class SilosController : BaseApplicationController
    {
        //
        // GET: /Silos/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public SilosController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(147);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OPR_Silo> items = dc.OPR_Silo.OrderBy(a => a.IdSilo);

            var pagina = new PaginatedList<OPR_Silo>(items, pageIndex, pageSize);

            ViewData["MensajeError"]       = Request["MensajeError"];
            ViewData["MensajeExito"]       = Request["MensajeExito"];
            ViewData["PuedeCrear"]         = CheckPermiso(148);
            ViewData["PuedeEditar"]        = CheckPermiso(149);
            ViewData["PuedeEliminar"]      = CheckPermiso(150);
            ViewData["PuedeVerDensidades"] = CheckPermiso(156);
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(148);

            var silo = new OPR_Silo();

            ViewData["densidadList"] = SetDensidad(null);
            ViewData["tipoSiloList"] = SetTipoSilo(null);
            return View("crear", silo);
        }

        [HttpPost]
        public ActionResult Crear(OPR_Silo silo)
        {
            CheckPermisoAndRedirect(148);
            if (ModelState.IsValid)
            {
                try
                {
                    silo.Habilitado = true;
                    silo.FechaHoraIns = DateTime.Now;
                    silo.IpIns = RemoteAddr();
                    silo.UserIns = User.Identity.Name;
                    dc.OPR_Silo.InsertOnSubmit(silo);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = silo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["densidadList"] = SetDensidad(silo.IdDensidad);
            ViewData["tipoSiloList"] = SetTipoSilo(silo.IdTipoSilo);
            return View("crear", silo);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(149);

            var silo = dc.OPR_Silo.SingleOrDefault(x => x.IdSilo == id);
            if (silo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el silo"); }

            ViewData["densidadList"] = SetDensidad(silo.IdDensidad);
            ViewData["tipoSiloList"] = SetTipoSilo(silo.IdTipoSilo);
            return View("crear", silo);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(149);

            var silo = dc.OPR_Silo.SingleOrDefault(x => x.IdSilo == id);
            if (silo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el silo"); }

            if (TryUpdateModel(silo, new string[] { "IdTipoSilo", "Descripcion", "AlturaBase", "AlturaCono", "IdDensidad", "Habilitado" }))
            {
                try
                {
                    silo.UserUpd = User.Identity.Name;
                    silo.FechaHoraUpd = DateTime.Now;
                    silo.IpUpd = RemoteAddr();

                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = silo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["densidadList"] = SetDensidad(silo.IdDensidad);
            ViewData["tipoSiloList"] = SetTipoSilo(silo.IdTipoSilo);
            return View("crear", silo);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(150);

            var silo = dc.OPR_Silo.SingleOrDefault(x => x.IdSilo == id);
            if (silo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el silo"); }

            string MensajeError = "";
            string MensajeExito = "";
            try
            {
                silo.Habilitado = false;
                silo.UserUpd = User.Identity.Name;
                silo.FechaHoraUpd = DateTime.Now;
                silo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("El silo {0} ha sido deshabilitado", silo.Descripcion);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }

        private IEnumerable<SelectListItem> SetDensidad(int? IdDensidad)
        {
            IEnumerable<SelectListItem> selectList =
                from X in dc.OPR_Densidad
                where X.Habilitado == true
                orderby X.OPR_TipoDensidad.Descripcion
                select new SelectListItem
                {
                    Selected = (X.IdDensidad == IdDensidad && IdDensidad != null),
                    Text = string.Format("{0:N3} ({1})", X.Valor, X.OPR_TipoDensidad.Descripcion),
                    Value = X.IdDensidad.ToString()
                };
            return selectList;
        }

        private IEnumerable<SelectListItem> SetTipoSilo(int? IdTipoSilo)
        {
            IEnumerable<SelectListItem> selectList =
                from X in dc.OPR_TipoSilo
                orderby X.Descripcion
                select new SelectListItem
                {
                    Selected = (X.IdTipoSilo == IdTipoSilo && IdTipoSilo != null),
                    Text = X.Descripcion,
                    Value = X.IdTipoSilo.ToString()
                };
            return selectList;
        }
    }
}
