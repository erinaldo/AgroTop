using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALCertificadoTechnicalCertificateController : BaseApplicationController
    {
        // GET: CALCertificadoTechnicalCertificate
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALCertificadoTechnicalCertificateController()
        {
            SetCurrentModulo(10);
        }

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


            List<CAL_CertificadoTechnicalCertificate> list = dcSoftwareCalidad.CAL_CertificadoTechnicalCertificate
                .Where(X => X.Habilitado == true && X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect)
                .OrderByDescending(X => X.FechaHoraIns)
                .OrderByDescending(X => X.IdCertificadoTechnicalCertificate)
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
            CAL_CertificadoTechnicalCertificate cert = new CAL_CertificadoTechnicalCertificate();
            return View(cert);
        }

        [HttpPost]
        public ActionResult Crear(CAL_CertificadoTechnicalCertificate cert, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(378);
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar Luego
                    cert.ContainerNumber = "";
                    cert.LotNumber       = "";
                    cert.ProductionDate  = "";
                    cert.BestBefore      = "";
                    cert.BagsPerLot      = "";
                    cert.CertificateKey  = "";

                    // Rescatados del Form
                    cert.Habilitado      = true;
                    cert.FechaHoraIns    = DateTime.Now;
                    cert.IpIns           = RemoteAddr();
                    cert.UserIns         = User.Identity.Name;
                    dcSoftwareCalidad.CAL_CertificadoTechnicalCertificate.InsertOnSubmit(cert);
                    dcSoftwareCalidad.SubmitChanges();

                    // Actualizado
                    var contenedores = dcSoftwareCalidad.CAL_GetContenedoresPorDetalleOrdenProduccion_Pallets(cert.IdDetalleOrdenProduccion);
                    var fechas       = dcSoftwareCalidad.CAL_GetFechasElaboracionPorOrdenProduccion_Pallets(cert.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdOrdenProduccion);

                    foreach (var contenedor in contenedores)
                        cert.ContainerNumber += string.Format("{0}={1} Bags<br>", contenedor.NContenedor, contenedor.SacosEnElContenedor);
                    cert.ContainerNumber = cert.ContainerNumber.Remove(cert.ContainerNumber.Length - 1);
                    cert.LotNumber       = cert.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.LoteComercial;

                    foreach (var fecha in fechas)
                    {
                        cert.ProductionDate += string.Format("{0:dd/MM/yyyy}<br>", fecha.FechaElaboracion);
                        cert.BestBefore     += string.Format("{0:dd/MM/yyyy}<br>", fecha.FechaVencimiento);
                    }
                    cert.ProductionDate  = cert.ProductionDate.Remove(cert.ProductionDate.Length - 1);
                    cert.BestBefore      = cert.BestBefore.Remove(cert.BestBefore.Length - 1);

                    var ft = dcSoftwareCalidad.CAL_GetSacosPaletizadosPorDetalleOrdenProduccion(cert.IdDetalleOrdenProduccion).FirstOrDefault();
                    if (ft != null)
                        cert.BagsPerLot = string.Format("{0:N2} bags", ft.SacosPaletizados);
                    dcSoftwareCalidad.SubmitChanges();

                    CAL_CertificadoTechnicalCertificateAnalisisPeriodico certificadoTechnicalCertificateAnalisisPeriodico = new CAL_CertificadoTechnicalCertificateAnalisisPeriodico()
                    {
                        IdCertificadoTechnicalCertificate = cert.IdCertificadoTechnicalCertificate,
                        FechaHoraIns                      = DateTime.Now,
                        IpIns                             = RemoteAddr(),
                        UserIns                           = User.Identity.Name
                    };
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoPesticidas"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoPesticidas"], out int IdAnalisisPeriodicoPesticidas))
                            certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoPesticidas = IdAnalisisPeriodicoPesticidas;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMetalesPesados"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMetalesPesados"], out int IdAnalisisPeriodicoMetalesPesados))
                            certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados = IdAnalisisPeriodicoMetalesPesados;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMicotoxinas"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMicotoxinas"], out int IdAnalisisPeriodicoMicotoxinas))
                            certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas = IdAnalisisPeriodicoMicotoxinas;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMicrobiologia"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMicrobiologia"], out int IdAnalisisPeriodicoMicrobiologia))
                            certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia = IdAnalisisPeriodicoMicrobiologia;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoNutricional"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoNutricional"], out int IdAnalisisPeriodicoNutricional))
                            certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoNutricional = IdAnalisisPeriodicoNutricional;
                    dcSoftwareCalidad.CAL_CertificadoTechnicalCertificateAnalisisPeriodico.InsertOnSubmit(certificadoTechnicalCertificateAnalisisPeriodico);
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
            CAL_CertificadoTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoTechnicalCertificate == id && X.Habilitado == true);
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
            CAL_CertificadoTechnicalCertificateAnalisisPeriodico certificadoTechnicalCertificateAnalisisPeriodico = dcSoftwareCalidad.CAL_CertificadoTechnicalCertificateAnalisisPeriodico.SingleOrDefault(X => X.IdCertificadoTechnicalCertificate == cert.IdCertificadoTechnicalCertificate);
            if (certificadoTechnicalCertificateAnalisisPeriodico != null)
            {
                cert.IdAnalisisPeriodicoPesticidas     = certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoPesticidas;
                cert.IdAnalisisPeriodicoMetalesPesados = certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados;
                cert.IdAnalisisPeriodicoMicrobiologia  = certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia;
                cert.IdAnalisisPeriodicoMicotoxinas    = certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas;
                cert.IdAnalisisPeriodicoNutricional    = certificadoTechnicalCertificateAnalisisPeriodico.IdAnalisisPeriodicoNutricional;
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

            var ft = dcSoftwareCalidad.CAL_GetFichaTecnicaPorDetalleOrdenProduccion_Pallets(cert.IdDetalleOrdenProduccion).FirstOrDefault();
            var responsableArea = dcSoftwareCalidad.CAL_ResponsableArea.FirstOrDefault(X => X.IdArea == 2);

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/pepsico-inc-2020.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA"                            , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "NO"                               , cert.CertificateNumber.ToString());
            RepTemp(ref htmlContent, "AÑO"                              , DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "PRODUCT"                          , cert.CAL_DetalleOrdenProduccion.CAL_Subproducto.Nombre);
            RepTemp(ref htmlContent, "LOTNUMBER"                        , cert.LotNumber);
            RepTemp(ref htmlContent, "PRODUCTIONDATE"                   , cert.ProductionDate);
            RepTemp(ref htmlContent, "FINISHEDPROCESSCODE"              , cert.FinishedProcessCode);
            RepTemp(ref htmlContent, "CONTAINERNUMBER"                  , cert.ContainerNumber);
            RepTemp(ref htmlContent, "VESSEL"                           , cert.Vessel);
            RepTemp(ref htmlContent, "BAGSPERLOT"                       , cert.BagsPerLot);
            RepTemp(ref htmlContent, "BESTBEFORE"                       , cert.BestBefore);
            RepTemp(ref htmlContent, "SHELFLIFE"                        , ft.VidaUtil.Value.ToString());
            RepTemp(ref htmlContent, "MINTEMPERATURE"                   , ft.TempMinima.ToString());
            RepTemp(ref htmlContent, "MAXTEMPERATURE"                   , ft.TempMaxima.ToString());
            RepTemp(ref htmlContent, "DEGREES"                          , ft.Humedad.ToString());
            RepTemp(ref htmlContent, "RESPONSABLE"                      , responsableArea != null ? responsableArea.GetUser(responsableArea.UserID.Value) != null ? responsableArea.GetUser(responsableArea.UserID.Value).FullName : "" : "");
            RepTemp(ref htmlContent, "STYLE__PESTICIDESCHARACTERISTICS" , style__pesticidescharacteristics);
            RepTemp(ref htmlContent, "STYLE__HEAVYMETALSCHARACTERISTICS", style__heavymetalscharacteristics);
            RepTemp(ref htmlContent, "STYLE__MICROBIOLOGICALPARAMETERS" , style__microbiologicalparameters);
            RepTemp(ref htmlContent, "STYLE__MYCOTOXINSCHARACTERISTICS" , style__mycotoxinscharacteristics);
            RepTemp(ref htmlContent, "STYLE__CHEMICALPARAMETERS"        , style__chemicalparameters);

            #region 1. Análisis físico-químico

            // 1. Análisis físico-químico

            var analisisList = dcSoftwareCalidad.CAL_GetPepsiCoIncChemicalPhysicalAnalysis(cert.IdDetalleOrdenProduccion);
            StringBuilder builder = new StringBuilder();
            foreach (var analisis in analisisList)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine("    <td align='center' class='header'>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + Formatter.Format_en(analisis.AvgValue.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.Specification + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaFreq + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopFreq + "</td>");
                builder.AppendLine("</tr>");
            }

            RepTemp(ref htmlContent, "CHEMICALANDPHYSICALPARAMETERS", builder.ToString());

            #endregion

            #region 2. Análisis de pesticidas

            // 2. Análisis de pesticidas

            builder = new StringBuilder();
            int IdAnalisisPeriodicoPesticidasSelect = cert.IdAnalisisPeriodicoPesticidas ?? 0;
            var analisisPesticidesList = dcSoftwareCalidad.CAL_GetPepsiCoIncPesticideParameter(IdAnalisisPeriodicoPesticidasSelect);
            foreach (var analisis in analisisPesticidesList)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine("    <td align='center' class='header'>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.Specification + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaFreq + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopFreq + "</td>");
                builder.AppendLine("</tr>");
            }

            RepTemp(ref htmlContent, "PESTICIDESCHARACTERISTICS", builder.ToString());

            #endregion

            #region 3. Análisis de metales pesados

            // 3. Análisis de metales pesados

            builder = new StringBuilder();
            int IdAnalisisPeriodicoMetalesPesadosSelect = cert.IdAnalisisPeriodicoMetalesPesados ?? 0;
            var analisisHeavyMetalsList = dcSoftwareCalidad.CAL_GetPepsiCoIncHeavyMetalParameter(IdAnalisisPeriodicoMetalesPesadosSelect);

            foreach (var analisis in analisisHeavyMetalsList)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine("    <td align='center' class='header'>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.Specification + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaFreq + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopFreq + "</td>");
                builder.AppendLine("</tr>");
            }

            RepTemp(ref htmlContent, "HEAVYMETALSCHARACTERISTICS", builder.ToString());

            #endregion

            #region 4. Análisis microbiológico

            // 4. Análisis microbiológico

            builder = new StringBuilder();
            int IdAnalisisPeriodicoMicrobiologiaSelect = cert.IdAnalisisPeriodicoMicrobiologia ?? 0;
            var analisisMicrobiologicalList = dcSoftwareCalidad.CAL_GetPepsiCoIncMicrobiologicalParameter(IdAnalisisPeriodicoMicrobiologiaSelect);
            foreach (var analisis in analisisMicrobiologicalList)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine("    <td align='center' class='header'>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.Specification + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaFreq + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopFreq + "</td>");
                builder.AppendLine("</tr>");
            }

            RepTemp(ref htmlContent, "MICROBIOLOGICALPARAMETERS", builder.ToString());

            #endregion

            #region 5. Análisis de micotoxinas

            // 5. Análisis de micotoxinas

            builder = new StringBuilder();
            int IdAnalisisPeriodicoMicotoxinasSelect = cert.IdAnalisisPeriodicoMicotoxinas ?? 0;
            var analisisMycotoxinsList = dcSoftwareCalidad.CAL_GetPepsiCoIncMycotoxinParameter(IdAnalisisPeriodicoMicotoxinasSelect);

            foreach (var analisis in analisisMycotoxinsList)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine("    <td align='center' class='header'>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.Specification + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaFreq + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopFreq + "</td>");
                builder.AppendLine("</tr>");
            }

            RepTemp(ref htmlContent, "MYCOTOXINSCHARACTERISTICS", builder.ToString());

            #endregion

            #region 6. Análisis nutricional

            // 6. Análisis nutricional

            builder = new StringBuilder();
            int IdAnalisisPeriodicoNutricionalSelect = cert.IdAnalisisPeriodicoNutricional ?? 0;
            var analisisChemicalList = dcSoftwareCalidad.CAL_GetPepsiCoIncChemicalParameter(IdAnalisisPeriodicoNutricionalSelect);
            foreach (var analisis in analisisChemicalList)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine("    <td align='center' class='header'>" + analisis.Nombre_en + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + Formatter.Format_en(analisis.Value, analisis.UM_en, analisis.FormatString_en) + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.Specification + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.IndiaFreq + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopMethod + "</td>");
                builder.AppendLine("    <td align='center' class='dato'>" + analisis.AvenatopFreq + "</td>");
                builder.AppendLine("</tr>");
            }

            RepTemp(ref htmlContent, "CHEMICALPARAMETERS", builder.ToString());

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
                    IdTipoCertificado = 8,
                    IdCertificado     = cert.IdCertificadoTechnicalCertificate,
                    certkey           = cert.CertificateKey,
                    Habilitado        = true,
                    FechaHoraIns      = DateTime.Now,
                    IpIns             = RemoteAddr(),
                    UserIns           = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = cert.IdCertificadoTechnicalCertificate, certKey = certKey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certKey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT_CERTIFICATEOFANALYSIS_{0}_{1}.txt", cert.IdCertificadoTechnicalCertificate, certKey)), FileMode.OpenOrCreate, FileAccess.Write);
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
            CAL_CertificadoTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoTechnicalCertificate == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", cert.CertificateKey));
                return File(pdf, "application/pdf", String.Format("Technical Certificate {0}-{1}.pdf", cert.CertificateNumber, cert.FechaHoraIns.Year));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CertificadoTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoTechnicalCertificate == id && X.Habilitado == true);
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
                okMsg = String.Format("El certificado {0} ha sido eliminado", cert.IdCertificadoTechnicalCertificate);
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
            CAL_CertificadoTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoTechnicalCertificate == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            List<CAL_CertificadoControlVersion> list = dcSoftwareCalidad.CAL_CertificadoControlVersion.Where(X => X.IdCertificado == cert.IdCertificadoTechnicalCertificate && X.IdTipoCertificado == 8 && X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();
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
            CAL_CertificadoTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoTechnicalCertificate == id && X.Habilitado == true);
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
                return File(pdf, "application/pdf", String.Format("Technical Certificate {0}-{1} V{2:dd-MM-yy HH;mm;ss}.pdf", cert.CertificateNumber, cert.FechaHoraIns.Year, controlVersion.FechaHoraIns));
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