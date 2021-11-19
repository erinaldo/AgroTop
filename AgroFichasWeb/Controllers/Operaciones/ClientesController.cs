using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class ClientesController : BaseApplicationController
    {
        //
        // GET: /Clientes/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public ClientesController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(195);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OPR_Cliente> items = dc.OPR_Cliente.Where(X => X.Habilitado).OrderBy(a => a.IdCliente);
            var pagina = new PaginatedList<OPR_Cliente>(items, pageIndex, pageSize);

            ViewData["MensajeError"]  = Request["MensajeError"];
            ViewData["MensajeExito"]  = Request["MensajeExito"];
            ViewData["PuedeCrear"]    = CheckPermiso(196);
            ViewData["PuedeEditar"]   = CheckPermiso(197);
            ViewData["PuedeEliminar"] = CheckPermiso(198);
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(196);
            var cliente = new OPR_Cliente();
            return View("crear", cliente);
        }

        [HttpPost]
        public ActionResult Crear(OPR_Cliente cliente)
        {
            CheckPermisoAndRedirect(196);
            if (ModelState.IsValid)
            {
                try
                {
                    cliente.Habilitado = true;
                    cliente.FechaHoraIns = DateTime.Now;
                    cliente.IpIns = RemoteAddr();
                    cliente.UserIns = User.Identity.Name;
                    dc.OPR_Cliente.InsertOnSubmit(cliente);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = cliente.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", cliente);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(197);
            var cliente = dc.OPR_Cliente.SingleOrDefault(x => x.IdCliente == id);
            if (cliente == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el cliente"); }

            return View("crear", cliente);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(197);
            var cliente = dc.OPR_Cliente.SingleOrDefault(x => x.IdCliente == id);
            if (cliente == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el cliente"); }

            try
            {
                UpdateModel(cliente, new string[] { "Nombre" });

                cliente.UserUpd = User.Identity.Name;
                cliente.FechaHoraUpd = DateTime.Now;
                cliente.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = cliente.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", cliente);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(198);
            var cliente = dc.OPR_Cliente.SingleOrDefault(x => x.IdCliente == id);
            if (cliente == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el cliente"); }

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                cliente.Habilitado = false;
                cliente.UserUpd = User.Identity.Name;
                cliente.FechaHoraUpd = DateTime.Now;
                cliente.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("El cliente {0} ha sido eliminado", cliente.Nombre);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }
    }
}
