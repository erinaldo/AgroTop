using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALEspesoresProductoController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALEspesoresProductoController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALEspesoresProducto
        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(268);
            List<CAL_EspesorProducto> list = dc.CAL_EspesorProducto.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(269),
                                                      CheckPermiso(268),
                                                      CheckPermiso(270),
                                                      CheckPermiso(246));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(269);
            CAL_EspesorProducto espesor = new CAL_EspesorProducto();
            return View("crear", espesor);
        }

        [HttpPost]
        public ActionResult Crear(CAL_EspesorProducto espesor)
        {
            CheckPermisoAndRedirect(269);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(espesor.Observaciones))
                        espesor.Observaciones = "";

                    espesor.Avg = Convert.ToDecimal((espesor.Max + espesor.Min) / 2);
                    espesor.Habilitado = true;
                    espesor.FechHoraIns = DateTime.Now;
                    espesor.IpIns = RemoteAddr();
                    espesor.UserIns = User.Identity.Name;
                    dc.CAL_EspesorProducto.InsertOnSubmit(espesor);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = espesor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", espesor);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(270);
            var espesor = dc.CAL_EspesorProducto.SingleOrDefault(X => X.IdEspesorProducto == id && X.Habilitado == true);
            if (espesor == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el espesor de producto", okMsg = "" }); }
            return View("crear", espesor);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(270);
            var espesor = dc.CAL_EspesorProducto.SingleOrDefault(X => X.IdEspesorProducto == id && X.Habilitado == true);
            if (espesor == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el espesor de producto", okMsg = "" }); }

            try
            {
                UpdateModel(espesor, new string[] { "Min", "Max", "Observaciones" });

                if (string.IsNullOrEmpty(espesor.Observaciones))
                    espesor.Observaciones = "";

                espesor.Avg = Convert.ToDecimal((espesor.Max + espesor.Min) / 2);
                espesor.UserUpd = User.Identity.Name;
                espesor.FechaHoraUpd = DateTime.Now;
                espesor.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = espesor.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", espesor);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(271);
            var espesor = dc.CAL_EspesorProducto.SingleOrDefault(X => X.IdEspesorProducto == id && X.Habilitado == true);
            if (espesor == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el espesor de producto", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                espesor.Habilitado = false;
                espesor.UserUpd = User.Identity.Name;
                espesor.FechaHoraUpd = DateTime.Now;
                espesor.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El espesor de producto {0} ha sido eliminado", espesor.IdEspesorProducto);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}