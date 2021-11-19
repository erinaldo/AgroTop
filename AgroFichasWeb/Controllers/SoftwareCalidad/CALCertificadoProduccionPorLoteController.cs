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
    public class CALCertificadoProduccionPorLoteController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALCertificadoProduccionPorLoteController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALCertificadoProduccionPorLote
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

            List<CAL_CertificadoProduccionPorLote> list = dcSoftwareCalidad.CAL_CertificadoProduccionPorLote
                .Where(X => X.Habilitado == true && X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect)
                .OrderByDescending(X => X.FechaHoraIns)
                .OrderByDescending(X => X.IdCertificadoProduccionPorLote).ToList();
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
            CAL_CertificadoProduccionPorLote cert = new CAL_CertificadoProduccionPorLote();
            return View(cert);
        }

        [HttpPost]
        public ActionResult Crear(CAL_CertificadoProduccionPorLote cert, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(378);
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar Luego
                    cert.certkey               = "";

                    // Rescatados del Form
                    cert.Habilitado            = true;
                    cert.FechaHoraIns          = DateTime.Now;
                    cert.IpIns                 = RemoteAddr();
                    cert.UserIns               = User.Identity.Name;
                    dcSoftwareCalidad.CAL_CertificadoProduccionPorLote.InsertOnSubmit(cert);
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
            CAL_CertificadoProduccionPorLote cert = dcSoftwareCalidad.CAL_CertificadoProduccionPorLote.SingleOrDefault(X => X.IdCertificadoProduccionPorLote == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            var produccionPorLote          = dcSoftwareCalidad.CAL_GetProduccionPorLote(cert.CAL_DetalleOrdenProduccion.IdOrdenProduccion).ToList();
            var cabecera                   = produccionPorLote.First(X => X.IdDetalleOrdenProduccion == cert.IdDetalleOrdenProduccion);
            var produccionPorLoteProductos = produccionPorLote.Where(X => X.IdDetalleOrdenProduccion == cert.IdDetalleOrdenProduccion);
            var contenedores               = "";
            var responsableArea = dcSoftwareCalidad.CAL_ResponsableArea.FirstOrDefault(X => X.IdArea == 2);

            foreach (var produccionPorLoteProducto in produccionPorLoteProductos)
            {
                contenedores += "<tr>";
                contenedores += string.Format("<td style='text-align:center'>{0}</td>"           , produccionPorLoteProducto.NContenedor);
                contenedores += string.Format("<td style='text-align:center'>{0:dd/MM/yyyy}</td>", produccionPorLoteProducto.FechaElaboracion.Value);
                contenedores += string.Format("<td style='text-align:center'>{0}</td>"           , produccionPorLoteProducto.SacosDespachados);
                contenedores += string.Format("<td style='text-align:center'>{0:N0} KG</td>"     , produccionPorLoteProducto.CntKgSacos);
                contenedores += "</tr>";
            }

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/certificado-produccion-por-lote.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA"        , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "NO"           , cert.NCertificado.ToString());
            RepTemp(ref htmlContent, "AÑO"          , DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "CLIENTE"      , cabecera.RazonSocial);
            RepTemp(ref htmlContent, "CLIENTEPAIS"  , cabecera.PaisNombre);
            RepTemp(ref htmlContent, "CNTSACOS"     , produccionPorLoteProductos.Sum(X => X.SacosDespachados).ToString());
            RepTemp(ref htmlContent, "CNTKILOS"     , cabecera.Peso.ToString());
            RepTemp(ref htmlContent, "PRODUCTO"     , cabecera.Subproducto);
            RepTemp(ref htmlContent, "LOTECOMERCIAL", cabecera.LoteComercial);
            RepTemp(ref htmlContent, "CONTENEDORES" , contenedores);
            RepTemp(ref htmlContent, "SACO"         , cert.CAL_DetalleOrdenProduccion.CAL_Saco.Nombre);
            RepTemp(ref htmlContent, "RESPONSABLE", responsableArea != null ? responsableArea.GetUser(responsableArea.UserID.Value) != null ? responsableArea.GetUser(responsableArea.UserID.Value).FullName : "" : "");

            try
            {
                string certKey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.html", certKey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

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

                cert.certkey = certKey;
                dcSoftwareCalidad.SubmitChanges();

                CAL_CertificadoControlVersion controlVersion = new CAL_CertificadoControlVersion()
                {
                    IdTipoCertificado = 5,
                    IdCertificado     = cert.IdCertificadoProduccionPorLote,
                    certkey           = cert.certkey,
                    Habilitado        = true,
                    FechaHoraIns      = DateTime.Now,
                    IpIns             = RemoteAddr(),
                    UserIns           = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = cert.IdCertificadoProduccionPorLote, certKey = certKey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certKey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT_CERTIFICADODEVIDAUTIL_{0}_{1}.txt", cert.IdCertificadoProduccionPorLote, certKey)), FileMode.OpenOrCreate, FileAccess.Write);
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
            CAL_CertificadoProduccionPorLote cert = dcSoftwareCalidad.CAL_CertificadoProduccionPorLote.SingleOrDefault(X => X.IdCertificadoProduccionPorLote == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", cert.certkey));
                return File(pdf, "application/pdf", String.Format("Certificado de Producción Por Lote {0}-{1}.pdf", cert.NCertificado, cert.FechaHoraIns.Year));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CertificadoProduccionPorLote cert = dcSoftwareCalidad.CAL_CertificadoProduccionPorLote.SingleOrDefault(X => X.IdCertificadoProduccionPorLote == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cert.Habilitado   = false;
                cert.UserUpd      = User.Identity.Name;
                cert.FechaHoraUpd = DateTime.Now;
                cert.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El certificado {0} ha sido eliminado", cert.IdCertificadoProduccionPorLote);
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
            CAL_CertificadoProduccionPorLote cert = dcSoftwareCalidad.CAL_CertificadoProduccionPorLote.SingleOrDefault(X => X.IdCertificadoProduccionPorLote == id && X.Habilitado == true);
            if (cert == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            List<CAL_CertificadoControlVersion> list = dcSoftwareCalidad.CAL_CertificadoControlVersion.Where(X => X.IdCertificado == cert.IdCertificadoProduccionPorLote && X.IdTipoCertificado == 5 && X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();
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
            CAL_CertificadoProduccionPorLote cert = dcSoftwareCalidad.CAL_CertificadoProduccionPorLote.SingleOrDefault(X => X.IdCertificadoProduccionPorLote == id && X.Habilitado == true);
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
                return File(pdf, "application/pdf", String.Format("Certificado de Producción Por Lote {0}-{1} V{2:dd-MM-yy HH;mm;ss}.pdf", cert.NCertificado, cert.FechaHoraIns.Year, controlVersion.FechaHoraIns));
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

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}