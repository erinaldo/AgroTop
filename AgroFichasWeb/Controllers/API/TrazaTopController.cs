using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.API.Comunes;
using AgroFichasWeb.Models.API.TrazaTop;
using AgroFichasWeb.Models.TrazaTop;
using AgroFichasWeb.Models.TrazaTop.Documentos;
using ForceManagerLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Routing;
using System.Web.Script.Serialization;

namespace AgroFichasWeb.Controllers.API
{
    [WebsiteAuthorize]
    public class TrazaTopController : BaseApplicationApiController
    {
        Models.AgroFichasDBDataContext context = new Models.AgroFichasDBDataContext();

        #region Solicitudes de Contratos
        /*
         * SOLICITUDES DE CONTRATOS
         * *******************************************************************/

        #region Agricultor

        [HttpGet]
        public JsonResult<List<ResponseAgricultor>> GetAgricultores([FromUri] RequestSolicitudContrato request)
        {
            CheckPermisoAndRedirect(1032);

            TrazaTop trazaTop = new TrazaTop();
            trazaTop.SincronizarSolicitudesContratos(request.IsDevelopment);

            List<ResponseAgricultor> responseList = new List<ResponseAgricultor>();
            var agricultors = (from a in context.Agricultor
                               join sc in context.SolicitudContrato on a.Rut equals sc.Rut
                               where !sc.Verificado.Value
                               select new { SolicitudContrato = sc, Agricultor = a }).ToList();

            foreach (var agricultor in agricultors)
            {
                responseList.Add(new ResponseAgricultor()
                {
                    IdAgricultor            = agricultor.Agricultor.IdAgricultor,
                    Rut                     = agricultor.Agricultor.Rut,
                    Nombre                  = agricultor.Agricultor.Nombre,
                    Email                   = agricultor.Agricultor.Email,
                    NombreRepresentate      = agricultor.Agricultor.NombreRepresentate,
                    RutRepresentate         = agricultor.Agricultor.RutRepresentate,
                    IdRegion                = agricultor.Agricultor.IdRegion,
                    NombreRegion            = agricultor.Agricultor.Region != null ? agricultor.Agricultor.Region.Nombre : "",
                    IdProvincia             = agricultor.Agricultor.IdProvincia,
                    NombreProvincia         = agricultor.Agricultor.Provincia != null ? agricultor.Agricultor.Provincia.Nombre : "",
                    IdComuna                = agricultor.Agricultor.IdComuna,
                    NombreComuna            = agricultor.Agricultor.Comuna != null ? agricultor.Agricultor.Comuna.Nombre : "",
                    Direccion               = agricultor.Agricultor.Direccion,
                    RolAvaluo               = agricultor.Agricultor.RolAvaluo,
                    InscripcionFS           = agricultor.Agricultor.InscripcionFS,
                    InscripcionNum          = agricultor.Agricultor.InscripcionNum,
                    InscripcionAno          = agricultor.Agricultor.InscripcionAno,
                    CoberturaSeguro         = agricultor.Agricultor.CoberturaSeguro,
                    IdTituloExplotacion     = agricultor.Agricultor.IdTituloExplotacion,
                    NombreTituloExplotacion = agricultor.Agricultor.TituloExplotacion != null ? agricultor.Agricultor.TituloExplotacion.Nombre : "",
                    IdSolicitudContrato     = agricultor.SolicitudContrato.IdSolicitudContrato,
                    EstadoSolicitudContrato = agricultor.SolicitudContrato.EstadoSolicitudContrato != null ? agricultor.SolicitudContrato.EstadoSolicitudContrato.Nombre : ""
                });
            }

            return Json(responseList);
        }

        [HttpGet]
        public JsonResult<ResponseAgricultor> GetAgricultor([FromUri] Models.API.TrazaTop.RequestAgricultor request)
        {
            CheckPermisoAndRedirect(1032);

            ResponseAgricultor response = new ResponseAgricultor();
            if (request != null)
            {
                var agricultor = (from a in context.Agricultor
                                  join sc in context.SolicitudContrato on a.Rut equals sc.Rut
                                  where sc.IdSolicitudContrato == request.IdSolicitudContrato
                                  select new { SolicitudContrato = sc, Agricultor = a }).SingleOrDefault();
                if (agricultor != null)
                { 
                    response = new ResponseAgricultor()
                    {
                        IdAgricultor            = agricultor.Agricultor.IdAgricultor,
                        Rut                     = agricultor.Agricultor.Rut,
                        Nombre                  = agricultor.Agricultor.Nombre,
                        Email                   = agricultor.Agricultor.Email,
                        NombreRepresentate      = agricultor.Agricultor.NombreRepresentate,
                        RutRepresentate         = agricultor.Agricultor.RutRepresentate,
                        IdRegion                = agricultor.Agricultor.IdRegion,
                        NombreRegion            = agricultor.Agricultor.Region != null ? agricultor.Agricultor.Region.Nombre : "",
                        IdProvincia             = agricultor.Agricultor.IdProvincia,
                        NombreProvincia         = agricultor.Agricultor.Provincia != null ? agricultor.Agricultor.Provincia.Nombre : "",
                        IdComuna                = agricultor.Agricultor.IdComuna,
                        NombreComuna            = agricultor.Agricultor.Comuna != null ? agricultor.Agricultor.Comuna.Nombre : "",
                        Direccion               = agricultor.Agricultor.Direccion,
                        RolAvaluo               = agricultor.Agricultor.RolAvaluo,
                        InscripcionFS           = agricultor.Agricultor.InscripcionFS,
                        InscripcionNum          = agricultor.Agricultor.InscripcionNum,
                        InscripcionAno          = agricultor.Agricultor.InscripcionAno,
                        CoberturaSeguro         = agricultor.Agricultor.CoberturaSeguro,
                        IdTituloExplotacion     = agricultor.Agricultor.IdTituloExplotacion,
                        NombreTituloExplotacion = agricultor.Agricultor.TituloExplotacion != null ? agricultor.Agricultor.TituloExplotacion.Nombre : "",
                        IdSolicitudContrato     = agricultor.SolicitudContrato.IdSolicitudContrato,
                        EstadoSolicitudContrato = agricultor.SolicitudContrato.EstadoSolicitudContrato != null ? agricultor.SolicitudContrato.EstadoSolicitudContrato.Nombre : ""
                    };
                }
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult<ResponseGuardadoEdicion> SetAgricultor([FromBody] Models.API.TrazaTop.RequestAgricultor request)
        {
            CheckPermisoAndRedirect(1035);

            ResponseGuardadoEdicion response = new ResponseGuardadoEdicion();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
                response.OK = false;
                response.Mensaje = javaScriptSerializer.Serialize(errMsg);
            }
            else
            {
                Models.SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == request.IdSolicitudContrato);
                if (solicitudContrato != null)
                {
                    Models.Agricultor agricultor = context.Agricultor.SingleOrDefault(a => a.IdAgricultor == request.IdAgricultor);
                    if (agricultor != null)
                    {
                        // Agricultor
                        agricultor.IdAgricultor        = request.IdAgricultor;
                        agricultor.Rut                 = request.Rut;
                        agricultor.Nombre              = request.Nombre;
                        agricultor.RutRepresentate     = request.RutRepresentate;
                        agricultor.NombreRepresentate  = request.NombreRepresentate;
                        agricultor.IdRegion            = request.IdRegion;
                        agricultor.IdProvincia         = request.IdProvincia;
                        agricultor.IdComuna            = request.IdComuna;
                        agricultor.Direccion           = request.Direccion;
                        agricultor.Email               = request.Email;
                        agricultor.RolAvaluo           = request.RolAvaluo;
                        agricultor.InscripcionFS       = request.InscripcionFS;
                        agricultor.InscripcionNum      = request.InscripcionNum;
                        agricultor.InscripcionAno      = request.InscripcionAno;
                        agricultor.CoberturaSeguro     = request.CoberturaSeguro;
                        agricultor.IdTituloExplotacion = request.IdTituloExplotacion;
                        context.SubmitChanges();

                        // Verificado Fichas
                        solicitudContrato.IdAgricultor     = agricultor.IdAgricultor;
                        solicitudContrato.VerificadoFichas = true;
                        solicitudContrato.FechaHoraUpd     = DateTime.Now;
                        solicitudContrato.IpUpd            = RemoteAddr();
                        solicitudContrato.UserUpd          = User.Identity.Name;
                        context.SubmitChanges();

                        response.OK = true;
                        response.Mensaje = "Agricultor Confirmado";
                    }
                    else
                    {
                        response.OK = false;
                        response.Mensaje = "El Agricultor no existe";
                    }
                }
                else
                {
                    response.OK = false;
                    response.Mensaje = "La Solicitud de Contrato no existe";
                }
            }
            return Json(response);
        }

        #endregion

        #region ForceManager CRM

        [HttpGet]
        public JsonResult<List<ResponseSolicitudContrato>> GetSolicitudesContratosCRM([FromUri] RequestSolicitudContrato request)
        {
            CheckPermisoAndRedirect(1032);

            TrazaTop trazaTop = new TrazaTop();
            trazaTop.SincronizarSolicitudesContratos(request.IsDevelopment);

            List<ResponseSolicitudContrato> responseList = new List<ResponseSolicitudContrato>();
            List<Models.SolicitudContrato> solicitudContratos = context.SolicitudContrato.Where(sc => !sc.Verificado.Value).ToList();

            foreach (Models.SolicitudContrato solicitudContrato in solicitudContratos)
            {
                List<ResponseSolicitudContratoVariedad> responseSolicitudContratoVariedad = new List<ResponseSolicitudContratoVariedad>();
                List<Models.SolicitudContratoVariedad> solicitudContratoVariedades = context.SolicitudContratoVariedad.Where(scv => scv.IdSolicitudContrato == solicitudContrato.IdSolicitudContrato).ToList();
                foreach (Models.SolicitudContratoVariedad solicitudContratoVariedad in solicitudContratoVariedades)
                {
                    responseSolicitudContratoVariedad.Add(new ResponseSolicitudContratoVariedad()
                    {
                        IdSolicitudContrato = solicitudContratoVariedad.IdSolicitudContrato,
                        IdVariedad = solicitudContratoVariedad.IdVariedad,
                        Variedad = solicitudContratoVariedad.Variedad,
                        Hectareas = solicitudContratoVariedad.Hectareas,
                        Toneladas = solicitudContratoVariedad.Toneladas
                    });
                }

                responseList.Add(new ResponseSolicitudContrato()
                {
                    IdSolicitudContrato               = solicitudContrato.IdSolicitudContrato,
                    Rut                               = solicitudContrato.Rut,
                    NombreProveedor                   = solicitudContrato.NombreProveedor,
                    Cultivo                           = solicitudContrato.Cultivo,
                    PrecioCierre                      = solicitudContrato.PrecioCierre,
                    ToneladasCierre                   = solicitudContrato.ToneladasCierre,
                    TipoContrato                      = solicitudContrato.TipoContrato,
                    ComunaOrigen                      = solicitudContrato.ComunaOrigen,
                    SucursalEntrega                   = solicitudContrato.SucursalEntrega,
                    Hectareas                         = solicitudContrato.Hectareas,
                    ToneladasTotales                  = solicitudContrato.ToneladasTotales,
                    NombreAsesor                      = solicitudContrato.NombreAsesor,
                    EmailAsesor                       = solicitudContrato.EmailAsesor,
                    Predio                            = solicitudContrato.Predio,
                    VerificadoCRM                     = solicitudContrato.VerificadoCRM,
                    VerificadoFichas                  = solicitudContrato.VerificadoFichas,
                    ContratoCreado                    = solicitudContrato.ContratoCreado,
                    CierreCreado                      = solicitudContrato.CierreCreado,
                    IdTemporada                       = solicitudContrato.IdTemporada,
                    ResponseSolicitudContratoVariedad = responseSolicitudContratoVariedad
                });
            }

            return Json(responseList);
        }

        [HttpPost]
        public JsonResult<ResponseGuardadoEdicion> SetVerificadoCRM([FromBody] Models.API.TrazaTop.RequestVerificado request)
        {
            CheckPermisoAndRedirect(1035);

            ResponseGuardadoEdicion response = new ResponseGuardadoEdicion();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
                response.OK = false;
                response.Mensaje = javaScriptSerializer.Serialize(errMsg);
            }
            else
            {
                Models.SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == request.IdSolicitudContrato);
                if (solicitudContrato != null)
                {
                    TrazaTop trazaTop = new TrazaTop();
                    ForceManagerLib.Models.TrazaTop.RequestVerificado requestVerificado = new ForceManagerLib.Models.TrazaTop.RequestVerificado()
                    {
                        IdSolicitudContrato = solicitudContrato.IdSolicitudContrato,
                        IdEstado            = request.IdEstado,
                        Nombre              = request.Nombre
                    };
                    ForceManagerLib.Models.TrazaTop.ResponseVerificado responseVerificado = trazaTop.SetVerificado(requestVerificado);
                    if (responseVerificado.OK)
                    {
                        switch (request.IdEstado)
                        {
                            case 1:
                                // No Verificado
                                solicitudContrato.VerificadoCRM = false;
                                break;
                            case 2:
                                // Verificado
                                solicitudContrato.VerificadoCRM = true;
                                break;
                            case 3:
                                // Notificado
                                solicitudContrato.VerificadoCRM = false;
                                break;
                        }

                        solicitudContrato.IdEstado     = request.IdEstado;
                        solicitudContrato.FechaHoraUpd = DateTime.Now;
                        solicitudContrato.IpUpd        = RemoteAddr();
                        solicitudContrato.UserUpd      = User.Identity.Name;
                        context.SubmitChanges();

                        response.OK = true;
                        response.Mensaje = "Solicitud de Contrato Actualizada";
                    }
                    else
                    {
                        response.OK = false;
                        response.Mensaje = string.Format("Error - {0}", responseVerificado.Mensaje);
                    }
                }
                else
                {
                    response.OK = false;
                    response.Mensaje = "La Solicitud de Contrato no existe";
                }
            }
            return Json(response);
        }

        #endregion

        #region Fichas

        [HttpGet]
        public JsonResult<List<ResponseSolicitudContrato>> GetSolicitudesContratos([FromUri] RequestSolicitudContratoFiltrada request)
        {
            CheckPermisoAndRedirect(1032);

            List<ResponseSolicitudContrato> responseList = new List<ResponseSolicitudContrato>();
            List<Models.SolicitudContrato> solicitudContratos;

            if (request != null)
            {
                var IdTemporadaSelect = request.IdTemporada ?? 0;
                var KeySelect         = request.Key         ?? "";

                solicitudContratos = (from sc in context.SolicitudContrato
                                      where (IdTemporadaSelect == 0 || sc.IdTemporada  == IdTemporadaSelect) &&
                                            (KeySelect == ""                                                 ||
                                             sc.IdSolicitudContrato.ToString().Contains(KeySelect)           ||
                                             sc.Rut.Contains(KeySelect)                                      ||
                                             sc.NombreProveedor.Contains(KeySelect)                          ||
                                             sc.Cultivo.Contains(KeySelect)                                  ||
                                             sc.PrecioCierre.ToString().Contains(KeySelect)                  ||
                                             sc.ToneladasCierre.ToString().Contains(KeySelect)               ||
                                             sc.TipoContrato.Contains(KeySelect)                             ||
                                             sc.ComunaOrigen.Contains(KeySelect)                             ||
                                             sc.SucursalEntrega.Contains(KeySelect)                          ||
                                             sc.Predio.Contains(KeySelect)                                   ||
                                             sc.Hectareas.ToString().Contains(KeySelect)                     ||
                                             sc.ToneladasTotales.ToString().Contains(KeySelect)              ||
                                             sc.NombreAsesor.Contains(KeySelect)                             ||
                                             sc.EmailAsesor.Contains(KeySelect))
                                      && sc.Verificado.Value
                                      select sc).ToList();
            }
            else
            {
                solicitudContratos = context.SolicitudContrato.Where(sc => sc.Verificado.Value).ToList();
            }

            foreach (Models.SolicitudContrato solicitudContrato in solicitudContratos)
            {
                List<ResponseSolicitudContratoVariedad> responseSolicitudContratoVariedad = new List<ResponseSolicitudContratoVariedad>();
                List<Models.SolicitudContratoVariedad> solicitudContratoVariedades = context.SolicitudContratoVariedad.Where(scv => scv.IdSolicitudContrato == solicitudContrato.IdSolicitudContrato).ToList();
                foreach (Models.SolicitudContratoVariedad solicitudContratoVariedad in solicitudContratoVariedades)
                {
                    responseSolicitudContratoVariedad.Add(new ResponseSolicitudContratoVariedad()
                    {
                        IdSolicitudContrato = solicitudContratoVariedad.IdSolicitudContrato,
                        IdVariedad          = solicitudContratoVariedad.IdVariedad,
                        Variedad            = solicitudContratoVariedad.Variedad,
                        Hectareas           = solicitudContratoVariedad.Hectareas,
                        Toneladas           = solicitudContratoVariedad.Toneladas
                    });
                }

                responseList.Add(new ResponseSolicitudContrato()
                {
                    IdSolicitudContrato               = solicitudContrato.IdSolicitudContrato,
                    Rut                               = solicitudContrato.Rut,
                    NombreProveedor                   = solicitudContrato.NombreProveedor,
                    Cultivo                           = solicitudContrato.Cultivo,
                    PrecioCierre                      = solicitudContrato.PrecioCierre,
                    ToneladasCierre                   = solicitudContrato.ToneladasCierre,
                    TipoContrato                      = solicitudContrato.TipoContrato,
                    ComunaOrigen                      = solicitudContrato.ComunaOrigen,
                    SucursalEntrega                   = solicitudContrato.SucursalEntrega,
                    Hectareas                         = solicitudContrato.Hectareas,
                    ToneladasTotales                  = solicitudContrato.ToneladasTotales,
                    NombreAsesor                      = solicitudContrato.NombreAsesor,
                    EmailAsesor                       = solicitudContrato.EmailAsesor,
                    Predio                            = solicitudContrato.Predio,
                    VerificadoCRM                     = solicitudContrato.VerificadoCRM,
                    VerificadoFichas                  = solicitudContrato.VerificadoFichas,
                    ContratoCreado                    = solicitudContrato.ContratoCreado,
                    CierreCreado                      = solicitudContrato.CierreCreado,
                    PDFCreado                         = solicitudContrato.PDFCreado,
                    IdTemporada                       = solicitudContrato.IdTemporada,
                    ResponseSolicitudContratoVariedad = responseSolicitudContratoVariedad
                });
            }

            return Json(responseList);
        }

        [HttpPost]
        public JsonResult<ResponseGuardadoEdicion> SetVerificadoFichas([FromBody] Models.API.TrazaTop.RequestVerificadoFichas request)
        {
            CheckPermisoAndRedirect(1035);

            ResponseGuardadoEdicion response = new ResponseGuardadoEdicion();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
                response.OK = false;
                response.Mensaje = javaScriptSerializer.Serialize(errMsg);
            }
            else
            {
                Models.SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == request.IdSolicitudContrato);
                if (solicitudContrato != null)
                {
                    if (!request.VerificadoFichas)
                        solicitudContrato.IdAgricultor = null;
                    solicitudContrato.VerificadoFichas = request.VerificadoFichas;
                    solicitudContrato.FechaHoraUpd     = DateTime.Now;
                    solicitudContrato.IpUpd            = RemoteAddr();
                    solicitudContrato.UserUpd          = User.Identity.Name;
                    context.SubmitChanges();

                    response.OK = true;
                    response.Mensaje = "Solicitud de Contrato Actualizada";
                }
                else
                {
                    response.OK = false;
                    response.Mensaje = "La Solicitud de Contrato no existe";
                }
            }
            return Json(response);
        }

        [HttpPost]
        public JsonResult<ResponseGuardadoEdicion> SetContrato([FromBody] Models.API.TrazaTop.RequestContrato request)
        {
            CheckPermisoAndRedirect(12);

            ResponseGuardadoEdicion response = new ResponseGuardadoEdicion();

            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                if (!ModelState.IsValid)
                {
                    var errMsg = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
                    response.OK = false;
                    response.Mensaje = javaScriptSerializer.Serialize(errMsg);
                }
                else
                {
                    Models.SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == request.IdSolicitudContrato);
                    if (solicitudContrato != null)
                    {
                        Models.Empresa empresa = context.Empresa.Single(e => e.IdEmpresa == request.IdEmpresa);
                        solicitudContrato.GastosTransportePara = (request.IdGastosTransportePara == 1 ? empresa.Nombre.ToUpper() : "PRODUCTOR");
                        solicitudContrato.IdEmpresa = request.IdEmpresa;
                        solicitudContrato.FechaHoraUpd = DateTime.Now;
                        solicitudContrato.IpUpd = RemoteAddr();
                        solicitudContrato.UserUpd = User.Identity.Name;
                        context.SubmitChanges();

                        // Crea Contrato e ItemContrato
                        context.TRZ_CrearContrato(solicitudContrato.IdSolicitudContrato,
                                                  request.NumeroContrato.Trim(),
                                                  User.Identity.Name,
                                                  DateTime.Now,
                                                  RemoteAddr());

                        // Refresca SolicitudContrato para incluir los cambios recién hechos
                        context.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, solicitudContrato);

                        response.OK = true;
                        response.Mensaje = string.Format("Contrato Creado #{0}", solicitudContrato.IdContrato);

                        // Si es Contrato o Cierre de Negocio
                        if (solicitudContrato.IdTipoContrato == 2 || solicitudContrato.IdTipoContrato == 3)
                        {
                            #region Creando ControllerContext
                            ConveniosPrecioController controller = new ConveniosPrecioController();
                            RouteData routeData = new RouteData();
                            routeData.Values.Add("action", "CrearConvenioPrecio");
                            routeData.Values.Add("controller", "ConveniosPrecio");
                            System.Web.Mvc.ControllerContext controllerContext = new System.Web.Mvc.ControllerContext(new HttpContextWrapper(System.Web.HttpContext.Current), routeData, controller);
                            controller.ControllerContext = controllerContext;
                            #endregion

                            // Si Necesita Autorización es porque incluye un Convenio de Precio desde Force Manager
                            if (solicitudContrato.NecesitaAutorizacion.Value)
                            {
                                ConvenioPrecio convenioPrecio = controller.CrearDesdeSolicitudContrato(solicitudContrato.IdSolicitudContrato, context, User.Identity.Name, RemoteAddr());
                                if (convenioPrecio != null)
                                {
                                    // OK
                                    response.Mensaje = string.Format("{0}\r\nConvenio de Precio #{1}", response.Mensaje, convenioPrecio.IdConvenioPrecio);
                                }
                                else
                                {
                                    // Error
                                    response.Mensaje = string.Format("Contrato Creado #{0}\r\nEn espera de autorización.", solicitudContrato.IdContrato);
                                }
                            }
                            else
                            {
                                // Cierre de Negocio
                                if (solicitudContrato.IdTipoContrato == 3)
                                {
                                    //Si el Convenio de Precio no viene desde Force Manager, tomar el último de acuerdo a la Sucursal y Cultivo en Precio Spot, esto aplica para:
                                    //- Contrato
                                    PrecioSpot precioSpot = context.PrecioSpot.Where(ps => ps.IdCultivo == solicitudContrato.IdCultivo && ps.IdMoneda == 1 && ps.IdSucursal == solicitudContrato.IdSucursalEntrega).OrderByDescending(ps => ps.IdPrecioSpot).FirstOrDefault();
                                    if (precioSpot != null)
                                    {
                                        // Actualiza PrecioCierre y ToneladasCierre
                                        solicitudContrato.PrecioCierre = (int)precioSpot.Valor;
                                        solicitudContrato.ToneladasCierre = solicitudContrato.ToneladasTotales;
                                        solicitudContrato.FechaHoraUpd = DateTime.Now;
                                        solicitudContrato.IpUpd = RemoteAddr();
                                        solicitudContrato.UserUpd = User.Identity.Name;
                                        context.SubmitChanges();

                                        // Refresca SolicitudContrato para incluir los cambios recién hechos
                                        context.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, solicitudContrato);

                                        ConvenioPrecio convenioPrecio = controller.CrearDesdeSolicitudContrato(solicitudContrato.IdSolicitudContrato, context, User.Identity.Name, RemoteAddr());
                                        if (convenioPrecio != null)
                                        {
                                            // Creado en base a Precio Spot
                                            response.Mensaje = string.Format("{0}\r\nConvenio de Precio #{1} creado en base a Precio Spot.", response.Mensaje, convenioPrecio.IdConvenioPrecio);
                                        }
                                        else
                                        {
                                            // Error
                                            response.Mensaje = string.Format("Contrato Creado #{0}\r\nPero hubo un error al crear el Convenio de Precio, favor de crear manualmente o revisar entrada de datos.", solicitudContrato.IdContrato);
                                        }
                                    }
                                    else
                                    {
                                        // No hay Precio Spot
                                        response.Mensaje = string.Format("Contrato Creado #{0}\r\nPero hubo un error al crear el Convenio de Precio porque no viene desde Force Manager y no hay Precio Spot disponible para el Cultivo y Sucursal seleccionado, favor de crear manualmente o revisar entrada de datos.", solicitudContrato.IdContrato);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        response.OK = false;
                        response.Mensaje = "La Solicitud de Contrato no existe";
                    }
                }
            }
            catch (Exception exception)
            {
                response.OK = false;
                response.Mensaje = exception.Message;
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult<ResponseGuardadoEdicion> AnularContrato([FromBody] Models.API.TrazaTop.RequestAnularContrato request)
        {
            CheckPermisoAndRedirect(14);

            ResponseGuardadoEdicion response = new ResponseGuardadoEdicion();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
                response.OK = false;
                response.Mensaje = javaScriptSerializer.Serialize(errMsg);
            }
            else
            {
                Models.SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == request.IdSolicitudContrato);
                if (solicitudContrato != null)
                {
                    TrazaTop trazaTop = new TrazaTop();
                    trazaTop.SetContratoCreado(new ForceManagerLib.Models.TrazaTop.RequestContratoCreado()
                    {
                        IdSolicitudContrato = solicitudContrato.IdSolicitudContrato,
                        ContratoCreado = false
                    });

                    Models.Contrato contrato = context.Contrato.Single(c => c.IdContrato == solicitudContrato.IdContrato);
                    ConvenioPrecio convenioPrecio = context.ConvenioPrecio.SingleOrDefault(cp => cp.IdContrato == contrato.IdContrato);
                    bool convenioPrecioBorrado = true;
                    bool contratoBorrado = true;
                    if (convenioPrecio != null)
                    {
                        try
                        {
                            IQueryable<ConvenioPrecioAutorizacion> convenioPrecioAutorizacions = context.ConvenioPrecioAutorizacion.Where(cpa => cpa.IdConvenioPrecio == convenioPrecio.IdConvenioPrecio);
                            context.ConvenioPrecioAutorizacion.DeleteAllOnSubmit(convenioPrecioAutorizacions);

                            context.ConvenioPrecio.DeleteOnSubmit(convenioPrecio);
                            context.SubmitChanges();
                        }
                        catch
                        {
                            convenioPrecioBorrado = false;
                        }
                    }

                    if (convenioPrecioBorrado)
                    {
                        try
                        {
                            // Borrando Doctos
                            IQueryable<Models.DoctoContrato> doctoContratos = context.DoctoContrato.Where(dc => dc.IdContrato == contrato.IdContrato);
                            foreach (Models.DoctoContrato doctoContrato in doctoContratos)
                            {
                                context.DoctoContrato.DeleteOnSubmit(doctoContrato);
                            }

                            // Quitando Referencia
                            solicitudContrato.IdContrato = null;

                            // Eliminando Contrato
                            context.Contrato.DeleteOnSubmit(contrato);
                            context.SubmitChanges();
                        }
                        catch
                        {
                            contratoBorrado = false;
                        }
                    }

                    solicitudContrato.GastosTransportePara = "";
                    solicitudContrato.ContratoCreado       = false;
                    solicitudContrato.PDFCreado            = false;
                    solicitudContrato.IdEmpresa            = null;
                    solicitudContrato.FechaHoraUpd         = DateTime.Now;
                    solicitudContrato.IpUpd                = RemoteAddr();
                    solicitudContrato.UserUpd              = User.Identity.Name;
                    context.SubmitChanges();

                    if (contratoBorrado)
                    {
                        response.OK = true;
                        response.Mensaje = "Contrato Anulado";
                    }
                    else
                    {
                        response.OK = true;
                        response.Mensaje = "Contrato Anulado, pero el registro de Contrato debe borrarse manualmente porque tiene registros asociados que no pueden ser elimnados de forma automática.";
                    }
                }
                else
                {
                    response.OK = false;
                    response.Mensaje = "La Solicitud de Contrato no existe";
                }
            }

            return Json(response);
        }

        [HttpPost]
        [Obsolete]
        public JsonResult<ResponseGuardadoEdicion> CrearPDFContrato([FromBody] Models.API.TrazaTop.RequestCrearPDFContrato request)
        {
            CheckPermisoAndRedirect(12);

            ResponseGuardadoEdicion response = new ResponseGuardadoEdicion();

            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                if (!ModelState.IsValid)
                {
                    var errMsg = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
                    response.OK = false;
                    response.Mensaje = javaScriptSerializer.Serialize(errMsg);
                }
                else
                {
                    Models.SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == request.IdSolicitudContrato);
                    if (solicitudContrato != null)
                    {
                        bool OK = true;
                        string errMsg = "";
                        bool TieneConvenioPrecio = false;

                        // Verifica si está Autorizado el Convenio de Precio
                        ConvenioPrecioAutorizacion convenioPrecioAutorizacion1 = context.ConvenioPrecioAutorizacion.SingleOrDefault(cpa => cpa.IdContrato == solicitudContrato.IdContrato);
                        if (convenioPrecioAutorizacion1 != null)
                        {
                            if (convenioPrecioAutorizacion1.Autorizada.HasValue)
                            {
                                if (convenioPrecioAutorizacion1.Autorizada.Value == false)
                                {
                                    // Es igual a 0
                                    OK = false;
                                    errMsg = "El Convenio de Precio no está autorizado";
                                }
                                else
                                {
                                    TieneConvenioPrecio = true;
                                }
                            }
                            else
                            {
                                // Es igual a NULL
                                OK = false;
                                errMsg = "El Convenio de Precio no está autorizado";
                            }
                        }
                        else
                        {
                            
                            if (solicitudContrato.PrecioCierre == 0)
                                // Si el Precio Cierre es $0, no hay Cierre, por lo tanto no hay Convenio.
                                TieneConvenioPrecio = false;
                            else
                                // Si no tiene Convenio Precio Autorización es porque se autorizó automáticamente
                                TieneConvenioPrecio = true;
                        }

                        if (OK)
                        {
                            // Valida Planilla y Crea el PDF de Docto de Contrato
                            switch (solicitudContrato.IdTipoContrato)
                            {
                                case 1:
                                    //Acuerdo Comercial
                                    AcuerdoComercial acuerdoComercial = new AcuerdoComercial(solicitudContrato.IdSolicitudContrato);
                                    if (acuerdoComercial.ExistePlanilla())
                                    {
                                        acuerdoComercial.CrearPDF(User.Identity.Name, DateTime.Now, RemoteAddr());
                                    }
                                    else
                                    {
                                        OK = false;
                                        errMsg = "La Planilla de Contrato del Acuerdo Comercial no existe";
                                    }
                                    break;
                                case 2:
                                    //Contrato
                                    Models.TrazaTop.Documentos.Contrato contrato = new Models.TrazaTop.Documentos.Contrato(solicitudContrato.IdSolicitudContrato);
                                    if (contrato.ExistePlanilla())
                                    {
                                        contrato.CrearPDF(User.Identity.Name, DateTime.Now, RemoteAddr());
                                    }
                                    else
                                    {
                                        OK = false;
                                        errMsg = "La Planilla de Contrato del Contrato no existe";
                                    }
                                    break;
                                case 3:
                                    //Cierre de Negocio
                                    CierreNegocio cierreNegocio = new CierreNegocio(solicitudContrato.IdSolicitudContrato);
                                    if (cierreNegocio.ExistePlanilla())
                                    {
                                        cierreNegocio.CrearPDF(User.Identity.Name, DateTime.Now, RemoteAddr());
                                    }
                                    else
                                    {
                                        OK = false;
                                        errMsg = "La Planilla de Contrato del Cierre de Negocio no existe";
                                    }
                                    break;
                            }

                            if (OK) 
                            {
                                // Si tiene Convenio de Precio, se crea el PDF.
                                if (TieneConvenioPrecio && solicitudContrato.IdTipoContrato != 3)
                                {
                                    CierrePrecio cierrePrecio = new CierrePrecio(solicitudContrato.IdSolicitudContrato);
                                    if (cierrePrecio.ExistePlanilla(true))
                                    {
                                        cierrePrecio.CrearPDF(User.Identity.Name, DateTime.Now, RemoteAddr());
                                    }
                                    else
                                    {
                                        OK = false;
                                        errMsg = "La Planilla de Contrato del Cierre de Precio no existe";
                                    }
                                }

                                // Si existe el Documento de Anexo de Cultivo se crea el PDF.
                                Anexo anexo = new Anexo(solicitudContrato.IdSolicitudContrato);
                                if (anexo.ExistePlanillaAnexo())
                                {
                                    anexo.CrearPDF(User.Identity.Name, DateTime.Now, RemoteAddr());
                                }
                            }
                        }

                        if (OK)
                        {
                            TrazaTop trazaTop = new TrazaTop();
                            trazaTop.SetContratoCreado(new ForceManagerLib.Models.TrazaTop.RequestContratoCreado()
                            {
                                IdSolicitudContrato = solicitudContrato.IdSolicitudContrato,
                                ContratoCreado = true
                            });

                            solicitudContrato.PDFCreado    = true;
                            solicitudContrato.FechaHoraUpd = DateTime.Now;
                            solicitudContrato.IpUpd        = RemoteAddr();
                            solicitudContrato.UserUpd      = User.Identity.Name;
                            context.SubmitChanges();

                            response.OK = true;
                            response.Mensaje = "PDF Creado";

                            // Subir Contratos y Anexos
                            ForceManagerLib.Models.TrazaTop.ResponseCreateDocumentResource responseCreateDocumentResource = trazaTop.SubirContratosYAnexos(new ForceManagerLib.Models.TrazaTop.RequestCreateDocumentResource()
                            {
                                IdSolicitudContrato = solicitudContrato.IdSolicitudContrato
                            });

                            if (responseCreateDocumentResource.OK)
                            {
                                // Yay
                                response.Mensaje += "\r\nContratos y Anexos Subidos a ForceManager";
                            }
                            else
                            {
                                response.Mensaje += string.Format("\r\nError al Subir Contratos y Anexos: {0}", responseCreateDocumentResource.Mensaje);
                            }

                            // Notificación
                            Notificacion.EnviarNotificacionContrato(solicitudContrato, out string errMsg2);
                            if (string.IsNullOrEmpty(errMsg2))
                            {
                                response.Mensaje += "\r\nNotificado el Asesor Agrícola";
                            }
                            else
                            {
                                response.Mensaje += "\r\nError al Notificar al Asesor Agrícola";
                            }
                        }
                        else
                        {
                            response.OK = OK;
                            response.Mensaje = errMsg;
                        }
                    }
                    else
                    {
                        response.OK = false;
                        response.Mensaje = "La Solicitud de Contrato no existe";
                    }
                }
            }
            catch (Exception exception)
            {
                response.OK = false;
                response.Mensaje = exception.ToString();
            }

            return Json(response);
        }

        #endregion

        #region Notificación

        [HttpPost]
        public JsonResult<ResponseNotificacion> NotificarVerificarDatos([FromBody] RequestNotificacion request)
        {
            CheckPermisoAndRedirect(1035);

            ResponseNotificacion response = new ResponseNotificacion();
            if (request != null)
            {
                Models.SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == request.IdSolicitudContrato);
                if (solicitudContrato != null)
                {
                    TrazaTop trazaTop = new TrazaTop();
                    ForceManagerLib.Models.TrazaTop.RequestVerificado requestVerificado = new ForceManagerLib.Models.TrazaTop.RequestVerificado()
                    {
                        IdSolicitudContrato = solicitudContrato.IdSolicitudContrato,
                        IdEstado = 3,
                        Nombre = "Notificado"
                    };
                    ForceManagerLib.Models.TrazaTop.ResponseVerificado responseVerificado = trazaTop.SetVerificado(requestVerificado);
                    if (responseVerificado.OK)
                    {
                        MensajeTipo mensajeTipo = new MensajeTipo() { 
                            Tipo                = request.Tipo,
                            Nombre              = solicitudContrato.NombreAsesor,
                            Email               = solicitudContrato.EmailAsesor,
                            Mensaje             = request.Mensaje,
                            IdSolicitudContrato = request.IdSolicitudContrato
                        };
                        string errMsg = "";
                        if (Notificacion.EnviarNotificacion(mensajeTipo, out errMsg))
                        {
                            solicitudContrato.IdEstado     = 3;
                            solicitudContrato.FechaHoraUpd = DateTime.Now;
                            solicitudContrato.IpUpd        = RemoteAddr();
                            solicitudContrato.UserUpd      = User.Identity.Name;

                            response.OK = true;
                            response.Mensaje = "Notificación Enviada";
                        }
                        else
                        {
                            response.OK = false;
                            response.Mensaje = errMsg;
                        }
                    }
                    else
                    {
                        response.OK = false;
                        response.Mensaje = responseVerificado.Mensaje;
                    }
                }
                else
                {
                    response.OK = false;
                    response.Mensaje = "La Solicitud de Contrato no existe";
                }
            }
            return Json(response);
        }

        #endregion

        #endregion

        #region Planillas de Contratos
        /*
         * PLANILLAS DE CONTRATOS
         * *******************************************************************/

        [HttpPost]
        public JsonResult<ResponseGuardadoEdicion> DelPlanillaContrato([FromBody] Models.API.TrazaTop.RequestPlanillaContrato request)
        {
            CheckPermisoAndRedirect(1039);

            ResponseGuardadoEdicion response = new ResponseGuardadoEdicion();
            if (request != null)
            {
                PlanillaContrato planillaContrato = context.PlanillaContrato.SingleOrDefault(pc => pc.IdPlanillaContrato == request.IdPlanillaContrato);
                if (planillaContrato != null)
                {
                    context.PlanillaContrato.DeleteOnSubmit(planillaContrato);
                    context.SubmitChanges();

                    response.OK = true;
                    response.Mensaje = "Planilla de Contrato eliminada";
                }
                else
                {
                    response.OK = false;
                    response.Mensaje = "La Planilla de Contrato no se encuentra";
                }
            }
            return Json(response);
        }

        [HttpGet]
        public JsonResult<ResponsePlanillaContrato> GetPlanillaContrato([FromUri] Models.API.TrazaTop.RequestPlanillaContrato request)
        {
            CheckPermisoAndRedirect(1033);

            ResponsePlanillaContrato response = new ResponsePlanillaContrato();
            if (request != null)
            {
                PlanillaContrato planillaContrato = context.PlanillaContrato.SingleOrDefault(pc => pc.IdPlanillaContrato == request.IdPlanillaContrato);
                if (planillaContrato != null)
                {
                    response = new ResponsePlanillaContrato()
                    {
                        IdPlanillaContrato = planillaContrato.IdPlanillaContrato,
                        Documento          = planillaContrato.Documento,
                        Observacion        = planillaContrato.Observacion,
                        IdTemporada        = planillaContrato.IdTemporada,
                        IdTipoContrato     = planillaContrato.IdTipoContrato,
                        IdCultivo          = planillaContrato.IdCultivo,
                        UserIns            = planillaContrato.UserIns,
                        FechaHoraIns       = planillaContrato.FechaHoraIns,
                        IpIns              = planillaContrato.IpIns,
                        UserUpd            = planillaContrato.UserUpd,
                        FechaHoraUpd       = planillaContrato.FechaHoraUpd,
                        IpUpd              = planillaContrato.IpUpd,
                    };
                }
            }
            return Json(response);
        }

        [HttpGet]
        public JsonResult<List<ResponsePlanillaContrato>> GetPlanillasContratos([FromUri] RequestPlanillaContrato request)
        {
            CheckPermisoAndRedirect(1033);

            List<ResponsePlanillaContrato> responseList = new List<ResponsePlanillaContrato>();
            if (request != null)
            {
                List<Models.PlanillaContrato> planillaContratos = context.PlanillaContrato.Where(pc => pc.IdTemporada == request.IdTemporada).ToList();
                if (request.IdTipoContrato.HasValue)
                    planillaContratos = planillaContratos.Where(pc => pc.IdTipoContrato == request.IdTipoContrato.Value).ToList();
                if (request.IdCultivo.HasValue)
                    planillaContratos = planillaContratos.Where(pc => pc.IdCultivo == request.IdCultivo.Value).ToList();

                foreach (Models.PlanillaContrato planillaContrato in planillaContratos)
                {
                    responseList.Add(new ResponsePlanillaContrato()
                    {
                        IdPlanillaContrato = planillaContrato.IdPlanillaContrato,
                        Documento          = planillaContrato.Documento,
                        Observacion        = planillaContrato.Observacion,
                        IdTemporada        = planillaContrato.IdTemporada,
                        IdTipoContrato     = planillaContrato.IdTipoContrato,
                        IdCultivo          = planillaContrato.IdCultivo,
                        UserIns            = planillaContrato.UserIns,
                        FechaHoraIns       = planillaContrato.FechaHoraIns,
                        IpIns              = planillaContrato.IpIns,
                        UserUpd            = planillaContrato.UserUpd,
                        FechaHoraUpd       = planillaContrato.FechaHoraUpd,
                        IpUpd              = planillaContrato.IpUpd,
                    });
                }
            }
            return Json(responseList);
        }

        [HttpPost]
        public JsonResult<ResponseGuardadoEdicion> SetPlanillaContrato([FromBody] Models.API.TrazaTop.RequestFormatoRTF request)
        {
            CheckPermisoAndRedirect(1037);

            ResponseGuardadoEdicion response = new ResponseGuardadoEdicion();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
                response.OK = false;
                response.Mensaje = javaScriptSerializer.Serialize(errMsg);
            }
            else
            {
                if (request.DocumentoN != "" && request.DocumentoR != "")
                {
                    string DocumentoM;
                    var fileStream = new FileStream(HttpContext.Current.Server.MapPath("~/App_Data/TrazaTop/plantillascontratos/" + request.DocumentoR + ".rtf"), FileMode.Open, FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        DocumentoM = streamReader.ReadToEnd();
                    }

                    PlanillaContrato planillaContrato = new PlanillaContrato()
                    {
                        Documento      = request.DocumentoN,
                        DocumentoM     = DocumentoM,
                        Observacion    = request.Observacion,
                        IdTemporada    = request.IdTemporada,
                        IdTipoContrato = request.IdTipoContrato,
                        IdCultivo      = request.IdCultivo,
                        UserIns        = request.UserIns,
                        FechaHoraIns   = request.FechaHoraIns,
                        IpIns          = request.IpIns,
                    };
                    context.PlanillaContrato.InsertOnSubmit(planillaContrato);
                    context.SubmitChanges();

                    response.OK = true;
                    response.Mensaje = "Planilla de Contrato guardada";
                }
                else
                {
                    response.OK = false;
                    response.Mensaje = "El Documento no es válido o no se adjunta";
                }
            }
            return Json(response);
        }

        [HttpPost]
        public HttpResponseMessage UploadFile()
        {
            CheckPermisoAndRedirect(1037);

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var file = httpRequest.Files[0];
                var fileName = System.Guid.NewGuid();
                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/App_Data/TrazaTop/plantillascontratos")))
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/App_Data/TrazaTop/plantillascontratos"));
                var path = HttpContext.Current.Server.MapPath("~/App_Data/TrazaTop/plantillascontratos/" + fileName + Path.GetExtension(file.FileName));
                file.SaveAs(path);

                ResponseUploadFile response = new ResponseUploadFile()
                {
                    FileName = file.FileName,
                    FilePath = fileName.ToString() // Mejor Así
                };
                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}