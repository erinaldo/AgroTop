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
    public class CALTiposArteSacosController : BaseApplicationController
    {
        //
        // GET: /CALTiposArteSacos/

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALTiposArteSacosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(251);
            List<CAL_TipoArteSaco> list = dc.CAL_TipoArteSaco.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(252),
                                                      CheckPermiso(251),
                                                      CheckPermiso(253),
                                                      CheckPermiso(254));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(252);
            CAL_TipoArteSaco tipoArte = new CAL_TipoArteSaco();
            return View("crear", tipoArte);
        }

        [HttpPost]
        public ActionResult Crear(CAL_TipoArteSaco tipoArte)
        {
            CheckPermisoAndRedirect(252);
            if (ModelState.IsValid)
            {
                try
                {
                    tipoArte.Descripcion = tipoArte.Descripcion.ToUpperInvariant();

                    tipoArte.Habilitado = true;
                    tipoArte.FechaHoraIns = DateTime.Now;
                    tipoArte.IpIns = RemoteAddr();
                    tipoArte.UserIns = User.Identity.Name;
                    dc.CAL_TipoArteSaco.InsertOnSubmit(tipoArte);
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
            }

            return View("crear", tipoArte);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(253);
            var tipoArte = dc.CAL_TipoArteSaco.SingleOrDefault(X => X.IdTipoArteSaco == id && X.Habilitado == true);
            if (tipoArte == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de arte del saco", okMsg = "" }); }
            return View("crear", tipoArte);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(253);
            var tipoArte = dc.CAL_TipoArteSaco.SingleOrDefault(X => X.IdTipoArteSaco == id && X.Habilitado == true);
            if (tipoArte == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de arte del saco", okMsg = "" }); }

            try
            {
                UpdateModel(tipoArte, new string[] { "Descripcion" });

                tipoArte.Descripcion = tipoArte.Descripcion.ToUpperInvariant();

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
            CheckPermisoAndRedirect(254);
            var tipoArte = dc.CAL_TipoArteSaco.SingleOrDefault(X => X.IdTipoArteSaco == id && X.Habilitado == true);
            if (tipoArte == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de arte del saco", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                tipoArte.Habilitado = false;
                tipoArte.UserUpd = User.Identity.Name;
                tipoArte.FechaHoraUpd = DateTime.Now;
                tipoArte.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El tipo de arte del saco {0} ha sido eliminado", tipoArte.Descripcion);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}