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
    public class CALParametrosAnalisisController : BaseApplicationController
    {
        // GET: CALParametrosAnalisis

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALParametrosAnalisisController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(319);
            List<CAL_ParametroAnalisis> list = dc.CAL_ParametroAnalisis.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(320),
                                                      CheckPermiso(319),
                                                      CheckPermiso(321),
                                                      CheckPermiso(322));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(320);
            CAL_ParametroAnalisis parametroAnalisis = new CAL_ParametroAnalisis();
            return View("Crear", parametroAnalisis);
        }

        [HttpPost]
        public ActionResult Crear(CAL_ParametroAnalisis parametroAnalisis)
        {
            CheckPermisoAndRedirect(320);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(parametroAnalisis.NombreCorto))
                        parametroAnalisis.NombreCorto = string.Empty;
                    if (string.IsNullOrEmpty(parametroAnalisis.FormatString))
                        parametroAnalisis.FormatString = string.Empty;

                    //Parámetros (en)
                    if (string.IsNullOrEmpty(parametroAnalisis.Nombre_en))
                        parametroAnalisis.Nombre_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroAnalisis.NombreCorto_en))
                        parametroAnalisis.NombreCorto_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroAnalisis.FormatString_en))
                        parametroAnalisis.FormatString_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroAnalisis.UM_en))
                        parametroAnalisis.UM_en = string.Empty;

                    parametroAnalisis.Habilitado = true;
                    parametroAnalisis.FechaHoraIns = DateTime.Now;
                    parametroAnalisis.IpIns = RemoteAddr();
                    parametroAnalisis.UserIns = User.Identity.Name;
                    dc.CAL_ParametroAnalisis.InsertOnSubmit(parametroAnalisis);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = parametroAnalisis.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", parametroAnalisis);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(321);
            CAL_ParametroAnalisis parametroAnalisis = dc.CAL_ParametroAnalisis.SingleOrDefault(X => X.IdParametroAnalisis == id && X.Habilitado == true);
            if (parametroAnalisis == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de análisis", okMsg = "" }); }

            return View("Crear", parametroAnalisis);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(321);
            CAL_ParametroAnalisis parametroAnalisis = dc.CAL_ParametroAnalisis.SingleOrDefault(X => X.IdParametroAnalisis == id && X.Habilitado == true);
            if (parametroAnalisis == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de análisis", okMsg = "" }); }

            try
            {
                UpdateModel(parametroAnalisis, new string[] { "Nombre", "NombreCorto", "FormatString", "UM", "Decimales", "Nombre_en", "NombreCorto_en", "FormatString_en", "UM_en" });

                if (string.IsNullOrEmpty(parametroAnalisis.NombreCorto))
                    parametroAnalisis.NombreCorto = string.Empty;
                if (string.IsNullOrEmpty(parametroAnalisis.FormatString))
                    parametroAnalisis.FormatString = string.Empty;

                //Parámetros (en)
                if (string.IsNullOrEmpty(parametroAnalisis.Nombre_en))
                    parametroAnalisis.Nombre_en = string.Empty;
                if (string.IsNullOrEmpty(parametroAnalisis.NombreCorto_en))
                    parametroAnalisis.NombreCorto_en = string.Empty;
                if (string.IsNullOrEmpty(parametroAnalisis.FormatString_en))
                    parametroAnalisis.FormatString_en = string.Empty;
                if (string.IsNullOrEmpty(parametroAnalisis.UM_en))
                    parametroAnalisis.UM_en = string.Empty;

                parametroAnalisis.UserUpd = User.Identity.Name;
                parametroAnalisis.FechaHoraUpd = DateTime.Now;
                parametroAnalisis.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = parametroAnalisis.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", parametroAnalisis);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(322);
            CAL_ParametroAnalisis parametroAnalisis = dc.CAL_ParametroAnalisis.SingleOrDefault(X => X.IdParametroAnalisis == id && X.Habilitado == true);
            if (parametroAnalisis == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de análisis", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                parametroAnalisis.Habilitado = false;
                parametroAnalisis.UserUpd = User.Identity.Name;
                parametroAnalisis.FechaHoraUpd = DateTime.Now;
                parametroAnalisis.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El parámetro de análisis {0} ha sido eliminado", parametroAnalisis.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}