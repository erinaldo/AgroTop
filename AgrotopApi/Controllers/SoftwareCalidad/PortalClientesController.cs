using AgrotopApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using MoreLinq;
using System;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    [Authorize]
    public class PortalClientesController : ApiController
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion

        #region 3. Funciones

        public Cliente GetCliente(int id)
        {
            return dcAgroFichas.Cliente.SingleOrDefault(X => X.IdCliente == id);
        }

        public Pais GetPais(string PaisCodigo)
        {
            return dcAgroFichas.Pais.SingleOrDefault(X => X.PaisCodigo == PaisCodigo);
        }

        public CAL_Producto GetProducto(int IdProducto)
        {
            return dcSoftwareCalidad.CAL_Producto.SingleOrDefault(X => X.IdProducto == IdProducto);
        }

        public CAL_Subproducto GetSubproducto(int IdSubproducto)
        {
            return dcSoftwareCalidad.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == IdSubproducto);
        }

        public CAL_FTTemperatura GetTemperatura(int IdTemperatura)
        {
            return dcSoftwareCalidad.CAL_FTTemperatura.SingleOrDefault(X => X.IdTemperatura == IdTemperatura);
        }

        public List<CAL_GetSacoPorIdFichaTecnicaResult> GetSaco(int IdFichaTecnica)
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetSacoPorIdFichaTecnica(IdFichaTecnica)
                    select O1).ToList();
        }

        public List<CAL_GetParametroAnalisisAsociadosPorFichaTecnicaResult> GetParametroAnalisis(int IdFichaTecnica)
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroAnalisisAsociadosPorFichaTecnica(IdFichaTecnica)
                    select O1).ToList();
        }

        public List<CAL_GetParametroPesticidaPorIdFichaTecnicaResult> GetParametroAnalisisPesticida(int IdFichaTecnica)
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroPesticidaPorIdFichaTecnica(IdFichaTecnica)
                    select O1).ToList();
        }

        public List<CAL_GetParametroMetalPesadoPorIdFichaTecnicaResult> GetParametroAnalisisMetalPesado(int IdFichaTecnica)
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroMetalPesadoPorIdFichaTecnica(IdFichaTecnica)
                    select O1).ToList();
        }

        public List<CAL_GetParametroMicotoxinaPorIdFichaTecnicaResult> GetParametroAnalisisMicotoxinas(int IdFichaTecnica)
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroMicotoxinaPorIdFichaTecnica(IdFichaTecnica)
                    select O1).ToList();
        }

        public List<CAL_GetParametroMicrobiologiaPorIdFichaTecnicaResult> GetParametroAnalisisMicrobiologia(int IdFichaTecnica)
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroMicrobiologiaPorIdFichaTecnica(IdFichaTecnica)
                    select O1).ToList();
        }

        public List<CAL_GetParametroNutricionalPorIdFichaTecnicaResult> GetParametroAnalisisNutricionales(int IdFichaTecnica)
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroNutricionalPorIdFichaTecnica(IdFichaTecnica)
                    select O1).ToList();
        }
        

        public List<CAL_FTFrecuenciaAnalisis> GetFrecuenciasAnalisis(int IdFichaTecnica)
        {
            return (from O1 in dcSoftwareCalidad.CAL_FTFrecuenciaAnalisis
                    where O1.IdFichaTecnica == IdFichaTecnica
                    select O1).ToList();
        }

        public List<CAL_FTControlVersion> GetControlVersiones(int IdFichaTecnica)
        {
            return dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == IdFichaTecnica).ToList();
        }

        public List<CAL_GetFichasTecnicasPorClienteSAPResult> GetFT(string id)
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetFichasTecnicasPorClienteSAP(id)
                    select O1).ToList();
        }

        
        #endregion

        #region 4. Fichas Tecnicas
        public List<SelectFichaTecnica> GetFichasTecnicas(string id)
        {
            List<SelectFichaTecnica> ftList = new List<SelectFichaTecnica>();
            var fts = GetFT(id);
            foreach (var ft in fts)
            {
                var sacos = GetSaco(ft.IdFichaTecnica);
                var sacosList = new List<SelectFichaTecnicaSacos>();
                foreach (var saco in sacos)
                {
                    sacosList.Add(new SelectFichaTecnicaSacos()
                    {
                         IdSaco      = saco.IdSaco,
                         Nombre      = saco.Nombre,
                         Peso        = saco.Peso,
                         Descripcion = saco.Descripcion,
                         ColorHilo   = saco.ColorHilo
                    });
                }

                var parametroanalisis = GetParametroAnalisis(ft.IdFichaTecnica);
                var parametroanalisisList = new List<SelectFichaTecnicaParametroAnalisis>();
                foreach (var parametro in parametroanalisis)
                {
                    parametroanalisisList.Add(new SelectFichaTecnicaParametroAnalisis()
                    {
                        IdParametroAnalisis = parametro.IdParametroAnalisis,
                        Nombre              = parametro.Nombre,
                        Nombre_en           = parametro.Nombre_en,
                        UM                  = parametro.UM,
                        UM_en               = parametro.UM_en,
                        MinValidValue       = parametro.MinValidValue.Value,
                        MaxValidValue       = parametro.MaxValidValue.Value
                    });
                }

                var parametroanalisispesticida      = GetParametroAnalisisPesticida(ft.IdFichaTecnica);
                var parametroanalisispesticidaList  = new List<SelectFichaTecnicaParametroPesticida>();
                foreach (var parametroPesticida in parametroanalisispesticida)
                {
                    parametroanalisispesticidaList.Add(new SelectFichaTecnicaParametroPesticida()
                    {
                        Nombre        = parametroPesticida.Nombre,
                        Nombre_en     = parametroPesticida.Nombre_en,
                        UM            = parametroPesticida.UM,
                        UM_en         = parametroPesticida.UM_en,
                        MinValidValue = parametroPesticida.MinValidValue.Value,
                        MaxValidValue = parametroPesticida.MaxValidValue.Value
                    });
                }

                var parametroanalisismetalespesados = GetParametroAnalisisMetalPesado(ft.IdFichaTecnica);
                var parametroanalisismetalespesadosList = new List<SelectFichaTecnicaParametroMetalesPesados>();
                foreach (var parametroMetalesPesados in parametroanalisismetalespesados)
                {
                    parametroanalisismetalespesadosList.Add(new SelectFichaTecnicaParametroMetalesPesados()
                    {
                        Nombre        = parametroMetalesPesados.Nombre,
                        Nombre_en     = parametroMetalesPesados.Nombre_en,
                        UM            = parametroMetalesPesados.UM,
                        UM_en         = parametroMetalesPesados.UM_en,
                        MinValidValue = parametroMetalesPesados.MinValidValue.Value,
                        MaxValidValue = parametroMetalesPesados.MaxValidValue.Value
                    });
                }

                var parametroanalisismicotoxinas = GetParametroAnalisisMicotoxinas(ft.IdFichaTecnica);
                var parametroanalisismicotoxinasList = new List<SelectFichaTecnicaParametroMicotoxinas>();
                foreach (var parametroMicotoxinas in parametroanalisismicotoxinas)
                {
                    parametroanalisismicotoxinasList.Add(new SelectFichaTecnicaParametroMicotoxinas()
                    {
                        Nombre        = parametroMicotoxinas.Nombre,
                        Nombre_en     = parametroMicotoxinas.Nombre_en,
                        UM            = parametroMicotoxinas.UM,
                        UM_en         = parametroMicotoxinas.UM_en,
                        MinValidValue = parametroMicotoxinas.MinValidValue.Value,
                        MaxValidValue = parametroMicotoxinas.MaxValidValue.Value
                    });
                }

                var parametroanalisismicrobiologia = GetParametroAnalisisMicrobiologia(ft.IdFichaTecnica);
                var parametroanalisismicrobiologiaList = new List<SelectFichaTecnicaParametroMicrobiologia>();
                foreach (var parametroMicrobiologia in parametroanalisismicrobiologia)
                {
                    parametroanalisismicrobiologiaList.Add(new SelectFichaTecnicaParametroMicrobiologia()
                    {
                        Nombre        = parametroMicrobiologia.Nombre,
                        Nombre_en     = parametroMicrobiologia.Nombre_en,
                        UM            = parametroMicrobiologia.UM,
                        UM_en         = parametroMicrobiologia.UM_en,
                        MinValidValue = parametroMicrobiologia.MinValidValue.Value,
                        MaxValidValue = parametroMicrobiologia.MaxValidValue.Value
                    });
                }

                var parametroanalisisnutricionales = GetParametroAnalisisNutricionales(ft.IdFichaTecnica);
                var parametroanalisisnutricionalesList = new List<SelectFichaTecnicaParametroNutricionales>();
                foreach (var parametroNutricionales in parametroanalisisnutricionales)
                {
                    parametroanalisisnutricionalesList.Add(new SelectFichaTecnicaParametroNutricionales()
                    {
                        Nombre        = parametroNutricionales.Nombre,
                        Nombre_en     = parametroNutricionales.Nombre_en,
                        UM            = parametroNutricionales.UM,
                        UM_en         = parametroNutricionales.UM_en,
                        MinValidValue = parametroNutricionales.MinValidValue.Value,
                        MaxValidValue = parametroNutricionales.MaxValidValue.Value
                    });
                }

                var frecuenciaanalisis = GetFrecuenciasAnalisis(ft.IdFichaTecnica);
                var frecuenciaanalisisList = new List<SelectFichaTecnicaFrecuenciaAnalisis>();
                foreach (var frecuenciaAnalisis in frecuenciaanalisis)
                {
                    frecuenciaanalisisList.Add(new SelectFichaTecnicaFrecuenciaAnalisis()
                    {
                        Tipo          = frecuenciaAnalisis.CAL_TipoAnalisis.Descripcion,
                        Tipo_en       = frecuenciaAnalisis.CAL_TipoAnalisis.Descripcion_en,
                        Frecuencia    = frecuenciaAnalisis.CAL_FrecuenciaAnalisis.Frecuencia,
                        Frecuencia_en = frecuenciaAnalisis.CAL_FrecuenciaAnalisis.Frecuencia_en
                    });
                }

                var controlversion = GetControlVersiones(ft.IdFichaTecnica);
                var controlversionList = new List<SelectFichaTecnicaControlVersion>();
                foreach (var controlVersion in controlversion)
                {
                    controlversionList.Add(new SelectFichaTecnicaControlVersion()
                    {
                        Version     = controlVersion.Version,
                        Item        = controlVersion.CAL_FTControlVersionItem.Nombre,
                        Cambios     = controlVersion.Cambios,
                        Motivo      = controlVersion.CAL_FTControlVersionMotivo.Descripcion,
                        Solicitante = controlVersion.CAL_FTControlVersionSolicitante.Nombre,
                        Fecha       = string.Format("{0:dd/MM/yy}", controlVersion.FechaHoraIns)
                    });
                }

                ftList.Add(new SelectFichaTecnica()
                {
                    IdFichaTecnica                  = ft.IdFichaTecnica,
                    Codigo                          = ft.Codigo,
                    Version                         = ft.Version,
                    IdCliente                       = ft.IdCliente,
                    IdClienteSap                    = id,
                    Cliente                         = GetCliente(ft.IdCliente).RazonSocial,
                    FamiliaProducto                 = GetProducto(ft.IdProducto).Nombre.ToString(),
                    Producto                        = GetSubproducto(ft.IdSubproducto).Nombre.ToString(),
                    Pais                            = GetPais(ft.PaisCodigo).PaisNombreLocal.ToString(),
                    Sag                             = string.Format("{0}", ft.Sag),
                    Fumigacion                      = string.Format("{0}", ft.Fumigacion),
                    PesoTotalPickingTest            = ft.PesoTotalPickingTest,
                    Granel                          = string.Format("{0}", ft.Granel),
                    Sacos                           = sacosList,
                    Temperatura                     = string.Format("{0} a {1} °C", GetTemperatura(ft.IdTemperatura.Value).MinValidValue, GetTemperatura(ft.IdTemperatura.Value).MaxValidValue),
                    HumedadRelativa                 = string.Format("{0}", ft.HumedadRelativa),
                    VidaUtil                        = ft.VidaUtil.Value,
                    ParametroAnalisis               = parametroanalisisList,
                    ParametroAnalisisPesticida      = parametroanalisispesticidaList,
                    ParametroAnalisisMetalesPesados = parametroanalisismetalespesadosList,
                    ParametroAnalisisMicotoxinas    = parametroanalisismicotoxinasList,
                    ParametroAnalisisMicrobiologia  = parametroanalisismicrobiologiaList,
                    ParametroAnalisisNutricionales  = parametroanalisisnutricionalesList,
                    FrecuenciaAnalisis              = frecuenciaanalisisList,
                    ControlVersion                  = controlversionList,
                    Observacion                     = string.Format("{0}", HttpUtility.HtmlEncode(ft.Observacion)),
                    VerificacionCliente             = string.Format("{0}", ft.VerificacionCliente)
                });
            }
            return ftList;
        }
        #endregion

        #region 5. Ordenes de Produción
        public List<SelectEmbarcador> GetEmbarcadores()
        {
            List<SelectEmbarcador> list = (from X in dcSoftwareCalidad.CAL_Exportador
                                                     orderby X.Nombre
                                                     select new SelectEmbarcador
                                                     {
                                                         IdEmbarcador = X.IdExportador,
                                                         Nombre = X.Nombre
                                                     }).ToList();
            return list;
        }

        public List<SelectConsignatario> GetConsignatarios(int id)
        {
            List<SelectConsignatario> list = new List<SelectConsignatario>();
            CAL_Exportador embarcador = dcSoftwareCalidad.CAL_Exportador.SingleOrDefault(X => X.IdExportador == id);
            List<Cliente> consignatarioList = (from X in dcAgroFichas.Cliente
                                               join Y in dcAgroFichas.ClienteEmpresa on X.IdCliente equals Y.IdCliente
                                               where Y.IdEmpresa == embarcador.IdEmpresa
                                               orderby X.RazonSocial
                                               select X).ToList();

            foreach (var consignatario in consignatarioList)
            {
                list.Add(new SelectConsignatario()
                {
                    IdConsignatario = consignatario.IdCliente,
                    Nombre = consignatario.RazonSocial
                });
            }

            return list;
        }

        public List<SelectPaises> GetPaises()
        {
            List<SelectPaises> list = (from X in dcAgroFichas.Pais
                                                     orderby X.PaisNombre
                                                     select new SelectPaises
                                                     {
                                                         PaisCodigo = X.PaisCodigo,
                                                         PaisNombre = X.PaisNombre
                                                     }).ToList();
            return list;
        }

        public List<SelectCarriers> GetCarriers()
        {
            List<SelectCarriers> selectList = (from X in dcSoftwareCalidad.Carrier
                                                     orderby X.Nombre
                                                     select new SelectCarriers
                                                     {
                                                         IdCarrier = X.IdCarrier,
                                                         Nombre = X.Nombre
                                                     }).ToList();
            return selectList;
        }

        public List<SelectBarco> GetBarcos(int id)
        {
            List<SelectBarco> list = new List<SelectBarco>();
            List<Barco> barcoList = dcSoftwareCalidad.Barco.Where(X => X.IdCarrier == id && X.Habilitado == true).ToList();

            foreach (var barco in barcoList)
            {
                list.Add(new SelectBarco()
                {
                    IdBarco = barco.IdBarco,
                    Nombre = barco.Nombre
                });
            }

            return list;
        }

        public List<SelectTransporteTerrestre> GetTransportistas()
        {
            List<SelectTransporteTerrestre> selectList = (from O1 in dcAgroFichas.LOG_Transportista
                                                     select new SelectTransporteTerrestre
                                                     {
                                                         IdTransportista = O1.IdTransportista,
                                                         Nombre = O1.Nombre
                                                     }).ToList();
            return selectList;
        }
        #endregion

        public List<SelectFamiliaProducto> GetFamiliaProducto()
        {
            List<SelectFamiliaProducto> list = (from X in dcSoftwareCalidad.vw_CAL_Producto.DistinctBy(X => X.Familia)
                                                orderby X.Familia
                                                select new SelectFamiliaProducto
                                                {
                                                    IdFamiliaProducto = X.IdProducto,
                                                    Nombre = X.Familia
                                                }).ToList();

            return list;
        }

        public List<SelectProducto> GetProductos()
        {
            List<SelectProducto> list = (from X in dcSoftwareCalidad.vw_CAL_ProductoConEspesor
                                         orderby X.Familia,X.Producto
                                         select new SelectProducto
                                         {
                                             IdProducto = X.IdSubproducto,
                                             Nombre = X.Producto,
                                             FamiliaProducto = new SelectFamiliaProducto()
                                             {
                                                 IdFamiliaProducto = X.IdProducto,
                                                 Nombre = X.Familia,
                                                 IdEspesor = (X.IdEspesorProducto == null ? 0 : X.IdEspesorProducto.Value),
                                                 Espesor = X.Espesor
                                             }
                                         }).ToList();

            return list;
        }

        public List<SelectEspesor> GetEspesorProductos()
        {
            List<SelectEspesor> list = (from X in dcSoftwareCalidad.CAL_EspesorProducto
                                        orderby X.IdEspesorProducto ascending
                                        select new SelectEspesor
                                        {
                                            IdEspesor = X.IdEspesorProducto,
                                            Min = X.Min,
                                            Max = X.Max,
                                            Avg = X.Avg
                                        }).ToList();

            return list;
        }

        public List<SelectSaco> GetSaco()
        {
            List<SelectSaco> list = (from X in dcSoftwareCalidad.CAL_Saco
                                         orderby X.Nombre ascending
                                         select new SelectSaco
                                         {
                                             IdSaco = X.IdSaco,
                                             Nombre = X.Nombre
                                         }).ToList();

            return list;
        }

        public List<SelectPesoSaco> GetPesoSaco(int idSaco)
        {
            List<SelectPesoSaco> list = (from X in dcSoftwareCalidad.CAL_PesoSaco
                                         join Y in dcSoftwareCalidad.CAL_PesoTipoSaco on X.IdPesoSaco equals Y.IdPesoSaco
                                         join Z in dcSoftwareCalidad.CAL_TipoSaco on Y.IdTipoSaco equals Z.IdTipoSaco
                                         join S in dcSoftwareCalidad.CAL_Saco on Z.IdTipoSaco equals S.IdTipoSaco
                                         where S.IdSaco == idSaco && X.Habilitado == true
                                         orderby X.Peso ascending
                                         select new SelectPesoSaco
                                         { 
                                             IdPesoSaco = X.IdPesoSaco,
                                             Peso = X.Peso
                                         }).ToList();

            return list;
        }

        public List<SelectContenedor> GetContenedor()
        {
            List<SelectContenedor> list = (from X in dcSoftwareCalidad.CAL_Contenedor
                                     orderby X.Tamaño ascending
                                     where X.AptoParaAlimentos == true
                                     select new SelectContenedor
                                     {
                                         IdContenedor = X.IdContenedor,
                                         Nombre = X.Tamaño.ToString()
                                     }).ToList();

            return list;
        }


        [HttpPost]
        public IHttpActionResult InsertOP(string loteComercial, string observaciones, string numeroViaje, int tipoOP, bool fumigacion, int idCliente, string paisCodigo, int idCarrier, int idExportador, int idBarco, bool inspeccionSAG)
        {
            CAL_OrdenProduccion ordenProduccion = new CAL_OrdenProduccion();

            try
            {
                ordenProduccion.LoteComercial = loteComercial.ToUpper();
                ordenProduccion.FichaTecnica = "DE ACUERDO A LA FICHA TÉCNICA DEL CLIENTE";
                if (string.IsNullOrEmpty(observaciones))
                    ordenProduccion.Observaciones = "";
                else
                    ordenProduccion.Observaciones = observaciones;

                if (string.IsNullOrEmpty(numeroViaje))
                    ordenProduccion.NumeroViaje = "";
                else
                    ordenProduccion.NumeroViaje = numeroViaje;

                ordenProduccion.IdExportador = idExportador;
                ordenProduccion.InspeccionSAG = inspeccionSAG;
                ordenProduccion.IdBarco = idBarco;
                ordenProduccion.IdCarrier = idCarrier;
                ordenProduccion.PaisCodigo = paisCodigo;
                ordenProduccion.IdCliente = idCliente;
                ordenProduccion.Fumigacion = fumigacion;
                ordenProduccion.IdTipoOrdenProduccion = tipoOP;
                ordenProduccion.InicioProduccion = DateTime.Now;
                ordenProduccion.TerminoProduccion = DateTime.Now;
                ordenProduccion.Autorizado = false;
                ordenProduccion.Terminada = false;
                ordenProduccion.AutorizadoAuto = false;
                ordenProduccion.Habilitado = true;
                ordenProduccion.FechaZarpe = DateTime.Now;
                ordenProduccion.Fecha = DateTime.Now;
                ordenProduccion.FechaHoraIns = DateTime.Now;
                ordenProduccion.IpIns = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                ordenProduccion.UserIns = "camiongo";
                dcSoftwareCalidad.CAL_OrdenProduccion.InsertOnSubmit(ordenProduccion);
                dcSoftwareCalidad.SubmitChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
            return Ok(ordenProduccion.IdOrdenProduccion);
        }

        [HttpPost]
        public IHttpActionResult InsertOPDetalleEnv(int idOP, int idSubProducto, int idEspesor, int idSaco, decimal cantidadProducto, string rowKey, int idContenedor, decimal cantidadContenedores, int cantidadSacos, int idPesoSaco, int sacosPorContenedor, int idFichaTecnica)
        {
            try
            {
                CAL_Producto cAL_Producto = (from S1 in dcSoftwareCalidad.CAL_Subproducto
                                             join P1 in dcSoftwareCalidad.CAL_Producto on S1.IdProducto equals P1.IdProducto
                                             where S1.IdSubproducto == idSubProducto
                                             && S1.Habilitado == true
                                             && P1.Habilitado == true
                                             select P1).SingleOrDefault();

                if (cAL_Producto == null)
                {
                    throw new HttpException(404, "No existe el producto");
                }

                CAL_DetalleOrdenProduccion detalleOrdenProduccion = new CAL_DetalleOrdenProduccion();
                detalleOrdenProduccion.IdOrdenProduccion = idOP;
                detalleOrdenProduccion.FechaHoraIns = DateTime.Now;
                detalleOrdenProduccion.IpIns = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                detalleOrdenProduccion.UserIns = "camiongo";
                detalleOrdenProduccion.RowKey = rowKey;
                detalleOrdenProduccion.IdProducto = cAL_Producto.IdProducto;
                detalleOrdenProduccion.IdSubproducto = idSubProducto;
                detalleOrdenProduccion.CantidadProducto = cantidadProducto;
                if (idEspesor != 0)
                    detalleOrdenProduccion.IdEspesorProducto = idEspesor;
                detalleOrdenProduccion.IdSaco = idSaco;
                detalleOrdenProduccion.IdContenedor = idContenedor;
                detalleOrdenProduccion.CantidadContenedores = cantidadContenedores;
                detalleOrdenProduccion.CantidadSacos = cantidadSacos;
                detalleOrdenProduccion.IdPesoSaco = idPesoSaco;
                detalleOrdenProduccion.SacosPorContenedor = sacosPorContenedor;
                detalleOrdenProduccion.IdFichaTecnica = idFichaTecnica;
                dcSoftwareCalidad.CAL_DetalleOrdenProduccion.InsertOnSubmit(detalleOrdenProduccion);
                dcSoftwareCalidad.SubmitChanges();
            }catch(Exception ex)
            {
                return BadRequest(ex.ToString());
            }
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult InsertOPDetalleGra(int idOP, int idSubProducto, decimal cantidadProducto, string rowKey, int idContenedor, decimal cantidadContenedores, int cantidadPorContenedor)
        {
            try
            {
                CAL_Producto cAL_Producto = (from S1 in dcSoftwareCalidad.CAL_Subproducto
                                             join P1 in dcSoftwareCalidad.CAL_Producto on S1.IdProducto equals P1.IdProducto
                                             where S1.IdSubproducto == idSubProducto
                                             && S1.Habilitado == true
                                             && P1.Habilitado == true
                                             select P1).SingleOrDefault();

                if (cAL_Producto == null)
                {
                    throw new HttpException(404, "No existe el producto");
                }

                CAL_DetalleOrdenProduccion detalleOrdenProduccion = new CAL_DetalleOrdenProduccion();
                detalleOrdenProduccion.IdOrdenProduccion = idOP;
                detalleOrdenProduccion.IdProducto = cAL_Producto.IdProducto;
                detalleOrdenProduccion.IdSubproducto = idSubProducto;
                detalleOrdenProduccion.CantidadProducto = cantidadProducto;
                detalleOrdenProduccion.IdContenedor = idContenedor;
                detalleOrdenProduccion.CantidadContenedores = cantidadContenedores;
                detalleOrdenProduccion.CantidadPorContenedor = cantidadPorContenedor;
                detalleOrdenProduccion.RowKey = rowKey;
                detalleOrdenProduccion.FechaHoraIns = DateTime.Now;
                detalleOrdenProduccion.IpIns = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                detalleOrdenProduccion.UserIns = "camiongo";
                dcSoftwareCalidad.CAL_DetalleOrdenProduccion.InsertOnSubmit(detalleOrdenProduccion);
                dcSoftwareCalidad.SubmitChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult InsertOPTransportista(int idOP, int idTransportista)
        {
            try
            {
                LOG_Transportista lOG_Transportista = dcAgroFichas.LOG_Transportista.SingleOrDefault(X => X.IdTransportista == idTransportista);
                if (lOG_Transportista == null)
                {
                    throw new HttpException(404, "El transportista no existe");
                }

                CAL_TransporteTerrestre transporteTerrestre = new CAL_TransporteTerrestre();
                transporteTerrestre.IdOrdenProduccion = idOP;
                transporteTerrestre.IdTransportista = idTransportista;
                transporteTerrestre.UserIns = "camiongo";
                transporteTerrestre.FechaHoraIns = DateTime.Now;
                transporteTerrestre.IpIns = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                dcSoftwareCalidad.CAL_TransporteTerrestre.InsertOnSubmit(transporteTerrestre);
                dcSoftwareCalidad.SubmitChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
            return Ok();
        }


        [HttpPost]
        public IHttpActionResult AnularOP(int IdOP)
        {
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == IdOP && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                throw new HttpException(404, "Item Not Found");
            }

            try
            {
                ordenProduccion.Habilitado = false;
                ordenProduccion.UserUpd = "camiongo";
                ordenProduccion.FechaHoraUpd = System.DateTime.Now;
                ordenProduccion.IpUpd = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                dcSoftwareCalidad.SubmitChanges();
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
