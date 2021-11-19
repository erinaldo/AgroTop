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
    public class CALCertificadoCertificateAnalysisController : BaseApplicationController
    {
        // GET: CALCertificadoCertificateAnalysis
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        public CALCertificadoCertificateAnalysisController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALCertificadoCertificateAnalysis
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

            List<CAL_CertificadoCertificateAnalysis> list = dcSoftwareCalidad.CAL_CertificadoCertificateAnalysis
                .Where(X => X.Habilitado == true && X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect)
                .OrderByDescending(X => X.FechaHoraIns)
                .OrderByDescending(X => X.IdCertificadoCertificateAnalysis)
                .ToList();
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
            CAL_CertificadoCertificateAnalysis cert = new CAL_CertificadoCertificateAnalysis();
            return View(cert);
        }

        [HttpPost]
        public ActionResult Crear(CAL_CertificadoCertificateAnalysis cert, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(378);
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar Luego
                    cert.ContainerNo    = "";
                    cert.BatchNo        = "";
                    cert.MFD            = "";
                    cert.ExpiryDate     = "";
                    cert.Quantity       = "";
                    cert.NoBags         = "";
                    cert.CertificateKey = "";

                    // Rescatados del Form
                    cert.Habilitado     = true;
                    cert.FechaHoraIns   = DateTime.Now;
                    cert.IpIns          = RemoteAddr();
                    cert.UserIns        = User.Identity.Name;
                    dcSoftwareCalidad.CAL_CertificadoCertificateAnalysis.InsertOnSubmit(cert);
                    dcSoftwareCalidad.SubmitChanges();

                    // Actualizado
                    foreach (var cnt in dcSoftwareCalidad.CAL_GetContenedoresPorDetalleOrdenProduccion_Pallets(cert.IdDetalleOrdenProduccion))
                    {
                        cert.ContainerNo += string.Format("{0},", cnt.NContenedor);
                    }
                    cert.ContainerNo      = cert.ContainerNo.Remove(cert.ContainerNo.Length - 1);
                    cert.BatchNo          = cert.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.LoteComercial;

                    foreach (var fec in dcSoftwareCalidad.CAL_GetFechasElaboracionPorOrdenProduccion_Pallets(cert.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdOrdenProduccion))
                    {
                        cert.MFD         += string.Format("{0:dd/MM/yyyy},", fec.FechaElaboracion);
                        cert.ExpiryDate  += string.Format("{0:dd/MM/yyyy},", fec.FechaVencimiento);
                    }
                    cert.MFD              = cert.MFD.Remove(cert.MFD.Length - 1);
                    cert.ExpiryDate       = cert.ExpiryDate.Remove(cert.ExpiryDate.Length - 1);

                    var ft = dcSoftwareCalidad.CAL_GetSacosPaletizadosPorDetalleOrdenProduccion(cert.IdDetalleOrdenProduccion).FirstOrDefault();
                    if (ft != null)
                    {
                        cert.Quantity     = string.Format("{0:N2} tns.", ft.SacosPaletizadosEnTn);
                        cert.NoBags       = string.Format("{0:N2} bags", ft.SacosPaletizados);
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    CAL_CertificadoCertificateAnalysisAnalisisPeriodico certificateAnalysisAnalisisPeriodico = new CAL_CertificadoCertificateAnalysisAnalisisPeriodico()
                    {
                        IdCertificadoCertificateAnalysis = cert.IdCertificadoCertificateAnalysis,
                        FechaHoraIns                     = DateTime.Now,
                        IpIns                            = RemoteAddr(),
                        UserIns                          = User.Identity.Name
                    };
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoPesticidas"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoPesticidas"], out int IdAnalisisPeriodicoPesticidas))
                            certificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoPesticidas = IdAnalisisPeriodicoPesticidas;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMetalesPesados"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMetalesPesados"], out int IdAnalisisPeriodicoMetalesPesados))
                            certificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados = IdAnalisisPeriodicoMetalesPesados;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMicotoxinas"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMicotoxinas"], out int IdAnalisisPeriodicoMicotoxinas))
                            certificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas = IdAnalisisPeriodicoMicotoxinas;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMicrobiologia"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMicrobiologia"], out int IdAnalisisPeriodicoMicrobiologia))
                            certificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia = IdAnalisisPeriodicoMicrobiologia;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoNutricional"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoNutricional"], out int IdAnalisisPeriodicoNutricional))
                            certificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoNutricional = IdAnalisisPeriodicoNutricional;
                    dcSoftwareCalidad.CAL_CertificadoCertificateAnalysisAnalisisPeriodico.InsertOnSubmit(certificateAnalysisAnalisisPeriodico);
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = cert.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View(cert);
        }

        public ActionResult CrearPDF(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoCertificateAnalysis cert = dcSoftwareCalidad.CAL_CertificadoCertificateAnalysis.SingleOrDefault(X => X.IdCertificadoCertificateAnalysis == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            #region 0. Análisis proximales

            string style__pesticidescharacteristics  = "display:none";
            string style__heavymetalscharacteristics = "display:none";
            string style__microbiologicalparameters  = "display:none";
            string style__mycotoxinscharacteristics  = "display:none";
            string style__chemicalparameters         = "display:none";
            CAL_CertificadoCertificateAnalysisAnalisisPeriodico certificadoCertificateAnalysisAnalisisPeriodico = dcSoftwareCalidad.CAL_CertificadoCertificateAnalysisAnalisisPeriodico.SingleOrDefault(X => X.IdCertificadoCertificateAnalysis == cert.IdCertificadoCertificateAnalysis);
            if (certificadoCertificateAnalysisAnalisisPeriodico != null)
            {
                cert.IdAnalisisPeriodicoPesticidas     = certificadoCertificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoPesticidas;
                cert.IdAnalisisPeriodicoMetalesPesados = certificadoCertificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados;
                cert.IdAnalisisPeriodicoMicrobiologia  = certificadoCertificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia;
                cert.IdAnalisisPeriodicoMicotoxinas    = certificadoCertificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas;
                cert.IdAnalisisPeriodicoNutricional    = certificadoCertificateAnalysisAnalisisPeriodico.IdAnalisisPeriodicoNutricional;
                if (cert.IdAnalisisPeriodicoPesticidas.HasValue)
                    style__pesticidescharacteristics = "display:block";
                if (cert.IdAnalisisPeriodicoMetalesPesados.HasValue)
                    style__heavymetalscharacteristics = "display:block";
                if (cert.IdAnalisisPeriodicoMicrobiologia.HasValue)
                    style__microbiologicalparameters = "display:block";
                if (cert.IdAnalisisPeriodicoMicotoxinas.HasValue)
                    style__mycotoxinscharacteristics = "display:block";
                if (cert.IdAnalisisPeriodicoNutricional.HasValue)
                    style__chemicalparameters = "display:block";
            }
            else
            {
                cert.IdAnalisisPeriodicoPesticidas     = null;
                cert.IdAnalisisPeriodicoMetalesPesados = null;
                cert.IdAnalisisPeriodicoMicrobiologia  = null;
                cert.IdAnalisisPeriodicoMicotoxinas    = null;
                cert.IdAnalisisPeriodicoNutricional    = null;
            }

            #endregion

            var responsableArea = dcSoftwareCalidad.CAL_ResponsableArea.FirstOrDefault(X => X.IdArea == 2);

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/marico-limited-2020.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA"                            , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "REPORTNO"                         , cert.CertificateNumber.ToString());
            RepTemp(ref htmlContent, "REPORTGENERATEDDATE"              , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "AÑO"                              , DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "PRODUCT"                          , cert.CAL_DetalleOrdenProduccion.CAL_Subproducto.Nombre);
            RepTemp(ref htmlContent, "CONTAINERNO"                      , cert.ContainerNo);
            RepTemp(ref htmlContent, "BATCHNO"                          , cert.BatchNo);
            RepTemp(ref htmlContent, "MFD"                              , cert.MFD);
            RepTemp(ref htmlContent, "EXPIRYDATE"                       , cert.ExpiryDate);
            RepTemp(ref htmlContent, "DATEOFDISPATCH"                   , cert.DispatchDate.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "QUANTITY"                         , cert.Quantity);
            RepTemp(ref htmlContent, "NOOFBAGS"                         , cert.NoBags);
            RepTemp(ref htmlContent, "COMMERCIALINVOICE"                , cert.CommercialInvoice);
            RepTemp(ref htmlContent, "GUIDE"                            , cert.Guide);
            RepTemp(ref htmlContent, "RESPONSABLE"                      , responsableArea != null ? responsableArea.GetUser(responsableArea.UserID.Value) != null ? responsableArea.GetUser(responsableArea.UserID.Value).FullName : "" : "");
            RepTemp(ref htmlContent, "STYLE__PESTICIDESCHARACTERISTICS" , style__pesticidescharacteristics);
            RepTemp(ref htmlContent, "STYLE__HEAVYMETALSCHARACTERISTICS", style__heavymetalscharacteristics);
            RepTemp(ref htmlContent, "STYLE__MYCOTOXINSCHARACTERISTICS" , style__mycotoxinscharacteristics);
            RepTemp(ref htmlContent, "STYLE__MICROBIOLOGICALPARAMETERS" , style__microbiologicalparameters);
            RepTemp(ref htmlContent, "STYLE__CHEMICALPARAMETERS"        , style__chemicalparameters);

            #region 1. Análisis físico-químico

            // 1. Análisis físico-químico

            // Creando lista con todos los análisis físico-químico por dia
            var analisisList = new List<CAL_GetMaricoLimitedChemicalPhysicalAnalysisResult>();
            var fechas       = dcSoftwareCalidad.CAL_GetFechasAnalisisPorDetalleOrdenProduccion_Pallets(cert.IdDetalleOrdenProduccion).ToList();
            foreach (var fecha in fechas)
            {
                var analisis = dcSoftwareCalidad.CAL_GetMaricoLimitedChemicalPhysicalAnalysis(cert.IdDetalleOrdenProduccion, fecha.FechaAnalisis).ToList();
                analisisList.AddRange(analisis);
            }

            // Contador de índice de análisis físico-químico
            int I = 0;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<table id='sensory-physicochemical-characteristics'>");
            builder.AppendLine("<thead>");
            builder.AppendLine("	<tr>");
            builder.AppendLine("		<th class='header' rowspan='2' width='10%'>No.</th>");
            builder.AppendLine("		<th class='header' rowspan='2' width='20%'>Parameter</th>");
            builder.AppendLine("		<th class='header' rowspan='2' width='20%'>Marico Spec</th>");
            builder.AppendLine("		<th class='header' colspan='" + fechas.Count + "' width='50%'>Test Results (Day)</th>");
            builder.AppendLine("	</tr>");
            builder.AppendLine("	<tr>");
            foreach (var fecha in fechas)
            {
                builder.AppendLine(string.Format("		<th class='header' align='center'>{0:dd/MM/yyyy}</th>", fecha.FechaAnalisis));
            }
            builder.AppendLine("	</tr>");
            builder.AppendLine("</thead>");
            builder.AppendLine("<tbody>");
            builder.AppendLine("	<tr>");
            builder.AppendLine("		<td align='center'>1.1</td>");
            builder.AppendLine("		<td>Appearance</td>");
            builder.AppendLine("		<td>Independent flakes of uniform size</td>");
            foreach (var fecha in fechas)
            {
                builder.AppendLine("		<td align='center'>Accepted</td>");
            }
            builder.AppendLine("	</tr>");
            builder.AppendLine("	<tr>");
            builder.AppendLine("		<td align='center'>1.2</td>");
            builder.AppendLine("		<td>Odor</td>");
            builder.AppendLine("		<td>Free from any objectionable odor</td>");
            foreach (var fecha in fechas)
            {
                builder.AppendLine("		<td align='center'>Accepted</td>");
            }
            builder.AppendLine("	</tr>");
            builder.AppendLine("	<tr>");
            builder.AppendLine("		<td align='center'>1.3</td>");
            builder.AppendLine("		<td>Taste of cooked product</td>");
            builder.AppendLine("		<td>Comparable to reference</td>");
            foreach (var fecha in fechas)
            {
                builder.AppendLine("		<td align='center'>Accepted</td>");
            }
            builder.AppendLine("	</tr>");
            // Primer loop para obtener la lista de parámetros de análisis
            foreach (var fechaEncabezado in fechas)
            {
                var encabezado = dcSoftwareCalidad.CAL_GetMaricoLimitedChemicalPhysicalAnalysis(cert.IdDetalleOrdenProduccion, fechaEncabezado.FechaAnalisis).ToList();
                foreach (var parametro in encabezado)
                {
                    builder.AppendLine("	</tr>");
                    builder.AppendLine("	<tr>");
                    builder.AppendLine("		<td align='center'>1." + (3 + ++I) + "</td>");
                    builder.AppendLine("		<td>" + parametro.Nombre_en + "</td>");
                    builder.AppendLine("		<td>" + parametro.MaricoSpec_PM + "</td>");
                    // Segundo loop para obtener la lista de parámetros de análisis por fecha
                    foreach (var fecha in fechas)
                    {
                        var analisisPorParametro = analisisList.SingleOrDefault(X => X.Fecha == fecha.FechaAnalisis && X.IdParametroAnalisis == parametro.IdParametroAnalisis);
                        if (analisisPorParametro != null)
                        {
                            builder.AppendLine("		<td align='center'>" + Formatter.Format_en(analisisPorParametro.AvgValue.Value, analisisPorParametro.UM_en, analisisPorParametro.FormatString_en) + "</td>");
                        }
                    }
                    builder.AppendLine("	</tr>");
                }
                break;
            }
            builder.AppendLine("	<tr>");
            builder.AppendLine("		<td align='center'>1." + (3 + ++I) + "</td>");
            builder.AppendLine("		<td>Enzyme Activity</td>");
            builder.AppendLine("		<td>Nil (Peroxidase Negative)</td>");
            foreach (var fecha in fechas)
            {
                builder.AppendLine("		<td align='center'>Negative</td>");
            }
            builder.AppendLine("	</tr>");
            builder.AppendLine("	<tr>");
            builder.AppendLine("		<td align='center'>1." + (3 + ++I) + "</td>");
            builder.AppendLine("		<td>Foreign Material</td>");
            builder.AppendLine("		<td>Free from all objectionable hazardous foreign material and substances (like glass, metal etc)</td>");
            foreach (var fecha in fechas)
            {
                builder.AppendLine("		<td align='center'>Absence</td>");
            }
            builder.AppendLine("	</tr>");
            builder.AppendLine("	<tr>");
            builder.AppendLine("		<td align='center'>1." + (3 + ++I) + "</td>");
            builder.AppendLine("		<td>Total Ash (Dry)</td>");
            builder.AppendLine("		<td>1.9% Max</td>");
            foreach (var fecha in fechas)
            {
                builder.AppendLine("		<td align='center'>-</td>");
            }
            builder.AppendLine("	</tr>");
            builder.AppendLine("</tbody>");
            builder.AppendLine("</table>");

            RepTemp(ref htmlContent, "SENSORYPHYSICOCHEMICALCHARACTERISTICS", builder.ToString());

            #endregion

            #region 2. Análisis de pesticidas

            // 2. Análisis de pesticidas

            // Contador de índice de análisis de pesticidas
            I = 0;
            builder = new StringBuilder();
            int IdAnalisisPeriodicoPesticidasSelect = cert.IdAnalisisPeriodicoPesticidas ?? 0;
            var analisisPesticidesList = dcSoftwareCalidad.CAL_GetMaricoLimitedPesticideParameter(IdAnalisisPeriodicoPesticidasSelect);

            builder.AppendLine("<table class='microbiological-characteristics ultima-tabla'>");
            builder.AppendLine("	<thead>");
            builder.AppendLine("		<tr>");
            builder.AppendLine("			<th class='header' width='10%'>No.</th>");
            builder.AppendLine("			<th class='header' width='30%'>Parameter</th>");
            builder.AppendLine("			<th class='header' width='30%'>Spec</th>");
            builder.AppendLine("			<th class='header' width='30%'>Results</th>");
            builder.AppendLine("		</tr>");
            builder.AppendLine("	</thead>");
            builder.AppendLine("	<tbody>");
            foreach (var analisis in analisisPesticidesList)
            {
                builder.AppendLine("		<tr>");
                builder.AppendLine("			<td align='center'>3." + ++I + "</td>");
                builder.AppendLine("			<td>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("			<td align='center'>" + analisis.MaricoSpec_PM + "</td>");
                builder.AppendLine("		    <td align='center'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("		</tr>");
            }
            builder.AppendLine("	</tbody>");
            builder.AppendLine("</table>");

            RepTemp(ref htmlContent, "PESTICIDESCHARACTERISTICS", builder.ToString());

            #endregion

            #region 3. Análisis de metales pesados

            // 3. Análisis de metales pesados

            // Contador de índice de análisis de metales pesados
            I = 0;
            builder = new StringBuilder();
            int IdAnalisisPeriodicoMetalesPesadosSelect = cert.IdAnalisisPeriodicoMetalesPesados ?? 0;
            var analisisHeavyMetalsList = dcSoftwareCalidad.CAL_GetMaricoLimitedHeavyMetalParameter(IdAnalisisPeriodicoMetalesPesadosSelect);

            builder.AppendLine("<table class='microbiological-characteristics ultima-tabla'>");
            builder.AppendLine("	<thead>");
            builder.AppendLine("		<tr>");
            builder.AppendLine("			<th class='header' width='10%'>No.</th>");
            builder.AppendLine("			<th class='header' width='30%'>Parameter</th>");
            builder.AppendLine("			<th class='header' width='30%'>Spec</th>");
            builder.AppendLine("			<th class='header' width='30%'>Results</th>");
            builder.AppendLine("		</tr>");
            builder.AppendLine("	</thead>");
            builder.AppendLine("	<tbody>");
            foreach (var analisis in analisisHeavyMetalsList)
            {
                builder.AppendLine("		<tr>");
                builder.AppendLine("			<td align='center'>3." + ++I + "</td>");
                builder.AppendLine("			<td>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("			<td align='center'>" + analisis.MaricoSpec_PM + "</td>");
                builder.AppendLine("		    <td align='center'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("		</tr>");
            }
            builder.AppendLine("	</tbody>");
            builder.AppendLine("</table>");

            RepTemp(ref htmlContent, "HEAVYMETALSCHARACTERISTICS", builder.ToString());

            #endregion

            #region 4. Análisis de micotoxinas

            // 4. Análisis de micotoxinas

            // Contador de índice de análisis de micotoxinas
            I = 0;
            builder = new StringBuilder();
            int IdAnalisisPeriodicoMicotoxinasSelect = cert.IdAnalisisPeriodicoMicotoxinas ?? 0;
            var analisisMycotoxinsList = dcSoftwareCalidad.CAL_GetMaricoLimitedMycotoxinParameter(IdAnalisisPeriodicoMicotoxinasSelect);

            builder.AppendLine("<table class='microbiological-characteristics ultima-tabla'>");
            builder.AppendLine("	<thead>");
            builder.AppendLine("		<tr>");
            builder.AppendLine("			<th class='header' width='10%'>No.</th>");
            builder.AppendLine("			<th class='header' width='30%'>Parameter</th>");
            builder.AppendLine("			<th class='header' width='30%'>Spec</th>");
            builder.AppendLine("			<th class='header' width='30%'>Results</th>");
            builder.AppendLine("		</tr>");
            builder.AppendLine("	</thead>");
            builder.AppendLine("	<tbody>");
            foreach (var analisis in analisisMycotoxinsList)
            {
                builder.AppendLine("		<tr>");
                builder.AppendLine("			<td align='center'>3." + ++I + "</td>");
                builder.AppendLine("			<td>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("			<td align='center'>" + analisis.MaricoSpec_PM + "</td>");
                builder.AppendLine("		    <td align='center'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("		</tr>");
            }
            builder.AppendLine("	</tbody>");
            builder.AppendLine("</table>");

            RepTemp(ref htmlContent, "MYCOTOXINSCHARACTERISTICS", builder.ToString());

            #endregion

            #region 5. Análisis microbiológico

            // 5. Análisis microbiológico

            // Contador de índice de análisis microbiológico
            I = 0;
            builder = new StringBuilder();
            int IdAnalisisPeriodicoMicrobiologiaSelect = cert.IdAnalisisPeriodicoMicrobiologia ?? 0;
            var analisisMicrobiologicalList = dcSoftwareCalidad.CAL_GetMaricoLimitedMicrobiologicalParameter(IdAnalisisPeriodicoMicrobiologiaSelect);

            builder.AppendLine("<table class='microbiological-characteristics ultima-tabla'>");
            builder.AppendLine("	<thead>");
            builder.AppendLine("		<tr>");
            builder.AppendLine("			<th class='header' width='10%'>No.</th>");
            builder.AppendLine("			<th class='header' width='30%'>Parameter</th>");
            builder.AppendLine("			<th class='header' width='30%'>Spec</th>");
            builder.AppendLine("			<th class='header' width='30%'>Results</th>");
            builder.AppendLine("		</tr>");
            builder.AppendLine("	</thead>");
            builder.AppendLine("	<tbody>");
            foreach (var analisis in analisisMicrobiologicalList)
            {
                builder.AppendLine("		<tr>");
                builder.AppendLine("			<td align='center'>3." + ++I + "</td>");
                builder.AppendLine("			<td>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("			<td align='center'>" + analisis.MaricoSpec_PM + "</td>");
                builder.AppendLine("		    <td align='center'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("		</tr>");
            }
            builder.AppendLine("	</tbody>");
            builder.AppendLine("</table>");

            RepTemp(ref htmlContent, "MICROBIOLOGICALCHARACTERISTICS", builder.ToString());

            #endregion

            #region 6. Análisis nutricional

            // 6. Análisis nutricional

            // Contador de índice de análisis nutricional
            I = 0;
            builder = new StringBuilder();
            int IdAnalisisPeriodicoNutricionalSelect = cert.IdAnalisisPeriodicoNutricional ?? 0;
            var analisisChemicalList = dcSoftwareCalidad.CAL_GetMaricoLimitedChemicalParameter(IdAnalisisPeriodicoNutricionalSelect);

            builder.AppendLine("<table id='nutritional-chemical-characteristics'>");
            builder.AppendLine("	<thead>");
            builder.AppendLine("		<tr>");
            builder.AppendLine("			<th class='header' width='10%'>No.</th>");
            builder.AppendLine("			<th class='header' width='30%'>Parameter</th>");
            builder.AppendLine("			<th class='header' width='30%'>Spec</th>");
            builder.AppendLine("			<th class='header' width='30%'>Results</th>");
            builder.AppendLine("		</tr>");
            builder.AppendLine("	</thead>");
            builder.AppendLine("	<tbody>");
            foreach (var analisis in analisisChemicalList)
            {
                builder.AppendLine("		<tr>");
                builder.AppendLine("			<td align='center'>3." + ++I + "</td>");
                builder.AppendLine("			<td>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("			<td align='center'>" + analisis.MaricoSpec_PM + "</td>");
                builder.AppendLine("		    <td align='center'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("		</tr>");
            }
            builder.AppendLine("	</tbody>");
            builder.AppendLine("</table>");

            RepTemp(ref htmlContent, "NUTRITIONALCHEMICALCHARACTERISTICS", builder.ToString());

            #endregion

            try
            {
                string certKey = Guid.NewGuid().ToString();

                #region HTML básico

                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.html", certKey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

                #endregion

                #region NReco

                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.pdf", certKey));
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

                #endregion

                cert.CertificateKey = certKey;
                dcSoftwareCalidad.SubmitChanges();

                CAL_CertificadoControlVersion controlVersion = new CAL_CertificadoControlVersion()
                {
                    IdTipoCertificado = 6,
                    IdCertificado     = cert.IdCertificadoCertificateAnalysis,
                    certkey           = cert.CertificateKey,
                    Habilitado        = true,
                    FechaHoraIns      = DateTime.Now,
                    IpIns             = RemoteAddr(),
                    UserIns           = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = cert.IdCertificadoCertificateAnalysis, certKey = certKey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certKey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT_CERTIFICATEOFANALYSIS_{0}_{1}.txt", cert.IdCertificadoCertificateAnalysis, certKey)), FileMode.OpenOrCreate, FileAccess.Write);
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
            CAL_CertificadoCertificateAnalysis cert = dcSoftwareCalidad.CAL_CertificadoCertificateAnalysis.SingleOrDefault(X => X.IdCertificadoCertificateAnalysis == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", cert.CertificateKey));
                return File(pdf, "application/pdf", String.Format("Certificate Of Analysis {0}-{1}.pdf", cert.CertificateNumber, cert.FechaHoraIns.Year));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CertificadoCertificateAnalysis cert = dcSoftwareCalidad.CAL_CertificadoCertificateAnalysis.SingleOrDefault(X => X.IdCertificadoCertificateAnalysis == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string errMsg = "";
            string okMsg  = "";

            try
            {
                cert.Habilitado   = false;
                cert.UserUpd      = User.Identity.Name;
                cert.FechaHoraUpd = DateTime.Now;
                cert.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El certificado {0} ha sido eliminado", cert.IdCertificadoCertificateAnalysis);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }

        public ActionResult ControlVersiones(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoCertificateAnalysis cert = dcSoftwareCalidad.CAL_CertificadoCertificateAnalysis.SingleOrDefault(X => X.IdCertificadoCertificateAnalysis == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            List<CAL_CertificadoControlVersion> list = dcSoftwareCalidad.CAL_CertificadoControlVersion.Where(X => X.IdCertificado == cert.IdCertificadoCertificateAnalysis && X.IdTipoCertificado == 6 && X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(378),
                                                      CheckPermiso(377),
                                                      CheckPermiso(379),
                                                      CheckPermiso(380));
            ViewData["cert"] = cert;
            return View(list);
        }

        public ActionResult DescargarPDF_ControlVersion(int id, string certkey)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoCertificateAnalysis cert = dcSoftwareCalidad.CAL_CertificadoCertificateAnalysis.SingleOrDefault(X => X.IdCertificadoCertificateAnalysis == id && X.Habilitado == true);
            if (cert == null)
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
                return File(pdf, "application/pdf", String.Format("Certificate Of Analysis {0}-{1} V{2:dd-MM-yy HH;mm;ss}.pdf", cert.CertificateNumber, cert.FechaHoraIns.Year, controlVersion.FechaHoraIns));
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

            return RedirectToAction("ControlVersiones", new { id = controlVersion.IdCertificado, errMsg, okMsg });
        }

        #region --- Funciones PRIVADAS ---

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}