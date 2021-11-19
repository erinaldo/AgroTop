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
    public class CALCarriersController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALCarriersController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(285);
            List<Carrier> list = dc.Carrier.Where(X=> X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(285),
                                                      CheckPermiso(286),
                                                      CheckPermiso(287),
                                                      CheckPermiso(288));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(286);
            Carrier carrier = new Carrier();
            return View("Crear", carrier);
        }

        [HttpPost]
        public ActionResult Crear(Carrier carrier)
        {
            CheckPermisoAndRedirect(286);
            if (ModelState.IsValid)
            {
                try
                {
                    carrier.Nombre = carrier.Nombre.ToString();

                    carrier.Habilitado = true;
                    carrier.FechaHoraIns = DateTime.Now;
                    carrier.IpIns = RemoteAddr();
                    carrier.UserIns = User.Identity.Name;
                    dc.Carrier.InsertOnSubmit(carrier);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = carrier.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", carrier);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(287);

            Carrier carrier = dc.Carrier.SingleOrDefault(X => X.IdCarrier == id);
            if (carrier == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el carrier", okMsg = "" });
            }

            return View("Crear", carrier);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(287);
            var carrier = dc.Carrier.SingleOrDefault(X => X.IdCarrier == id && X.Habilitado == true);
            if (carrier == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el carrier", okMsg = "" }); }

            try
            {
                UpdateModel(carrier, new string[] { "Nombre" });

                carrier.Nombre = carrier.Nombre.ToString();

                carrier.UserUpd = User.Identity.Name;
                carrier.FechaHoraUpd = DateTime.Now;
                carrier.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = carrier.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", carrier);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(288);
            var carrier = dc.Carrier.SingleOrDefault(X => X.IdCarrier == id && X.Habilitado == true);
            if (carrier == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el carrier", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                carrier.Habilitado = false;
                carrier.UserUpd = User.Identity.Name;
                carrier.FechaHoraUpd = DateTime.Now;
                carrier.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El carrier {0} ha sido eliminado", carrier.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}
