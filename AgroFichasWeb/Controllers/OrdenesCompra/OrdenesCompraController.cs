using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.OrdenesCompra;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers.OrdenesCompra
{
    public class OrdenesCompraController : BaseApplicationController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public OrdenesCompraController()
        {
            SetCurrentModulo(8);
        }

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idempresa", Request.QueryString["idempresa"] ?? "" },
                { "IdProyecto", Request.QueryString["IdProyecto"] ?? "" },
                { "IdEstado", Request.QueryString["IdEstado"] ?? "" },
                { "fechaDesde", Request.QueryString["fechaDesde"] ?? "" },
                { "fechaHasta", Request.QueryString["fechaHasta"] ?? "" },
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Index(int? pageIndex, int? idEmpresa, int? idProyecto, int? idEstado, DateTime? fechaDesde, DateTime? fechaHasta, string key = "")
        {
            CheckPermisoAndRedirect(214);
            int pageSize = this.DefaultPageSize;
            if (pageIndex < 1)
                pageIndex = 1;

            var idEmpresaSelect  = idEmpresa ?? 0;
            var idProyectoSelect = idProyecto ?? 0;
            var idEstadoSelect   = idEstado ?? 0;
            var fechaDesdeSelect = (fechaDesde.HasValue ? fechaDesde.Value : new DateTime(2017, 1, 1));
            var fechaHastaSelect = (fechaHasta.HasValue ? fechaHasta.Value : DateTime.Now);

            IQueryable<OC_OrdenCompra> items = from X in dc.OC_OrdenCompra
                                               where (idEmpresaSelect == 0 || X.IdEmpresa == idEmpresaSelect)
                                               && (idProyectoSelect == 0 || X.IdProyecto == idProyectoSelect)
                                               && (idEstadoSelect == 0 || X.IdEstado == idEstadoSelect)
                                               && ((X.FechaHoraIns >= fechaDesdeSelect && X.FechaHoraIns <= fechaHastaSelect))
                                               && (key == "" || X.IdLiquidacion.ToString().Contains(key) || X.IdProveedor.Contains(key) || X.CondicionPago.Contains(key) || (X.OC.HasValue && X.OC.Value.ToString().Contains(key)) || X.Firma.Contains(key))
                                                  orderby X.FechaDocumento ascending
                                                  select X;

            var model = new PaginatedList<OC_OrdenCompra>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idempresa", Request.QueryString["idempresa"] ?? "" },
                { "IdProyecto", Request.QueryString["IdProyecto"] ?? "" },
                { "IdEstado", Request.QueryString["IdEstado"] ?? "" },
                { "fechaDesde", Request.QueryString["fechaDesde"] ?? "" },
                { "fechaHasta", Request.QueryString["fechaHasta"] ?? "" },
            };

            var puedeEditar = CheckPermiso(216);
            var puedeEliminar = CheckPermiso(217);
            var columnas = 11 + (puedeEditar ? 1 : 0) + (puedeEliminar ? 1 : 0);
            ViewData["puedeEditar"] = puedeEditar;
            ViewData["puedeEliminar"] = puedeEliminar;
            ViewData["columnas"] = columnas;
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            SetProyecto(idProyecto);
            SetEstado(idEstado);
            return View(model);
        }

        public ActionResult Editar(int idLiquidacion, int idProyecto)
        {
            CheckPermisoAndRedirect(216);

            string msgErr = "";
            string msgOk = "";

            var model = new OrdenCompraViewModel();

            var proyecto = dc.OC_Proyecto.SingleOrDefault(X => X.IdProyecto == idProyecto);
            if (proyecto == null)
            {
                msgErr = "No se ha encontrado el proyecto";
                return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
            }

            model.IdProyecto = proyecto.IdProyecto;

            if (proyecto.IdProyecto == 1)
            {
                var liquidacion = dc.Liquidacion.SingleOrDefault(X => X.IdLiquidacion == idLiquidacion && X.Nulo == false);
                if (liquidacion == null)
                {
                    msgErr = "No se ha encontrado la liquidación de abastecimiento";
                    return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
                }

                var ordenCompra = dc.OC_OrdenCompra.Where(X => X.IdLiquidacion == idLiquidacion).ToList();
                if (ordenCompra.Count == 0)
                {
                    msgErr = "No se ha encontrado la orden de compra de la liquidación #" + liquidacion.IdLiquidacion;
                    return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
                }

                var primeraGlosa = ordenCompra.FirstOrDefault();

                model.IdLiquidacion = liquidacion.IdLiquidacion;
                model.SetMaterialesSelectList(liquidacion.IdEmpresa, primeraGlosa.IdMaterial, dc);
                model.IdProveedor = primeraGlosa.IdProveedor;
                model.IdMaterial = primeraGlosa.IdMaterial;
                model.CondicionPago = primeraGlosa.CondicionPago;
                model.OC = primeraGlosa.OC;
            }

            if (proyecto.IdProyecto == 2)
            {
                var liquidacion = dc.LOG_Liquidacion.SingleOrDefault(X => X.IdLiquidacion == idLiquidacion);
                if (liquidacion == null)
                {
                    msgErr = "No se ha encontrado la liquidación de logística y corretaje";
                    return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
                }

                var ordenCompra = dc.OC_OrdenCompra.Where(X => X.IdLiquidacion == liquidacion.IdLiquidacion && X.OC_Material.CodigoMaterial != "FIPRPA01" && X.OC_Material.CodigoMaterial != "FIDEPE01").ToList();
                if (ordenCompra.Count == 0)
                {
                    msgErr = "No se ha encontrado la orden de compra de la liquidación #" + liquidacion.IdLiquidacion;
                    return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
                }

                var primeraGlosa = ordenCompra.FirstOrDefault(); // Glosa no automática creada por el usuario

                model.IdLiquidacion = liquidacion.IdLiquidacion;
                model.SetMaterialesSelectList(liquidacion.LOG_Requerimiento.IdEmpresa, primeraGlosa.IdMaterial, dc);
                model.IdProveedor = primeraGlosa.IdProveedor;
                model.IdMaterial = primeraGlosa.IdMaterial;
                model.CondicionPago = primeraGlosa.CondicionPago;
                model.OC = primeraGlosa.OC;

            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Editar(OrdenCompraViewModel ordenCompraViewModel)
        {
            CheckPermisoAndRedirect(216);

            string msgErr = "";
            string msgOk = "";

            if (!ordenCompraViewModel.ValidateFirstLoadEditar(ordenCompraViewModel))
            {
                msgErr = ordenCompraViewModel.ErrorMessage;
                return RedirectToAction("Editar", new { idLiquidacion = ordenCompraViewModel.IdLiquidacion, idProyecto = ordenCompraViewModel.IdProyecto, msgerr = msgErr, msgok = msgOk });
            }

            if (ordenCompraViewModel.IdProyecto == 1)
            {
                var ordenCompra = dc.OC_OrdenCompra.Where(X => X.IdLiquidacion == ordenCompraViewModel.IdLiquidacion).ToList();
                foreach (var glosa in ordenCompra)
                {
                    glosa.IdProveedor = ordenCompraViewModel.IdProveedor;
                    glosa.IdMaterial = ordenCompraViewModel.IdMaterial;
                    glosa.CondicionPago = ordenCompraViewModel.CondicionPago;
                    glosa.OC = ordenCompraViewModel.OC;
                    dc.SubmitChanges();
                }
            }

            if (ordenCompraViewModel.IdProyecto == 2)
            {
                var ordenCompra = dc.OC_OrdenCompra.Where(X => X.IdLiquidacion == ordenCompraViewModel.IdLiquidacion).ToList();
                foreach (var glosa in ordenCompra)
                {
                    if (glosa.OC_Material.CodigoMaterial != "FIPRPA01" && glosa.OC_Material.CodigoMaterial != "FIDEPE01")
                    {
                        glosa.IdMaterial = ordenCompraViewModel.IdMaterial;
                        glosa.CondicionPago = ordenCompraViewModel.CondicionPago;
                    }
                    glosa.IdProveedor = ordenCompraViewModel.IdProveedor;
                    glosa.OC = ordenCompraViewModel.OC;

                    if (ordenCompraViewModel.OC.HasValue)
                    {
                        glosa.IdEstado = 2;
                    }
                    else
                    {
                        glosa.IdEstado = 1;
                    }

                    dc.SubmitChanges();
                }

                var liquidacion = dc.LOG_Liquidacion.Single(X => X.IdLiquidacion == ordenCompraViewModel.IdLiquidacion);
                if (ordenCompraViewModel.OC.HasValue)
                {
                    liquidacion.OC = ordenCompraViewModel.OC;
                    dc.SubmitChanges();
                }
                else
                {
                    liquidacion.OC = null;
                    dc.SubmitChanges();
                }
            }

            msgOk = "Se ha modificado la orden de compra asociada a la liquidación #" + ordenCompraViewModel.IdLiquidacion;

            return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
        }

        public ActionResult Eliminar(int idLiquidacion)
        {
            CheckPermisoAndRedirect(217);

            string msgErr = "";
            string msgOk = "";

            var ordenDeCompra = dc.OC_OrdenCompra.Where(X => X.IdLiquidacion == idLiquidacion).ToList();
            if (ordenDeCompra.Count == 0)
            {
                msgErr = string.Format("La orden de compra de la liquidación #{0} no existe", idLiquidacion);
            }
            else
            {
                try
                {
                    // Eliminando el Xml
                    var PrimeraGlosa = ordenDeCompra.First();
                    var FullPath = string.Format(@"{0}\{1}", ConfigurationManager.AppSettings["OcsPendientes"], string.Format("OC_P2_L{0}_F{1}.xml", PrimeraGlosa.IdLiquidacion, PrimeraGlosa.Firma));
                    if (System.IO.File.Exists(FullPath))
                    {
                        System.IO.File.Delete(FullPath);
                    }

                    foreach (var glosa in ordenDeCompra)
                    {
                        dc.OC_OrdenCompra.DeleteOnSubmit(glosa);
                        dc.SubmitChanges();
                    }

                    msgOk = String.Format("La orden compra de la liquidación #{0} ha sido eliminada", idLiquidacion);
                }
                catch (Exception ex)
                {
                    msgErr = ex.Message;
                }
            }

            return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
        }

        public ActionResult CrearXml(int idLiquidacion, int idProyecto)
        {
            CheckPermisoAndRedirect(214);

            string msgErr = "";
            string msgOk = "";

            var ordenDeCompra = dc.OC_OrdenCompra.Where(X => X.IdLiquidacion == idLiquidacion).ToList();
            if (ordenDeCompra.Count == 0)
            {
                msgErr = string.Format("La orden de compra de la liquidación #{0} no existe", idLiquidacion);
            }
            else
            {
                try
                {
                    // Eliminando el Xml
                    OrdenCompraViewModel ordenCompraViewModel = new OrdenCompraViewModel();
                    ordenCompraViewModel.CrearXmlOrdenDeCompra(ordenDeCompra, dc);

                    msgOk = "El Xml ha sido creado correctamente";
                }
                catch (Exception ex)
                {
                    msgErr = ex.Message;
                }
            }

            return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
        }

        private void SetProyecto(int? IdProyecto)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OC_Proyecto
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdProyecto == IdProyecto && IdProyecto != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdProyecto.ToString()
                                                     };
            ViewData["proyectosList"] = selectList;
        }

        private void SetEstado(int? IdEstado)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OC_Estado
                                                     orderby X.IdEstado
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdEstado == IdEstado && IdEstado != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdEstado.ToString()
                                                     };
            ViewData["estadosList"] = selectList;
        }
    }
}