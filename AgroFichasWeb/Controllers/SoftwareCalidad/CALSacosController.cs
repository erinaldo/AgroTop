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
    public class CALSacosController : BaseApplicationController
    {
        //
        // GET: /CALSacos/

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALSacosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(255);
            List<CAL_Saco> list = dc.CAL_Saco.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(256),
                                                      CheckPermiso(255),
                                                      CheckPermiso(257),
                                                      CheckPermiso(246));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(256);
            CAL_Saco saco = new CAL_Saco();
            return View("crear", saco);
        }

        [HttpPost]
        public ActionResult Crear(CAL_Saco saco)
        {
            CheckPermisoAndRedirect(256);
            if (ModelState.IsValid)
            {
                try
                {
                    saco.Nombre = saco.Nombre.ToUpperInvariant();

                    saco.Habilitado = true;
                    saco.FechaHoraIns = DateTime.Now;
                    saco.IpIns = RemoteAddr();
                    saco.UserIns = User.Identity.Name;
                    dc.CAL_Saco.InsertOnSubmit(saco);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = saco.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", saco);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(257);
            var saco = dc.CAL_Saco.SingleOrDefault(X => X.IdSaco == id && X.Habilitado == true);
            if (saco == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el saco", okMsg = "" }); }
            return View("crear", saco);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(257);
            var tipoArte = dc.CAL_Saco.SingleOrDefault(X => X.IdSaco == id && X.Habilitado == true);
            if (tipoArte == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el saco", okMsg = "" }); }

            try
            {
                UpdateModel(tipoArte, new string[] { "Nombre", "IdTipoSaco", "IdTipoArteSaco", "IdTipoColorHiloSaco", "Recubierto" });

                tipoArte.Nombre = tipoArte.Nombre.ToUpperInvariant();

                tipoArte.UserUpd = User.Identity.Name;
                tipoArte.FechaHoraUpd = DateTime.Now;
                tipoArte.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = tipoArte.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", tipoArte);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(259);
            var tipoArte = dc.CAL_Saco.SingleOrDefault(X => X.IdSaco == id && X.Habilitado == true);
            if (tipoArte == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el saco", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                tipoArte.Habilitado = false;
                tipoArte.UserUpd = User.Identity.Name;
                tipoArte.FechaHoraUpd = DateTime.Now;
                tipoArte.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El saco {0} ha sido eliminado", tipoArte.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}
