using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class ProductosController : BaseApplicationController
    {
        //
        // GET: /Capacidades/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public ProductosController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(199);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OPR_Producto> items = dc.OPR_Producto.Where(X => X.Habilitado).OrderBy(a => a.IdProducto);
            var pagina = new PaginatedList<OPR_Producto>(items, pageIndex, pageSize);

            ViewData["MensajeError"]  = Request["MensajeError"];
            ViewData["MensajeExito"]  = Request["MensajeExito"];
            ViewData["PuedeCrear"]    = CheckPermiso(200);
            ViewData["PuedeEditar"]   = CheckPermiso(201);
            ViewData["PuedeEliminar"] = CheckPermiso(202);

            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(200);
            var producto = new OPR_Producto();
            ViewData["tipoProductosList"] = SetTipoProductos(null);
            return View("crear", producto);
        }

        [HttpPost]
        public ActionResult Crear(OPR_Producto producto)
        {
            CheckPermisoAndRedirect(200);
            if (ModelState.IsValid)
            {
                try
                {
                    producto.Habilitado = true;
                    producto.FechaHoraIns = DateTime.Now;
                    producto.IpIns = RemoteAddr();
                    producto.UserIns = User.Identity.Name;
                    dc.OPR_Producto.InsertOnSubmit(producto);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = producto.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["tipoProductosList"] = SetTipoProductos(producto.IdTipoProducto);
            return View("crear", producto);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(201);
            var producto = dc.OPR_Producto.SingleOrDefault(x => x.IdProducto == id);
            if (producto == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el producto"); }
            ViewData["tipoProductosList"] = SetTipoProductos(producto.IdTipoProducto);
            return View("crear", producto);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(201);
            var producto = dc.OPR_Producto.SingleOrDefault(x => x.IdProducto == id);
            if (producto == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el producto"); }

            try
            {
                UpdateModel(producto, new string[] { "IdTipoProducto", "Descripcion" });

                producto.UserUpd = User.Identity.Name;
                producto.FechaHoraUpd = DateTime.Now;
                producto.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = producto.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["tipoProductosList"] = SetTipoProductos(producto.IdTipoProducto);
            return View("crear", producto);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(202);
            var producto = dc.OPR_Producto.SingleOrDefault(x => x.IdProducto == id );
            if (producto == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el producto"); }

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                producto.Habilitado = false;
                producto.UserUpd = User.Identity.Name;
                producto.FechaHoraUpd = DateTime.Now;
                producto.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("El producto {0} ha sido eliminado", producto.Descripcion);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }

        private IEnumerable<SelectListItem> SetTipoProductos(int? IdTipoProducto)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OPR_TipoProducto
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoProducto == IdTipoProducto && IdTipoProducto != null),
                                                         Text = string.Format("{0}", X.Descripcion),
                                                         Value = X.IdTipoProducto.ToString()
                                                     };
            return selectList;
        }
    }
}
