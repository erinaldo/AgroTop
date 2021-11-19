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
    public class CALFTControlVersionMotivosController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTControlVersionMotivosController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFTControlVersionMotivos
        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(333);
            List<CAL_FTControlVersionMotivo> list = dcSoftwareCalidad.CAL_FTControlVersionMotivo.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(333);
            CAL_FTControlVersionMotivo cAL_FTControlVersionMotivo = new CAL_FTControlVersionMotivo();
            return View("Crear", cAL_FTControlVersionMotivo);
        }

        [HttpPost]
        public ActionResult Crear(CAL_FTControlVersionMotivo cAL_FTControlVersionMotivo)
        {
            CheckPermisoAndRedirect(333);
            if (ModelState.IsValid)
            {
                try
                {
                    cAL_FTControlVersionMotivo.Habilitado = true;
                    cAL_FTControlVersionMotivo.FechaHoraIns = DateTime.Now;
                    cAL_FTControlVersionMotivo.IpIns = RemoteAddr();
                    cAL_FTControlVersionMotivo.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_FTControlVersionMotivo.InsertOnSubmit(cAL_FTControlVersionMotivo);
                    dcSoftwareCalidad.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = cAL_FTControlVersionMotivo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", cAL_FTControlVersionMotivo);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(333);
            var cAL_FTControlVersionMotivo = dcSoftwareCalidad.CAL_FTControlVersionMotivo.SingleOrDefault(X => X.IdMotivo == id && X.Habilitado == true);
            if (cAL_FTControlVersionMotivo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el motivo de cambio de control de versión", okMsg = "" }); }
            return View("Crear", cAL_FTControlVersionMotivo);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(333);
            var cAL_FTControlVersionMotivo = dcSoftwareCalidad.CAL_FTControlVersionMotivo.SingleOrDefault(X => X.IdMotivo == id && X.Habilitado == true);
            if (cAL_FTControlVersionMotivo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el motivo de cambio de control de versión", okMsg = "" }); }

            try
            {
                UpdateModel(cAL_FTControlVersionMotivo, new string[] { "Descripcion" });
                cAL_FTControlVersionMotivo.UserUpd = User.Identity.Name;
                cAL_FTControlVersionMotivo.FechaHoraUpd = DateTime.Now;
                cAL_FTControlVersionMotivo.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = cAL_FTControlVersionMotivo.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", cAL_FTControlVersionMotivo);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(333);
            var cAL_FTControlVersionMotivo = dcSoftwareCalidad.CAL_FTControlVersionMotivo.SingleOrDefault(X => X.IdMotivo == id && X.Habilitado == true);
            if (cAL_FTControlVersionMotivo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el motivo de cambio de control de versión", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cAL_FTControlVersionMotivo.Habilitado = false;
                cAL_FTControlVersionMotivo.UserUpd = User.Identity.Name;
                cAL_FTControlVersionMotivo.FechaHoraUpd = DateTime.Now;
                cAL_FTControlVersionMotivo.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El motivo de cambio de control de versión {0} ha sido eliminado", cAL_FTControlVersionMotivo.Descripcion);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}