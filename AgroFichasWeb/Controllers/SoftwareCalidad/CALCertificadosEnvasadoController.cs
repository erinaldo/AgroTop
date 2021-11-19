using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALCertificadosEnvasadoController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        public CALCertificadosEnvasadoController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALCertificadosEnvasado
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(377);

            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = from us in dcAgroFichas.PlantaUsuario
                                                  join pl in dcAgroFichas.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
                                                  where us.UserID == CurrentUser.UserID
                                                  && pl.Habilitado
                                                  select new SelectListItem
                                                  {
                                                      Value = pl.IdPlantaProduccion.ToString(),
                                                      Text = pl.Nombre,
                                                      Selected = IdPlantaProduccionSelect == pl.IdPlantaProduccion
                                                  };
            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            List<CAL_CertificadoEnvasado> list = dcSoftwareCalidad.CAL_CertificadoEnvasado
                .Where(X => X.Habilitado == true && X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect)
                .OrderByDescending(X => X.FechaHoraIns)
                .OrderByDescending(X => X.IdCertificadoEnvasado).ToList();
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["PlantaProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(378),
                                                      CheckPermiso(377),
                                                      CheckPermiso(379),
                                                      CheckPermiso(380));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(378);
            CAL_CertificadoEnvasado certificadoEnvasado = new CAL_CertificadoEnvasado();
            return View(certificadoEnvasado);
        }

        [HttpPost]
        public ActionResult Crear(CAL_CertificadoEnvasado certificadoEnvasado, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(378);
            if (ModelState.IsValid)
            {
                try
                {
                    CAL_GetFichaTecnicaPorDetalleOrdenProduccion_PalletsResult result = dcSoftwareCalidad.CAL_GetFichaTecnicaPorDetalleOrdenProduccion_Pallets(certificadoEnvasado.IdDetalleOrdenProduccion).FirstOrDefault();
                    if (result == null)
                        return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

                    // Actualizar Luego
                    certificadoEnvasado.CodigoInterno    = result.Codigo;
                    certificadoEnvasado.NEspecificacion  = result.Version;
                    certificadoEnvasado.Version          = result.Version;
                    certificadoEnvasado.LoteComercial    = "";
                    certificadoEnvasado.Toneladas        = 0;
                    certificadoEnvasado.Reserva          = "";
                    certificadoEnvasado.PaisOrigen       = "";
                    certificadoEnvasado.PaisDestino      = "";
                    certificadoEnvasado.Embarcador       = "";
                    certificadoEnvasado.Consignatario    = "";
                    certificadoEnvasado.certkey          = "";
                    certificadoEnvasado.NFactura         = "0";
                    // Rescatados del Form
                    certificadoEnvasado.Producto         = certificadoEnvasado.Producto.ToUpper();
                    certificadoEnvasado.FechaElaboracion = certificadoEnvasado.FechaElaboracion.ToUpper();
                    certificadoEnvasado.MaterialEmpaque  = certificadoEnvasado.MaterialEmpaque.ToUpper();
                    certificadoEnvasado.CodigoProceso    = certificadoEnvasado.CodigoProceso.ToUpper();
                    certificadoEnvasado.Habilitado       = true;
                    certificadoEnvasado.FechaHoraIns     = DateTime.Now;
                    certificadoEnvasado.IpIns            = RemoteAddr();
                    certificadoEnvasado.UserIns          = User.Identity.Name;
                    dcSoftwareCalidad.CAL_CertificadoEnvasado.InsertOnSubmit(certificadoEnvasado);
                    dcSoftwareCalidad.SubmitChanges();
                    // Actualizado
                    certificadoEnvasado.LoteComercial    = certificadoEnvasado.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.LoteComercial;
                    certificadoEnvasado.Toneladas        = certificadoEnvasado.CAL_DetalleOrdenProduccion.CantidadProducto;
                    certificadoEnvasado.Reserva          = string.Format("{0} {1} {2}", certificadoEnvasado.CAL_LEPallets.Carrier.Nombre, certificadoEnvasado.CAL_LEPallets.Barco.Nombre, certificadoEnvasado.CAL_LEPallets.NReserva);
                    certificadoEnvasado.PaisOrigen       = "CHILE";
                    certificadoEnvasado.PaisDestino      = certificadoEnvasado.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.GetPais(certificadoEnvasado.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.PaisCodigo).PaisNombre.ToUpper();
                    certificadoEnvasado.Embarcador       = "AVENATOP SpA";
                    certificadoEnvasado.Consignatario    = certificadoEnvasado.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.GetCliente(certificadoEnvasado.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdCliente).RazonSocial.ToUpper();
                    certificadoEnvasado.NFactura         = string.Format("{0}", certificadoEnvasado.CAL_LEPallets.NFactura);
                    dcSoftwareCalidad.SubmitChanges();

                    CAL_CertificadoEnvasadoAnalisisPeriodico certificadoEnvasadoAnalisisPeriodico = new CAL_CertificadoEnvasadoAnalisisPeriodico()
                    {
                        IdCertificadoEnvasado = certificadoEnvasado.IdCertificadoEnvasado,
                        FechaHoraIns          = DateTime.Now,
                        IpIns                 = RemoteAddr(),
                        UserIns               = User.Identity.Name
                    };
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoPesticidas"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoPesticidas"], out int IdAnalisisPeriodicoPesticidas))
                            certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoPesticidas = IdAnalisisPeriodicoPesticidas;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMetalesPesados"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMetalesPesados"], out int IdAnalisisPeriodicoMetalesPesados))
                            certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados = IdAnalisisPeriodicoMetalesPesados;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMicotoxinas"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMicotoxinas"], out int IdAnalisisPeriodicoMicotoxinas))
                            certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas = IdAnalisisPeriodicoMicotoxinas;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMicrobiologia"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMicrobiologia"], out int IdAnalisisPeriodicoMicrobiologia))
                            certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia = IdAnalisisPeriodicoMicrobiologia;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoNutricional"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoNutricional"], out int IdAnalisisPeriodicoNutricional))
                            certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoNutricional = IdAnalisisPeriodicoNutricional;
                    dcSoftwareCalidad.CAL_CertificadoEnvasadoAnalisisPeriodico.InsertOnSubmit(certificadoEnvasadoAnalisisPeriodico);
                    dcSoftwareCalidad.SubmitChanges();

                    string[] ids = { };

                    if (!string.IsNullOrEmpty(formCollection["IdMetodologias"]))
                        ids = formCollection["IdMetodologias"].Split(',');

                    foreach (string idx in ids)
                    {
                        CAL_CertificadoEnvasadoMetodologia certificadoEnvasadoMetodologia = new CAL_CertificadoEnvasadoMetodologia()
                        {
                            IdCertificadoEnvasado = certificadoEnvasado.IdCertificadoEnvasado,
                            IdMetodologia         = int.Parse(idx),
                            FechaHoraIns          = DateTime.Now,
                            IpIns                 = RemoteAddr(),
                            UserIns               = User.Identity.Name
                        };
                        dcSoftwareCalidad.CAL_CertificadoEnvasadoMetodologia.InsertOnSubmit(certificadoEnvasadoMetodologia);
                        dcSoftwareCalidad.SubmitChanges();
                    }

                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = certificadoEnvasado.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View(certificadoEnvasado);
        }

        public ActionResult CrearPDF(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoEnvasado certificadoEnvasado = dcSoftwareCalidad.CAL_CertificadoEnvasado.SingleOrDefault(X => X.IdCertificadoEnvasado == id && X.Habilitado == true);
            if (certificadoEnvasado == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            //CAL_LEPalletsContenedor contenedor = dcSoftwareCalidad.CAL_LEPalletsContenedor.SingleOrDefault(X=> X.IdLEPallets == certificadoEnvasado.IdLEPallets);

            CAL_ResponsableArea responsableArea = dcSoftwareCalidad.CAL_ResponsableArea.FirstOrDefault(X => X.IdArea == 2);
            PlantaProduccion plantaProduccion = dcAgroFichas.PlantaProduccion.FirstOrDefault(X => X.IdPlantaProduccion == certificadoEnvasado.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta);

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/calidad_final_template.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA"      , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "NO"         , certificadoEnvasado.NCertificado.ToString());
            RepTemp(ref htmlContent, "AÑO"        , DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "PROD"       , certificadoEnvasado.Producto);
            RepTemp(ref htmlContent, "FTCOD"      , certificadoEnvasado.CodigoInterno);
            RepTemp(ref htmlContent, "FTVER"      , certificadoEnvasado.Version.ToString());
            RepTemp(ref htmlContent, "FTVER"      , certificadoEnvasado.Version.ToString());
            RepTemp(ref htmlContent, "LOTCOM"     , certificadoEnvasado.LoteComercial);
            RepTemp(ref htmlContent, "FECELAB"    , certificadoEnvasado.FechaElaboracion);
            RepTemp(ref htmlContent, "MATEMP"     , certificadoEnvasado.MaterialEmpaque);
            RepTemp(ref htmlContent, "CODPROC"    , certificadoEnvasado.CodigoProceso);
            RepTemp(ref htmlContent, "TON"        , certificadoEnvasado.Toneladas.ToString("N2"));
            RepTemp(ref htmlContent, "RSRVA"      , certificadoEnvasado.Reserva);
            RepTemp(ref htmlContent, "ORG"        , certificadoEnvasado.PaisOrigen);
            RepTemp(ref htmlContent, "DST"        , certificadoEnvasado.PaisDestino);
            RepTemp(ref htmlContent, "EMB"        , certificadoEnvasado.Embarcador);
            RepTemp(ref htmlContent, "CONSIG"     , certificadoEnvasado.Consignatario);
            RepTemp(ref htmlContent, "FACT"       , string.Format("{0}", certificadoEnvasado.NFactura));
            RepTemp(ref htmlContent, "PLANTAPRO"  , plantaProduccion.Nombre);
            RepTemp(ref htmlContent, "RESPONSABLE", responsableArea != null ? responsableArea.GetUser(responsableArea.UserID.Value) != null ? responsableArea.GetUser(responsableArea.UserID.Value).FullName : "" : "");

            #region 1. Análisis Granulometría

            StringBuilder builderGranulometria = new StringBuilder();
            List<CAL_GetTestGranulometriaPalletsPorDetalleOrdenProduccionYListaEmpaqueResult> testsGranulometria = dcSoftwareCalidad.CAL_GetTestGranulometriaPalletsPorDetalleOrdenProduccionYListaEmpaque(certificadoEnvasado.IdDetalleOrdenProduccion, certificadoEnvasado.IdLEPallets).ToList();
            foreach (var test in testsGranulometria)
            {
                builderGranulometria.AppendLine("<tr>");
                builderGranulometria.AppendLine(string.Format("<td class='text-align-left'>{0}</td>"  , test.Nombre));
                builderGranulometria.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.UM));
                builderGranulometria.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", Formatter.Format(test.Value.Value, test.UM, test.FormatString, test.IdParametroAnalisis)));
                builderGranulometria.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", string.Format("<table><tr><td align='center'>{0}</td><td align='center'>{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString, test.IdParametroAnalisis), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString, test.IdParametroAnalisis))));
                builderGranulometria.AppendLine("<td class='text-align-center'>Por Lote</td>");
                builderGranulometria.AppendLine("</tr>");
            }
            RepTemp(ref htmlContent, "GRANULOMETRIA", builderGranulometria.ToString());

            #endregion

            CAL_CertificadoEnvasadoAnalisisPeriodico certificadoEnvasadoAnalisisPeriodico = dcSoftwareCalidad.CAL_CertificadoEnvasadoAnalisisPeriodico.SingleOrDefault(X => X.IdCertificadoEnvasado == certificadoEnvasado.IdCertificadoEnvasado);
            if (certificadoEnvasadoAnalisisPeriodico != null)
            {
                #region 2. Análisis Pesticidas

                if (certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoPesticidas.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisPesticidaResult> tests = GetTestAnalisisPesticida(certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoPesticidas.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class='text-align-left'>{0}</td>"  , test.Nombre));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", Formatter.Format(test.Value, test.UM, test.FormatString)));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", string.Format("<table><tr><td align='center'>{0}</td><td align='center'>{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSPESTICIDAS"      , builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSPESTICIDAS", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSPESTICIDAS", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSPESTICIDAS"      , string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSPESTICIDAS", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSPESTICIDAS", "display: none");
                }

                #endregion

                #region 3. Análisis Metales Pesados

                if (certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisMetalPesadoResult> tests = GetTestAnalisisMetalPesado(certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class='text-align-left'>{0}</td>"  , test.Nombre));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", Formatter.Format(test.Value, test.UM, test.FormatString)));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", string.Format("<table><tr><td align='center'>{0}</td><td align='center'>{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSMETALESPESADOS"      , builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMETALESPESADOS", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMETALESPESADOS", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSMETALESPESADOS"      , string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMETALESPESADOS", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMETALESPESADOS", "display: none");
                }

                #endregion

                #region 4. Análisis Micotoxinas

                if (certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisMicotoxinaResult> tests = GetTestAnalisisMicotoxina(certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class='text-align-left'>{0}</td>"  , test.Nombre));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", Formatter.Format(test.Value, test.UM, test.FormatString)));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", string.Format("<table><tr><td align='center'>{0}</td><td align='center'>{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSMICOTOXINAS"      , builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMICOTOXINAS", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMICOTOXINAS", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSMICOTOXINAS"      , string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMICOTOXINAS", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMICOTOXINAS", "display: none");
                }

                #endregion

                #region 5. Análisis Microbiología

                if (certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisMicrobiologiaResult> tests = GetTestAnalisisMicrobiologia(certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class='text-align-left'>{0}</td>"  , test.Nombre));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", Formatter.Format(test.Value, test.UM, test.FormatString)));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", string.Format("<table><tr><td align='center'>{0}</td><td align='center'>{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSMICROBIOLOGICOS"      , builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMICROBIOLOGICOS", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMICROBIOLOGICOS", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSMICROBIOLOGICOS"      , string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMICROBIOLOGICOS", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMICROBIOLOGICOS", "display: none");
                }

                #endregion

                #region 6. Análisis Nutricional

                if (certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoNutricional.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisNutricionalResult> tests = GetTestAnalisisNutricional(certificadoEnvasadoAnalisisPeriodico.IdAnalisisPeriodicoNutricional.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class='text-align-left'>{0}</td>"  , test.Nombre));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", Formatter.Format(test.Value, test.UM, test.FormatString)));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", string.Format("<table><tr><td align='center'>{0}</td><td align='center'>{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class='text-align-center'>{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSNUTRICIONALES"      , builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSNUTRICIONALES", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSNUTRICIONALES", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSNUTRICIONALES"      , string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSNUTRICIONALES", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSNUTRICIONALES", "display: none");
                }

                #endregion
            }
            else
            {
                #region No hay análisis periódicos

                RepTemp(ref htmlContent, "PARAMETROSPESTICIDAS"           , string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSPESTICIDAS"     , "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSPESTICIDAS"     , "display: none");
                RepTemp(ref htmlContent, "PARAMETROSMETALESPESADOS"       , string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSMETALESPESADOS" , "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSMETALESPESADOS" , "display: none");
                RepTemp(ref htmlContent, "PARAMETROSMICOTOXINAS"          , string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSMICOTOXINAS"    , "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSMICOTOXINAS"    , "display: none");
                RepTemp(ref htmlContent, "PARAMETROSMICROBIOLOGICOS"      , string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSMICROBIOLOGICOS", "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSMICROBIOLOGICOS", "display: none");
                RepTemp(ref htmlContent, "PARAMETROSNUTRICIONALES"        , string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSNUTRICIONALES"  , "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSNUTRICIONALES"  , "display: none");

                #endregion
            }

            #region Metodologías

            StringBuilder builderMetodologia = new StringBuilder();
            List<CAL_CertificadoEnvasadoMetodologia> metodologiasList = dcSoftwareCalidad.CAL_CertificadoEnvasadoMetodologia.Where(X => X.IdCertificadoEnvasado == certificadoEnvasado.IdCertificadoEnvasado).ToList();
            if (metodologiasList.Count > 0)
            {
                foreach (CAL_CertificadoEnvasadoMetodologia metodologia in metodologiasList)
                {
                    builderMetodologia.AppendLine("<tr>");
                    builderMetodologia.AppendLine(string.Format("<td class='text-align-left'><strong>{0}</strong></td>", metodologia.CAL_Metodologia.Analisis));
                    builderMetodologia.AppendLine(string.Format("<td class='text-align-left'>: {0}</td>", metodologia.CAL_Metodologia.Tecnica));
                    builderMetodologia.AppendLine("</tr>");
                }
                RepTemp(ref htmlContent, "METODOLOGIA"      , builderMetodologia.ToString());
                RepTemp(ref htmlContent, "TITULOMETODOLOGIA", "");
                RepTemp(ref htmlContent, "CUERPOMETODOLOGIA", "");
            }
            else
            {
                RepTemp(ref htmlContent, "METODOLOGIA"      , string.Empty);
                RepTemp(ref htmlContent, "TITULOMETODOLOGIA", "display: none");
                RepTemp(ref htmlContent, "CUERPOMETODOLOGIA", "display: none");
            }

            #endregion

            try
            {
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.html", certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.pdf", certkey));
                byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()
                {
                    Orientation = NReco.PdfGenerator.PageOrientation.Portrait,
                    Margins = new NReco.PdfGenerator.PageMargins()
                    {
                        Bottom = 15,
                        Left   = 15,
                        Right  = 15,
                        Top    = 15
                    },
                    Size = NReco.PdfGenerator.PageSize.Letter,
                }).GeneratePdf(htmlContent);
                System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO

                certificadoEnvasado.certkey = certkey;
                dcSoftwareCalidad.SubmitChanges();

                CAL_CertificadoControlVersion controlVersion = new CAL_CertificadoControlVersion()
                {
                    IdTipoCertificado = 2,
                    IdCertificado     = certificadoEnvasado.IdCertificadoEnvasado,
                    certkey           = certificadoEnvasado.certkey,
                    Habilitado        = true,
                    FechaHoraIns      = DateTime.Now,
                    IpIns             = RemoteAddr(),
                    UserIns           = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = certificadoEnvasado.IdCertificadoEnvasado, certkey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT{0}_{1}.txt", certificadoEnvasado.IdCertificadoEnvasado, certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine("No se pudo generar el PDF del certificado");
                writer.WriteLine(ex.ToString());
                writer.Close();

                return RedirectToAction("Index", new { errMsg = "No se pudo generar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult DescargarPDF(int id, string certkey)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoEnvasado certificadoEnvasado= dcSoftwareCalidad.CAL_CertificadoEnvasado.SingleOrDefault(X => X.IdCertificadoEnvasado == id && X.certkey == certkey && X.Habilitado == true);
            if (certificadoEnvasado == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", certificadoEnvasado.certkey));
                return File(pdf, "application/pdf", String.Format("Certificado de Calidad {0}-{1}.pdf", certificadoEnvasado.NCertificado, certificadoEnvasado.FechaHoraIns.Year));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CertificadoEnvasado certificadoEnvasado = dcSoftwareCalidad.CAL_CertificadoEnvasado.SingleOrDefault(X => X.IdCertificadoEnvasado == id && X.Habilitado == true);
            if (certificadoEnvasado == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string errMsg = "";
            string okMsg  = "";

            try
            {
                certificadoEnvasado.Habilitado   = false;
                certificadoEnvasado.UserUpd      = User.Identity.Name;
                certificadoEnvasado.FechaHoraUpd = DateTime.Now;
                certificadoEnvasado.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El certificado {0} ha sido eliminado", certificadoEnvasado.IdCertificadoEnvasado);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult ControlVersiones(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoEnvasado certificadoEnvasado = dcSoftwareCalidad.CAL_CertificadoEnvasado.SingleOrDefault(X => X.IdCertificadoEnvasado == id && X.Habilitado == true);
            if (certificadoEnvasado == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            List<CAL_CertificadoControlVersion> list = dcSoftwareCalidad.CAL_CertificadoControlVersion.Where(X => X.IdCertificado == certificadoEnvasado.IdCertificadoEnvasado && X.IdTipoCertificado == 2 && X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(378),
                                                      CheckPermiso(377),
                                                      CheckPermiso(379),
                                                      CheckPermiso(380));
            ViewData["certificadoEnvasado"] = certificadoEnvasado;
            return View(list);
        }

        public ActionResult DescargarPDF_ControlVersion(int id, string certkey)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoEnvasado certificadoEnvasado = dcSoftwareCalidad.CAL_CertificadoEnvasado.SingleOrDefault(X => X.IdCertificadoEnvasado == id && X.Habilitado == true);
            if (certificadoEnvasado == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            CAL_CertificadoControlVersion controlVersion = dcSoftwareCalidad.CAL_CertificadoControlVersion.SingleOrDefault(X => X.certkey == certkey);
            if (controlVersion == null)
            {
                return RedirectToAction("ControlVersiones", new { errMsg = "No se ha encontrado el control de versión", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", controlVersion.certkey));
                return File(pdf, "application/pdf", String.Format("Certificado de Calidad {0}-{1} V{2:dd-MM-yy HH;mm;ss}.pdf", certificadoEnvasado.NCertificado, certificadoEnvasado.FechaHoraIns.Year, controlVersion.FechaHoraIns));
            }
            catch
            {
                return RedirectToAction("ControlVersiones", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar_ControlVersion(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CertificadoControlVersion controlVersion = dcSoftwareCalidad.CAL_CertificadoControlVersion.SingleOrDefault(X => X.IdCertificadoControlVersion == id);
            if (controlVersion == null)
            {
                return RedirectToAction("ControlVersiones", new { errMsg = "No se ha encontrado el control de versión", okMsg = "" });
            }

            string errMsg = "";
            string okMsg  = "";

            try
            {
                controlVersion.Habilitado   = false;
                controlVersion.UserUpd      = User.Identity.Name;
                controlVersion.FechaHoraUpd = DateTime.Now;
                controlVersion.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El control de versión {0} ha sido eliminado", controlVersion.IdCertificadoControlVersion);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("ControlVersiones", new { id = controlVersion.IdCertificado, errMsg = errMsg, okMsg = okMsg });
        }

        #region --- Funciones PRIVADAS ---

        private List<CAL_GetTestAnalisisPesticidaResult> GetTestAnalisisPesticida(int IdAnalisisPeriodicoPesticidas)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisPesticida(IdAnalisisPeriodicoPesticidas).ToList();
        }

        private List<CAL_GetTestAnalisisMetalPesadoResult> GetTestAnalisisMetalPesado(int IdAnalisisPeriodicoMetalesPesados)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisMetalPesado(IdAnalisisPeriodicoMetalesPesados).ToList();
        }

        private List<CAL_GetTestAnalisisMicotoxinaResult> GetTestAnalisisMicotoxina(int IdAnalisisPeriodicoMicotoxinas)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisMicotoxina(IdAnalisisPeriodicoMicotoxinas).ToList();
        }

        private List<CAL_GetTestAnalisisMicrobiologiaResult> GetTestAnalisisMicrobiologia(int IdAnalisisPeriodicoMicrobiologia)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisMicrobiologia(IdAnalisisPeriodicoMicrobiologia).ToList();
        }

        private List<CAL_GetTestAnalisisNutricionalResult> GetTestAnalisisNutricional(int IdAnalisisPeriodicoNutricional)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisNutricional(IdAnalisisPeriodicoNutricional).ToList();
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}