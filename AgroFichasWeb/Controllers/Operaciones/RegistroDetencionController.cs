using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class RegistroDetencionController : BaseApplicationController
    {
        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        //
        // GET: /RegistroDetencion/

        public RegistroDetencionController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(186);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var model = new RegistroDetencionViewModel();
            var Turno = ResolveTurno(dc);
            var registroTurnoActual = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == DateTime.Now.Date && X.IdTurno == Turno.IdTurno && X.Habilitado == true);

            var puedeCrear = CheckPermiso(187);
            var puedeCrearEspecial = CheckPermiso(190);

            // Si el registro de turno es igual al registro de turno actual
            if (registroTurnoActual == registroTurno)
            {
                // Si el usuario que creó el registro de turno y el usuario actual son distintos
                if (registroTurno.UserID != CurrentUser.UserID && !CheckPermiso(190))
                {
                    // No puede crear porque es diferente el operador
                    model.MensajeError = "Solo el operador creador del turno puede registar las detenciones";
                    puedeCrear = false;
                }
            }
            else
            {
                // Está fuera de turno y no puede crear
                puedeCrear = false;
            }

            model.MensajeError        = (string.IsNullOrEmpty(model.MensajeError) ? Request["MensajeError"] : "<br>" + model.MensajeError);
            model.MensajeExito        = Request["MensajeExito"];
            model.RegistroDetenciones = dc.OPR_RegistroDetencion.Where(X => X.IdRegistroTurno == registroTurno.IdRegistroTurno && X.Habilitado == true).ToList();
            model.PuedeCrear          = (puedeCrear || puedeCrearEspecial);
            model.PuedeEditar         = CheckPermiso(188);
            model.PuedeEliminar       = CheckPermiso(189);
            model.RegistroTurno       = registroTurno;

            model.TurnoAnteriorSiguiente = dc.OPR_GetTurnoAnteriorSiguiente(model.RegistroTurno.Correlativo).SingleOrDefault();

            return View(model);
        }

        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(187);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var registroDetencion = new OPR_RegistroDetencion();

            ViewData["areaList"] = SetArea(null);
            ViewData["lineaProduccionList"] = SetLineaProduccion(null);
            ViewData["tipoDetencionList"] = SetTipoDetencion(null);
            ViewData["causaDetencionList"] = SetCausaDetencion(null);
            ViewData["registroTurno"] = registroTurno;
            return View("Registrar", registroDetencion);
        }

        [HttpPost]
        public ActionResult Registrar(int id, OPR_RegistroDetencion registroDetencion, FormCollection formValues)
        {
            CheckPermisoAndRedirect(187);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            if (ModelState.IsValid)
            {
                try
                {
                    if (registroDetencion.Observaciones == null)
                        registroDetencion.Observaciones = "";
                    if (registroDetencion.HoraPartida == null)
                        registroDetencion.HoraPartida = null;

                    registroDetencion.IdRegistroTurno = registroTurno.IdRegistroTurno;
                    registroDetencion.Habilitado = true;
                    registroDetencion.FechaHoraIns = DateTime.Now;
                    registroDetencion.IpIns = RemoteAddr();
                    registroDetencion.UserIns = User.Identity.Name;
                    dc.OPR_RegistroDetencion.InsertOnSubmit(registroDetencion);
                    dc.SubmitChanges();
                    return RedirectToAction("Index", "RegistroDetencion", new { id = registroTurno.IdRegistroTurno });
                }
                catch
                {
                    var rv = registroDetencion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["areaList"] = SetArea(registroDetencion.IdArea);
            ViewData["lineaProduccionList"] = SetLineaProduccion(registroDetencion.IdLineaProduccion);
            ViewData["tipoDetencionList"] = SetTipoDetencion(registroDetencion.IdTipoDetencion);
            ViewData["causaDetencionList"] = SetCausaDetencion(registroDetencion.IdCausaDetencion);
            ViewData["registroTurno"] = registroTurno;
            return View("Registrar", registroDetencion);
        }

        public ActionResult EditarRegistro(int id)
        {
            CheckPermisoAndRedirect(188);
            var registroDetencion = dc.OPR_RegistroDetencion.SingleOrDefault(x => x.IdRegistroDetencion == id && x.Habilitado == true);
            if (registroDetencion == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de detención");

            ViewData["areaList"] = SetArea(registroDetencion.IdArea);
            ViewData["lineaProduccionList"] = SetLineaProduccion(registroDetencion.IdLineaProduccion);
            ViewData["tipoDetencionList"] = SetTipoDetencion(registroDetencion.IdTipoDetencion);
            ViewData["causaDetencionList"] = SetCausaDetencion(registroDetencion.IdCausaDetencion);
            ViewData["registroTurno"] = registroDetencion.OPR_RegistroTurno;
            return View("Registrar", registroDetencion);
        }

        [HttpPost]
        public ActionResult EditarRegistro(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(188);
            var registroDetencion = dc.OPR_RegistroDetencion.SingleOrDefault(x => x.IdRegistroDetencion == id && x.Habilitado == true);
            if (registroDetencion == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de detención");

            try
            {
                UpdateModel(registroDetencion, new string[] { "IdTipoDetencion", "IdLineaProduccion", "IdCausaDetencion", "IdEquipo", "HoraDetencion", "HoraPartida", "Observaciones", "IdArea" });

                if (registroDetencion.Observaciones == null)
                    registroDetencion.Observaciones = "";
                if (registroDetencion.HoraPartida == null)
                    registroDetencion.HoraPartida = null;

                registroDetencion.UserUpd = User.Identity.Name;
                registroDetencion.FechaHoraUpd = DateTime.Now;
                registroDetencion.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("Index", "RegistroDetencion", new { id = registroDetencion.IdRegistroTurno });
            }
            catch
            {
                var rv = registroDetencion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["areaList"] = SetArea(registroDetencion.IdArea);
            ViewData["lineaProduccionList"] = SetLineaProduccion(registroDetencion.IdLineaProduccion);
            ViewData["tipoDetencionList"] = SetTipoDetencion(registroDetencion.IdTipoDetencion);
            ViewData["causaDetencionList"] = SetCausaDetencion(registroDetencion.IdCausaDetencion);
            ViewData["registroTurno"] = registroDetencion.OPR_RegistroTurno;
            return View("Registrar", registroDetencion);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(189);
            var registroDetencion = dc.OPR_RegistroDetencion.SingleOrDefault(x => x.IdRegistroDetencion == id && x.Habilitado == true);
            if (registroDetencion == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de detención");

            string MensajeError = "";
            string MensajeExito = "";
            try
            {
                registroDetencion.Habilitado = false;
                registroDetencion.UserUpd = User.Identity.Name;
                registroDetencion.FechaHoraUpd = DateTime.Now;
                registroDetencion.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("El registro de detención {0} ha sido eliminado", registroDetencion.IdRegistroDetencion);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { id = registroDetencion.IdRegistroTurno, MensajeError = MensajeError, MensajeExito = MensajeExito });
        }

        private IEnumerable<SelectListItem> SetArea(int? IdArea)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OPR_Area
                                                     orderby X.IdArea
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdArea == IdArea && IdArea != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdArea.ToString()
                                                     };
            return selectList;
        }

        private IEnumerable<SelectListItem> SetLineaProduccion(int? IdLineaProduccion)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OPR_LineaProduccion
                                                     where X.Habilitado == true
                                                     orderby X.IdLineaProduccion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdLineaProduccion == IdLineaProduccion && IdLineaProduccion != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdLineaProduccion.ToString()
                                                     };
            return selectList;
        }

        private IEnumerable<SelectListItem> SetTipoDetencion(int? IdTipoDetencion)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OPR_TipoDetencion
                                                     orderby X.IdTipoDetencion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoDetencion == IdTipoDetencion && IdTipoDetencion != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdTipoDetencion.ToString()
                                                     };
            return selectList;
        }

        private IEnumerable<SelectListItem> SetCausaDetencion(int? IdCausaDetencion)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OPR_CausaDetencion
                                                     orderby X.IdCausaDetencion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCausaDetencion == IdCausaDetencion && IdCausaDetencion != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdCausaDetencion.ToString()
                                                     };
            return selectList;
        }

        public JsonResult GetEquipos(int idLineaProduccion, int idEquipo)
        {
            try
            {
                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem { Text = "-- Seleccione Equipo --", Value = "", Selected = false });

                var equipos = (from x in dc.OPR_Equipo
                               join y in dc.OPR_EquipoPorLineaProduccion on x.IdEquipo equals y.IdEquipo
                               join z in dc.OPR_LineaProduccion on y.IdLineaProduccion equals z.IdLineaProduccion
                               where x.Habilitado == true
                               && z.Habilitado == true
                               && z.IdLineaProduccion == idLineaProduccion
                               select x);
                foreach (var equipo in equipos)
                {
                    items.Add(new SelectListItem { Text = equipo.Descripcion, Value = equipo.IdEquipo.ToString(), Selected = (equipo.IdEquipo == idEquipo && idEquipo != 0) });
                }

                return Json(items);
            }
            catch
            {
                return null;
            }
        }
    }
}
