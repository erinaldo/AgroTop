using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class TipoDensidadesController : BaseApplicationController
    {
        //
        // GET: /TipoDensidades/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public TipoDensidadesController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(160);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OPR_TipoDensidad> items = dc.OPR_TipoDensidad.Where(x => x.Habilitado == true).OrderBy(a => a.IdTipoDensidad);
            var pagina = new PaginatedList<OPR_TipoDensidad>(items, pageIndex, pageSize);

            ViewData["MensajeError"]  = Request["MensajeError"];
            ViewData["MensajeExito"]  = Request["MensajeExito"];
            ViewData["PuedeCrear"]    = CheckPermiso(161);
            ViewData["PuedeEditar"]   = CheckPermiso(162);
            ViewData["PuedeEliminar"] = CheckPermiso(163);
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(161);
            var tipoDensidad = new OPR_TipoDensidad();
            return View("crear", tipoDensidad);
        }

        [HttpPost]
        public ActionResult Crear(OPR_TipoDensidad tipoDensidad)
        {
            CheckPermisoAndRedirect(161);
            if (ModelState.IsValid)
            {
                try
                {
                    tipoDensidad.Sigla = "NA";
                    tipoDensidad.Habilitado = true;
                    tipoDensidad.FechaHoraIns = DateTime.Now;
                    tipoDensidad.IpIns = RemoteAddr();
                    tipoDensidad.UserIns = User.Identity.Name;
                    dc.OPR_TipoDensidad.InsertOnSubmit(tipoDensidad);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = tipoDensidad.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", tipoDensidad);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(162);
            var tipoDensidad = dc.OPR_TipoDensidad.SingleOrDefault(x => x.IdTipoDensidad == id && x.Habilitado == true);
            if (tipoDensidad == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el tipo de densidad"); }
            return View("crear", tipoDensidad);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(162);
            var tipoDensidad = dc.OPR_TipoDensidad.SingleOrDefault(x => x.IdTipoDensidad == id && x.Habilitado == true);
            if (tipoDensidad == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el tipo de densidad"); }

            try
            {
                UpdateModel(tipoDensidad, new string[] { "Descripcion" });

                tipoDensidad.UserUpd = User.Identity.Name;
                tipoDensidad.FechaHoraUpd = DateTime.Now;
                tipoDensidad.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = tipoDensidad.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", tipoDensidad);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(163);
            var tipoDensidad = dc.OPR_TipoDensidad.SingleOrDefault(x => x.IdTipoDensidad == id && x.Habilitado == true);
            if (tipoDensidad == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el tipo de densidad"); }

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                tipoDensidad.Habilitado = false;
                tipoDensidad.UserUpd = User.Identity.Name;
                tipoDensidad.FechaHoraUpd = DateTime.Now;
                tipoDensidad.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("El tipo de densidad {0} ha sido eliminado", tipoDensidad.Descripcion);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }
    }
}
