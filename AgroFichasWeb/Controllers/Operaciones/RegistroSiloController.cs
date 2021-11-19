using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class RegistroSiloController : BaseApplicationController
    {
        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        //
        // GET: /RegistroSilo/

        public RegistroSiloController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(151);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var model = new RegistroSiloViewModel();
            var turno = ResolveTurno(dc);
            var registroTurnoActual = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == DateTime.Now.Date && X.IdTurno == turno.IdTurno && X.Habilitado == true);

            var puedeCrear = CheckPermiso(152);
            var puedeCrearEspecial = CheckPermiso(155);

            // Si el registro de turno es igual al registro de turno actual
            if (registroTurnoActual == registroTurno)
            {
                // Si el usuario que creó el registro de turno y el usuario actual son distintos
                if (registroTurno.UserID != CurrentUser.UserID && !CheckPermiso(155) )
                {
                    // No puede crear porque es diferente el operador
                    model.MensajeError = "Solo el operador creador del turno puede registar los silos";
                    puedeCrear = false;
                }
            }
            else
            {
                // Está fuera de turno y no puede crear
                puedeCrear = false;
            }

            model.MensajeError = Request["MensajeError"];
            model.MensajeExito = Request["MensajeExito"];

            model.RegistroSilos            = dc.OPR_RegistroSilo.Where(X => X.IdRegistroTurno == registroTurno.IdRegistroTurno && X.OPR_Silo.IdTipoSilo == 1).ToList();
            model.RegistroSilosExt         = dc.OPR_RegistroSilo.Where(X => X.IdRegistroTurno == registroTurno.IdRegistroTurno && X.OPR_Silo.IdTipoSilo == 2).ToList();
            model.PuedeCrear               = ((puedeCrear || puedeCrearEspecial) && model.RegistroSilos.Count == 0);
            model.PuedeEditar              = CheckPermiso(153);
            model.PuedeEliminar            = CheckPermiso(154);
            model.RegistroTurno            = registroTurno;
            model.RegistroSilosEfectivo    = new List<OPR_RegistroSilo>(); // Inicializador
            model.RegistroSilosEfectivoExt = new List<OPR_RegistroSilo>();

            var idRegistroTurno = dc.OPR_GetRegistroTurnoSiloEfectivoPorCorrelativo(registroTurno.Correlativo);
            if (idRegistroTurno != 0)
            {
                model.RegistroSilosEfectivo    = dc.OPR_RegistroSilo.Where(X => X.IdRegistroTurno == idRegistroTurno && X.OPR_Silo.IdTipoSilo == 1).ToList();
                model.RegistroSilosEfectivoExt = dc.OPR_RegistroSilo.Where(X => X.IdRegistroTurno == idRegistroTurno && X.OPR_Silo.IdTipoSilo == 2).ToList();
                model.RegistroTurnoEfectivo    = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == idRegistroTurno);
            }

            model.TurnoAnteriorSiguiente = dc.OPR_GetTurnoAnteriorSiguiente(model.RegistroTurno.Correlativo).SingleOrDefault();

            return View(model);
        }

        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(152);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var model = new RegistroSiloViewModel();
            model.RegistroTurno = registroTurno;

            var silos = dc.OPR_Silo.Where(X => X.Habilitado == true).ToList();
            model.Silos = silos;
            foreach (var silo in silos)
            {
                if (silo.IdTipoSilo == 1)
                    model.IdSilos += silo.IdSilo + ",";
                if (silo.IdTipoSilo == 2)
                    model.IdSilosExt += silo.IdSilo + ",";
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Registrar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(152);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var silos = dc.OPR_Silo.Where(X => X.Habilitado == true).ToList();
            foreach (var silo in silos)
            {
                if (!int.TryParse(formValues[string.Format("silo_{0}", silo.IdSilo)], out int unidades))
                    throw new HttpException(404, "Las unidades no son válidas");
                if (!decimal.TryParse(formValues[string.Format("cantidad_{0}", silo.IdSilo)], out decimal cantidadCubicada))
                    throw new HttpException(404, "La cantidad cubicada no es válida");

                var registroSilo = new OPR_RegistroSilo();
                registroSilo.IdSilo = silo.IdSilo;
                registroSilo.UserIns = CurrentUser.UserIns;
                registroSilo.FechaHoraIns = DateTime.Now;
                registroSilo.IpIns = RemoteAddr();
                registroSilo.IdRegistroTurno = registroTurno.IdRegistroTurno;
                registroSilo.Unidades = unidades;
                registroSilo.CantidadCubicada = cantidadCubicada;
                dc.OPR_RegistroSilo.InsertOnSubmit(registroSilo);
            }

            dc.SubmitChanges();

            return RedirectToAction("Index", "RegistroSilo", new { id = registroTurno.IdRegistroTurno });
        }

        public ActionResult EditarRegistro(int id)
        {
            CheckPermisoAndRedirect(153);
            var registroSilo = dc.OPR_RegistroSilo.SingleOrDefault(X => X.IdRegistroSilo == id);
            if (registroSilo == null)
                throw new HttpException(404, "No se ha encontrado el registro del silo");

            return View(registroSilo);
        }

        [HttpPost]
        public ActionResult EditarRegistro(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(153);
            var registroSilo = dc.OPR_RegistroSilo.SingleOrDefault(X => X.IdRegistroSilo == id);
            if (registroSilo == null)
                throw new HttpException(404, "No se ha encontrado el registro del silo");
            if (!int.TryParse(formValues[string.Format("silo_{0}", registroSilo.OPR_Silo.IdSilo)], out int unidades))
                throw new HttpException(404, "Las unidades no son válidas");
            if (!decimal.TryParse(formValues[string.Format("cantidad_{0}", registroSilo.OPR_Silo.IdSilo)], out decimal cantidadCubicada))
                throw new HttpException(404, "La cantidad cubicada no es válida");

            registroSilo.Unidades = unidades;
            registroSilo.CantidadCubicada = cantidadCubicada;
            registroSilo.UserUpd = CurrentUser.UserIns;
            registroSilo.FechaHoraUpd = DateTime.Now;
            registroSilo.IpUpd = RemoteAddr();
            dc.SubmitChanges();

            return RedirectToAction("Index", "RegistroSilo", new { id = registroSilo.IdRegistroTurno });
        }

        public ActionResult EliminarRegistro(int id)
        {
            CheckPermisoAndRedirect(154);
            var registroSilo = dc.OPR_RegistroSilo.SingleOrDefault(X => X.IdRegistroSilo == id);
            if (registroSilo == null)
                throw new HttpException(404, "No se ha encontrado el registro del silo");

            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == registroSilo.IdRegistroTurno && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                dc.OPR_RegistroSilo.DeleteOnSubmit(registroSilo);
                dc.SubmitChanges();
                MensajeExito = String.Format("El registro del silo {0} ha sido eliminado", id);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { id = registroTurno.IdRegistroTurno, MensajeError = MensajeError, MensajeExito = MensajeExito });
        }
    }
}
