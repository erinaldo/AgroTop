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
    public class CTRegistrarLlegadaController : BaseApplicationController
    {
        //
        // GET: /CTRegistrarLlegada/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public CTRegistrarLlegadaController()
        {
            SetCurrentModulo(9);
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(232);
            CTR_ControlTiempo controlTiempo = new CTR_ControlTiempo();
            ViewBag.Planificaciones = (from c in dc.CTR_PlanificacionSemanal where c.Habilitado == true select c).OrderBy(c => c.IdPlanificacionSemanal).ToList();
            ViewData["UserID"] = CurrentUser.UserID;
            return View(controlTiempo);
        }

        [HttpPost]
        public ActionResult Index(CTR_ControlTiempo controlTiempo, FormCollection formValues)
        {
            CheckPermisoAndRedirect(232);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(controlTiempo.TelefonoChofer))
                        controlTiempo.TelefonoChofer = "";
                    if (string.IsNullOrEmpty(controlTiempo.DUS))
                        controlTiempo.DUS = "";
                    if (string.IsNullOrEmpty(controlTiempo.Reserva))
                        controlTiempo.Reserva = "";

                    controlTiempo.RutTransportista = controlTiempo.RutTransportista.ToUpper();
                    controlTiempo.NombreTransportista = controlTiempo.NombreTransportista.ToUpper();
                    controlTiempo.Patente = controlTiempo.Patente.ToUpper();
                    controlTiempo.NombreChofer = controlTiempo.NombreChofer.ToUpper();
                    controlTiempo.TelefonoChofer = controlTiempo.TelefonoChofer.ToUpper();
                    controlTiempo.RutChofer = controlTiempo.RutChofer.ToUpper();
                    controlTiempo.Observaciones = "";

                    controlTiempo.Habilitado = true;
                    controlTiempo.IdEstado = 1;
                    //controlTiempo.NumeroGuia = 0;
                    controlTiempo.FechaLlegada = DateTime.Now;
                    controlTiempo.FechaHoraIns = DateTime.Now;
                    controlTiempo.IpIns = RemoteAddr();
                    controlTiempo.UserIns = User.Identity.Name;
                    dc.CTR_ControlTiempo.InsertOnSubmit(controlTiempo);
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
            ViewData["UserID"] = CurrentUser.UserID;
            return View("index", controlTiempo);
        }
    }
}