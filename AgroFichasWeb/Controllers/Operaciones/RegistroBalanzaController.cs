using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class RegistroBalanzaController : BaseApplicationController
    {
        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        //
        // GET: /RegistroBalanza/

        public RegistroBalanzaController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(169);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var model = new RegistroBalanzaViewModel();
            var Turno = ResolveTurno(dc);
            var registroTurnoActual = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == DateTime.Now.Date && X.IdTurno == Turno.IdTurno && X.Habilitado == true);

            var puedeCrear = CheckPermiso(170);
            var puedeCrearEspecial = CheckPermiso(173);

            // Si el registro de turno es igual al registro de turno actual
            if (registroTurnoActual == registroTurno)
            {
                // Si el usuario que creó el registro de turno y el usuario actual son distintos
                if (registroTurno.UserID != CurrentUser.UserID && !CheckPermiso(173))
                {
                    // No puede crear porque es diferente el operador
                    model.MensajeError = "Solo el operador creador del turno puede registar las balanzas";
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

            model.RegistroBalanzas         = dc.OPR_RegistroBalanza.Where(X => X.IdRegistroTurno == registroTurno.IdRegistroTurno).ToList();
            model.PuedeCrear               = ((puedeCrear || puedeCrearEspecial) && model.RegistroBalanzas.Count == 0);
            model.PuedeEditar              = CheckPermiso(171);
            model.PuedeEliminar            = CheckPermiso(172);
            model.RegistroTurno            = registroTurno;
            model.RegistroBalanzasEfectivo = new List<OPR_RegistroBalanza>(); // Inicializador

            var idRegistroTurno = dc.OPR_GetRegistroTurnoBalanzaEfectivoPorCorrelativo(registroTurno.Correlativo);
            if (idRegistroTurno != 0)
            {
                model.RegistroBalanzasEfectivo = dc.OPR_RegistroBalanza.Where(X => X.IdRegistroTurno == idRegistroTurno).ToList();
                model.RegistroTurnoEfectivo = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == idRegistroTurno);
            }

            model.TurnoAnteriorSiguiente = dc.OPR_GetTurnoAnteriorSiguiente(model.RegistroTurno.Correlativo).SingleOrDefault();

            return View(model);
        }

        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(170);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var model = new RegistroBalanzaViewModel();
            model.RegistroTurno = registroTurno;

            var balanzas = dc.OPR_Balanza.Where(X => X.Habilitado == true).ToList();
            model.Balanzas = balanzas;

            foreach (var balanza in balanzas)
            {
                model.IdBalanzas += balanza.IdBalanza + ",";
            }

            model.RegistroBalanzasEfectivo = new List<OPR_RegistroBalanza>(); // Inicializador

            var idRegistroTurno = dc.OPR_GetRegistroTurnoBalanzaEfectivoPorCorrelativo(registroTurno.Correlativo);
            if (idRegistroTurno != 0)
            {
                model.RegistroBalanzasEfectivo = dc.OPR_RegistroBalanza.Where(X => X.IdRegistroTurno == idRegistroTurno).ToList();
                model.RegistroTurnoEfectivo = dc.OPR_RegistroTurno.Single(X => X.IdRegistroTurno == idRegistroTurno);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Registrar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(170);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            List<OPR_RegistroBalanza> registroBalanzas = new List<OPR_RegistroBalanza>();

            var balanzas = dc.OPR_Balanza.Where(X => X.Habilitado == true).ToList();
            foreach (var balanza in balanzas)
            {
                if (!decimal.TryParse(formValues[string.Format("contador_{0}", balanza.IdBalanza)], out decimal contador))
                    throw new HttpException(404, "El contador de la balanza no es válido");

                var registroBalanza = new OPR_RegistroBalanza();
                registroBalanza.IdBalanza = balanza.IdBalanza;
                registroBalanza.Contador = contador;
                registroBalanza.UserIns = CurrentUser.UserIns;
                registroBalanza.FechaHoraIns = DateTime.Now;
                registroBalanza.IpIns = RemoteAddr();
                registroBalanza.IdRegistroTurno = registroTurno.IdRegistroTurno;
                dc.OPR_RegistroBalanza.InsertOnSubmit(registroBalanza);
            }

            dc.SubmitChanges();

            return RedirectToAction("Index", "RegistroBalanza", new { id = registroTurno.IdRegistroTurno });
        }

        public ActionResult EditarRegistro(int id)
        {
            CheckPermisoAndRedirect(171);
            var registroBalanza = dc.OPR_RegistroBalanza.SingleOrDefault(X => X.IdRegistroBalanza == id);
            if (registroBalanza == null)
                throw new HttpException(404, "No se ha encontrado el registro de la balanza");

            return View(registroBalanza);
        }

        [HttpPost]
        public ActionResult EditarRegistro(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(171);
            var registroBalanza = dc.OPR_RegistroBalanza.SingleOrDefault(X => X.IdRegistroBalanza == id);
            if (registroBalanza == null)
                throw new HttpException(404, "No se ha encontrado el registro de la balanza");

            if (!decimal.TryParse(formValues[string.Format("contador_{0}", registroBalanza.OPR_Balanza.IdBalanza)], out decimal contador))
                throw new HttpException(404, "El contador de la balanza no es válido");

            registroBalanza.Contador = contador;
            registroBalanza.UserUpd = CurrentUser.UserIns;
            registroBalanza.FechaHoraUpd = DateTime.Now;
            registroBalanza.IpUpd = RemoteAddr();
            dc.SubmitChanges();

            return RedirectToAction("Index", "RegistroBalanza", new { id = registroBalanza.IdRegistroTurno });
        }

        public ActionResult EliminarRegistro(int id)
        {
            CheckPermisoAndRedirect(172);
            var registroBalanza = dc.OPR_RegistroBalanza.SingleOrDefault(X => X.IdRegistroBalanza == id);
            if (registroBalanza == null)
                throw new HttpException(404, "No se ha encontrado el registro de la balanza");

            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == registroBalanza.IdRegistroTurno && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                dc.OPR_RegistroBalanza.DeleteOnSubmit(registroBalanza);
                dc.SubmitChanges();
                MensajeExito = String.Format("El registro de la balanza {0} ha sido eliminado", id);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { id = registroTurno.IdRegistroTurno, MensajeError = MensajeError, MensajeExito = MensajeExito });
        }
    }
}