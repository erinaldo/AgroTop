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
    public class CALMetodologiasController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALMetodologiasController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALMetodologias
        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(381);
            List<CAL_Metodologia> list = dc.CAL_Metodologia.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(382),
                                                      CheckPermiso(381),
                                                      CheckPermiso(383),
                                                      CheckPermiso(384));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(382);
            CAL_Metodologia cAL_Metodologia = new CAL_Metodologia();
            return View("Crear", cAL_Metodologia);
        }

        [HttpPost]
        public ActionResult Crear(CAL_Metodologia cAL_Metodologia)
        {
            CheckPermisoAndRedirect(382);
            if (ModelState.IsValid)
            {
                try
                {
                    cAL_Metodologia.Habilitado = true;
                    cAL_Metodologia.FechaHoraIns = DateTime.Now;
                    cAL_Metodologia.IpIns = RemoteAddr();
                    cAL_Metodologia.UserIns = User.Identity.Name;
                    dc.CAL_Metodologia.InsertOnSubmit(cAL_Metodologia);
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = cAL_Metodologia.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", cAL_Metodologia);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(383);
            CAL_Metodologia cAL_Metodologia = dc.CAL_Metodologia.SingleOrDefault(X => X.IdMetodologia == id && X.Habilitado == true);
            if (cAL_Metodologia == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la metodología", okMsg = "" });
            }

            return View("Crear", cAL_Metodologia);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(383);
            CAL_Metodologia cAL_Metodologia = dc.CAL_Metodologia.SingleOrDefault(X => X.IdMetodologia == id && X.Habilitado == true);
            if (cAL_Metodologia == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la metodología", okMsg = "" });
            }

            try
            {
                UpdateModel(cAL_Metodologia, new string[] { "Analisis", "Tecnica" });

                cAL_Metodologia.UserUpd = User.Identity.Name;
                cAL_Metodologia.FechaHoraUpd = DateTime.Now;
                cAL_Metodologia.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = cAL_Metodologia.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", cAL_Metodologia);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(384);
            CAL_Metodologia cAL_Metodologia = dc.CAL_Metodologia.SingleOrDefault(X => X.IdMetodologia == id && X.Habilitado == true);
            if (cAL_Metodologia == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la metodología", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cAL_Metodologia.Habilitado = false;
                cAL_Metodologia.UserUpd = User.Identity.Name;
                cAL_Metodologia.FechaHoraUpd = DateTime.Now;
                cAL_Metodologia.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("La metodología {0} ha sido eliminada", cAL_Metodologia.Analisis);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}