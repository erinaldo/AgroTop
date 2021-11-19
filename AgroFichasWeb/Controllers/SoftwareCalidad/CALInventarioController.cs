using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALInventarioController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALInventarioController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALInventario
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(308);
            List<CAL_Insumo> list = dc.CAL_Insumo.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(309),
                                                      CheckPermiso(308),
                                                      CheckPermiso(310),
                                                      CheckPermiso(311));
            return View(list);
        }

        public ActionResult AutorizarEntrada(int id, int idInsumoEntrada)
        {
            CheckPermisoAndRedirect(312);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            var insumoEntrada = dc.CAL_InsumoEntrada.SingleOrDefault(X => X.IdInsumoEntrada == idInsumoEntrada);
            if (insumoEntrada == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado la entrada de insumo", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                insumoEntrada.IdEstado = 2;
                insumoEntrada.UserUpd = User.Identity.Name;
                insumoEntrada.FechaHoraUpd = DateTime.Now;
                insumoEntrada.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                // Aumentando Stock
                insumo.Stock += insumoEntrada.Cantidad;
                dc.SubmitChanges();

                // Eliminando CAL_InsumoEntradaRechazado
                foreach (var causaRechazo in dc.CAL_InsumoEntradaRechazado.Where(X => X.IdInsumoEntrada == insumoEntrada.IdInsumoEntrada))
                {
                    dc.CAL_InsumoEntradaRechazado.DeleteOnSubmit(causaRechazo);
                    dc.SubmitChanges();
                }

                okMsg = "La entrada de insumo ha sido autorizada";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Entradas", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult CrearEntrada(int id)
        {
            CheckPermisoAndRedirect(309);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            CAL_InsumoEntrada insumoEntrada = new CAL_InsumoEntrada();
            insumoEntrada.FechaCompra = DateTime.Now;
            insumoEntrada.FechaLlegada = DateTime.Now;
            return View("CrearEntrada", insumoEntrada);
        }

        [HttpPost]
        public ActionResult CrearEntrada(int id, CAL_InsumoEntrada insumoEntrada, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(309);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            if (ModelState.IsValid)
            {
                try
                {
                    insumoEntrada.IdInsumo = insumo.IdInsumo;
                    insumoEntrada.IdEstado = 1;
                    insumoEntrada.FechaHoraIns = DateTime.Now;
                    insumoEntrada.IpIns = RemoteAddr();
                    insumoEntrada.UserIns = User.Identity.Name;
                    dc.CAL_InsumoEntrada.InsertOnSubmit(insumoEntrada);
                    dc.SubmitChanges();

                    if (insumoEntrada.Reingreso.HasValue)
                    {
                        if (insumoEntrada.Reingreso.Value)
                        {
                            var causaReingreso = dc.CAL_InsumoCausaReingreso.Single(X => X.IdCausaReingreso == insumoEntrada.IdCausaReingreso.Value);
                            if (causaReingreso.Reutilizable)
                            {
                                // Aumentando Stock Segunda
                                if (causaReingreso.IdCausaReingreso == 2)
                                    insumo.StockSegunda += insumoEntrada.Cantidad;
                                // Aumentando Stock
                                else if (causaReingreso.IdCausaReingreso == 3)
                                    insumo.Stock += insumoEntrada.Cantidad;
                                dc.SubmitChanges();

                                // Autorizando Entrada
                                insumoEntrada.IdEstado = 2;
                                dc.SubmitChanges();
                            }
                            else
                            {
                                // Aumentando Stock Rechazado
                                insumo.StockRechazado += insumoEntrada.Cantidad;

                                // Rechazando Entrada
                                insumoEntrada.IdEstado = 3;
                                dc.SubmitChanges();
                            }
                        }
                    }

                    return RedirectToAction("Entradas", new { id = id });
                }
                catch
                {
                    var rv = insumoEntrada.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearEntrada", insumoEntrada);
        }

        public ActionResult CrearInsumo()
        {
            CheckPermisoAndRedirect(309);
            CAL_Insumo insumo = new CAL_Insumo();
            return View("CrearInsumo", insumo);
        }

        [HttpPost]
        public ActionResult CrearInsumo(CAL_Insumo insumo, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(309);
            if (ModelState.IsValid)
            {
                try
                {
                    insumo.Autorizado = false;
                    insumo.Stock = 0;
                    insumo.Habilitado = true;
                    insumo.FechaHoraIns = DateTime.Now;
                    insumo.IpIns = RemoteAddr();
                    insumo.UserIns = User.Identity.Name;
                    dc.CAL_Insumo.InsertOnSubmit(insumo);
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = insumo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearInsumo", insumo);
        }

        public ActionResult CrearSalida(int id)
        {
            CheckPermisoAndRedirect(309);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            if (insumo.Stock == 0)
                return RedirectToAction("Salidas", new { id = id, errMsg = "El insumo no tiene stock", okMsg = "" });

            CAL_InsumoSalida insumoSalida = new CAL_InsumoSalida();
            return View("CrearSalida", insumoSalida);
        }

        [HttpPost]
        public ActionResult CrearSalida(int id, CAL_InsumoSalida insumoSalida, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(309);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            if (insumo.Stock < insumoSalida.Cantidad)
                return RedirectToAction("Salidas", new { id = id, errMsg = "No puedes retirar más insumos de los que hay en stock", okMsg = "" });

            if (ModelState.IsValid)
            {
                try
                {
                    insumoSalida.IdInsumo = insumo.IdInsumo;
                    insumoSalida.FechaHoraIns = DateTime.Now;
                    insumoSalida.IpIns = RemoteAddr();
                    insumoSalida.UserIns = User.Identity.Name;
                    dc.CAL_InsumoSalida.InsertOnSubmit(insumoSalida);
                    dc.SubmitChanges();

                    // Restando Stock
                    insumo.Stock -= insumoSalida.Cantidad;
                    dc.SubmitChanges();

                    return RedirectToAction("Salidas", new { id = id });
                }
                catch
                {
                    var rv = insumoSalida.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearSalida", insumoSalida);
        }

        public ActionResult EditarInsumo(int id)
        {
            CheckPermisoAndRedirect(310);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }
            return View("CrearInsumo", insumo);
        }

        [HttpPost]
        public ActionResult EditarInsumo(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(310);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            try
            {
                UpdateModel(insumo, new string[] { "IdTipoInsumo", "Nombre" });

                insumo.UserUpd = User.Identity.Name;
                insumo.FechaHoraUpd = DateTime.Now;
                insumo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = insumo.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("CrearInsumo", insumo);
        }

        public ActionResult EliminarEntrada(int id, int idInsumoEntrada)
        {
            CheckPermisoAndRedirect(311);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            var insumoEntrada = dc.CAL_InsumoEntrada.SingleOrDefault(X => X.IdInsumoEntrada == idInsumoEntrada);
            if (insumoEntrada == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado la entrada de insumo", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                if (insumoEntrada.IdEstado != 3 || (insumoEntrada.Reingreso.HasValue && insumoEntrada.Reingreso.Value && insumoEntrada.IdCausaReingreso != 1))
                {
                    // Restando Stock
                    insumo.Stock -= insumoEntrada.Cantidad;
                    dc.SubmitChanges();
                }

                dc.CAL_InsumoEntrada.DeleteOnSubmit(insumoEntrada);
                dc.SubmitChanges();
                okMsg = "La entrada de insumo ha sido eliminada";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Entradas", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult EliminarInsumo(int id)
        {
            CheckPermisoAndRedirect(311);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                insumo.Habilitado = false;
                insumo.UserUpd = User.Identity.Name;
                insumo.FechaHoraUpd = DateTime.Now;
                insumo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El insumo {0} ha sido eliminado", insumo.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult EliminarSalida(int id, int idInsumoSalida)
        {
            CheckPermisoAndRedirect(311);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            var insumoSalida = dc.CAL_InsumoSalida.SingleOrDefault(X => X.IdInsumoSalida == idInsumoSalida);
            if (insumoSalida == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado la salida de insumo", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                // Aumentando Stock
                insumo.Stock += insumoSalida.Cantidad;
                dc.SubmitChanges();

                dc.CAL_InsumoSalida.DeleteOnSubmit(insumoSalida);
                dc.SubmitChanges();
                okMsg = "La salida de insumo ha sido eliminada";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Salidas", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Entradas(int id)
        {
            CheckPermisoAndRedirect(308);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            List<CAL_InsumoEntrada> list = dc.CAL_InsumoEntrada.Where(X => X.IdInsumo == id).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(308),
                Crear = CheckPermiso(309),
                Actualizar = CheckPermiso(310),
                Borrar = CheckPermiso(311),
                AutorizarRechazarEntradaItem = CheckPermiso(312)
            };

            ViewData["insumo"] = insumo;
            return View(list);
        }

        public ActionResult RechazarEntrada(int id, int idInsumoEntrada)
        {
            CheckPermisoAndRedirect(312);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            var insumoEntrada = dc.CAL_InsumoEntrada.SingleOrDefault(X => X.IdInsumoEntrada == idInsumoEntrada);
            if (insumoEntrada == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado la entrada de insumo", okMsg = "" }); }

            ViewData["insumo"] = insumo;
            ViewData["causaRechazoList"] = dc.CAL_InsumoCausaRechazo.OrderBy(X => X.Descripcion).ToList();
            return View("RechazarEntrada", insumoEntrada);
        }

        [HttpPost]
        public ActionResult RechazarEntrada(int id, int idInsumoEntrada, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(312);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            var insumoEntrada = dc.CAL_InsumoEntrada.SingleOrDefault(X => X.IdInsumoEntrada == idInsumoEntrada);
            if (insumoEntrada == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado la entrada de insumo", okMsg = "" }); }

            if (string.IsNullOrEmpty(formCollection["causaRechazo"]))
            {
                return RedirectToAction("RechazarEntrada", new { id = id, idInsumoEntrada = idInsumoEntrada, errMsg = "Debe seleccionar al menos una causa de rechazo", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                insumoEntrada.IdEstado = 3;
                insumoEntrada.UserUpd = User.Identity.Name;
                insumoEntrada.FechaHoraUpd = DateTime.Now;
                insumoEntrada.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                var causaRechazoList = formCollection["causaRechazo"].Split(new char[] { ',' });
                foreach (var causaRechazo in causaRechazoList)
                {
                    CAL_InsumoEntradaRechazado insumoEntradaRechazado = new CAL_InsumoEntradaRechazado()
                    {
                        IdInsumoEntrada = insumoEntrada.IdInsumoEntrada,
                        IdCausaRechazo = causaRechazo.ToInt(0),
                        UserIns = User.Identity.Name,
                        FechaHoraIns = DateTime.Now,
                        IpIns = RemoteAddr()
                    };
                    dc.CAL_InsumoEntradaRechazado.InsertOnSubmit(insumoEntradaRechazado);
                    dc.SubmitChanges();
                }

                okMsg = "La entrada de insumo ha sido rechazada";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Entradas", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Salidas(int id)
        {
            CheckPermisoAndRedirect(308);
            var insumo = dc.CAL_Insumo.SingleOrDefault(X => X.IdInsumo == id && X.Habilitado == true);
            if (insumo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el insumo", okMsg = "" }); }

            List<CAL_InsumoSalida> list = dc.CAL_InsumoSalida.Where(X => X.IdInsumo == id).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(309),
                                                      CheckPermiso(308),
                                                      CheckPermiso(310),
                                                      CheckPermiso(311));
            ViewData["insumo"] = insumo;
            return View(list);
        }
    }
}