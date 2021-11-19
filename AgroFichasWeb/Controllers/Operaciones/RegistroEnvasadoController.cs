using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class RegistroEnvasadoController : BaseApplicationController
    {
        //
        // GET: /RegistroEnvasado/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public RegistroEnvasadoController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(203);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var model = new RegistroEnvasadoViewModel();
            var Turno = ResolveTurno(dc);
            var registroTurnoActual = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == DateTime.Now.Date && X.IdTurno == Turno.IdTurno && X.Habilitado == true);

            var puedeCrear = CheckPermiso(204);
            var puedeCrearEspecial = CheckPermiso(207);

            // Si el registro de turno es igual al registro de turno actual
            if (registroTurnoActual == registroTurno)
            {
                // Si el usuario que creó el registro de turno y el usuario actual son distintos
                if (registroTurno.UserID != CurrentUser.UserID && !CheckPermiso(207))
                {
                    // No puede crear porque es diferente el operador
                    model.MensajeError = "Solo el operador creador del turno puede registar el envasado";
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
            model.RegistroEnvasados = dc.OPR_RegistroEnvasado.Where(X => X.IdRegistroTurno == registroTurno.IdRegistroTurno && X.Habilitado == true).ToList();
            model.PuedeCrear = (puedeCrear || puedeCrearEspecial);
            model.PuedeEditar = CheckPermiso(188);
            model.PuedeEliminar = CheckPermiso(189);
            model.RegistroTurno = registroTurno;

            model.TurnoAnteriorSiguiente = dc.OPR_GetTurnoAnteriorSiguiente(model.RegistroTurno.Correlativo).SingleOrDefault();

            return View(model);
        }

        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(204);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var registroDetencion = new OPR_RegistroEnvasado();

            ViewData["productosList"] = SetProductos(null);
            ViewData["clientesList"] = SetClientes(null);
            ViewData["registroTurno"] = registroTurno;
            return View("Registrar", registroDetencion);
        }

        [HttpPost]
        public ActionResult Registrar(int id, OPR_RegistroEnvasado registroEnvasado, FormCollection formValues)
        {
            CheckPermisoAndRedirect(204);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            if (ModelState.IsValid)
            {
                try
                {
                    registroEnvasado.IdRegistroTurno = registroTurno.IdRegistroTurno;
                    registroEnvasado.Habilitado = true;
                    registroEnvasado.FechaHoraIns = DateTime.Now;
                    registroEnvasado.IpIns = RemoteAddr();
                    registroEnvasado.UserIns = User.Identity.Name;
                    dc.OPR_RegistroEnvasado.InsertOnSubmit(registroEnvasado);
                    dc.SubmitChanges();
                    return RedirectToAction("Index", "RegistroEnvasado", new { id = registroTurno.IdRegistroTurno });
                }
                catch
                {
                    var rv = registroEnvasado.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["productosList"] = SetProductos(registroEnvasado.IdProducto);
            ViewData["clientesList"] = SetClientes(registroEnvasado.IdCliente);
            ViewData["registroTurno"] = registroTurno;
            return View("Registrar", registroEnvasado);
        }

        public ActionResult EditarRegistro(int id)
        {
            CheckPermisoAndRedirect(205);
            var registroEnvasado = dc.OPR_RegistroEnvasado.SingleOrDefault(x => x.IdRegistroEnvasado == id && x.Habilitado == true);
            if (registroEnvasado == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de envasado");

            ViewData["productosList"] = SetProductos(registroEnvasado.IdProducto);
            ViewData["clientesList"] = SetClientes(registroEnvasado.IdCliente);
            ViewData["registroTurno"] = registroEnvasado.OPR_RegistroTurno;
            return View("Registrar", registroEnvasado);
        }

        [HttpPost]
        public ActionResult EditarRegistro(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(205);
            var registroEnvasado = dc.OPR_RegistroEnvasado.SingleOrDefault(x => x.IdRegistroEnvasado == id && x.Habilitado == true);
            if (registroEnvasado == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de envasado");

            if (TryUpdateModel(registroEnvasado, new string[] { "IdProducto", "IdCliente", "Lote", "EnvasadoSac", "Peso", "Retenido" }))
            {
                try
                {
                    registroEnvasado.UserUpd = User.Identity.Name;
                    registroEnvasado.FechaHoraUpd = DateTime.Now;
                    registroEnvasado.IpUpd = RemoteAddr();

                    dc.SubmitChanges();
                    return RedirectToAction("Index", "RegistroEnvasado", new { id = registroEnvasado.IdRegistroTurno });
                }
                catch
                {
                    var rv = registroEnvasado.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["productosList"] = SetProductos(registroEnvasado.IdProducto);
            ViewData["clientesList"] = SetClientes(registroEnvasado.IdCliente);
            ViewData["registroTurno"] = registroEnvasado.OPR_RegistroTurno;
            return View("Registrar", registroEnvasado);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(206);
            var registroEnvasado = dc.OPR_RegistroEnvasado.SingleOrDefault(x => x.IdRegistroEnvasado == id && x.Habilitado == true);
            if (registroEnvasado == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el registro de envasado");

            string MensajeError = "";
            string MensajeExito = "";
            try
            {
                registroEnvasado.Habilitado = false;
                registroEnvasado.UserUpd = User.Identity.Name;
                registroEnvasado.FechaHoraUpd = DateTime.Now;
                registroEnvasado.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("El registro de envasado {0} ha sido eliminado", registroEnvasado.IdRegistroEnvasado);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { id = registroEnvasado.IdRegistroTurno, MensajeError = MensajeError, MensajeExito = MensajeExito });
        }

        private IEnumerable<SelectListItem> SetClientes(int? IdCliente)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OPR_Cliente
                                                     where X.Habilitado == true
                                                     orderby X.IdCliente
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCliente == IdCliente && IdCliente != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdCliente.ToString()
                                                     };
            return selectList;
        }

        private IEnumerable<SelectListItem> SetProductos(int? IdProducto)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OPR_Producto
                                                     where X.Habilitado == true
                                                     orderby X.IdProducto
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdProducto == IdProducto && IdProducto != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdProducto.ToString()
                                                     };
            return selectList;
        }
    }
}