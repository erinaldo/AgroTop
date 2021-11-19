using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using MoreLinq;
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
    public class CALCertificadoBatchControlTechnicalCertificateController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALCertificadoBatchControlTechnicalCertificateController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALCertificadoBatchControlTechnicalCertificate
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

            List<CAL_CertificadoBatchControlTechnicalCertificate> list = dcSoftwareCalidad.CAL_CertificadoBatchControlTechnicalCertificate
                .Where(X => X.Habilitado == true && X.CAL_LEPallets.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect)
                .OrderByDescending(X => X.FechaHoraIns)
                .OrderByDescending(X => X.IdCertificadoBatchControlTechnicalCertificate).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(378),
                                                      CheckPermiso(377),
                                                      CheckPermiso(379),
                                                      CheckPermiso(380));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(378);
            CAL_CertificadoBatchControlTechnicalCertificate cert = new CAL_CertificadoBatchControlTechnicalCertificate();
            return View(cert);
        }

        [HttpPost]
        public ActionResult Crear(CAL_CertificadoBatchControlTechnicalCertificate cert, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(378);
            if (ModelState.IsValid)
            {
                try
                {
                    var fts = dcSoftwareCalidad.CAL_GetFichasTecnicasPorListaEmpaque_Pallets(cert.IdLEPallets).ToList();
                    var CustomerCode = "";
                    var VersionSpecification = "";
                    foreach (var ft in fts)
                    {
                        CustomerCode += ft.Codigo + "/";
                        VersionSpecification += ft.Version + "/";
                    }
                    cert.CustomerCode = CustomerCode.Remove(CustomerCode.Length - 1);
                    cert.VersionSpecification = VersionSpecification.Remove(VersionSpecification.Length - 1);

                    // Actualizar Luego
                    cert.ProductionDate = "";
                    cert.CustomerName = "";
                    cert.Container = 0;
                    cert.AmountProduct = 0;
                    cert.CertifiedTon = 0;
                    cert.BookingNumber = "";
                    cert.ShippingLineAndVessel = "";
                    cert.Lot = "";
                    cert.CertificateKey = "";

                    // Rescatados del Form
                    cert.ProductionDate = cert.ProductionDate.ToUpper();
                    cert.ProcessCode = cert.ProcessCode.ToUpper();
                    cert.Habilitado = true;
                    cert.FechaHoraIns = DateTime.Now;
                    cert.IpIns = RemoteAddr();
                    cert.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_CertificadoBatchControlTechnicalCertificate.InsertOnSubmit(cert);
                    dcSoftwareCalidad.SubmitChanges();

                    // Actualizado
                    cert.CustomerName = cert.CAL_LEPallets.CAL_OrdenProduccion.GetCliente(cert.CAL_LEPallets.CAL_OrdenProduccion.IdCliente).RazonSocial.ToUpper();
                    cert.Lot = cert.CAL_LEPallets.CAL_OrdenProduccion.LoteComercial;
                    cert.Container = (from X in dcSoftwareCalidad.CAL_LEPallets
                                      join Y in dcSoftwareCalidad.CAL_LEPalletsContenedor on X.IdLEPallets equals Y.IdLEPallets
                                      where X.IdLEPallets == cert.IdLEPallets
                                      select Y).Count();
                    cert.AmountProduct = (from X in dcSoftwareCalidad.CAL_LEPallets
                                          join Y in dcSoftwareCalidad.CAL_LEPalletsContenedor on X.IdLEPallets equals Y.IdLEPallets
                                          where X.IdLEPallets == cert.IdLEPallets
                                          select Y).Sum(X => X.PesoBruto);
                    cert.CertifiedTon = Convert.ToDecimal(((from X in dcSoftwareCalidad.CAL_LEPallets
                                                            join Y in dcSoftwareCalidad.CAL_LEPalletsContenedor on X.IdLEPallets equals Y.IdLEPallets
                                                            where X.IdLEPallets == cert.IdLEPallets
                                                            select Y).Sum(X => X.PesoBruto) / 1000M));
                    cert.BookingNumber = cert.CAL_LEPallets.NReserva;
                    cert.ShippingLineAndVessel = string.Format("{0}, {1}", cert.CAL_LEPallets.Carrier.Nombre, cert.CAL_LEPallets.Barco.Nombre);
                    cert.Lot = cert.CAL_LEPallets.CAL_OrdenProduccion.LoteComercial;

                    foreach (var fecha in dcSoftwareCalidad.CAL_GetFechasElaboracionPorOrdenProduccion_Pallets(cert.CAL_LEPallets.CAL_OrdenProduccion.IdOrdenProduccion))
                    {
                        cert.ProductionDate += string.Format("{0:dd/MM/yyyy},", fecha.FechaElaboracion);
                    }

                    cert.ProductionDate = cert.ProductionDate.Remove(cert.ProductionDate.Length - 1);
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

        public ActionResult CrearPDF_en(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoBatchControlTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoBatchControlTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoBatchControlTechnicalCertificate == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            var idDetalleOP = dcSoftwareCalidad.CAL_GetIDSControlGroatsPorDetalleYContenedor_ListaEmpaquePallets(cert.IdLEPallets).First();
            var produccionPorLote = dcSoftwareCalidad.CAL_GetProduccionPorLote(idDetalleOP.IdOrdenProduccion).ToList();
            var cabecera = produccionPorLote.First(X => X.IdDetalleOrdenProduccion == idDetalleOP.IdDetalleOrdenProduccion);

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/batch-control-of-technical-certificate.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "NO", cert.CertificateNumber.ToString());
            RepTemp(ref htmlContent, "AÑO", DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "CUSTOMERCODE", cert.CustomerCode);
            RepTemp(ref htmlContent, "VERSIONSPECIFICATION", cert.VersionSpecification);
            RepTemp(ref htmlContent, "CUSTOMERNAME", cert.CustomerName);
            RepTemp(ref htmlContent, "CONTAINER", cert.Container.ToString());
            RepTemp(ref htmlContent, "AMOUNTOFPRODUCT", cert.AmountProduct.ToString("N0"));
            RepTemp(ref htmlContent, "CERTIFIEDTON", cert.CertifiedTon.ToString("N2"));
            RepTemp(ref htmlContent, "BOOKING", cert.BookingNumber);
            RepTemp(ref htmlContent, "SHIPPINGLINE", cert.ShippingLineAndVessel);
            RepTemp(ref htmlContent, "LOT", cert.Lot);
            RepTemp(ref htmlContent, "PRODUCTIONDATE", cert.ProductionDate);
            RepTemp(ref htmlContent, "PROCESSCODE", cert.ProcessCode);
            RepTemp(ref htmlContent, "PRESENTATION", cert.Presentation);
            RepTemp(ref htmlContent, "PRODUCTO", cabecera.Subproducto);

            StringBuilder builder = new StringBuilder();
            var IDS = dcSoftwareCalidad.CAL_GetIDSControlGroatsPorDetalleYContenedor_ListaEmpaquePallets(cert.IdLEPallets).ToList();

            foreach (var ID in IDS)
            {
                var resultados = dcSoftwareCalidad.CAL_GetPromedioPorProductoYPorDiaControlGroatsPorDetalleYContenedor_ListaEmpaquePallets(ID.IdDetalleOrdenProduccion, ID.IdLEPallets, ID.IdContenedor).ToList();
                var resultadosPromedio = dcSoftwareCalidad.CAL_GetTestGranulometriaPalletsPorDetalleOrdenProduccionYListaEmpaqueYContenedor(ID.IdDetalleOrdenProduccion, ID.IdLEPallets).ToList();
                var header = resultados.FirstOrDefault();
                var fechas = resultados.Select(X => X.FechaDespacho).DistinctBy(Y => Y.Value).ToList();

                #region Tabla

                builder.AppendLine("<table class='analisis-sobre-100-grs'>");
                builder.AppendLine("<thead>");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>Date</th>");
                builder.AppendLine("<th>Container</th>");
                var parametros = resultados.Where(X => X.FechaDespacho == header.FechaDespacho).ToList();
                foreach (var parametro in parametros)
                {
                    builder.AppendLine(string.Format("<th>{0}<br><small style='font-size: 10px;'>{1}</small></th>", parametro.Nombre_en, parametro.UM_en));
                }
                builder.AppendLine("</tr>");
                builder.AppendLine("</thead>");
                builder.AppendLine("<tbody>");
                foreach (var fecha in fechas)
                {
                    var primer = resultados.Where(X => X.FechaDespacho == fecha.Value).FirstOrDefault();
                    builder.AppendLine("<tr>");
                    builder.AppendLine(string.Format("<td>{0:dd/MM/yyyy}</td>", primer.FechaDespacho));
                    builder.AppendLine(string.Format("<td>{0}</td>", primer.NContenedor));
                    foreach (var resultado in resultados.Where(X => X.FechaDespacho == fecha.Value))
                    {
                        builder.AppendLine(string.Format("<td>{0:N2}</td>", resultado.Value));
                    }
                    builder.AppendLine("</tr>");
                }
                builder.AppendLine("<tr>");
                builder.AppendLine("<td style='text-align: right;' colspan='2'><strong>Averages</strong>:</td>");
                foreach (var resultadoPromedio in resultadosPromedio)
                {
                    builder.AppendLine(string.Format("<td>{0:N2}</td>", resultadoPromedio.Value));
                }
                builder.AppendLine("</tbody>");
                builder.AppendLine("</table>");

                #endregion
            }

            RepTemp(ref htmlContent, "ANALYSISON100GRSQUICKOAT", builder.ToString());

            try
            {
                string certKey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.html", certKey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.pdf", certKey));
                byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()
                {
                    Orientation = NReco.PdfGenerator.PageOrientation.Landscape,
                    Margins = new NReco.PdfGenerator.PageMargins()
                    {
                        Bottom = 15,
                        Left = 15,
                        Right = 15,
                        Top = 15
                    },
                    Size = NReco.PdfGenerator.PageSize.Letter,
                }).GeneratePdf(htmlContent);
                System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO

                cert.CertificateKey = certKey;
                dcSoftwareCalidad.SubmitChanges();

                CAL_CertificadoControlVersion controlVersion = new CAL_CertificadoControlVersion()
                {
                    IdTipoCertificado = 1,
                    IdCertificado = cert.IdCertificadoBatchControlTechnicalCertificate,
                    certkey = cert.CertificateKey,
                    Habilitado = true,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr(),
                    UserIns = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = cert.IdCertificadoBatchControlTechnicalCertificate, certKey = certKey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certKey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT_BATCHCONTROLOFTECHNICALCERTIFICATE_{0}_{1}.txt", cert.IdCertificadoBatchControlTechnicalCertificate, certKey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine("No se pudo generar el PDF del certificado");
                writer.WriteLine(ex.ToString());
                writer.Close();

                return RedirectToAction("Index", new { errMsg = "No se pudo generar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult CrearPDF_es(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoBatchControlTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoBatchControlTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoBatchControlTechnicalCertificate == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            var idDetalleOP = dcSoftwareCalidad.CAL_GetIDSControlGroatsPorDetalleYContenedor_ListaEmpaquePallets(cert.IdLEPallets).First();
            var produccionPorLote = dcSoftwareCalidad.CAL_GetProduccionPorLote(idDetalleOP.IdOrdenProduccion).ToList();
            var cabecera = produccionPorLote.First(X => X.IdDetalleOrdenProduccion == idDetalleOP.IdDetalleOrdenProduccion);

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/control_lote_certificado_tecnico.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "NO", cert.CertificateNumber.ToString());
            RepTemp(ref htmlContent, "AÑO", DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "CUSTOMERCODE", cert.CustomerCode);
            RepTemp(ref htmlContent, "VERSIONSPECIFICATION", cert.VersionSpecification);
            RepTemp(ref htmlContent, "CUSTOMERNAME", cert.CustomerName);
            RepTemp(ref htmlContent, "CONTAINER", cert.Container.ToString());
            RepTemp(ref htmlContent, "AMOUNTOFPRODUCT", cert.AmountProduct.ToString("N0"));
            RepTemp(ref htmlContent, "CERTIFIEDTON", cert.CertifiedTon.ToString("N2"));
            RepTemp(ref htmlContent, "BOOKING", cert.BookingNumber);
            RepTemp(ref htmlContent, "SHIPPINGLINE", cert.ShippingLineAndVessel);
            RepTemp(ref htmlContent, "LOT", cert.Lot);
            RepTemp(ref htmlContent, "PRODUCTIONDATE", cert.ProductionDate);
            RepTemp(ref htmlContent, "PROCESSCODE", cert.ProcessCode);
            RepTemp(ref htmlContent, "PRESENTATION", cert.Presentation);
            RepTemp(ref htmlContent, "PRODUCTO", cabecera.Subproducto);

            StringBuilder builder = new StringBuilder();
            var IDS = dcSoftwareCalidad.CAL_GetIDSControlGroatsPorDetalleYContenedor_ListaEmpaquePallets(cert.IdLEPallets).ToList();


            var resultadosPromedio = dcSoftwareCalidad.CAL_GetTestGranulometriaPalletsPorDetalleOrdenProduccionYListaEmpaqueYContenedor(idDetalleOP.IdDetalleOrdenProduccion, idDetalleOP.IdLEPallets).ToList();
            
            #region Tabla

            builder.AppendLine("<table class='analisis-sobre-100-grs'>");
            builder.AppendLine("<thead>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Fecha</th>");
            builder.AppendLine("<th>Contenedor</th>");
            //var parametros = resultados.Where(X => X.FechaDespacho == header.FechaDespacho).ToList();
            foreach (var r in resultadosPromedio)
            {
                builder.AppendLine(string.Format("<th>{0}<br><small style='font-size: 10px;'>{1}</small></th>", r.Nombre, r.UM));
            }
            builder.AppendLine("</tr>");
            builder.AppendLine("</thead>");
            builder.AppendLine("<tbody>");
            foreach (var ID in IDS)
            {
                var resultados = dcSoftwareCalidad.CAL_GetPromedioPorProductoYPorDiaControlGroatsPorDetalleYContenedor_ListaEmpaquePallets(ID.IdDetalleOrdenProduccion, ID.IdLEPallets, ID.IdContenedor).ToList();
                var fechas = resultados.Select(X => X.FechaDespacho).DistinctBy(Y => Y.Value).ToList();
                //var header = resultados.FirstOrDefault();

                foreach (var fecha in fechas)
                {
                    var primer = resultados.Where(X => X.FechaDespacho == fecha.Value).FirstOrDefault();
                    builder.AppendLine("<tr>");
                    builder.AppendLine(string.Format("<td>{0:dd/MM/yyyy}</td>", primer.FechaDespacho));
                    builder.AppendLine(string.Format("<td>{0}</td>", primer.NContenedor));
                    foreach (var resultado in resultados.Where(X => X.FechaDespacho == fecha.Value))
                    {
                        builder.AppendLine(string.Format("<td>{0:N2}</td>", resultado.Value));
                    }
                    builder.AppendLine("</tr>");
                }
            }
            builder.AppendLine("<tr>");
            builder.AppendLine("<td style='text-align: right;' colspan='2'><strong>Promedios</strong>:</td>");
            foreach (var resultadoPromedio in resultadosPromedio)
            {
                builder.AppendLine(string.Format("<td>{0:N2}</td>", resultadoPromedio.Value));
            }
            builder.AppendLine("</tbody>");
            builder.AppendLine("</table>");

            #endregion


            RepTemp(ref htmlContent, "ANALYSISON100GRSQUICKOAT", builder.ToString());

            try
            {
                string certKey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.html", certKey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.pdf", certKey));
                byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()
                {
                    Orientation = NReco.PdfGenerator.PageOrientation.Landscape,
                    Margins = new NReco.PdfGenerator.PageMargins()
                    {
                        Bottom = 15,
                        Left = 15,
                        Right = 15,
                        Top = 15
                    },
                    Size = NReco.PdfGenerator.PageSize.Letter,
                }).GeneratePdf(htmlContent);
                System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO

                cert.CertificateKey = certKey;
                dcSoftwareCalidad.SubmitChanges();

                CAL_CertificadoControlVersion controlVersion = new CAL_CertificadoControlVersion()
                {
                    IdTipoCertificado = 1,
                    IdCertificado = cert.IdCertificadoBatchControlTechnicalCertificate,
                    certkey = cert.CertificateKey,
                    Habilitado = true,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr(),
                    UserIns = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = cert.IdCertificadoBatchControlTechnicalCertificate, certKey = certKey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certKey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT_BATCHCONTROLOFTECHNICALCERTIFICATE_{0}_{1}.txt", cert.IdCertificadoBatchControlTechnicalCertificate, certKey)), FileMode.OpenOrCreate, FileAccess.Write);
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
            CAL_CertificadoBatchControlTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoBatchControlTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoBatchControlTechnicalCertificate == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", cert.CertificateKey));
                return File(pdf, "application/pdf", String.Format("Batch Control Of Technical Certificate {0}-{1}.pdf", cert.CertificateNumber, cert.FechaHoraIns.Year));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CertificadoBatchControlTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoBatchControlTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoBatchControlTechnicalCertificate == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cert.Habilitado = false;
                cert.UserUpd = User.Identity.Name;
                cert.FechaHoraUpd = DateTime.Now;
                cert.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El certificado {0} ha sido eliminado", cert.IdCertificadoBatchControlTechnicalCertificate);
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
            CAL_CertificadoBatchControlTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoBatchControlTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoBatchControlTechnicalCertificate == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            List<CAL_CertificadoControlVersion> list = dcSoftwareCalidad.CAL_CertificadoControlVersion.Where(X => X.IdCertificado == cert.IdCertificadoBatchControlTechnicalCertificate && X.IdTipoCertificado == 1 && X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
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
            CAL_CertificadoBatchControlTechnicalCertificate cert = dcSoftwareCalidad.CAL_CertificadoBatchControlTechnicalCertificate.SingleOrDefault(X => X.IdCertificadoBatchControlTechnicalCertificate == id && X.Habilitado == true);
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
                return File(pdf, "application/pdf", String.Format("Batch Control Of Technical Certificate {0}-{1} V{2:dd-MM-yy HH;mm;ss}.pdf", cert.CertificateNumber, cert.FechaHoraIns.Year, controlVersion.FechaHoraIns));
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
            string okMsg = "";

            try
            {
                controlVersion.Habilitado = false;
                controlVersion.UserUpd = User.Identity.Name;
                controlVersion.FechaHoraUpd = DateTime.Now;
                controlVersion.IpUpd = RemoteAddr();
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

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}