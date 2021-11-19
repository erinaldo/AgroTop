using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class TurnosController : BaseApplicationController
    {
        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        //
        // GET: /Turno/

        public TurnosController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(143);
            var turno = ResolveTurno(dc);
            var model = new TurnoViewModel();
            SetLookups(ref model, turno);
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection frmValues)
        {
            // Comprobando si el turno ya estaba creado
            CheckPermisoAndRedirect(144);
            var turno = ResolveTurno(dc);
            var model = new TurnoViewModel();

            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdTurno == turno.IdTurno && X.FechaHoraIns.Date == DateTime.Now.Date && X.Habilitado == true);
            if (registroTurno != null)
            {
                model.MensajeError = "El turno ya ha sido creado";
            }
            else
            {
                OPR_Version version = dc.OPR_Version.Single(X => X.Activa == true);
                int? correlativo = dc.OPR_RegistroTurno.AsQueryable().Max(x => (int?)x.Correlativo);

                registroTurno = new OPR_RegistroTurno()
                {
                    IdTurno = turno.IdTurno,
                    UserIns = CurrentUser.UserName,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr(),
                    UserID = CurrentUser.UserID,
                    Habilitado = true,
                    Correlativo = (correlativo.HasValue ? correlativo.Value + 1 : 1),
                    IdVersion = version.IdVersion,
                    Observaciones = ""
                };
                dc.OPR_RegistroTurno.InsertOnSubmit(registroTurno);
                dc.SubmitChanges();

                model.MensajeExito = "El turno ha sido creado con éxito";
                int contadorTurnosFantasmas = registroTurno.ComprobarYCrearTurnosFantasmas(turno, registroTurno, RemoteAddr(), CurrentUser.UserID, dc);
                if (contadorTurnosFantasmas > 0)
                    model.MensajeExito += string.Format(" y además han sido creados {0} olvidados", contadorTurnosFantasmas);
            }

            SetLookups(ref model, turno);
            return View(model);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(146);
            var turno = ResolveTurno(dc);
            var model = new TurnoViewModel();

            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            if (registroTurno.UserID != CurrentUser.UserID)
            {
                model.MensajeError = string.Format("Solo el operador creador del turno puede eliminar este turno #{0}", registroTurno.IdRegistroTurno);
                SetLookups(ref model, turno);
                return View("Index", model);
            }

            try
            {
                registroTurno.Correlativo = 0;
                registroTurno.Habilitado = false;
                registroTurno.UserUpd = User.Identity.Name;
                registroTurno.FechaHoraUpd = DateTime.Now;
                registroTurno.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                model.MensajeExito = "El turno ha sido eliminado correctamente";
            }
            catch (Exception ex)
            {
                model.MensajeError = ex.Message;
            }

            SetLookups(ref model, turno);
            return View("Index", model);
        }

        public ActionResult Observaciones(int id)
        {
            CheckPermisoAndRedirect(143);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            return View(registroTurno);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Observaciones(int id, FormCollection frmValues)
        {
            CheckPermisoAndRedirect(143);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            UpdateModel(registroTurno, new string[] { "Observaciones" });

            if (registroTurno.Observaciones == null)
                registroTurno.Observaciones = "";

            registroTurno.UserUpd = User.Identity.Name;
            registroTurno.FechaHoraUpd = DateTime.Now;
            registroTurno.IpUpd = RemoteAddr();
            dc.SubmitChanges();
            return RedirectToAction("Index", "Turnos");
        }

        private void SetLookups(ref TurnoViewModel model, OPR_Turno turno)
        {
            model.PuedeCrear = CheckPermiso(144);
            model.PuedeEditar = CheckPermiso(145);
            model.PuedeEliminar = CheckPermiso(146);
            model.PuedeVerBalanzas = CheckPermiso(169);
            model.PuedeVerSilos = CheckPermiso(151);
            model.PuedeVerDetenciones = CheckPermiso(186);
            model.PuedeVerEnvasado = CheckPermiso(203);
            model.PuedeVerGeneracionMaxisacos = CheckPermiso(208);
            model.PuedeVerProductividad = CheckPermiso(213);
            model.Operador = CurrentUser;
            model.RegistroTurnos = dc.vw_OPR_RegistroTurnos.OrderByDescending(X => X.Correlativo).ToList();
            model.RegistroTurnoActual = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == DateTime.Now.Date && X.IdTurno == turno.IdTurno && X.Habilitado == true);
            model.Turno = turno;
        }

        public ActionResult CrearPdfObservaciones(int id)
        {
            CheckPermisoAndRedirect(143);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var guid = registroTurno.CrearPdfObservaciones();
            if (string.IsNullOrEmpty(guid))
                throw new Exception("No se pudo crear el PDF");

            var pdf = Server.MapPath(string.Format("~/App_Data/pdfs/operaciones/observaciones/{0}.pdf", string.Format("{0}-{1}", registroTurno.Correlativo, guid)));

            return File(pdf, "application/pdf", String.Format("Informe de Observaciones-{0}.pdf", registroTurno.Correlativo));
        }
    }
}
