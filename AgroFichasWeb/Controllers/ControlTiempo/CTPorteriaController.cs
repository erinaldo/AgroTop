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
    public class CTPorteriaController : BaseApplicationController
    {
        //
        // GET: /CTPorteria/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public CTPorteriaController()
        {
            SetCurrentModulo(9);
        }

        public ActionResult Index(int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(276);
            DateTime dateTime = DateTime.Now;

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

            CultureInfo culture = CultureInfo.CurrentCulture;
            int firstday = culture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            int weeknum = culture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            int year = (weeknum == 52 && dateTime.Month == 1 ? dateTime.Year - 1 : dateTime.Year);

            List<CTR_GetRecepcionPorteriaResult> list = dc.CTR_GetRecepcionPorteria(year, weeknum, dateTime, IdPlantaProduccionSelect).ToList();

            ViewData["PlantasProduccion"] = plantas;
            ViewData["dateTime"] = dateTime;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(277),
                                                      CheckPermiso(276),
                                                      CheckPermiso(277),
                                                      CheckPermiso(277));
            return View(list);
        }

        public ActionResult RegistrarLlegada(int id)
        {
            CheckPermisoAndRedirect(277);
            var planificacionSemanal = dc.CTR_PlanificacionSemanal.SingleOrDefault(X => X.IdPlanificacionSemanal == id);
            if (planificacionSemanal == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la planificación semanal", okMsg = "" });
            }


            return View(planificacionSemanal);
        }

        [HttpPost]
        public ActionResult RegistrarLlegada(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(277);
            var planificacionSemanal = dc.CTR_PlanificacionSemanal.SingleOrDefault(X => X.IdPlanificacionSemanal == id);
            if (planificacionSemanal == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la planificación semanal", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {



                    UpdateModel(planificacionSemanal, new string[] { "RUT", "Transportista", "Patente", "Chofer", "Telefono", "RutChofer", "EmailChofer", "TipoCamion" });
                    if (string.IsNullOrEmpty(planificacionSemanal.Telefono))
                        planificacionSemanal.Telefono = "";

                    CTR_ControlTiempo controlTiempo = new CTR_ControlTiempo()
                    {
                        IdPlantaProduccion     = planificacionSemanal.IdPlantaProduccion,
                        IdEmpresa              = planificacionSemanal.IdEmpresa,
                        IdProducto             = planificacionSemanal.IdProducto,
                        IdEnvase               = planificacionSemanal.IdEnvase,
                        IdCliente              = planificacionSemanal.IdCliente,
                        RutTransportista       = planificacionSemanal.RUT,
                        NombreTransportista    = planificacionSemanal.Transportista.ToUpper(),
                        Patente                = planificacionSemanal.Patente.ToUpper(),
                        NumeroGuia             = 0,
                        IdEstado               = 1,
                        FechaLlegada           = DateTime.Now,
                        Habilitado             = true,
                        FechaHoraIns           = DateTime.Now,
                        IpIns                  = RemoteAddr(),
                        UserIns                = User.Identity.Name,
                        NombreChofer           = planificacionSemanal.Chofer.ToUpper(),
                        TelefonoChofer         = planificacionSemanal.Telefono.ToUpper(),
                        DUS                    = planificacionSemanal.DUS,
                        Reserva                = planificacionSemanal.Reserva,
                        Observaciones          = "",
                        EmailChofer            = planificacionSemanal.EmailChofer,
                        RutChofer              = planificacionSemanal.RutChofer,
                        TipoCamion             = planificacionSemanal.TipoCamion,
                        OVS                    = planificacionSemanal.OVS,
                        FRS                    = planificacionSemanal.FRS,
                        IdPlanificacionSemanal = planificacionSemanal.IdPlanificacionSemanal,
                        
                    };
                    dc.CTR_ControlTiempo.InsertOnSubmit(controlTiempo);
                    dc.SubmitChanges();

                    CTR_Porteria porteria = new CTR_Porteria()
                    {
                        IdPlanificacionSemanal = planificacionSemanal.IdPlanificacionSemanal,
                        Presentado             = true,
                        FechaHoraIns           = DateTime.Now,
                        IpIns                  = RemoteAddr(),
                        UserIns                = User.Identity.Name
                    };
                    dc.CTR_Porteria.InsertOnSubmit(porteria);
                    dc.SubmitChanges();

                    return Redirect("~/controltiempo");
                }
                catch
                {
                    var rv = planificacionSemanal.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View(planificacionSemanal);
        }
    }
}