using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers.Fichas
{
    [WebsiteAuthorize]
    public class ClientesEmpresaController : BaseApplicationController
    {
        //
        // GET: /ClientesEmpresa/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ClientesEmpresaController()
        {
            SetCurrentModulo(1);
        }

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex",  Request.QueryString["pageIndex"] ?? "0" },
                { "idEmpresa",  Request.QueryString["idEmpresa"] ?? "" },
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Index(int? pageIndex, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(238);
            int pageSize = this.DefaultPageSize;
            if (pageIndex < 1)
                pageIndex = 1;

            var idEmpresaSelect = idEmpresa ?? 0;

            IQueryable<Cliente> items = (from X in dc.Cliente
                                         join Y in dc.ClienteEmpresa on X.IdCliente equals Y.IdCliente
                                         where (idEmpresaSelect == 0 || Y.IdEmpresa == idEmpresaSelect)
                                         && (key == "" ||
                                         X.DNI.Contains(key) ||
                                         X.RazonSocial.Contains(key) ||
                                         X.PaisCodigo.Contains(key) ||
                                         X.Telefono.Contains(key) ||
                                         X.Telefono2.Contains(key) ||
                                         X.EmailCliente.Contains(key) ||
                                         X.EmailContactoComercial.Contains(key))
                                         orderby X.IdCliente ascending
                                         select X).Distinct();
            var pagina = new PaginatedList<Cliente>(items, pageIndex, pageSize);
            pagina.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", pagina.PageIndex },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
            };

            var columnas = 8 + (CheckPermiso(240) ? 1 : 0) + (CheckPermiso(241) ? 1 : 0);
            ViewData["columnas"] = columnas;
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["MensajeError"] = Request["MensajeError"];
            ViewData["MensajeExito"] = Request["MensajeExito"];
            ViewData["PuedeCrear"] = CheckPermiso(239);
            ViewData["PuedeEditar"] = CheckPermiso(240);
            ViewData["PuedeEliminar"] = CheckPermiso(241);
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(239);
            var cliente = new Cliente();
            return View("crear", cliente);
        }

        [HttpPost]
        public ActionResult Crear(Cliente cliente, FormCollection formValues)
        {
            CheckPermisoAndRedirect(239);
            if (ModelState.IsValid && cliente.ValidacionEmpresas(ModelState, System.Web.HttpContext.Current))
            {
                try
                {
                    if (string.IsNullOrEmpty(cliente.DNI))
                        cliente.DNI = "";

                    if (string.IsNullOrEmpty(cliente.Telefono))
                        cliente.Telefono = "";

                    if (string.IsNullOrEmpty(cliente.Telefono2))
                        cliente.Telefono2 = "";

                    if (string.IsNullOrEmpty(cliente.EmailCliente))
                        cliente.EmailCliente = "";

                    if (string.IsNullOrEmpty(cliente.EmailContactoComercial))
                        cliente.EmailContactoComercial = "";

                    cliente.SetDefaults();

                    cliente.Habilitado = true;
                    cliente.FechaHoraIns = DateTime.Now;
                    cliente.IpIns = RemoteAddr();
                    cliente.UserIns = User.Identity.Name;
                    dc.Cliente.InsertOnSubmit(cliente);
                    dc.SubmitChanges();

                    string[] ids = { };

                    if (formValues["chkEmpresa"] != null && formValues["chkEmpresa"] != "")
                        ids = formValues["chkEmpresa"].Split(',');

                    foreach (string idx in ids)
                    {
                        var newEmpresa = new ClienteEmpresa()
                        {
                            IdCliente = cliente.IdCliente,
                            IdEmpresa = int.Parse(idx),
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        };
                        cliente.ClienteEmpresa.Add(newEmpresa);
                    }

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
            else
            {

            }

            return View("crear", cliente);
        }
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(240);
            var cliente = dc.Cliente.SingleOrDefault(x => x.IdCliente == id);
            if (cliente == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el cliente"); }

            return View("crear", cliente);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(240);
            var cliente = dc.Cliente.SingleOrDefault(x => x.IdCliente == id);
            if (cliente == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el cliente"); }

            if (cliente.ValidacionEmpresas(ModelState, System.Web.HttpContext.Current))
            {
                try
                {
                    UpdateModel(cliente, new string[] { "DNI", "RazonSocial", "PaisCodigo", "Telefono", "Telefono2", "EmailCliente", "EmailContactoComercial", "IDOleotop", "IDAvenatop", "IDGranotop", "IDSaprosem", "IDICI" });

                    if (string.IsNullOrEmpty(cliente.DNI))
                        cliente.DNI = "";

                    if (string.IsNullOrEmpty(cliente.Telefono))
                        cliente.Telefono = "";

                    if (string.IsNullOrEmpty(cliente.Telefono2))
                        cliente.Telefono2 = "";

                    if (string.IsNullOrEmpty(cliente.EmailCliente))
                        cliente.EmailCliente = "";

                    if (string.IsNullOrEmpty(cliente.EmailContactoComercial))
                        cliente.EmailContactoComercial = "";

                    cliente.SetDefaults();

                    cliente.UserUpd = User.Identity.Name;
                    cliente.FechaHoraUpd = DateTime.Now;
                    cliente.IpUpd = RemoteAddr();
                    dc.SubmitChanges();

                    string[] ids = { };

                    if (formValues["chkEmpresa"] != null && formValues["chkEmpresa"] != "")
                        ids = formValues["chkEmpresa"].Split(',');

                    foreach (var clienteEmpresa in cliente.ClienteEmpresa)
                        if (ids.SingleOrDefault(idx => idx == clienteEmpresa.IdEmpresa.ToString()) == null)
                            dc.ClienteEmpresa.DeleteOnSubmit(clienteEmpresa);

                    foreach (string idx in ids)
                    {
                        if (cliente.ClienteEmpresa.SingleOrDefault(c => c.IdEmpresa == int.Parse(idx)) == null)
                        {
                            var newEmpresa = new ClienteEmpresa()
                            {
                                IdCliente = cliente.IdCliente,
                                IdEmpresa = int.Parse(idx),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = User.Identity.Name
                            };
                            cliente.ClienteEmpresa.Add(newEmpresa);
                        }
                    }

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

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(241);
            var cliente = dc.Cliente.SingleOrDefault(x => x.IdCliente == id);
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
                MensajeExito = String.Format("El cliente {0} ha sido eliminado", cliente.RazonSocial);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }
    }
}
