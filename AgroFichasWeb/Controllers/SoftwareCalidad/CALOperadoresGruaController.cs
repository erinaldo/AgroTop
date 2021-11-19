using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    public class CALOperadoresGruaController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALOperadoresGruaController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALOperadoresGrua
        public ActionResult Index(int? id, DateTime? fecha)
        {
            CheckPermisoAndRedirect(406);

            if (id != null && fecha != null)
            {
                // Resuelve turno actual en base a la jornada activa
                CAL_GetTurno2AnteriorSiguienteResult turno2 = dcSoftwareCalidad.CAL_GetTurno2AnteriorSiguiente(Convert.ToDateTime(fecha)).SingleOrDefault();
                CAL_Turno2 turno = dcSoftwareCalidad.CAL_Turno2.SingleOrDefault(X => X.IdTurno == turno2.TAct && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);
                PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();
                // Lista completa de pallets asociados al turno actual
                List<CAL_Pale> pales = dcSoftwareCalidad.CAL_Pale.Where(X => X.FechaHoraIns.Date == Convert.ToDateTime(fecha).Date && X.IdTurno2 == turno2.TAct 
                && X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == pu.IdPlantaProduccion).ToList();
                

                ViewData["fecha"] = fecha;
                ViewData["turno"] = turno;
                ViewData["errMsg"] = (Request["errMsg"] ?? "");
                ViewData["okMsg"] = (Request["okMsg"] ?? "");

                ViewData["TurnoAnteriorSiguiente"] = dcSoftwareCalidad.CAL_GetTurno2AnteriorSiguiente(fecha).SingleOrDefault();
                return View("Pallets", pales);
            }
            else 
            {
                // Resuelve turno actual en base a la jornada activa
                CAL_GetTurno2AnteriorSiguienteResult turno2 = dcSoftwareCalidad.CAL_GetTurno2AnteriorSiguiente(DateTime.Now).SingleOrDefault();
                CAL_Turno2 turno = dcSoftwareCalidad.CAL_Turno2.SingleOrDefault(X => X.IdTurno == turno2.TAct && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);
                PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();
                // Lista completa de pallets asociados al turno actual
                List<CAL_Pale> pales = dcSoftwareCalidad.CAL_Pale.Where(X => X.FechaHoraIns.Date == DateTime.Now.Date && X.IdTurno2 == turno2.TAct && X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == pu.IdPlantaProduccion).ToList();


                ViewData["fecha"] = DateTime.Now.Date;
                ViewData["turno"] = turno;
                ViewData["errMsg"] = (Request["errMsg"] ?? "");
                ViewData["okMsg"] = (Request["okMsg"] ?? "");

                ViewData["TurnoAnteriorSiguiente"] = dcSoftwareCalidad.CAL_GetTurno2AnteriorSiguiente(DateTime.Now).SingleOrDefault();
                return View("Pallets", pales);
            }
                

        }

        [HttpPost]
        public ActionResult Index(int IdPale)
        {
            CheckPermisoAndRedirect(406);

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            if (pale.FechaCargaHoraIns.HasValue == false)
            {
                pale.UserCargaIns      = User.Identity.Name;
                pale.FechaCargaHoraIns = DateTime.Now;
                pale.IpCargaIns        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
            }

            return View("Pallet", pale);
        }
    }
}