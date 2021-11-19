using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.ControlTiempo
{
    [WebsiteAuthorize]
    public class CTRegistrarPesajeInicialController : BaseApplicationController
    {
        //
        // GET: /CTRegistrarPesajeInicial/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public CTRegistrarPesajeInicialController()
        {
            SetCurrentModulo(9);
        }

        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(233);

            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = from us in dc.PlantaUsuario
                                                  join pl in dc.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
                                                  where us.UserID == CurrentUser.UserID
                                                  && pl.Habilitado
                                                  select new SelectListItem
                                                  {
                                                      Value = pl.IdPlantaProduccion.ToString(),
                                                      Text = pl.Nombre,
                                                      Selected = IdPlantaProduccionSelect == pl.IdPlantaProduccion
                                                  };

            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            List<CTR_ControlTiempo> controlesTiempo = dc.CTR_ControlTiempo.Where(X => X.IdEstado == 1 && X.Habilitado == true && X.IdPlantaProduccion == IdPlantaProduccionSelect).ToList();
            ViewData["PlantasProduccion"] = plantas;
            return View(controlesTiempo);
        }

        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(233);

            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.Habilitado == true);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            ViewBag.Romana = (from c in dc.CTR_MantenedorRomana where (c.EsPlanta == true && c.Nombre == controlTiempo.PlantaProduccion.Nombre && c.Vigente == true) select c).SingleOrDefault();
            return View(controlTiempo);
        }

        [HttpPost]
        public ActionResult Registrar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(233);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.Habilitado == true);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(controlTiempo, new string[] { "PesoInicial" });

                    if (controlTiempo.ValidacionPesoInicial(controlTiempo, ModelState))
                    {
                        controlTiempo.IdEstado = 2;
                        controlTiempo.FechaPesajeInicial = DateTime.Now;
                        controlTiempo.FechaHoraUpd = DateTime.Now;
                        controlTiempo.IpUpd = RemoteAddr();
                        controlTiempo.UserUpd = User.Identity.Name;
                        dc.SubmitChanges();
                        return Redirect("~/ctregistrarpesajeinicial");
                    }
                }
                catch
                {
                    var rv = controlTiempo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("registrar", controlTiempo);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(233);
            ViewData["UserID"] = CurrentUser.UserID;
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.Habilitado == true);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            return View(controlTiempo);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(233);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.Habilitado == true);
            if (controlTiempo == null) { return RedirectToAction("editar", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(controlTiempo, new string[] {"IdEmpresa", "IdProducto", "IdCliente" });

                    controlTiempo.FechaHoraUpd = DateTime.Now;
                    controlTiempo.IpUpd = RemoteAddr();
                    controlTiempo.UserUpd = User.Identity.Name;
                    dc.SubmitChanges();
                    return RedirectToAction("registrar", new { id = controlTiempo.IdControlTiempo });
                }
                catch
                {
                    var rv = controlTiempo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("editar", new { id = controlTiempo.IdControlTiempo });
        }


    }
}