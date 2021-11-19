using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class RegistroGeneracionMaxisacosController : BaseApplicationController
    {
        //
        // GET: /RegistroGeneracionMaxisacos/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public RegistroGeneracionMaxisacosController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(208);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var model = new RegistroGeneracionMaxisacosViewModel();
            var Turno = ResolveTurno(dc);
            var registroTurnoActual = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == DateTime.Now.Date && X.IdTurno == Turno.IdTurno && X.Habilitado == true);

            var puedeCrear = CheckPermiso(209);
            var puedeCrearEspecial = CheckPermiso(212);

            // Si el registro de turno es igual al registro de turno actual
            if (registroTurnoActual == registroTurno)
            {
                // Si el usuario que creó el registro de turno y el usuario actual son distintos
                if (registroTurno.UserID != CurrentUser.UserID && !CheckPermiso(212))
                {
                    // No puede crear porque es diferente el operador
                    model.MensajeError = "Solo el operador creador del turno puede registar la generación de subproducto";
                    puedeCrear = false;
                }
            }
            else
            {
                // Está fuera de turno y no puede crear
                puedeCrear = false;
            }

            model.MensajeError = (string.IsNullOrEmpty(model.MensajeError) ? Request["MensajeError"] : "<br>" + model.MensajeError);
            model.MensajeExito = Request["MensajeExito"];
            model.RegistroGeneracionMaxisacos = dc.OPR_RegistroGeneracionMaxisacos.Where(X => X.IdRegistroTurno == registroTurno.IdRegistroTurno && X.Habilitado == true).ToList();
            model.PuedeCrear = (puedeCrear || puedeCrearEspecial);
            model.PuedeEditar = CheckPermiso(210);
            model.PuedeEliminar = CheckPermiso(211);
            model.RegistroTurno = registroTurno;

            model.TurnoAnteriorSiguiente = dc.OPR_GetTurnoAnteriorSiguiente(model.RegistroTurno.Correlativo).SingleOrDefault();

            return View(model);
        }

        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(209);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var registroGeneracionMaxisacos = new OPR_RegistroGeneracionMaxisacos();

            ViewData["registroTurno"] = registroTurno;
            return View("Registrar", registroGeneracionMaxisacos);
        }

        [HttpPost]
        public ActionResult Registrar(int id, OPR_RegistroGeneracionMaxisacos registroGeneracionMaxisacos, FormCollection formValues)
        {
            CheckPermisoAndRedirect(209);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            if (ModelState.IsValid)
            {
                try
                {
                    registroGeneracionMaxisacos.IdRegistroTurno = registroTurno.IdRegistroTurno;
                    registroGeneracionMaxisacos.Habilitado = true;
                    registroGeneracionMaxisacos.FechaHoraIns = DateTime.Now;
                    registroGeneracionMaxisacos.IpIns = RemoteAddr();
                    registroGeneracionMaxisacos.UserIns = User.Identity.Name;
                    dc.OPR_RegistroGeneracionMaxisacos.InsertOnSubmit(registroGeneracionMaxisacos);
                    dc.SubmitChanges();
                    return RedirectToAction("Index", "RegistroGeneracionMaxisacos", new { id = registroTurno.IdRegistroTurno });
                }
                catch
                {
                    var rv = registroGeneracionMaxisacos.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["registroTurno"] = registroTurno;
            return View("Registrar", registroGeneracionMaxisacos);
        }

        public ActionResult EditarRegistro(int id)
        {
            CheckPermisoAndRedirect(210);
            var registroGeneracionMaxisacos = dc.OPR_RegistroGeneracionMaxisacos.SingleOrDefault(x => x.IdRegistroGeneracionMaxisacos == id && x.Habilitado == true);
            if (registroGeneracionMaxisacos == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de subproducto");

            ViewData["registroTurno"] = registroGeneracionMaxisacos.OPR_RegistroTurno;
            return View("Registrar", registroGeneracionMaxisacos);
        }

        [HttpPost]
        public ActionResult EditarRegistro(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(210);
            var registroGeneracionMaxisacos = dc.OPR_RegistroGeneracionMaxisacos.SingleOrDefault(x => x.IdRegistroGeneracionMaxisacos == id && x.Habilitado == true);
            if (registroGeneracionMaxisacos == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de subproducto");

            if (TryUpdateModel(registroGeneracionMaxisacos, new string[] { "Despedradora", "Desperdicio", "Sortex" }))
            {
                try
                {
                    registroGeneracionMaxisacos.UserUpd = User.Identity.Name;
                    registroGeneracionMaxisacos.FechaHoraUpd = DateTime.Now;
                    registroGeneracionMaxisacos.IpUpd = RemoteAddr();

                    dc.SubmitChanges();
                    return RedirectToAction("Index", "RegistroGeneracionMaxisacos", new { id = registroGeneracionMaxisacos.IdRegistroTurno });
                }
                catch
                {
                    var rv = registroGeneracionMaxisacos.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["registroTurno"] = registroGeneracionMaxisacos.OPR_RegistroTurno;
            return View("Registrar", registroGeneracionMaxisacos);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(211);
            var registroGeneracionMaxisacos = dc.OPR_RegistroGeneracionMaxisacos.SingleOrDefault(x => x.IdRegistroGeneracionMaxisacos == id && x.Habilitado == true);
            if (registroGeneracionMaxisacos == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de subproducto");

            string MensajeError = "";
            string MensajeExito = "";
            try
            {
                registroGeneracionMaxisacos.Habilitado = false;
                registroGeneracionMaxisacos.UserUpd = User.Identity.Name;
                registroGeneracionMaxisacos.FechaHoraUpd = DateTime.Now;
                registroGeneracionMaxisacos.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("El registro de subproductos {0} ha sido eliminado", registroGeneracionMaxisacos.IdRegistroGeneracionMaxisacos);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { id = registroGeneracionMaxisacos.IdRegistroTurno, MensajeError = MensajeError, MensajeExito = MensajeExito });
        }
    }
}