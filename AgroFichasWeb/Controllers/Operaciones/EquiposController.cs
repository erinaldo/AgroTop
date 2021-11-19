using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class EquiposController : BaseApplicationController
    {
        //
        // GET: /Equipos/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public EquiposController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(182);
            IQueryable<OPR_Equipo> items = dc.OPR_Equipo.Where(x => x.Habilitado == true).OrderBy(a => a.IdEquipo);
            var pagina = new List<OPR_Equipo>(items);

            ViewData["MensajeError"]  = Request["MensajeError"];
            ViewData["MensajeExito"]  = Request["MensajeExito"];
            ViewData["PuedeCrear"]    = CheckPermiso(183);
            ViewData["PuedeEditar"]   = CheckPermiso(184);
            ViewData["PuedeEliminar"] = CheckPermiso(185);
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(183);
            var equipo = new OPR_Equipo();
            return View("crear", equipo);
        }

        [HttpPost]
        public ActionResult Crear(OPR_Equipo equipo)
        {
            CheckPermisoAndRedirect(183);
            if (ModelState.IsValid)
            {
                try
                {
                    equipo.Habilitado = true;
                    equipo.FechaHoraIns = DateTime.Now;
                    equipo.IpIns = RemoteAddr();
                    equipo.UserIns = User.Identity.Name;
                    dc.OPR_Equipo.InsertOnSubmit(equipo);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = equipo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", equipo);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(184);
            var equipo = dc.OPR_Equipo.SingleOrDefault(x => x.IdEquipo == id && x.Habilitado == true);
            if (equipo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el equipo"); }
            return View("crear", equipo);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(184);
            var equipo = dc.OPR_Equipo.SingleOrDefault(x => x.IdEquipo == id && x.Habilitado == true);
            if (equipo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el equipo"); }

            try
            {
                UpdateModel(equipo, new string[] { "Descripcion" });

                equipo.UserUpd = User.Identity.Name;
                equipo.FechaHoraUpd = DateTime.Now;
                equipo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = equipo.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", equipo);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(185);
            var equipo = dc.OPR_Equipo.SingleOrDefault(x => x.IdEquipo == id && x.Habilitado == true);
            if (equipo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el equipo"); }

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                equipo.Habilitado = false;
                equipo.UserUpd = User.Identity.Name;
                equipo.FechaHoraUpd = DateTime.Now;
                equipo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("El equipo {0} ha sido eliminado", equipo.Descripcion);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }
    }
}
