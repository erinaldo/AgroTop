using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.ControlTiempo
{
    [WebsiteAuthorize]
    public class CTProductosController : BaseApplicationController
    {
        //
        // GET: /CTProductos/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public CTProductosController()
        {
            SetCurrentModulo(9);
        }

        public ActionResult Index(int? id)
        {
            List<PlantaUsuario> pu = dc.PlantaUsuario.Where(x => x.UserID == CurrentUser.UserID).ToList();
            

            CheckPermisoAndRedirect(228);
            List<CTR_Producto> items = (from p in dc.CTR_Producto                                      //dc.CTR_Producto.Where(X => X.Habilitado == true).ToList();
                                        join pp in dc.CTR_ProductoPlanta on p.IdProducto equals pp.IdProducto
                                        join puu in dc.PlantaUsuario on pp.IdPlantaProduccion equals puu.IdPlantaProduccion
                                        where (p.Habilitado == true) &&
                                        (puu.UserID == CurrentUser.UserID)
                                        select p).Distinct().ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["UserID"] = CurrentUser.UserID;
            ViewData["plantasUsuario"] = pu;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(229),
                                                      CheckPermiso(228),
                                                      CheckPermiso(230),
                                                      CheckPermiso(231));
            return View(items);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(229);
            var producto = new CTR_Producto();

            ViewData["PlantasProduccion"] = producto.GetPlantas(CurrentUser.UserID, producto.IdProducto);
            return View("crear", producto);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(CTR_Producto producto, FormCollection formValues)
        {
            CheckPermisoAndRedirect(229);
            if (ModelState.IsValid && producto.ValidacionEntrada(ModelState, System.Web.HttpContext.Current))
            {
                try
                {
                    producto.Habilitado = true;
                    producto.FechaHoraIns = DateTime.Now;
                    producto.IpIns = RemoteAddr();
                    producto.UserIns = User.Identity.Name;
                    dc.CTR_Producto.InsertOnSubmit(producto);
                    dc.SubmitChanges();

                    string[] idsEmpresa = { };

                    if (formValues["chkEmpresa"] != null && formValues["chkEmpresa"] != "")
                        idsEmpresa = formValues["chkEmpresa"].Split(',');

                    foreach (string idx in idsEmpresa)
                    {
                        var newEmpresa = new CTR_ProductoEmpresa()
                        {
                            IdProducto = producto.IdProducto,
                            IdEmpresa = int.Parse(idx),
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        };
                        producto.CTR_ProductoEmpresa.Add(newEmpresa);
                    }

                    dc.SubmitChanges();

                    string[] idsEnvase = { };

                    if (formValues["chkEnvase"] != null && formValues["chkEnvase"] != "")
                        idsEnvase = formValues["chkEnvase"].Split(',');

                    foreach (string idx in idsEnvase)
                    {
                        var newEnvase = new CTR_ProductoEnvase()
                        {
                            IdProducto = producto.IdProducto,
                            IdEnvase = int.Parse(idx),
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        };
                        producto.CTR_ProductoEnvase.Add(newEnvase);
                    }

                    dc.SubmitChanges();

                    string[] idsPlantas = { };

                    if (formValues["chkPlanta"] != null && formValues["chkPlanta"] != "")
                        idsPlantas = formValues["chkPlanta"].Split(',');

                    foreach (string idx in idsPlantas)
                    {
                        var newPlanta = new CTR_ProductoPlanta()
                        {
                            IdProducto = producto.IdProducto,
                            IdPlantaProduccion = int.Parse(idx),
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        };
                        producto.CTR_ProductoPlanta.Add(newPlanta);
                    }

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

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(230);

            var producto = dc.CTR_Producto.SingleOrDefault(X => X.IdProducto == id);

            if (producto == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el producto", okMsg = "" });
            }

            ViewData["PlantasProduccion"] = producto.GetPlantas(CurrentUser.UserID, producto.IdProducto);

            return View("crear", producto);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(230);

            var producto = dc.CTR_Producto.SingleOrDefault(X => X.IdProducto == id);
            if (producto == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el producto", okMsg = "" });
            }

            if (producto.ValidacionEntrada(ModelState, System.Web.HttpContext.Current))
            {
                try
                {
                    UpdateModel(producto, new string[] { "Nombre" });

                    producto.UserUpd = User.Identity.Name;
                    producto.FechaHoraUpd = DateTime.Now;
                    producto.IpUpd = RemoteAddr();
                    dc.SubmitChanges();

                    string[] idsEmpresa = { };

                    if (formValues["chkEmpresa"] != null && formValues["chkEmpresa"] != "")
                        idsEmpresa = formValues["chkEmpresa"].Split(',');

                    foreach (var productoEmpresa in producto.CTR_ProductoEmpresa)
                        if (idsEmpresa.SingleOrDefault(idx => idx == productoEmpresa.IdEmpresa.ToString()) == null)
                            dc.CTR_ProductoEmpresa.DeleteOnSubmit(productoEmpresa);

                    foreach (string idx in idsEmpresa)
                    {
                        if (producto.CTR_ProductoEmpresa.SingleOrDefault(c => c.IdEmpresa == int.Parse(idx)) == null)
                        {
                            var newEmpresa = new CTR_ProductoEmpresa()
                            {
                                IdProducto = producto.IdProducto,
                                IdEmpresa = int.Parse(idx),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = User.Identity.Name
                            };
                            producto.CTR_ProductoEmpresa.Add(newEmpresa);
                        }
                    }

                    dc.SubmitChanges();

                    string[] idsEnvase = { };

                    if (formValues["chkEnvase"] != null && formValues["chkEnvase"] != "")
                        idsEnvase = formValues["chkEnvase"].Split(',');

                    foreach (var productoEnvase in producto.CTR_ProductoEnvase)
                        if (idsEnvase.SingleOrDefault(idx => idx == productoEnvase.IdEnvase.ToString()) == null)
                            dc.CTR_ProductoEnvase.DeleteOnSubmit(productoEnvase);

                    foreach (string idx in idsEnvase)
                    {
                        if (producto.CTR_ProductoEnvase.SingleOrDefault(c => c.IdEnvase == int.Parse(idx)) == null)
                        {
                            var newEnvase = new CTR_ProductoEnvase()
                            {
                                IdProducto = producto.IdProducto,
                                IdEnvase = int.Parse(idx),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = User.Identity.Name
                            };
                            producto.CTR_ProductoEnvase.Add(newEnvase);
                        }
                    }

                    dc.SubmitChanges();

                    string[] idsPlanta = { };

                    if (formValues["chkPlanta"] != null && formValues["chkPlanta"] != "")
                        idsPlanta = formValues["chkPlanta"].Split(',');

                    foreach (var productoPlanta in producto.CTR_ProductoPlanta)
                        if (idsPlanta.SingleOrDefault(idx => idx == productoPlanta.IdPlantaProduccion.ToString()) == null)
                            dc.CTR_ProductoPlanta.DeleteOnSubmit(productoPlanta);

                    foreach (string idx in idsPlanta)
                    {
                        if (producto.CTR_ProductoPlanta.SingleOrDefault(c => c.IdPlantaProduccion == int.Parse(idx)) == null)
                        {
                            var newPlanta = new CTR_ProductoPlanta()
                            {
                                IdProducto = producto.IdProducto,
                                IdPlantaProduccion = int.Parse(idx),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = User.Identity.Name
                            };
                            producto.CTR_ProductoPlanta.Add(newPlanta);
                        }
                    }

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

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(231);

            var producto = dc.CTR_Producto.SingleOrDefault(X => X.IdProducto == id);
            if (producto == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el producto", okMsg = "" });
            }

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