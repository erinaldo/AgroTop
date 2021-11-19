using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.SoftwareCalidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALParametrosAnalisisProductoController : BaseApplicationController
    {
        // GET: CALParametrosAnalisisProducto

        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALParametrosAnalisisProductoController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(319);
            List<vw_CAL_ParametroAnalisisProducto> list = dcSoftwareCalidad.vw_CAL_ParametroAnalisisProducto.ToList();

            AsociarParametrosAnalisisProductoViewModel asociarParametrosAnalisisProductoViewModel = new AsociarParametrosAnalisisProductoViewModel();
            asociarParametrosAnalisisProductoViewModel.Productos = list;

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(320),
                                                      CheckPermiso(319),
                                                      CheckPermiso(321),
                                                      CheckPermiso(322));
            return View(asociarParametrosAnalisisProductoViewModel);
        }

        public ActionResult Asociar()
        {
            CheckPermisoAndRedirect(320);
            CAL_ParametroAnalisisProducto parametroAnalisisProducto = new CAL_ParametroAnalisisProducto();
            return View("Asociar", parametroAnalisisProducto);
        }

        [HttpPost]
        public ActionResult Asociar(CAL_ParametroAnalisisProducto parametroAnalisisProducto)
        {
            CheckPermisoAndRedirect(320);
            if (ModelState.IsValid)
            {
                string errMsg = "";
                string okMsg = "";

                CAL_ParametroAnalisisProducto cAL_ParametroAnalisisProducto = dcSoftwareCalidad.CAL_ParametroAnalisisProducto.SingleOrDefault(X => X.IdParametroAnalisis == parametroAnalisisProducto.IdParametroAnalisis && X.IdProducto == parametroAnalisisProducto.IdProducto);
                if (cAL_ParametroAnalisisProducto != null)
                {
                    errMsg = "Ya has asociado ese parámetro de análisis a la familia de productos";
                    return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
                }

                try
                {
                    parametroAnalisisProducto.FechaHoraIns = DateTime.Now;
                    parametroAnalisisProducto.IpIns = RemoteAddr();
                    parametroAnalisisProducto.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_ParametroAnalisisProducto.InsertOnSubmit(parametroAnalisisProducto);
                    dcSoftwareCalidad.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = parametroAnalisisProducto.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Asociar", parametroAnalisisProducto);
        }

        public ActionResult Editar(int id, int IdProducto)
        {
            CheckPermisoAndRedirect(321);
            CAL_ParametroAnalisisProducto parametroAnalisisProducto = dcSoftwareCalidad.CAL_ParametroAnalisisProducto.SingleOrDefault(X => X.IdParametroAnalisis == id && X.IdProducto == IdProducto);
            if (parametroAnalisisProducto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de análisis asociado", okMsg = "" }); }

            return View("Editar", parametroAnalisisProducto);
        }

        [HttpPost]
        public ActionResult Editar(int id, int IdProducto, FormCollection formValues)
        {
            CheckPermisoAndRedirect(321);
            CAL_ParametroAnalisisProducto parametroAnalisisProducto = dcSoftwareCalidad.CAL_ParametroAnalisisProducto.SingleOrDefault(X => X.IdParametroAnalisis == id && X.IdProducto == IdProducto);
            if (parametroAnalisisProducto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de análisis asociado", okMsg = "" }); }

            try
            {
                UpdateModel(parametroAnalisisProducto, new string[] { "Orden" });

                parametroAnalisisProducto.UserUpd = User.Identity.Name;
                parametroAnalisisProducto.FechaHoraUpd = DateTime.Now;
                parametroAnalisisProducto.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = parametroAnalisisProducto.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Editar", parametroAnalisisProducto);
        }

        public ActionResult Eliminar(int id, int IdProducto)
        {
            CheckPermisoAndRedirect(322);
            CAL_ParametroAnalisisProducto parametroAnalisisProducto = dcSoftwareCalidad.CAL_ParametroAnalisisProducto.SingleOrDefault(X => X.IdParametroAnalisis == id && X.IdProducto == IdProducto);
            if (parametroAnalisisProducto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de análisis asociado", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dcSoftwareCalidad.CAL_ParametroAnalisisProducto.DeleteOnSubmit(parametroAnalisisProducto);
                dcSoftwareCalidad.SubmitChanges();
                okMsg = "El parámetro de análisis asociado ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}