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
    public class CALFTControlVersionItemsController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTControlVersionItemsController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFTControlVersionItems
        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(332);
            List<CAL_FTControlVersionItem> list = dcSoftwareCalidad.CAL_FTControlVersionItem.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(332);
            CAL_FTControlVersionItem cAL_FTControlVersionItem = new CAL_FTControlVersionItem();
            return View("Crear", cAL_FTControlVersionItem);
        }

        [HttpPost]
        public ActionResult Crear(CAL_FTControlVersionItem cAL_FTControlVersionItem)
        {
            CheckPermisoAndRedirect(332);
            if (ModelState.IsValid)
            {
                try
                {
                    cAL_FTControlVersionItem.Habilitado = true;
                    cAL_FTControlVersionItem.FechaHoraIns = DateTime.Now;
                    cAL_FTControlVersionItem.IpIns = RemoteAddr();
                    cAL_FTControlVersionItem.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_FTControlVersionItem.InsertOnSubmit(cAL_FTControlVersionItem);
                    dcSoftwareCalidad.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = cAL_FTControlVersionItem.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", cAL_FTControlVersionItem);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(332);
            var cAL_FTControlVersionItem = dcSoftwareCalidad.CAL_FTControlVersionItem.SingleOrDefault(X => X.IdItem == id && X.Habilitado == true);
            if (cAL_FTControlVersionItem == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el ítem de control de versión", okMsg = "" }); }
            return View("crear", cAL_FTControlVersionItem);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(332);
            var cAL_FTControlVersionItem = dcSoftwareCalidad.CAL_FTControlVersionItem.SingleOrDefault(X => X.IdItem == id && X.Habilitado == true);
            if (cAL_FTControlVersionItem == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el ítem de control de versión", okMsg = "" }); }

            try
            {
                UpdateModel(cAL_FTControlVersionItem, new string[] { "Nombre" });
                cAL_FTControlVersionItem.UserUpd = User.Identity.Name;
                cAL_FTControlVersionItem.FechaHoraUpd = DateTime.Now;
                cAL_FTControlVersionItem.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = cAL_FTControlVersionItem.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", cAL_FTControlVersionItem);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(332);
            var cAL_FTControlVersionItem = dcSoftwareCalidad.CAL_FTControlVersionItem.SingleOrDefault(X => X.IdItem == id && X.Habilitado == true);
            if (cAL_FTControlVersionItem == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el ítem de control de versión", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cAL_FTControlVersionItem.Habilitado = false;
                cAL_FTControlVersionItem.UserUpd = User.Identity.Name;
                cAL_FTControlVersionItem.FechaHoraUpd = DateTime.Now;
                cAL_FTControlVersionItem.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El ítem de control de versión {0} ha sido eliminado", cAL_FTControlVersionItem.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}