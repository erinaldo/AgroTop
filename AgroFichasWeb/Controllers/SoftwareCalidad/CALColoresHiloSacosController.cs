using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALColoresHiloSacosController : BaseApplicationController
    {
        //
        // GET: /CALColoresHiloSacos/

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALColoresHiloSacosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(247);
            List<CAL_TipoColorHiloSaco> list = dc.CAL_TipoColorHiloSaco.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(248),
                                                      CheckPermiso(247),
                                                      CheckPermiso(249),
                                                      CheckPermiso(250));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(248);
            CAL_TipoColorHiloSaco colorHilo = new CAL_TipoColorHiloSaco();
            return View("crear", colorHilo);
        }

        [HttpPost]
        public ActionResult Crear(CAL_TipoColorHiloSaco colorHilo)
        {
            CheckPermisoAndRedirect(248);
            if (ModelState.IsValid)
            {
                try
                {
                    colorHilo.Nombre = colorHilo.Nombre.ToUpperInvariant();

                    colorHilo.Habilitado = true;
                    colorHilo.FechaHoraIns = DateTime.Now;
                    colorHilo.IpIns = RemoteAddr();
                    colorHilo.UserIns = User.Identity.Name;
                    dc.CAL_TipoColorHiloSaco.InsertOnSubmit(colorHilo);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = colorHilo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", colorHilo);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(249);
            var colorHilo = dc.CAL_TipoColorHiloSaco.SingleOrDefault(X => X.IdTipoColorHiloSaco == id && X.Habilitado == true);
            if (colorHilo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el color del hilo del saco", okMsg = "" }); }
            return View("crear", colorHilo);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(249);
            var colorHilo = dc.CAL_TipoColorHiloSaco.SingleOrDefault(X => X.IdTipoColorHiloSaco == id && X.Habilitado == true);
            if (colorHilo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el color del hilo del saco", okMsg = "" }); }

            try
            {
                UpdateModel(colorHilo, new string[] { "Nombre" });

                colorHilo.Nombre = colorHilo.Nombre.ToUpperInvariant();

                colorHilo.UserUpd = User.Identity.Name;
                colorHilo.FechaHoraUpd = DateTime.Now;
                colorHilo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = colorHilo.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", colorHilo);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(250);
            var colorHilo = dc.CAL_TipoColorHiloSaco.SingleOrDefault(X => X.IdTipoColorHiloSaco == id && X.Habilitado == true);
            if (colorHilo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el color del hilo del saco", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                colorHilo.Habilitado = false;
                colorHilo.UserUpd = User.Identity.Name;
                colorHilo.FechaHoraUpd = DateTime.Now;
                colorHilo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El color del hilo del saco {0} ha sido eliminado", colorHilo.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}