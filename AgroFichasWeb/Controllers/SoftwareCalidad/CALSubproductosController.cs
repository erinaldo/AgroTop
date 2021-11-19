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
    public class CALSubproductosController : BaseApplicationController
    {
        //
        // GET: /CALSubproductos/

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALSubproductosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(264);
            List<CAL_Subproducto> list = dc.CAL_Subproducto.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(265),
                                                      CheckPermiso(264),
                                                      CheckPermiso(266),
                                                      CheckPermiso(267));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(265);
            CAL_Subproducto subproducto  = new CAL_Subproducto();
            return View("crear", subproducto);
        }

        [HttpPost]
        public ActionResult Crear(CAL_Subproducto subproducto)
        {
            CheckPermisoAndRedirect(265);
            if (ModelState.IsValid)
            {
                try
                {
                    subproducto.Nombre = subproducto.Nombre.ToUpperInvariant();

                    subproducto.Habilitado = true;
                    subproducto.FechaHoraIns = DateTime.Now;
                    subproducto.IpIns = RemoteAddr();
                    subproducto.UserIns = User.Identity.Name;
                    dc.CAL_Subproducto.InsertOnSubmit(subproducto);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = subproducto.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", subproducto);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(266);
            var subproducto = dc.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == id && X.Habilitado == true);
            if (subproducto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el subproducto", okMsg = "" }); }
            return View("crear", subproducto);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(266);
            var subproducto = dc.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == id && X.Habilitado == true);
            if (subproducto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el subproducto", okMsg = "" }); }

            try
            {
                UpdateModel(subproducto, new string[] { "Nombre", "IdProducto" });

                subproducto.Nombre = subproducto.Nombre.ToUpperInvariant();

                subproducto.UserUpd = User.Identity.Name;
                subproducto.FechaHoraUpd = DateTime.Now;
                subproducto.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = subproducto.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", subproducto);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(267);
            var subproducto = dc.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == id && X.Habilitado == true);
            if (subproducto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el subproducto", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                subproducto.Habilitado = false;
                subproducto.UserUpd = User.Identity.Name;
                subproducto.FechaHoraUpd = DateTime.Now;
                subproducto.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El subproducto {0} ha sido eliminado", subproducto.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}