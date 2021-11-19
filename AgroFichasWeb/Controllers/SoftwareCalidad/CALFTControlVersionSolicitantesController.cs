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
    public class CALFTControlVersionSolicitantesController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTControlVersionSolicitantesController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFTControlVersionSolicitantes
        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(334);
            List<CAL_FTControlVersionSolicitante> list = dcSoftwareCalidad.CAL_FTControlVersionSolicitante.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(334);
            CAL_FTControlVersionSolicitante cAL_FTControlVersionSolicitante = new CAL_FTControlVersionSolicitante();
            return View("Crear", cAL_FTControlVersionSolicitante);
        }

        [HttpPost]
        public ActionResult Crear(CAL_FTControlVersionSolicitante cAL_FTControlVersionSolicitante)
        {
            CheckPermisoAndRedirect(334);
            if (ModelState.IsValid)
            {
                try
                {
                    cAL_FTControlVersionSolicitante.Habilitado = true;
                    cAL_FTControlVersionSolicitante.FechaHoraIns = DateTime.Now;
                    cAL_FTControlVersionSolicitante.IpIns = RemoteAddr();
                    cAL_FTControlVersionSolicitante.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_FTControlVersionSolicitante.InsertOnSubmit(cAL_FTControlVersionSolicitante);
                    dcSoftwareCalidad.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = cAL_FTControlVersionSolicitante.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", cAL_FTControlVersionSolicitante);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(334);
            var cAL_FTControlVersionSolicitante = dcSoftwareCalidad.CAL_FTControlVersionSolicitante.SingleOrDefault(X => X.IdSolicitante == id && X.Habilitado == true);
            if (cAL_FTControlVersionSolicitante == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el solicitante de cambio de control de versión", okMsg = "" }); }
            return View("Crear", cAL_FTControlVersionSolicitante);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(334);
            var cAL_FTControlVersionSolicitante = dcSoftwareCalidad.CAL_FTControlVersionSolicitante.SingleOrDefault(X => X.IdSolicitante == id && X.Habilitado == true);
            if (cAL_FTControlVersionSolicitante == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el solicitante de cambio de control de versión", okMsg = "" }); }

            try
            {
                UpdateModel(cAL_FTControlVersionSolicitante, new string[] { "Nombre" });
                cAL_FTControlVersionSolicitante.UserUpd = User.Identity.Name;
                cAL_FTControlVersionSolicitante.FechaHoraUpd = DateTime.Now;
                cAL_FTControlVersionSolicitante.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = cAL_FTControlVersionSolicitante.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", cAL_FTControlVersionSolicitante);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(334);
            var cAL_FTControlVersionSolicitante = dcSoftwareCalidad.CAL_FTControlVersionSolicitante.SingleOrDefault(X => X.IdSolicitante == id && X.Habilitado == true);
            if (cAL_FTControlVersionSolicitante == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el solicitante de cambio de control de versión", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cAL_FTControlVersionSolicitante.Habilitado = false;
                cAL_FTControlVersionSolicitante.UserUpd = User.Identity.Name;
                cAL_FTControlVersionSolicitante.FechaHoraUpd = DateTime.Now;
                cAL_FTControlVersionSolicitante.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El solicitante de cambio de control de versión {0} ha sido eliminado", cAL_FTControlVersionSolicitante.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}