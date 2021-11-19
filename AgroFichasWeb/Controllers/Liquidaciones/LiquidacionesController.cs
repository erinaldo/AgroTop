using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using AgroFichasWeb.ViewModels.Liquidaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class LiquidacionesController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public LiquidacionesController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        #region Index

        /*
         * Index
         * *******************************************************************/

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "idCultivoContrato", Request.QueryString["idCultivoContrato"] ?? "" },
                { "idEstadoLiquidacion", Request.QueryString["idEstadoLiquidacion"] ?? ""}
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Index(int? pageIndex, int? idTemporada, int? idEmpresa, int? idCultivoContrato, int? idEstadoLiquidacion, string key = "")
        {
            CheckPermisoAndRedirect(47);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Search
            var rut = key.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var idCultivoContratoSelect = idCultivoContrato ?? 0;
            var idEstadoLiquidacionSelect = idEstadoLiquidacion ?? 0;
            bool estado = false;
            

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<Liquidacion> items;
            if (adminTodos)
            {
                if (idEstadoLiquidacion == 2)
                {
                    idEstadoLiquidacionSelect = 0;
                    estado = true;
                    items = from liq in dc.Liquidacion
                            where liq.IdTemporada == temporada.IdTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && (liq.TotalDescuentos > 0)
                               && ((idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               || (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => dc.ProcesoIngreso.Single(y => y.IdProcesoIngreso == x.IdProcesoIngreso).IdCultivoContrato == idCultivoContrato)))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                            orderby liq.IdLiquidacion descending
                            select liq;
                }
                else
                {
                    items = from liq in dc.Liquidacion
                            where liq.IdTemporada == temporada.IdTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && ((idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               || (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => dc.ProcesoIngreso.Single(y => y.IdProcesoIngreso == x.IdProcesoIngreso).IdCultivoContrato == idCultivoContrato)))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                            orderby liq.IdLiquidacion descending
                            select liq;
                }
            }
            else
            {
                if (idEstadoLiquidacion == 2)
                {
                    idEstadoLiquidacionSelect = 0;
                    estado = true;
                    items = from liq in dc.Liquidacion
                            join ua in dc.UsuarioAgricultor on liq.IdAgricultor equals ua.IdAgricultor
                            join su in dc.SYS_User on ua.UserID equals su.UserID
                            where liq.IdTemporada == temporada.IdTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && (liq.TotalDescuentos > 0)
                               && ((idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               || (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => dc.ProcesoIngreso.Single(y => y.IdProcesoIngreso == x.IdProcesoIngreso).IdCultivoContrato == idCultivoContrato)))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                               && ua.UserID == CurrentUser.UserID
                            orderby liq.IdLiquidacion descending
                            select liq;
                }
                else
                {
                    items = from liq in dc.Liquidacion
                            join ua in dc.UsuarioAgricultor on liq.IdAgricultor equals ua.IdAgricultor
                            join su in dc.SYS_User on ua.UserID equals su.UserID
                            where liq.IdTemporada == temporada.IdTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && ((idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               || (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => dc.ProcesoIngreso.Single(y => y.IdProcesoIngreso == x.IdProcesoIngreso).IdCultivoContrato == idCultivoContrato)))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                               && ua.UserID == CurrentUser.UserID
                            orderby liq.IdLiquidacion descending
                            select liq;
                }
            }

            var model = new PaginatedList<Liquidacion>(items, pageIndex, this.DefaultPageSize);
            if(estado)
            {
                idEstadoLiquidacionSelect = 2;
            }
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "idCultivoContrato", Request.QueryString["idCultivoContrato"] ?? "" },
                { "idEstadoLiquidacion", Request.QueryString["idEstadoLiquidacion"] ?? ""}
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["idEstadoLiquidacion"] = idEstadoLiquidacionSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["estadosLiquidacion"] = dc.EstadoLiquidacion.ToList();
            ViewData["key"] = key;
            SetCultivoContrato(idCultivoContratoSelect);

            return View(model);
        }

        public ActionResult LiquidacionesDescuentos(int? pageIndex, int? idTemporada, int? idEmpresa, int? idCultivoContrato, int? idEstadoLiquidacion, string key = "")
        {
            CheckPermisoAndRedirect(47);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Search
            var rut = key.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var idCultivoContratoSelect = idCultivoContrato ?? 0;
            var idEstadoLiquidacionSelect = idEstadoLiquidacion ?? 0;

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<Liquidacion> items;

            bool estado = false;

            if (adminTodos)
            {
                if (idEstadoLiquidacion == 2)
                {
                    idEstadoLiquidacionSelect = 0;
                    estado = true;
                    items = from liq in dc.Liquidacion
                            where liq.IdTemporada == temporada.IdTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && (liq.TotalDescuentos > 0)
                               && ((idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               || (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => dc.ProcesoIngreso.Single(y => y.IdProcesoIngreso == x.IdProcesoIngreso).IdCultivoContrato == idCultivoContrato)))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                               && liq.TotalDescuentos > 0
                            orderby liq.IdLiquidacion descending
                            select liq;
                }
                else
                {
                    items = from liq in dc.Liquidacion
                            where liq.IdTemporada == temporada.IdTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && ((idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               || (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => dc.ProcesoIngreso.Single(y => y.IdProcesoIngreso == x.IdProcesoIngreso).IdCultivoContrato == idCultivoContrato)))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                               && liq.TotalDescuentos > 0
                            orderby liq.IdLiquidacion descending
                            select liq;
                }
            }
            else
            {
                if (idEstadoLiquidacion == 2)
                {
                    idEstadoLiquidacionSelect = 0;
                    estado = true;
                    items = from liq in dc.Liquidacion
                            join ua in dc.UsuarioAgricultor on liq.IdAgricultor equals ua.IdAgricultor
                            join su in dc.SYS_User on ua.UserID equals su.UserID
                            where liq.IdTemporada == temporada.IdTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && (liq.TotalDescuentos > 0)
                               && ((idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               || (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => dc.ProcesoIngreso.Single(y => y.IdProcesoIngreso == x.IdProcesoIngreso).IdCultivoContrato == idCultivoContrato)))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                               && ua.UserID == CurrentUser.UserID
                               && liq.TotalDescuentos > 0
                            orderby liq.IdLiquidacion descending
                            select liq;
                }
                else
                {
                    items = from liq in dc.Liquidacion
                            join ua in dc.UsuarioAgricultor on liq.IdAgricultor equals ua.IdAgricultor
                            join su in dc.SYS_User on ua.UserID equals su.UserID
                            where liq.IdTemporada == temporada.IdTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && ((idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               || (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => dc.ProcesoIngreso.Single(y => y.IdProcesoIngreso == x.IdProcesoIngreso).IdCultivoContrato == idCultivoContrato)))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                               && ua.UserID == CurrentUser.UserID
                               && liq.TotalDescuentos > 0
                            orderby liq.IdLiquidacion descending
                            select liq;
                }

            }

            var model = new PaginatedList<Liquidacion>(items, pageIndex, this.DefaultPageSize);

            if (estado)
            {
                idEstadoLiquidacionSelect = 2;
            }

            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "idCultivoContrato", Request.QueryString["idCultivoContrato"] ?? "" },
                { "idEstadoLiquidacion", Request.QueryString["idEstadoLiquidacion"] ?? ""}
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["idEstadoLiquidacion"] = idEstadoLiquidacionSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["estadosLiquidacion"] = dc.EstadoLiquidacion.ToList();
            ViewData["key"] = key;
            SetCultivoContrato(idCultivoContratoSelect);

            return View(model);
        }

        public ActionResult IndexExport(int idTemporada, int? idEmpresa, int? idCultivoContrato, int? idEstadoLiquidacion, string key = "")
        {
            CheckPermisoAndRedirect(47);

            var idEmpresaSelect = idEmpresa ?? 0;
            var idCultivoContratoSelect = idCultivoContrato ?? 0;
            var idEstadoLiquidacionSelect = idEstadoLiquidacion ?? 0;

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<LiquidacionesExportModel> items;

            bool estado = false;

            if (adminTodos)
            {
                if (idEstadoLiquidacion == 2)
                {
                    idEstadoLiquidacionSelect = 0;
                    estado = true;
                    items = from liq in dc.Liquidacion
                            where liq.IdTemporada == idTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && (liq.TotalDescuentos > 0)
                               && (idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               && (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => x.IdCultivoContrato == idCultivoContratoSelect))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                            orderby liq.IdLiquidacion descending
                            select LiquidacionesExportModel.FromLiquidacion(liq);
                }
                else
                {
                    items = from liq in dc.Liquidacion
                            where liq.IdTemporada == idTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && (idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               && (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => x.IdCultivoContrato == idCultivoContratoSelect))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                            orderby liq.IdLiquidacion descending
                            select LiquidacionesExportModel.FromLiquidacion(liq);
                }
            }
            else
            {
                if (idEstadoLiquidacion == 2)
                {
                    items = from liq in dc.Liquidacion
                            join ua in dc.UsuarioAgricultor on liq.IdAgricultor equals ua.IdAgricultor
                            join su in dc.SYS_User on ua.UserID equals su.UserID
                            where liq.IdTemporada == idTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && (liq.TotalDescuentos > 0)
                               && (idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               && (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => x.IdCultivoContrato == idCultivoContratoSelect))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                               && ua.UserID == CurrentUser.UserID
                            orderby liq.IdLiquidacion descending
                            select LiquidacionesExportModel.FromLiquidacion(liq);
                }
                else
                {
                    items = from liq in dc.Liquidacion
                            join ua in dc.UsuarioAgricultor on liq.IdAgricultor equals ua.IdAgricultor
                            join su in dc.SYS_User on ua.UserID equals su.UserID
                            where liq.IdTemporada == idTemporada
                               && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                               && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                               && (idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                               && (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => x.IdCultivoContrato == idCultivoContratoSelect))
                               && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                               && ua.UserID == CurrentUser.UserID
                            orderby liq.IdLiquidacion descending
                            select LiquidacionesExportModel.FromLiquidacion(liq);
                }
            }

            return new CsvActionResult<LiquidacionesExportModel>(items.ToList(), "Liquidaciones.csv", 1, ';', null);
        }

        public ActionResult NominaExport(int idTemporada, int? idEmpresa, int? idCultivoContrato, int? idEstadoLiquidacion, string key = "")
        {
            CheckPermisoAndRedirect(47);

            var idEmpresaSelect = idEmpresa ?? 0;
            var idCultivoContratoSelect = idCultivoContrato ?? 0;
            var idEstadoLiquidacionSelect = idEstadoLiquidacion ?? 0;

            IQueryable<NominasExportModel> items;
            
            if (idEstadoLiquidacion == 2)
            {
                items = from doctoliq in dc.DoctoLiquidacion
                        join liq in dc.Liquidacion on doctoliq.IdLiquidacion equals liq.IdLiquidacion
                        where liq.IdTemporada == idTemporada
                           && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                           && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                           && (liq.TotalDescuentos > 0)
                           && (idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                           && (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => x.IdCultivoContrato == idCultivoContratoSelect))
                           && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                        orderby liq.IdLiquidacion descending
                        select NominasExportModel.FromDoctoLiquidacion(doctoliq);
            }
            else
            {
                items = from doctoliq in dc.DoctoLiquidacion
                        join liq in dc.Liquidacion on doctoliq.IdLiquidacion equals liq.IdLiquidacion
                        where liq.IdTemporada == idTemporada
                           && (idEmpresaSelect == 0 || liq.IdEmpresa == idEmpresaSelect)
                           && (idEstadoLiquidacionSelect == 0 || liq.IdEstado == idEstadoLiquidacionSelect)
                           && (idCultivoContratoSelect == 0 || liq.PrecioIngreso.Any(x => x.ProcesoIngreso.IdCultivoContrato == idCultivoContratoSelect))
                           && (idCultivoContratoSelect == 0 || liq.PrecioIngresoNulo.Any(x => x.IdCultivoContrato == idCultivoContratoSelect))
                           && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                        orderby liq.IdLiquidacion descending
                        select NominasExportModel.FromDoctoLiquidacion(doctoliq);
            }

            return new CsvActionResult<NominasExportModel>(items.ToList(), "Nomina.csv", 1, ';', null);
        }

        public ActionResult DetalleLiquidacion(int id)
        {
            CheckPermisoAndRedirect(47);
            var model = dc.Liquidacion.Single(li => li.IdLiquidacion == id);
            return View(model);
        }

        public ActionResult DetalleDescuentos(int id)
        {
            CheckPermisoAndRedirect(47);
            var model = dc.Liquidacion.Single(li => li.IdLiquidacion == id);
            return View(model);
        }

        public ActionResult PrintLiquidacion(int id, int copias = 1)
        {
            CheckPermisoAndRedirect(47);
            var model = dc.Liquidacion.Single(li => li.IdLiquidacion == id);
            ViewData["copias"] = copias;
            return View("DetalleLiquidacionPrintable", model);
        }

        public ActionResult Anular(int id)
        {
            CheckPermisoAndRedirect(50);

            var routeValues = new RouteValueDictionary();
            try
            {
                var liquidacion = dc.Liquidacion.Single(l => l.IdLiquidacion == id);
                var idsIngresos = liquidacion.PrecioIngreso.Select(pi => pi.IdProcesoIngreso).Distinct().ToArray();

                liquidacion.Nulo = true;
                liquidacion.UserNulo = CurrentUser.UserName;
                liquidacion.IpNulo = RemoteAddr();
                liquidacion.FechaHoraNulo = DateTime.Now;

                foreach (var precio in liquidacion.PrecioIngreso)
                {
                    //Guardamos una copia para que las liquidaciones nulas mantengan su información original
                    var precioNulo = new PrecioIngresoNulo()
                    {
                        IdPrecioIngreso = precio.IdPrecioIngreso,
                        IdLiquidacion = precio.IdLiquidacion.Value,
                        IdProcesoIngreso = precio.IdProcesoIngreso,
                        IdItemContrato = precio.IdItemContrato,
                        IdConvenioPrecio = precio.IdConvenioPrecio,
                        Cantidad = precio.Cantidad,
                        PrecioUnidad = precio.PrecioUnidad,
                        IdMoneda = precio.IdMoneda,
                        UserIns = precio.UserIns,
                        FechaHoraIns = precio.FechaHoraIns,
                        IpIns = precio.IpIns,
                        UserUpd = precio.UserUpd,
                        FechaHoraUpd = precio.FechaHoraUpd,
                        IpUpd = precio.IpUpd,
                        TasaCambio = precio.TasaCambio,
                        TotalNeto = precio.TotalNeto,
                        SobrePrecioPor = precio.SobrePrecioPor,
                        SobrePrecioTotal = precio.SobrePrecioTotal,
                        DescuentoPor = precio.DescuentoPor,
                        DescuentoTotal = precio.DescuentoTotal,
                        TotalMonedaPago = precio.TotalMonedaPago(),
                        IdMonedaPago = precio.IdMonedaPago(),
                        IdCultivoContrato = precio.ProcesoIngreso.IdCultivoContrato,
                        FechaHoraLlegada = precio.ProcesoIngreso.FechaHoraLlegada.Value,
                        NumeroGuia = precio.ProcesoIngreso.NumeroGuia,
                        PesoBruto = precio.ProcesoIngreso.PesoBruto.Value,
                        NumeroContrato = precio.ItemContrato.Contrato.NumeroContrato,
                        NombreContrato = precio.ItemContrato.Contrato.Agricultor.Nombre
                    };

                    foreach (var valor in precio.ProcesoIngreso.ValorAnalisis)
                    {
                        precioNulo.ValorAnalisisNulo.Add(new ValorAnalisisNulo()
                        {
                            IdProcesoIngreso = valor.IdProcesoIngreso,
                            IdParametroAnalisis = valor.IdParametroAnalisis,
                            Valor = valor.Valor,
                            UserIns = valor.UserIns,
                            FechaHoraIns = valor.FechaHoraIns,
                            IpIns = valor.IpIns,
                            UserUpd = valor.UserUpd,
                            FechaHoraUpd = valor.FechaHoraUpd,
                            IpUpd = valor.IpUpd
                        });
                    }

                    liquidacion.PrecioIngresoNulo.Add(precioNulo);

                    //Anular
                    precio.IdLiquidacion = null;
                }

                //Guardamos una copia para que las liquidaciones nulas mantengan su información original
                foreach (var descuento in liquidacion.DescuentoLiquidacion)
                {
                    liquidacion.DescuentoLiquidacionNulo.Add(new DescuentoLiquidacionNulo()
                    {
                        IdDescuentoLiquidacion = descuento.IdDescuentoLiquidacion,
                        IdLiquidacion = descuento.IdLiquidacion,
                        IdDescuento = descuento.IdDescuento,
                        Monto = descuento.Monto,
                        UserIns = descuento.UserIns,
                        FechaHoraIns = descuento.FechaHoraIns,
                        IpIns = descuento.IpIns,
                        UserUpd = descuento.UserUpd,
                        FechaHoraUpd = descuento.FechaHoraUpd,
                        IpUpd = descuento.IpUpd,
                        RutAgricultorDescuento = descuento.Descuento.Agricultor.Rut,
                        NombreAgricultorDescuento = descuento.Descuento.Agricultor.Nombre,
                        NombreTipoDescuento = descuento.Descuento.TipoDescuento.Nombre,
                        NumeroDocumento = descuento.Descuento.NumeroDocumento,
                        Institucion = descuento.Descuento.Institucion,
                        IdMoneda = descuento.Descuento.IdMoneda
                    });
                }

                //Guardamos una copia para que las liquidaciones nulas mantengan su información original
                foreach (var saldo in liquidacion.SaldoCtaCteLiquidacion)
                {
                    liquidacion.SaldoCtaCteLiquidacionNulo.Add(new SaldoCtaCteLiquidacionNulo()
                    {
                        IdSaldoCtaCteLiquidacion = saldo.IdSaldoCtaCteLiquidacion,
                        IdLiquidacion = saldo.IdLiquidacion,
                        IdSaldoCtaCte = saldo.IdSaldoCtaCte,
                        Monto = saldo.Monto,
                        UserIns = saldo.UserIns,
                        FechaHoraIns = saldo.FechaHoraIns,
                        IpIns = saldo.IpIns,
                        UserUpd = saldo.UserUpd,
                        FechaHoraUpd = saldo.FechaHoraUpd,
                        IpUpd = saldo.IpUpd,
                        RutAgricultorSaldoCtaCte = saldo.SaldoCtaCte.Agricultor.Rut,
                        NombreAgricultorSaldoCtaCte = saldo.SaldoCtaCte.Agricultor.Nombre,
                        NombreEmpresa = saldo.SaldoCtaCte.Empresa.Nombre
                    });
                }

                //Anular
                dc.DescuentoLiquidacion.DeleteAllOnSubmit(liquidacion.DescuentoLiquidacion);
                dc.SaldoCtaCteLiquidacion.DeleteAllOnSubmit(liquidacion.SaldoCtaCteLiquidacion);

                dc.SubmitChanges();

                foreach (var idProcesoIngreso in idsIngresos)
                {
                    var ingreso = dc.ProcesoIngreso.Single(ing => ing.IdProcesoIngreso == idProcesoIngreso);
                    ingreso.UpdatePesoYNetoLiquidado();
                    dc.SubmitChanges();
                }

                //Notifica Liquidación Anulada
                liquidacion.NotificaLiquidacionAnulada();

                routeValues.Add("msgok", "La liquidación fue anulada con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible anular la liquidación";
                if (ex.Message.Contains("FK_"))
                    msgerr += " porque tiene al menos un *** asociado";
                else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            return RedirectToAction("Index", IndexRouteValues(routeValues));
        }

        private void SetCultivoContrato(int? IdCultivoContrato)
        {
            var selectList = dc.CultivoContratoEmpresaParaFiltro.ToList();
            var js = new JavaScriptSerializer();
            ViewData["cultivoContratoList"] = js.Serialize(selectList);
            ViewData["idCultivoContratoSelect"] = IdCultivoContrato;
        }
        #endregion

        #region Pendientes Liquidación

        /*
         * Pendientes Liquidación
         * *******************************************************************/

        private RouteValueDictionary PendientesRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Pendientes(int? pageIndex, int? idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(46);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Search
            var rut = key.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            //Items
            var items = dc.AgricultoresPendientesLiquidacion(temporada.IdTemporada, idEmpresa, key, rut).ToList();

            var model = new PaginatedList<AgricultoresPendientesLiquidacionResult>(items, pageIndex, 1000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;


            return View(model);
        }

        public ActionResult Liquidar(int id, int idTemporadaLiq, int idEmpresaLiq, int idCultivo)
        {
            CheckPermisoAndRedirect(48);

            var model = new LiquidarViewModel(dc, id, idEmpresaLiq, idTemporadaLiq, idCultivo);

            ViewData["indexRouteValues"] = PendientesRouteValues(null);

            return View(model);
        }

        [HttpPost]
        public ActionResult Liquidar(LiquidarViewModel model)
        {
            CheckPermisoAndRedirect(48);

            model.LoadLookups(dc);
            model.Validate(ModelState, dc);

            if (ModelState.IsValid)
            {
                var liquidacion = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("liquidarfin", new { id = liquidacion.IdLiquidacion });
            }
            else
            {
                ViewData["indexRouteValues"] = PendientesRouteValues(null);
                return View(model);
            }
        }

        public ActionResult LiquidarFin(int id)
        {
            CheckPermisoAndRedirect(48);
            var liquidacion = dc.Liquidacion.Single(li => li.IdLiquidacion == id);

            return View(liquidacion);
        }

        public ActionResult EditarIngresos(int id)
        {
            CheckPermisoAndRedirect(70);

            var model = new LiquidarViewModel(dc, id);

            ViewData["indexRouteValues"] = PendientesRouteValues(null);
            return View("Liquidar", model);
        }

        public ActionResult EditarRetencion(int id)
        {
            CheckPermisoAndRedirect(85);

            var model = new RetencionViewModel(dc, id);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditarRetencion(RetencionViewModel model)
        {
            CheckPermisoAndRedirect(85);

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var liquidacion = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("editarretencionfin", new { id = liquidacion.IdLiquidacion });
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult EditarRetencionFin(int id)
        {
            CheckPermisoAndRedirect(85);
            var liquidacion = dc.Liquidacion.Single(li => li.IdLiquidacion == id);

            return View(liquidacion);
        }

        #endregion

        #region Asignar Descuentos

        /*
         * ASIGNAR DESCUENTOS
         * *******************************************************************/

        private RouteValueDictionary AsignarDescuentosRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult AsignarDescuentosIndex(int? pageIndex, int? idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(59);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Search
            var rut = key.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from lq in dc.Liquidacion
                        where lq.IdTemporada == temporada.IdTemporada
                           && !lq.Nulo
                           && !lq.TotalDescuentos.HasValue
                           && (idEmpresaSelect == 0 || lq.IdEmpresa == idEmpresaSelect)
                           && (key == "" || lq.Agricultor.Nombre.Contains(key) || lq.IdLiquidacion.ToString() == key || lq.Agricultor.Rut.Contains(rut))
                        orderby lq.IdLiquidacion
                        select lq;

            var model = new PaginatedList<Liquidacion>(items, pageIndex, 1000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult AsignarDescuentos(int id)
        {
            CheckPermisoAndRedirect(59);

            var model = new AsignarDescuentosViewModel(dc, id);

            ViewData["indexRouteValues"] = AsignarDescuentosRouteValues(null);

            return View(model);
        }

        [HttpPost]
        public ActionResult AsignarDescuentos(AsignarDescuentosViewModel model)
        {
            CheckPermisoAndRedirect(59);

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var liquidacion = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("asignardescuentosfin", new { id = liquidacion.IdLiquidacion });
            }
            else
            {
                ViewData["indexRouteValues"] = AsignarDescuentosRouteValues(null);
                return View(model);
            }
        }


        public ActionResult AsignarDescuentosFin(int id)
        {
            CheckPermisoAndRedirect(59);
            var liquidacion = dc.Liquidacion.Single(li => li.IdLiquidacion == id);

            return View(liquidacion);
        }

        public ActionResult EditarDescuentos(int id)
        {
            CheckPermisoAndRedirect(74);

            var model = new AsignarDescuentosViewModel(dc, id);

            ViewData["indexRouteValues"] = AsignarDescuentosRouteValues(null);

            return View(model);
        }

        public ActionResult AnularDescuentos(int id)
        {
            CheckPermisoAndRedirect(74);

            var liquidacion = dc.Liquidacion.Single(li => li.IdLiquidacion == id);
            if (!liquidacion.EsDescuentosAnulable())
                throw new Exception("No es posible anular estos descuentos. Su estado no lo permite: " + liquidacion.EstadoLiquidacion.Nombre);

            dc.AnularDescuentosLiquidacion(liquidacion.IdLiquidacion);

            return RedirectToAction("anulardescuentosfin", new { id = liquidacion.IdLiquidacion });
        }

        public ActionResult AnularDescuentosFin(int id)
        {
            CheckPermisoAndRedirect(74);
            var liquidacion = dc.Liquidacion.Single(li => li.IdLiquidacion == id);

            return View(liquidacion);
        }

        #endregion

        #region Autorizar Ingresos

        /*
         * AUTORIZAR INGRESOS
         * *******************************************************************/

        private RouteValueDictionary AutorizarIngresosRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult AutorizarIngresosIndex(int? pageIndex, int? idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(68);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Search
            var rut = key.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from lq in dc.Liquidacion
                        where lq.IdTemporada == temporada.IdTemporada
                           && !lq.Nulo
                           && !lq.AutorizadaIngresos.HasValue
                           && (idEmpresaSelect == 0 || lq.IdEmpresa == idEmpresaSelect)
                           && (key == "" || lq.Agricultor.Nombre.Contains(key) || lq.IdLiquidacion.ToString() == key || lq.Agricultor.Rut.Contains(rut))
                        orderby lq.IdLiquidacion
                        select lq;

            var model = new PaginatedList<Liquidacion>(items, pageIndex, 1000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult AutorizarIngresos(int id)
        {
            CheckPermisoAndRedirect(68);

            var model = new AutorizarIngresosViewModel(dc, id);

            ViewData["indexRouteValues"] = AutorizarIngresosRouteValues(null);

            return View(model);
        }

        [HttpPost]
        public ActionResult AutorizarIngresos(AutorizarIngresosViewModel model)
        {
            CheckPermisoAndRedirect(68);

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var liquidacion = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("autorizaringresosindex", new { idTemporada = model.Liquidacion.IdTemporada, msgok = String.Format("Los ingresos han sido {0} con éxito", liquidacion.AutorizadaIngresos.Value ? "autorizados" : "rechazados") });
            }
            else
            {
                ViewData["indexRouteValues"] = AutorizarIngresosRouteValues(null);
                return View(model);
            }
        }
        #endregion

        #region Autorizar Descuentos

        /*
         * AUTORIZAR DESCUENTOS
         * *******************************************************************/

        private RouteValueDictionary AutorizarDescuentosRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult AutorizarDescuentosIndex(int? pageIndex, int? idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(69);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Search
            var rut = key.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from lq in dc.Liquidacion
                        where lq.IdTemporada == temporada.IdTemporada
                           && !lq.Nulo
                           && lq.TotalDescuentos.HasValue
                           && !lq.AutorizadaDescuentos.HasValue
                           && (idEmpresaSelect == 0 || lq.IdEmpresa == idEmpresaSelect)
                           && (key == "" || lq.Agricultor.Nombre.Contains(key) || lq.IdLiquidacion.ToString() == key || lq.Agricultor.Rut.Contains(rut))
                        orderby lq.IdLiquidacion
                        select lq;

            var model = new PaginatedList<Liquidacion>(items, pageIndex, 1000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult AutorizarDescuentos(int id)
        {
            CheckPermisoAndRedirect(69);

            var model = new AutorizarDescuentosViewModel(dc, id);

            ViewData["indexRouteValues"] = AutorizarDescuentosRouteValues(null);

            return View(model);
        }

        [HttpPost]
        public ActionResult AutorizarDescuentos(AutorizarDescuentosViewModel model)
        {
            CheckPermisoAndRedirect(69);

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var liquidacion = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("autorizardescuentosindex", new { idTemporada = model.Liquidacion.IdTemporada, msgok = String.Format("Los descuentos han sido {0} con éxito", liquidacion.AutorizadaDescuentos.Value ? "autorizados" : "rechazados") });
            }
            else
            {
                ViewData["indexRouteValues"] = AutorizarDescuentosRouteValues(null);
                return View(model);
            }
        }

        public ActionResult AutorizarDescuentosRapido(int id)
        {
            CheckPermisoAndRedirect(69);

            var model = new AutorizarDescuentosViewModel(dc, id);
            model.Autorizado = true;

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var liquidacion = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("autorizardescuentosindex", new { idTemporada = model.Liquidacion.IdTemporada, msgok = String.Format("Los descuentos han sido {0} con éxito", liquidacion.AutorizadaDescuentos.Value ? "autorizados" : "rechazados") });
            }
            else
            {
                ViewData["indexRouteValues"] = AutorizarDescuentosRouteValues(null);
                return View(model);
            }
        }
        #endregion

        #region Doctos Liquidación

        public ActionResult CrearDocto(int id)
        {
            CheckPermisoAndRedirect(99);
            var model = new CrearDoctoLiquidacionViewModel(dc, id);

            return View("DoctoLiquidacion", model);
        }

        [HttpPost]
        public ActionResult CrearDocto([Bind(Exclude = "Fecha, FechaPagoEspecial")] CrearDoctoLiquidacionViewModel model)
        {
            CheckPermisoAndRedirect(99);

            model.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();
            model.FechaPagoEspecial = DateParser.DateFromRequest("FechaPagoEspecial").GetValueOrDefault();

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var docto = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("creardoctofin", new { id = docto.IdLiquidacion });
            }
            else
            {
                //ViewData["indexRouteValues"] = AsignarDescuentosRouteValues(null);
                return View("DoctoLiquidacion", model);
            }
        }

        public ActionResult CrearDoctoFin(int id)
        {
            CheckPermisoAndRedirect(99);
            var liquidacion = dc.Liquidacion.Single(li => li.IdLiquidacion == id);

            return View(liquidacion);
        }

        public ActionResult EditarDocto(int id)
        {
            CheckPermisoAndRedirect(99);

            var model = new CrearDoctoLiquidacionViewModel();
            model.Load(dc, id);

            return View("DoctoLiquidacion", model);
        }

        [HttpPost]
        public ActionResult EditarDocto([Bind(Exclude = "Fecha, FechaPagoEspecial")] CrearDoctoLiquidacionViewModel model)
        {
            CheckPermisoAndRedirect(99);

            model.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();
            model.FechaPagoEspecial = DateParser.DateFromRequest("FechaPagoEspecial").GetValueOrDefault();

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var docto = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("creardoctofin", new { id = docto.IdLiquidacion });
            }
            else
            {
                return View("DoctoLiquidacion", model);
            }
        }

        public ActionResult EliminarDocto(int id)
        {
            CheckPermisoAndRedirect(99);

            var docto = dc.DoctoLiquidacion.SingleOrDefault(d => d.IdDoctoLiquidacion == id);
            if (docto != null)
            {
                dc.DoctoLiquidacion.DeleteOnSubmit(docto);
                dc.SubmitChanges();
            }

            return RedirectToAction("creardoctofin", new { id = docto.IdLiquidacion });
        }

        public ActionResult MarcarLiquidacionFacturada(int id)
        {
            CheckPermisoAndRedirect(99);
            var liquidacion = dc.Liquidacion.Single(X => X.IdLiquidacion == id);

            liquidacion.IdEstado = 5;
            liquidacion.FechaHoraUpd = DateTime.Now;
            liquidacion.UserUpd = User.Identity.Name;
            liquidacion.IpUpd = RemoteAddr();
            dc.SubmitChanges();

            return View(liquidacion);
        }

        public ActionResult PagarLiquidaciones(int? idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(1029);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Search
            var rut = key.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;

            var model = new PagarLiquidacionesViewModel(dc, temporada.IdTemporada, idEmpresaSelect, rut);

            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporada"] = model.Temporada;
            ViewData["temporadas"] = temporadas;
            return View(model);
        }

        [HttpPost]
        public ActionResult PagarLiquidaciones([Bind(Exclude = "IdEmpresa")] PagarLiquidacionesViewModel model)
        {
            CheckPermisoAndRedirect(1029);
            model.Validate(dc, ModelState);
            if (ModelState.IsValid)
            {
                model.Persist(dc, User.Identity.Name, RemoteAddr());
                return RedirectToAction("pagarliquidacionesfin");
            }
            else
            {
                model.LoadLookups(dc);
                ViewData["idEmpresa"] = model.IdEmpresa;
                ViewData["empresas"] = dc.Empresa.ToList();

                if (model.Items != null)
                {
                    foreach (var item in model.Items)
                    {
                        item.Liquidacion = dc.Liquidacion.Single(X => X.IdLiquidacion == item.IdLiquidacion);
                    }
                }

                Temporada temporada;
                List<Temporada> temporadas = ResolveTemporadas(dc, model.IdTemporada, out temporada);
                ViewData["temporada"] = temporada;
                ViewData["temporadas"] = temporadas;

                return View(model);
            }
        }

        public ActionResult PagarLiquidacionesFin()
        {
            CheckPermisoAndRedirect(1029);
            return View();
        }

        #endregion

        #region Doctos Re-Liquidación

        public ActionResult CrearDoctoreliquidacion(int id)
        {
            CheckPermisoAndRedirect(99);
            var model = new CrearDoctoReLiquidacionViewModel(dc, id);

            return View("DoctoReLiquidacion", model);
        }

        [HttpPost]
        public ActionResult CrearDoctoreliquidacion([Bind(Exclude = "Fecha, FechaPagoEspecial")] CrearDoctoReLiquidacionViewModel model)
        {
            CheckPermisoAndRedirect(99);

            model.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();
            model.FechaPagoEspecial = DateParser.DateFromRequest("FechaPagoEspecial").GetValueOrDefault();

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var docto = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("CrearDoctoFin", new { id = docto.IdLiquidacion });
            }
            else
            {
                //ViewData["indexRouteValues"] = AsignarDescuentosRouteValues(null);
                return View("DoctoReLiquidacion", model);
            }
        }

        public ActionResult EditarDoctoreliquidacion(int id)
        {
            CheckPermisoAndRedirect(99);

            var model = new CrearDoctoReLiquidacionViewModel();
            model.Load(dc, id);

            return View("DoctoReLiquidacion", model);
        }

        [HttpPost]
        public ActionResult EditarDoctoreliquidacion([Bind(Exclude = "Fecha, FechaPagoEspecial")] CrearDoctoReLiquidacionViewModel model)
        {
            CheckPermisoAndRedirect(99);

            model.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();
            model.FechaPagoEspecial = DateParser.DateFromRequest("FechaPagoEspecial").GetValueOrDefault();

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var docto = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("creardoctofin", new { id = docto.IdLiquidacion });
            }
            else
            {
                return View("DoctoReLiquidacion", model);
            }
        }
        #endregion
    }
}
