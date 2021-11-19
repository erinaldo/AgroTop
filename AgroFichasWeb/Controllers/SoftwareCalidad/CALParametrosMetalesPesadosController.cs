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
    public class CALParametrosMetalesPesadosController : BaseApplicationController
    {
        // GET: CALParametrosMetalesPesados

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALParametrosMetalesPesadosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(340);
            List<CAL_ParametroMetalPesado> list = dc.CAL_ParametroMetalPesado.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(341),
                                                      CheckPermiso(340),
                                                      CheckPermiso(342),
                                                      CheckPermiso(343));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(341);
            CAL_ParametroMetalPesado parametroMetalPesado = new CAL_ParametroMetalPesado();
            return View("Crear", parametroMetalPesado);
        }

        [HttpPost]
        public ActionResult Crear(CAL_ParametroMetalPesado parametroMetalPesado)
        {
            CheckPermisoAndRedirect(341);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(parametroMetalPesado.FormatString))
                        parametroMetalPesado.FormatString = string.Empty;

                    //Parámetros (en)
                    if (string.IsNullOrEmpty(parametroMetalPesado.Nombre_en))
                        parametroMetalPesado.Nombre_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroMetalPesado.FormatString_en))
                        parametroMetalPesado.FormatString_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroMetalPesado.UM_en))
                        parametroMetalPesado.UM_en = string.Empty;

                    parametroMetalPesado.Habilitado = true;
                    parametroMetalPesado.FechaHoraIns = DateTime.Now;
                    parametroMetalPesado.IpIns = RemoteAddr();
                    parametroMetalPesado.UserIns = User.Identity.Name;
                    dc.CAL_ParametroMetalPesado.InsertOnSubmit(parametroMetalPesado);
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = parametroMetalPesado.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", parametroMetalPesado);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(342);
            CAL_ParametroMetalPesado parametroMetalPesado = dc.CAL_ParametroMetalPesado.SingleOrDefault(X => X.IdParametroMetalPesado == id && X.Habilitado == true);
            if (parametroMetalPesado == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de metal pesado", okMsg = "" }); }

            return View("Crear", parametroMetalPesado);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(342);
            CAL_ParametroMetalPesado parametroMetalPesado = dc.CAL_ParametroMetalPesado.SingleOrDefault(X => X.IdParametroMetalPesado == id && X.Habilitado == true);
            if (parametroMetalPesado == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de metal pesado", okMsg = "" }); }

            try
            {
                UpdateModel(parametroMetalPesado, new string[] { "Nombre", "NombreCorto", "FormatString", "UM", "Decimales", "Nombre_en", "NombreCorto_en", "FormatString_en", "UM_en" });

                if (string.IsNullOrEmpty(parametroMetalPesado.FormatString))
                    parametroMetalPesado.FormatString = string.Empty;

                //Parámetros (en)
                if (string.IsNullOrEmpty(parametroMetalPesado.Nombre_en))
                    parametroMetalPesado.Nombre_en = string.Empty;
                if (string.IsNullOrEmpty(parametroMetalPesado.FormatString_en))
                    parametroMetalPesado.FormatString_en = string.Empty;
                if (string.IsNullOrEmpty(parametroMetalPesado.UM_en))
                    parametroMetalPesado.UM_en = string.Empty;

                parametroMetalPesado.UserUpd = User.Identity.Name;
                parametroMetalPesado.FechaHoraUpd = DateTime.Now;
                parametroMetalPesado.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = parametroMetalPesado.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", parametroMetalPesado);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(343);
            CAL_ParametroMetalPesado parametroMetalPesado = dc.CAL_ParametroMetalPesado.SingleOrDefault(X => X.IdParametroMetalPesado == id && X.Habilitado == true);
            if (parametroMetalPesado == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de metal pesado", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                parametroMetalPesado.Habilitado = false;
                parametroMetalPesado.UserUpd = User.Identity.Name;
                parametroMetalPesado.FechaHoraUpd = DateTime.Now;
                parametroMetalPesado.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El parámetro de metal pesado {0} ha sido eliminado", parametroMetalPesado.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}