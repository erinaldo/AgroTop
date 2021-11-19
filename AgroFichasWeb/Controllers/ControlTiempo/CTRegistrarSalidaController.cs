using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.ControlTiempo
{
    [WebsiteAuthorize]
    public class CTRegistrarSalidaController : BaseApplicationController
    {
        //
        // GET: /CTRegistrarSalida/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public CTRegistrarSalidaController()
        {
            SetCurrentModulo(9);
        }

        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(235);

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

            ViewData["PlantasProduccion"] = plantas;

            List<CTR_ControlTiempo> controlesTiempo = dc.CTR_ControlTiempo.Where(X => X.IdEstado == 3 && X.Habilitado == true && X.IdPlantaProduccion == IdPlantaProduccionSelect).ToList();
            return View(controlesTiempo);
        }

        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(235);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            return View(controlTiempo);
        }

        [HttpPost]
        public ActionResult Registrar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(235);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            if (ModelState.IsValid)
            {
                try
                {
                    controlTiempo.IdEstado = 4;
                    controlTiempo.FechaSalida = DateTime.Now;
                    controlTiempo.FechaHoraUpd = DateTime.Now;
                    controlTiempo.IpUpd = RemoteAddr();
                    controlTiempo.UserUpd = User.Identity.Name;
                    dc.SubmitChanges();
                    return Redirect("~/controltiempo");
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
    }
}