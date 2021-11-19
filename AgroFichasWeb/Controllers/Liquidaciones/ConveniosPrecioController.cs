using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using AgroFichasWeb.ViewModels.Liquidaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class ConveniosPrecioController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ConveniosPrecioController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        #region Index

        /*
         * INDEX
         * *******************************************************************/

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
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

        public ActionResult Index(int? pageIndex, int? idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(15);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;

            var items = from cp in dc.ConvenioPrecio
                        join co in dc.Contrato on cp.IdContrato equals co.IdContrato
                        join ag in dc.Agricultor on co.IdAgricultor equals ag.IdAgricultor
                        where co.IdTemporada == temporada.IdTemporada
                           && (idEmpresaSelect == 0 || co.IdEmpresa == idEmpresaSelect)
                           && (key == "" || ag.Nombre.Contains(key) || co.Empresa.Nombre.Contains(key))
                        orderby co.NumeroContrato, cp.Prioridad
                        select cp;

            var model = new PaginatedList<ConvenioPrecio>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key }, 
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            //ViewData
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["key"] = key;
            
            return View(model);
        }

        public ActionResult IndexExport(int idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(15);

            var idEmpresaSelect = idEmpresa ?? 0;

            var items = from cp in dc.ConvenioPrecio
                        join co in dc.Contrato on cp.IdContrato equals co.IdContrato
                        join ag in dc.Agricultor on co.IdAgricultor equals ag.IdAgricultor
                        where co.IdTemporada == idTemporada
                           && (idEmpresaSelect == 0 || co.IdEmpresa == idEmpresaSelect)
                           && (key == "" || ag.Nombre.Contains(key) || co.Empresa.Nombre.Contains(key))
                        orderby co.NumeroContrato, cp.Prioridad
                        select ConveniosPrecioExportModel.FromConvenioPrecio(cp);

            return new CsvActionResult<ConveniosPrecioExportModel>(items.ToList(), "ConveniosPrecios.csv", 1, ';', null);
        }

        #endregion

        #region Crear
        /*
         * CREAR
         * *******************************************************************/

        public ActionResult Crear(int? idContrato)
        {
            CheckPermisoAndRedirect(16);

            var model = ConvenioPrecioViewModel.CreateEmpty(dc, idContrato);

            //SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("ConvenioPrecio", model);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(ConvenioPrecioViewModel model)
        {
            CheckPermisoAndRedirect(16);
            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                model.SetDefaults();

                if (model.ValidateAutorizacion(ControllerContext, User.Identity.Name, RemoteAddr(), out int idConvenioPrecioAutorizacion, out string msg))
                {
                    var convenio = model.Persist(ControllerContext, dc, User.Identity.Name, RemoteAddr(), out bool requiereNotificacion);

                    if (requiereNotificacion)
                    {
                        return RedirectToAction("Notificar", new { id = convenio.IdConvenioPrecio, backto = Request["backto"] });
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(Request["backto"]))
                            return Redirect(Request["backto"]);
                        else
                            return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("AutorizacionRequerida", new { id = idConvenioPrecioAutorizacion, msgok = msg });
                }
            }
            else
            {
                ViewData["indexRouteValues"] = IndexRouteValues(null);
                return View("ConvenioPrecio", model);
            }
        }

        public ActionResult AutorizacionRequerida(int id)
        {
            var model = new AutorizarPreciosViewModel(dc, id);

            return View("AutorizacionRequerida", model);
        }
        #endregion

        #region Notificar
        /*
         * NOTIFICAR 
         * *******************************************************************/

        public ActionResult Notificar(int id)
        {
            CheckPermisoAndRedirect(new int[] { 16, 17, 1023, 1024 });

            var model = dc.ConvenioPrecio.SingleOrDefault(c => c.IdConvenioPrecio == id);
            if (model == null)
                throw new HttpException(404, "Convenio precio not found");

            return View(model);
        }
        
        [HttpPost] 
        public ActionResult Notificar(int id, List<DestinatarioNotificacion> Destinatarios, FormCollection formValues)
        {
            CheckPermisoAndRedirect(new int[] { 16, 17, 1023, 1024 });

            var model = dc.ConvenioPrecio.SingleOrDefault(c => c.IdConvenioPrecio == id);
            if (model == null)
                throw new HttpException(404, "Convenio precio not found");

            var result = model.NotificarActualizacion(dc, ControllerContext, Destinatarios.Where(d => d.Seleccionado).ToList(), out string msg);
            var msgok = "";
            var msgerr = "";

            if (result)
                msgok = msg;
            else
                msgerr = msg;

            return RedirectToAction("Index", new { msgok = msgok, msgerr = msgerr });
        }

        public ActionResult NotificarRechazo(int id, bool? esRechazo)
        {
            CheckPermisoAndRedirect(new int[] { 1023, 1024 });

            var model = dc.ConvenioPrecioAutorizacion.SingleOrDefault(c => c.IdConvenioPrecioAutorizacion == id);
            if (model == null)
                throw new HttpException(404, "Autorización precio not found");

            return View(model);
        }

        [HttpPost]
        public ActionResult NotificarRechazo(int id, List<DestinatarioNotificacion> Destinatarios, FormCollection formValues)
        {
            CheckPermisoAndRedirect(new int[] { 1023, 1024 });

            var model = dc.ConvenioPrecioAutorizacion.SingleOrDefault(c => c.IdConvenioPrecioAutorizacion == id);
            if (model == null)
                throw new HttpException(404, "Autorización precio not found");

            var result = model.NotificarRechazo(ControllerContext, Destinatarios.Where(d => d.Seleccionado).ToList(), out string msg);
            var msgok = "";
            var msgerr = "";

            if (result)
                msgok = msg;
            else
                msgerr = msg;

            return RedirectToAction("Index", new { msgok = msgok, msgerr = msgerr });
        }

        #endregion

        #region Autorizar Precios

        /*
         * AUTORIZAR PRECIOS
         * *******************************************************************/

        private RouteValueDictionary AutorizarPreciosRouteValues(RouteValueDictionary routeValues)
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

        public ActionResult AutorizarPreciosIndex(int? pageIndex, int? idTemporada, int? idEmpresa)
        {
            CheckPermisoAndRedirect(new int[] { 1023, 1024});

            //Temporadas
            var temporadas = ResolveTemporadas(dc, idTemporada, out Temporada temporada);

            //
            var puedeGerencia = SYS_User.Current().HasPermiso(1023);
            var puedeAbastecimiento = SYS_User.Current().HasPermiso(1024);

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from ap in dc.ConvenioPrecioAutorizacion
                        where ap.Contrato.IdTemporada == temporada.IdTemporada
                           && !ap.Autorizada.HasValue
                           && (idEmpresaSelect == 0 || ap.Contrato.IdEmpresa == idEmpresaSelect)
                           && ((ap.IdNivelAutorizacion == 1 && puedeGerencia) || (ap.IdNivelAutorizacion == 2 && puedeAbastecimiento))
                        orderby ap.IdConvenioPrecioAutorizacion
                        select ap;

            var model = new PaginatedList<ConvenioPrecioAutorizacion>(items, pageIndex, 1000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;

            return View(model);
        }

        public ActionResult AutorizarPrecios(int id)
        {
            CheckPermisoAndRedirect(new int[] { 1023, 1024 });

            var model = new AutorizarPreciosViewModel(dc, id);

            ViewData["indexRouteValues"] = AutorizarPreciosRouteValues(null);

            return View(model);
        }

        [HttpPost]
        public ActionResult AutorizarPrecios(AutorizarPreciosViewModel model)
        {
            model.LoadLookups(dc);

            if (model.Autorizacion.IdNivelAutorizacion == 1) //Gerencia
                CheckPermisoAndRedirect(1023);
            else if (model.Autorizacion.IdNivelAutorizacion == 2) //Abastecimiento
                CheckPermisoAndRedirect(1024);
            else
                throw new Exception("IdNivelAutorizacion no válido.");

            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                var autorizacion = model.Persist(ControllerContext, dc, CurrentUser.UserName, RemoteAddr(), out bool requiereNotificacion);
                if (!autorizacion.Autorizada.Value)
                {
                    return RedirectToAction("NotificarRechazo", new { id = autorizacion.IdConvenioPrecioAutorizacion, backto = Request["backto"] });
                }
                else if (requiereNotificacion)
                {
                    return RedirectToAction("Notificar", new { id = autorizacion.IdConvenioPrecio, backto = Request["backto"] });
                }
                else
                {
                    if (!String.IsNullOrEmpty(Request["backto"]))
                        return Redirect(Request["backto"]);
                    else
                        return RedirectToAction("AutorizarPrecios", AutorizarPreciosRouteValues(null));
                }

            }
            else
            {
                ViewData["indexRouteValues"] = AutorizarPreciosRouteValues(null);
                return View(model);
            }
        }

        public ActionResult Autorizaciones(int? pageIndex, int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(1025);

            //Temporadas
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out Temporada temporada);

            //Items
            var items = from cpa in dc.ConvenioPrecioAutorizacion
                        join co in dc.Contrato on cpa.IdContrato equals co.IdContrato
                        join ag in dc.Agricultor on co.IdAgricultor equals ag.IdAgricultor
                        where co.IdTemporada == temporada.IdTemporada
                           && (key == "" || ag.Nombre.Contains(key) || co.Empresa.Nombre.Contains(key))
                        orderby cpa.IdConvenioPrecioAutorizacion descending
                        select cpa;

            var model = new PaginatedList<ConvenioPrecioAutorizacion>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" }
            };

            //ViewData
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult EliminarAutorizacion(int id)
        {
            CheckPermisoAndRedirect(1028);
            var autorizacion = dc.ConvenioPrecioAutorizacion.SingleOrDefault(p => p.IdConvenioPrecioAutorizacion == id);

            var routeValues = new RouteValueDictionary();
            try
            {
                if (autorizacion != null)
                {
                    if (autorizacion.Autorizada != null)
                        throw new Exception("La autorización ya fue procesada.");

                    dc.ConvenioPrecioAutorizacion.DeleteOnSubmit(autorizacion);
                    dc.SubmitChanges();
                }
                routeValues.Add("msgok", "La autorización de precio fue eliminada con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible eliminar la autorización de precio";
                //if (ex.Message.Contains("FK_PrecioIngreso_ConvenioPrecio"))
                //    msgerr += " porque tiene al menos un ingreso asociado";
                //else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            if (!String.IsNullOrEmpty(Request["backto"]))
                return Redirect(Request["backto"] + "&msgok=La autorización de precio fue eliminada con éxito");
            else
                return RedirectToAction("Autorizaciones", IndexRouteValues(routeValues));
        }

        public ActionResult Autorizacion(int id)
        {
            CheckPermisoAndRedirect(1025);

            var model = new AutorizarPreciosViewModel(dc, id);

            ViewData["indexRouteValues"] = IndexRouteValues(null);

            return View(model);
        }

        public ActionResult AutorizarTodo()
        {
            CheckPermisoAndRedirect(1031);

            StringBuilder stringBuilder = new StringBuilder();

            //Items
            var items = from ap in dc.ConvenioPrecioAutorizacion
                        where ap.Contrato.IdTemporada >= 7
                           && !ap.Autorizada.HasValue
                        orderby ap.IdConvenioPrecioAutorizacion
                        select ap;

            foreach (var item in items)
            {
                var model = new AutorizarPreciosViewModel(dc, item.IdConvenioPrecioAutorizacion);
                model.Autorizado = true;
                model.Validate(ModelState);
                if (ModelState.IsValid)
                {
                    var autorizacion = model.Persist(ControllerContext, dc, CurrentUser.UserName, RemoteAddr(), out bool requiereNotificacion);
                    stringBuilder.AppendLine(autorizacion.IdConvenioPrecioAutorizacion.ToString() + "<br>");
                }
            }

            return Content(stringBuilder.ToString());
        }

        #endregion

        #region Editar Precios        
        /*
         * EDITAR PRECIOS
         * *******************************************************************/

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(17);

            var model = ConvenioPrecioViewModel.Create(dc, id);

            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("ConvenioPrecio", model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(17);
            var model = ConvenioPrecioViewModel.Create(dc, id);
            model.Ajustes = new List<ConvenioPrecioAjusteViewModel>(); //Clean ajustes para que el binder funcione correctamente
            var idContratoOrig = model.IdContrato;

            var fields = new string[] {
                "IdContrato",
                "IdMoneda",
                "Cantidad",
                "EsPiso",
                "PrecioUnidad",
                "Habilitado",
                "Comentarios",
                "Sucursales",
                "Ajustes"
            };

            TryUpdateModel(model, fields);
            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                if (model.IdContrato != idContratoOrig)
                    model.Prioridad = ConvenioPrecio.NextPrioridad(dc, model.IdContrato);

                model.SetDefaults();

                if (model.ValidateAutorizacion(ControllerContext, User.Identity.Name, RemoteAddr(), out int idConvenioPrecioAutorizacion, out string msg))
                {
                    var convenio = model.Persist(ControllerContext, dc, User.Identity.Name, RemoteAddr(), out bool requiereNotificacion);

                    if (requiereNotificacion)
                    {
                        return RedirectToAction("Notificar", new { id = convenio.IdConvenioPrecio, backto = Request["backto"] });
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(Request["backto"]))
                            return Redirect(Request["backto"]);
                        else
                            return RedirectToAction("Index", IndexRouteValues(null));
                    }
                }
                else
                {
                    return RedirectToAction("AutorizacionRequerida", new { id = idConvenioPrecioAutorizacion, msgok = msg });
                }
            }

            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("ConvenioPrecio", model);
        }

        public ActionResult Subir(int id)
        {
            return Mover(id, true);
        }

        public ActionResult Bajar(int id)
        {
            return Mover(id, false);
        }

        public ActionResult Mover(int id, bool up)
        {
            CheckPermisoAndRedirect(17);
            var convenio = dc.ConvenioPrecio.SingleOrDefault(c => c.IdConvenioPrecio == id);
            if (convenio != null)
            {
                dc.ConvenioPrecio_MovePrioridad(convenio.IdConvenioPrecio, (up) ? -1 : 1);
            }

            if (!String.IsNullOrEmpty(Request["backto"]))
                return Redirect(Request["backto"]);
            else
                return RedirectToAction("Index", IndexRouteValues(null));
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(18);
            var convenio = dc.ConvenioPrecio.SingleOrDefault(p => p.IdConvenioPrecio == id);

            var routeValues = new RouteValueDictionary();
            try
            {
                if (convenio != null)
                {
                    IQueryable<ConvenioPrecioAutorizacion> convenioPrecioAutorizacions = dc.ConvenioPrecioAutorizacion.Where(cpa => cpa.IdConvenioPrecio == convenio.IdConvenioPrecio);
                    dc.ConvenioPrecioAutorizacion.DeleteAllOnSubmit(convenioPrecioAutorizacions);

                    dc.ConvenioPrecio.DeleteOnSubmit(convenio);
                    dc.SubmitChanges();
                }
                routeValues.Add("msgok", "El convenio de precio fue eliminado con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible eliminar el convenio de precio";
                if (ex.Message.Contains("FK_PrecioIngreso_ConvenioPrecio"))
                    msgerr += " porque tiene al menos un ingreso asociado";
                else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            if (!String.IsNullOrEmpty(Request["backto"]))
                return Redirect(Request["backto"] + "&msgok=El convenio de cambio de moneda fue eliminado con éxito");
            else
                return RedirectToAction("Index", IndexRouteValues(routeValues));
        }

        #endregion

        #region Rangos
        public ActionResult Rango(int id)
        {
            CheckRangoAccess(id);
            
            var model = RangoPrecioViewModel.Create(dc, id);

            return View(model);
        }

        [HttpPost]
        public ActionResult Rango(RangoPrecioViewModel model)
        {
            CheckRangoAccess(model.IdNivelRangoPrecio);

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                model.SetDefaults();
                model.Persist(ControllerContext, dc, User.Identity.Name, RemoteAddr());

                ViewData["msgok"] = "Los cambios fueron guardados con éxito.";
            }
            
            return View(model);
        }

        private void CheckRangoAccess(int idNivelRangoPrecio)
        {
            if (idNivelRangoPrecio != 1 && idNivelRangoPrecio != 2)
                throw new ArgumentOutOfRangeException("IdNivelRangoPrecio", $"No se encontró el Nivel de Rango de Negocio solicitado ({idNivelRangoPrecio})");

            if (idNivelRangoPrecio == 1)
                CheckPermisoAndRedirect(1021);
            else if (idNivelRangoPrecio == 2)
                CheckPermisoAndRedirect(1022);
        }
        #endregion

        #region Solicitudes

        private RouteValueDictionary SolicitudPreciosRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idCultivo", Request.QueryString["idCultivo"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Solicitudes(int? pageIndex, int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(1027);

            //Temporadas
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out Temporada temporada);

            //Items
            var items = from sp in dc.SolicitudPrecio
                        join ag in dc.Agricultor on sp.IdAgricultor equals ag.IdAgricultor
                        where sp.IdTemporada == temporada.IdTemporada
                           && (key == "" || ag.Nombre.Contains(key) || sp.Cultivo.Nombre.Contains(key))
                        orderby sp.IdSolicitudPrecio descending
                        select sp;

            var model = new PaginatedList<SolicitudPrecio>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" }
            };

            //ViewData
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult Solicitud(int id)
        {
            var model = dc.SolicitudPrecio.SingleOrDefault(s => s.IdSolicitudPrecio == id);
            if (model == null)
                throw new HttpException(404, "Solicitud not found");

            ViewData["indexRouteValues"] = IndexRouteValues(null);

            return View(model);
        }

        public ActionResult ProcesarSolicitudPreciosIndex(int? pageIndex, int? idTemporada, int? idCultivo)
        {
            CheckPermisoAndRedirect(1026);

            //Temporadas
            var temporadas = ResolveTemporadas(dc, idTemporada, out Temporada temporada);

            //Items
            var idCultivoSelect = idCultivo ?? 0;
            var items = from sp in dc.SolicitudPrecio
                        where sp.IdTemporada == temporada.IdTemporada
                           && (idCultivoSelect == 0 || sp.IdCultivo == idCultivoSelect)
                           && !sp.Procesado
                        orderby sp.IdConvenioPrecioAutorizacion
                        select sp;

            var model = new PaginatedList<SolicitudPrecio>(items, pageIndex, 1000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idCultivo", Request.QueryString["idCultivo"] ?? "" }
            };

            //ViewData
            ViewData["idCultivo"] = idCultivoSelect;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;

            return View(model);
        }

        public ActionResult ProcesarSolicitudPrecio(int id)
        {
            CheckPermisoAndRedirect(1026);

            var model = new ProcesarSolicitudPrecioViewModel(dc, id);

            ViewData["indexRouteValues"] = SolicitudPreciosRouteValues(null);

            return View(model);
        }

        [HttpPost]
        public ActionResult ProcesarSolicitudPrecio(ProcesarSolicitudPrecioViewModel model)
        {
            model.LoadLookups(dc);

            model.Validate(ModelState);


            if (ModelState.IsValid)
            {
                var routeValues = new RouteValueDictionary
                {
                    { "msgok", "La solicitud fue procesada con éxito." }
                };

                var solicitud = model.Persist(ControllerContext, dc, CurrentUser.UserName, RemoteAddr(), out string msgAutorizacion);

                if (solicitud.IdConvenioPrecio.HasValue) //el convenio de precio se creó, es necesario notificar porque es nuevo
                {
                    return RedirectToAction("Notificar", new { id = solicitud.IdConvenioPrecio, backto = Url.Action("ProcesarSolicitudPreciosIndex") });
                }
                else if (solicitud.IdConvenioPrecioAutorizacion.HasValue) //es necesario autorizar el convenio de precio creado 
                {
                    return RedirectToAction("AutorizacionRequerida", new { id = solicitud.IdConvenioPrecioAutorizacion, msgok = msgAutorizacion });
                }
                else
                {
                    return RedirectToAction("ProcesarSolicitudPreciosIndex", SolicitudPreciosRouteValues(routeValues));
                }
            }
            else
            {
                ViewData["indexRouteValues"] = SolicitudPreciosRouteValues(null);
                return View(model);
            }
        }

        #endregion

        #region API

        /*
         * API
         * *******************************************************************/

        public ConvenioPrecio CrearDesdeSolicitudContrato(int? idSolicitudContrato, AgroFichasDBDataContext context, string userName, string ipAddress)
        {
            CheckPermisoAndRedirect(16);

            //16      Crear convenios precio
            //17      Editar convenios precio
            //1023    Autorizar Precios Gerencia
            //1024    Autorizar Precios Operaciones

            ConvenioPrecio convenioPrecio = null;

            SolicitudContrato solicitudContrato = context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == idSolicitudContrato);

            ConvenioPrecioViewModel model = ConvenioPrecioViewModel.CreateEmpty(dc, solicitudContrato.IdContrato);
            model.LoadLookups(context);

            model.IdMoneda = 1;
            model.Cantidad = solicitudContrato.ToneladasCierre * 1000;
            model.PrecioUnidad = solicitudContrato.PrecioCierre;

            SelectorSucursalViewModel sucursal = model.Sucursales.Single(s => s.IdSucursal == solicitudContrato.IdSucursalEntrega);
            sucursal.Seleccionado = true;

            model.SetDefaults();

            if (model.ValidateAutorizacion(ControllerContext, userName, ipAddress, out int idConvenioPrecioAutorizacion, out string msg))
            {
                convenioPrecio = model.Persist(ControllerContext, context, userName, ipAddress, out bool requiereNotificacion);
                if (requiereNotificacion)
                {
                    convenioPrecio.NotificarActualizacion(dc, ControllerContext, convenioPrecio.DestanatariosNotificacion(), out msg);
                }
            }

            return convenioPrecio;
        }

        #endregion
    }
}
