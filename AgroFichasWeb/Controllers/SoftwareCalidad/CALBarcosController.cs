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
    public class CALBarcosController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALBarcosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(289);
            List<Barco> list = dc.Barco.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(289),
                                                      CheckPermiso(290),
                                                      CheckPermiso(291),
                                                      CheckPermiso(292));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(290);
            Barco barco = new Barco();
            return View("Crear", barco);
        }

        [HttpPost]
        public ActionResult Crear(Barco barco)
        {
            CheckPermisoAndRedirect(290);
            if (ModelState.IsValid)
            {
                try
                {
                    barco.Nombre = barco.Nombre.ToUpperInvariant();

                    barco.Habilitado = true;
                    barco.FechaHoraIns = DateTime.Now;
                    barco.IpIns = RemoteAddr();
                    barco.UserIns = User.Identity.Name;
                    dc.Barco.InsertOnSubmit(barco);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = barco.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", barco);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(291);

            Barco barco = dc.Barco.SingleOrDefault(X => X.IdBarco == id && X.Habilitado == true);
            if (barco == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el barco", okMsg = "" });
            }

            return View("Crear", barco);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(291);
            var barco = dc.Barco.SingleOrDefault(X => X.IdBarco == id && X.Habilitado == true);
            if (barco == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el barco", okMsg = "" }); }

            try
            {
                UpdateModel(barco, new string[] { "Nombre", "IdCarrier" });

                barco.Nombre = barco.Nombre.ToUpperInvariant();

                barco.UserUpd = User.Identity.Name;
                barco.FechaHoraUpd = DateTime.Now;
                barco.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = barco.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", barco);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(292);
            var barco = dc.Barco.SingleOrDefault(X => X.IdBarco == id && X.Habilitado == true);
            if (barco == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el barco", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                barco.Habilitado = false;
                barco.UserUpd = User.Identity.Name;
                barco.FechaHoraUpd = DateTime.Now;
                barco.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El barco {0} ha sido eliminado", barco.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}
