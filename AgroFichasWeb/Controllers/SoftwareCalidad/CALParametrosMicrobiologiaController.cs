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
    public class CALParametrosMicrobiologiaController : BaseApplicationController 
    {
        // GET: CALParametrosMicrobiologia

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALParametrosMicrobiologiaController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(348);
            List<CAL_ParametroMicrobiologia> list = dc.CAL_ParametroMicrobiologia.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(349),
                                                      CheckPermiso(348),
                                                      CheckPermiso(350),
                                                      CheckPermiso(351));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(349);
            CAL_ParametroMicrobiologia parametroMicrobiologia = new CAL_ParametroMicrobiologia();
            return View("Crear", parametroMicrobiologia);
        }

        [HttpPost]
        public ActionResult Crear(CAL_ParametroMicrobiologia parametroMicrobiologia)
        {
            CheckPermisoAndRedirect(349);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(parametroMicrobiologia.FormatString))
                        parametroMicrobiologia.FormatString = string.Empty;

                    //Parámetros (en)
                    if (string.IsNullOrEmpty(parametroMicrobiologia.Nombre_en))
                        parametroMicrobiologia.Nombre_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroMicrobiologia.FormatString_en))
                        parametroMicrobiologia.FormatString_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroMicrobiologia.UM_en))
                        parametroMicrobiologia.UM_en = string.Empty;

                    parametroMicrobiologia.Habilitado = true;
                    parametroMicrobiologia.FechaHoraIns = DateTime.Now;
                    parametroMicrobiologia.IpIns = RemoteAddr();
                    parametroMicrobiologia.UserIns = User.Identity.Name;
                    dc.CAL_ParametroMicrobiologia.InsertOnSubmit(parametroMicrobiologia);
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = parametroMicrobiologia.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", parametroMicrobiologia);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(350);
            CAL_ParametroMicrobiologia parametroMicrobiologia = dc.CAL_ParametroMicrobiologia.SingleOrDefault(X => X.IdParametroMicrobiologia == id && X.Habilitado == true);
            if (parametroMicrobiologia == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de microbiología", okMsg = "" }); }

            return View("Crear", parametroMicrobiologia);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(350);
            CAL_ParametroMicrobiologia parametroMicrobiologia = dc.CAL_ParametroMicrobiologia.SingleOrDefault(X => X.IdParametroMicrobiologia == id && X.Habilitado == true);
            if (parametroMicrobiologia == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de microbiología", okMsg = "" }); }

            try
            {
                UpdateModel(parametroMicrobiologia, new string[] { "Nombre", "NombreCorto", "FormatString", "UM", "Decimales", "Nombre_en", "NombreCorto_en", "FormatString_en", "UM_en" });

                if (string.IsNullOrEmpty(parametroMicrobiologia.FormatString))
                    parametroMicrobiologia.FormatString = string.Empty;

                //Parámetros (en)
                if (string.IsNullOrEmpty(parametroMicrobiologia.Nombre_en))
                    parametroMicrobiologia.Nombre_en = string.Empty;
                if (string.IsNullOrEmpty(parametroMicrobiologia.FormatString_en))
                    parametroMicrobiologia.FormatString_en = string.Empty;
                if (string.IsNullOrEmpty(parametroMicrobiologia.UM_en))
                    parametroMicrobiologia.UM_en = string.Empty;

                parametroMicrobiologia.UserUpd = User.Identity.Name;
                parametroMicrobiologia.FechaHoraUpd = DateTime.Now;
                parametroMicrobiologia.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = parametroMicrobiologia.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", parametroMicrobiologia);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(351);
            CAL_ParametroMicrobiologia parametroMicrobiologia = dc.CAL_ParametroMicrobiologia.SingleOrDefault(X => X.IdParametroMicrobiologia == id && X.Habilitado == true);
            if (parametroMicrobiologia == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de microbiología", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                parametroMicrobiologia.Habilitado = false;
                parametroMicrobiologia.UserUpd = User.Identity.Name;
                parametroMicrobiologia.FechaHoraUpd = DateTime.Now;
                parametroMicrobiologia.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El parámetro de microbiología {0} ha sido eliminado", parametroMicrobiologia.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}