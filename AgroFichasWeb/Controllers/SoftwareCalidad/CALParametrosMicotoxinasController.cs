using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALParametrosMicotoxinasController : BaseApplicationController
    {
        // GET: CALParametrosMicotoxinas

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALParametrosMicotoxinasController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(344);
            List<CAL_ParametroMicotoxina> list = dc.CAL_ParametroMicotoxina.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(345),
                                                      CheckPermiso(344),
                                                      CheckPermiso(346),
                                                      CheckPermiso(347));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(345);
            CAL_ParametroMicotoxina parametroMicotoxina = new CAL_ParametroMicotoxina();
            return View("Crear", parametroMicotoxina);
        }

        [HttpPost]
        public ActionResult Crear(CAL_ParametroMicotoxina parametroMicotoxina)
        {
            CheckPermisoAndRedirect(345);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(parametroMicotoxina.NombreCorto))
                        parametroMicotoxina.NombreCorto = string.Empty;
                    if (string.IsNullOrEmpty(parametroMicotoxina.FormatString))
                        parametroMicotoxina.FormatString = string.Empty;

                    //Parámetros (en)
                    if (string.IsNullOrEmpty(parametroMicotoxina.Nombre_en))
                        parametroMicotoxina.Nombre_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroMicotoxina.NombreCorto_en))
                        parametroMicotoxina.NombreCorto_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroMicotoxina.FormatString_en))
                        parametroMicotoxina.FormatString_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroMicotoxina.UM_en))
                        parametroMicotoxina.UM_en = string.Empty;

                    parametroMicotoxina.Habilitado = true;
                    parametroMicotoxina.FechaHoraIns = DateTime.Now;
                    parametroMicotoxina.IpIns = RemoteAddr();
                    parametroMicotoxina.UserIns = User.Identity.Name;
                    dc.CAL_ParametroMicotoxina.InsertOnSubmit(parametroMicotoxina);
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = parametroMicotoxina.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", parametroMicotoxina);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(346);
            CAL_ParametroMicotoxina parametroMicotoxina = dc.CAL_ParametroMicotoxina.SingleOrDefault(X => X.IdParametroMicotoxina == id && X.Habilitado == true);
            if (parametroMicotoxina == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de micotoxina", okMsg = "" }); }

            return View("Crear", parametroMicotoxina);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(346);
            CAL_ParametroMicotoxina parametroMicotoxina = dc.CAL_ParametroMicotoxina.SingleOrDefault(X => X.IdParametroMicotoxina == id && X.Habilitado == true);
            if (parametroMicotoxina == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de micotoxina", okMsg = "" }); }

            try
            {
                UpdateModel(parametroMicotoxina, new string[] { "Nombre", "NombreCorto", "FormatString", "UM", "Decimales", "Nombre_en", "NombreCorto_en", "FormatString_en", "UM_en", "NombreCorto", "NombreCorto_en" });

                if (string.IsNullOrEmpty(parametroMicotoxina.NombreCorto))
                    parametroMicotoxina.NombreCorto = string.Empty;
                if (string.IsNullOrEmpty(parametroMicotoxina.FormatString))
                    parametroMicotoxina.FormatString = string.Empty;

                //Parámetros (en)
                if (string.IsNullOrEmpty(parametroMicotoxina.Nombre_en))
                    parametroMicotoxina.Nombre_en = string.Empty;
                if (string.IsNullOrEmpty(parametroMicotoxina.NombreCorto_en))
                    parametroMicotoxina.NombreCorto_en = string.Empty;
                if (string.IsNullOrEmpty(parametroMicotoxina.FormatString_en))
                    parametroMicotoxina.FormatString_en = string.Empty;
                if (string.IsNullOrEmpty(parametroMicotoxina.UM_en))
                    parametroMicotoxina.UM_en = string.Empty;

                parametroMicotoxina.UserUpd = User.Identity.Name;
                parametroMicotoxina.FechaHoraUpd = DateTime.Now;
                parametroMicotoxina.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = parametroMicotoxina.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", parametroMicotoxina);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(347);
            CAL_ParametroMicotoxina parametroMicotoxina = dc.CAL_ParametroMicotoxina.SingleOrDefault(X => X.IdParametroMicotoxina == id && X.Habilitado == true);
            if (parametroMicotoxina == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de micotoxina", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                parametroMicotoxina.Habilitado = false;
                parametroMicotoxina.UserUpd = User.Identity.Name;
                parametroMicotoxina.FechaHoraUpd = DateTime.Now;
                parametroMicotoxina.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El parámetro de micotoxina {0} ha sido eliminado", parametroMicotoxina.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}