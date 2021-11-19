using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALProductosController : BaseApplicationController
    {
        //
        // GET: /CALProductos/

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALProductosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(260);
            List<CAL_Producto> list = dc.CAL_Producto.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(261),
                                                      CheckPermiso(260),
                                                      CheckPermiso(262),
                                                      CheckPermiso(263));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(261);
            CAL_Producto producto = new CAL_Producto();
            return View("crear", producto);
        }

        [HttpPost]
        public ActionResult Crear(CAL_Producto producto)
        {
            CheckPermisoAndRedirect(261);
            if (ModelState.IsValid)
            {
                try
                {
                    producto.Nombre = producto.Nombre.ToUpperInvariant();

                    producto.Habilitado = true;
                    producto.FechaHoraIns = DateTime.Now;
                    producto.IpIns = RemoteAddr();
                    producto.UserIns = User.Identity.Name;
                    dc.CAL_Producto.InsertOnSubmit(producto);
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

            return View("crear", producto);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(262);
            var producto = dc.CAL_Producto.SingleOrDefault(X => X.IdProducto == id && X.Habilitado == true);
            if (producto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el producto", okMsg = "" }); }
            return View("crear", producto);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(262);
            var producto = dc.CAL_Producto.SingleOrDefault(X => X.IdProducto == id && X.Habilitado == true);
            if (producto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el producto", okMsg = "" }); }

            try
            {
                UpdateModel(producto, new string[] { "Nombre" });

                producto.Nombre = producto.Nombre.ToUpperInvariant();

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

            return View("crear", producto);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(263);
            var producto = dc.CAL_Producto.SingleOrDefault(X => X.IdProducto == id && X.Habilitado == true);
            if (producto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el producto", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                producto.Habilitado = false;
                producto.UserUpd = User.Identity.Name;
                producto.FechaHoraUpd = DateTime.Now;
                producto.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El producto {0} ha sido eliminado", producto.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}
