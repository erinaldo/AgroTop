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
    public class CALParametrosNutricionalesController : BaseApplicationController
    {
        // GET: CALParametrosNutricionales

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALParametrosNutricionalesController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(352);
            List<CAL_ParametroNutricional> list = dc.CAL_ParametroNutricional.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(353),
                                                      CheckPermiso(352),
                                                      CheckPermiso(354),
                                                      CheckPermiso(355));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(353);
            CAL_ParametroNutricional parametroNutricional = new CAL_ParametroNutricional();
            return View("Crear", parametroNutricional);
        }

        [HttpPost]
        public ActionResult Crear(CAL_ParametroNutricional parametroNutricional)
        {
            CheckPermisoAndRedirect(353);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(parametroNutricional.FormatString))
                        parametroNutricional.FormatString = string.Empty;

                    //Parámetros (en)
                    if (string.IsNullOrEmpty(parametroNutricional.Nombre_en))
                        parametroNutricional.Nombre_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroNutricional.FormatString_en))
                        parametroNutricional.FormatString_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroNutricional.UM_en))
                        parametroNutricional.UM_en = string.Empty;

                    parametroNutricional.Habilitado = true;
                    parametroNutricional.FechaHoraIns = DateTime.Now;
                    parametroNutricional.IpIns = RemoteAddr();
                    parametroNutricional.UserIns = User.Identity.Name;
                    dc.CAL_ParametroNutricional.InsertOnSubmit(parametroNutricional);
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = parametroNutricional.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", parametroNutricional);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(354);
            CAL_ParametroNutricional parametroNutricional = dc.CAL_ParametroNutricional.SingleOrDefault(X => X.IdParametroNutricional == id && X.Habilitado == true);
            if (parametroNutricional == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de nutricional", okMsg = "" }); }

            return View("Crear", parametroNutricional);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(354);
            CAL_ParametroNutricional parametroNutricional = dc.CAL_ParametroNutricional.SingleOrDefault(X => X.IdParametroNutricional == id && X.Habilitado == true);
            if (parametroNutricional == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de nutricional", okMsg = "" }); }

            try
            {
                UpdateModel(parametroNutricional, new string[] { "Nombre", "NombreCorto", "FormatString", "UM", "Decimales", "Nombre_en", "NombreCorto_en", "FormatString_en", "UM_en" });

                if (string.IsNullOrEmpty(parametroNutricional.FormatString))
                    parametroNutricional.FormatString = string.Empty;

                //Parámetros (en)
                if (string.IsNullOrEmpty(parametroNutricional.Nombre_en))
                    parametroNutricional.Nombre_en = string.Empty;
                if (string.IsNullOrEmpty(parametroNutricional.FormatString_en))
                    parametroNutricional.FormatString_en = string.Empty;
                if (string.IsNullOrEmpty(parametroNutricional.UM_en))
                    parametroNutricional.UM_en = string.Empty;

                parametroNutricional.UserUpd = User.Identity.Name;
                parametroNutricional.FechaHoraUpd = DateTime.Now;
                parametroNutricional.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = parametroNutricional.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", parametroNutricional);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(355);
            CAL_ParametroNutricional parametroNutricional = dc.CAL_ParametroNutricional.SingleOrDefault(X => X.IdParametroNutricional == id && X.Habilitado == true);
            if (parametroNutricional == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de nutricional", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                parametroNutricional.Habilitado = false;
                parametroNutricional.UserUpd = User.Identity.Name;
                parametroNutricional.FechaHoraUpd = DateTime.Now;
                parametroNutricional.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El parámetro de nutricional {0} ha sido eliminado", parametroNutricional.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}