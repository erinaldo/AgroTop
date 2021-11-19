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
    public class CALParametrosPesticidasController : BaseApplicationController
    {
        // GET: CALParametrosPesticidas

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALParametrosPesticidasController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(335);
            List<CAL_ParametroPesticida> list = dc.CAL_ParametroPesticida.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(336),
                                                      CheckPermiso(335),
                                                      CheckPermiso(321),
                                                      CheckPermiso(322));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(336);
            CAL_ParametroPesticida parametroPesticida = new CAL_ParametroPesticida();
            return View("Crear", parametroPesticida);
        }

        [HttpPost]
        public ActionResult Crear(CAL_ParametroPesticida parametroPesticida)
        {
            CheckPermisoAndRedirect(336);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(parametroPesticida.FormatString))
                        parametroPesticida.FormatString = string.Empty;

                    //Parámetros (en)
                    if (string.IsNullOrEmpty(parametroPesticida.Nombre_en))
                        parametroPesticida.Nombre_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroPesticida.FormatString_en))
                        parametroPesticida.FormatString_en = string.Empty;
                    if (string.IsNullOrEmpty(parametroPesticida.UM_en))
                        parametroPesticida.UM_en = string.Empty;

                    parametroPesticida.Habilitado = true;
                    parametroPesticida.FechaHoraIns = DateTime.Now;
                    parametroPesticida.IpIns = RemoteAddr();
                    parametroPesticida.UserIns = User.Identity.Name;
                    dc.CAL_ParametroPesticida.InsertOnSubmit(parametroPesticida);
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = parametroPesticida.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", parametroPesticida);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(337);
            CAL_ParametroPesticida parametroPesticida = dc.CAL_ParametroPesticida.SingleOrDefault(X => X.IdParametroPesticida == id && X.Habilitado == true);
            if (parametroPesticida == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de pesticida", okMsg = "" }); }

            return View("Crear", parametroPesticida);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(337);
            CAL_ParametroPesticida parametroPesticida = dc.CAL_ParametroPesticida.SingleOrDefault(X => X.IdParametroPesticida == id && X.Habilitado == true);
            if (parametroPesticida == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de pesticida", okMsg = "" }); }

            try
            {
                UpdateModel(parametroPesticida, new string[] { "Nombre", "NombreCorto", "FormatString", "UM", "Decimales", "Nombre_en", "NombreCorto_en", "FormatString_en", "UM_en" });

                if (string.IsNullOrEmpty(parametroPesticida.FormatString))
                    parametroPesticida.FormatString = string.Empty;

                //Parámetros (en)
                if (string.IsNullOrEmpty(parametroPesticida.Nombre_en))
                    parametroPesticida.Nombre_en = string.Empty;
                if (string.IsNullOrEmpty(parametroPesticida.FormatString_en))
                    parametroPesticida.FormatString_en = string.Empty;
                if (string.IsNullOrEmpty(parametroPesticida.UM_en))
                    parametroPesticida.UM_en = string.Empty;

                parametroPesticida.UserUpd = User.Identity.Name;
                parametroPesticida.FechaHoraUpd = DateTime.Now;
                parametroPesticida.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = parametroPesticida.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", parametroPesticida);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(338);
            CAL_ParametroPesticida parametroPesticida = dc.CAL_ParametroPesticida.SingleOrDefault(X => X.IdParametroPesticida == id && X.Habilitado == true);
            if (parametroPesticida == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de pesticida", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                parametroPesticida.Habilitado = false;
                parametroPesticida.UserUpd = User.Identity.Name;
                parametroPesticida.FechaHoraUpd = DateTime.Now;
                parametroPesticida.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El parámetro de pesticida {0} ha sido eliminado", parametroPesticida.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}