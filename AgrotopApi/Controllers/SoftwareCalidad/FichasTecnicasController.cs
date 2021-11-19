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
    public class FichasTecnicasController : ApiController
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
                        IdSaco = saco.IdSaco,
                        Nombre = saco.Nombre,
                        Peso = saco.Peso,
                        Descripcion = saco.Descripcion,
                        ColorHilo = saco.ColorHilo
                    });
                }

                var parametroanalisis = GetParametroAnalisis(ft.IdFichaTecnica);
                var parametroanalisisList = new List<SelectFichaTecnicaParametroAnalisis>();
                foreach (var parametro in parametroanalisis)
                {
                    parametroanalisisList.Add(new SelectFichaTecnicaParametroAnalisis()
                    {
                        IdParametroAnalisis = parametro.IdParametroAnalisis,
                        Nombre = parametro.Nombre,
                        Nombre_en = parametro.Nombre_en,
                        UM = parametro.UM,
                        UM_en = parametro.UM_en,
                        MinValidValue = parametro.MinValidValue.Value,
                        MaxValidValue = parametro.MaxValidValue.Value
                    });
                }

                var parametroanalisispesticida = GetParametroAnalisisPesticida(ft.IdFichaTecnica);
                var parametroanalisispesticidaList = new List<SelectFichaTecnicaParametroPesticida>();
                foreach (var parametroPesticida in parametroanalisispesticida)
                {
                    parametroanalisispesticidaList.Add(new SelectFichaTecnicaParametroPesticida()
                    {
                        Nombre = parametroPesticida.Nombre,
                        Nombre_en = parametroPesticida.Nombre_en,
                        UM = parametroPesticida.UM,
                        UM_en = parametroPesticida.UM_en,
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
                        Nombre = parametroMetalesPesados.Nombre,
                        Nombre_en = parametroMetalesPesados.Nombre_en,
                        UM = parametroMetalesPesados.UM,
                        UM_en = parametroMetalesPesados.UM_en,
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
                        Nombre = parametroMicotoxinas.Nombre,
                        Nombre_en = parametroMicotoxinas.Nombre_en,
                        UM = parametroMicotoxinas.UM,
                        UM_en = parametroMicotoxinas.UM_en,
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
                        Nombre = parametroMicrobiologia.Nombre,
                        Nombre_en = parametroMicrobiologia.Nombre_en,
                        UM = parametroMicrobiologia.UM,
                        UM_en = parametroMicrobiologia.UM_en,
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
                        Nombre = parametroNutricionales.Nombre,
                        Nombre_en = parametroNutricionales.Nombre_en,
                        UM = parametroNutricionales.UM,
                        UM_en = parametroNutricionales.UM_en,
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
                        Tipo = frecuenciaAnalisis.CAL_TipoAnalisis.Descripcion,
                        Tipo_en = frecuenciaAnalisis.CAL_TipoAnalisis.Descripcion_en,
                        Frecuencia = frecuenciaAnalisis.CAL_FrecuenciaAnalisis.Frecuencia,
                        Frecuencia_en = frecuenciaAnalisis.CAL_FrecuenciaAnalisis.Frecuencia_en
                    });
                }

                var controlversion = GetControlVersiones(ft.IdFichaTecnica);
                var controlversionList = new List<SelectFichaTecnicaControlVersion>();
                foreach (var controlVersion in controlversion)
                {
                    controlversionList.Add(new SelectFichaTecnicaControlVersion()
                    {
                        Version = controlVersion.Version,
                        Item = controlVersion.CAL_FTControlVersionItem.Nombre,
                        Cambios = controlVersion.Cambios,
                        Motivo = controlVersion.CAL_FTControlVersionMotivo.Descripcion,
                        Solicitante = controlVersion.CAL_FTControlVersionSolicitante.Nombre,
                        Fecha = string.Format("{0:dd/MM/yy}", controlVersion.FechaHoraIns)
                    });
                }

                ftList.Add(new SelectFichaTecnica()
                {
                    IdFichaTecnica = ft.IdFichaTecnica,
                    Codigo = ft.Codigo,
                    Version = ft.Version,
                    IdCliente = ft.IdCliente,
                    IdClienteSap = id,
                    Cliente = GetCliente(ft.IdCliente).RazonSocial,
                    FamiliaProducto = GetProducto(ft.IdProducto).Nombre.ToString(),
                    Producto = GetSubproducto(ft.IdSubproducto).Nombre.ToString(),
                    Pais = GetPais(ft.PaisCodigo).PaisNombreLocal.ToString(),
                    Sag = string.Format("{0}", ft.Sag),
                    Fumigacion = string.Format("{0}", ft.Fumigacion),
                    PesoTotalPickingTest = ft.PesoTotalPickingTest,
                    Granel = string.Format("{0}", ft.Granel),
                    Sacos = sacosList,
                    Temperatura = string.Format("{0} a {1} °C", GetTemperatura(ft.IdTemperatura.Value).MinValidValue, GetTemperatura(ft.IdTemperatura.Value).MaxValidValue),
                    HumedadRelativa = string.Format("{0}", ft.HumedadRelativa),
                    VidaUtil = ft.VidaUtil.Value,
                    ParametroAnalisis = parametroanalisisList,
                    ParametroAnalisisPesticida = parametroanalisispesticidaList,
                    ParametroAnalisisMetalesPesados = parametroanalisismetalespesadosList,
                    ParametroAnalisisMicotoxinas = parametroanalisismicotoxinasList,
                    ParametroAnalisisMicrobiologia = parametroanalisismicrobiologiaList,
                    ParametroAnalisisNutricionales = parametroanalisisnutricionalesList,
                    FrecuenciaAnalisis = frecuenciaanalisisList,
                    ControlVersion = controlversionList,
                    Observacion = string.Format("{0}", HttpUtility.HtmlEncode(ft.Observacion)),
                    VerificacionCliente = string.Format("{0}", ft.VerificacionCliente)
                });
            }
            return ftList;
        }

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
                                         orderby X.Familia, X.Producto
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

        [HttpPost]
        public IHttpActionResult UpdateVerificacionFT(int IdFichaTecnica, bool VerificacionCliente)
        {
            CAL_FT fichaTecnica = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == IdFichaTecnica && X.Habilitado == true);
            if (fichaTecnica == null)
            {
                throw new HttpException(404, "Item Not Found");
            }

            try
            {
                fichaTecnica.VerificacionCliente = VerificacionCliente;
                fichaTecnica.UserUpd = "camiongo";
                fichaTecnica.FechaHoraUpd = System.DateTime.Now;
                fichaTecnica.IpUpd = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
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
