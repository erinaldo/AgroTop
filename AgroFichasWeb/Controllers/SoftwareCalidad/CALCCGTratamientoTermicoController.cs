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
    public class CALCCGTratamientoTermicoController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        public CALCCGTratamientoTermicoController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALCCGTratamientoTermico
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


            List<CAL_CCGTratamientoTermico> list = dcSoftwareCalidad.CAL_CCGTratamientoTermico
                .Where(X => X.Habilitado == true && X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect)
                .OrderByDescending(X => X.FechaHoraIns)
                .OrderByDescending(X => X.IdCCGTratamientoTermico).ToList();
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
            CAL_CCGTratamientoTermico tratamientoTermico = new CAL_CCGTratamientoTermico();
            return View(tratamientoTermico);
        }

        [HttpPost]
        public ActionResult Crear(CAL_CCGTratamientoTermico tratamientoTermico, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(378);
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar Luego
                    tratamientoTermico.LoteComercial    = "";
                    tratamientoTermico.PaisDestino      = "";
                    tratamientoTermico.Consignatario    = "";
                    tratamientoTermico.certkey          = "";
                    // Rescatados del Form
                    tratamientoTermico.Habilitado       = true;
                    tratamientoTermico.FechaHoraIns     = DateTime.Now;
                    tratamientoTermico.IpIns            = RemoteAddr();
                    tratamientoTermico.UserIns          = User.Identity.Name;
                    dcSoftwareCalidad.CAL_CCGTratamientoTermico.InsertOnSubmit(tratamientoTermico);
                    dcSoftwareCalidad.SubmitChanges();
                    // Actualizado
                    tratamientoTermico.LoteComercial    = tratamientoTermico.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.LoteComercial;
                    tratamientoTermico.PaisDestino      = tratamientoTermico.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.GetPais(tratamientoTermico.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.PaisCodigo).PaisNombre;
                    tratamientoTermico.Consignatario    = tratamientoTermico.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.GetCliente(tratamientoTermico.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdCliente).RazonSocial.ToUpper();
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = tratamientoTermico.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View(tratamientoTermico);
        }

        public ActionResult CrearPDF(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CCGTratamientoTermico tratamientoTermico = dcSoftwareCalidad.CAL_CCGTratamientoTermico.SingleOrDefault(X => X.IdCCGTratamientoTermico == id && X.Habilitado == true);
            if (tratamientoTermico == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/certificado_tratamiento_termico_template.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA"  , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "NO"     , tratamientoTermico.NCertificado.ToString());
            RepTemp(ref htmlContent, "AÑO"    , DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "CONSIG" , tratamientoTermico.Consignatario);
            RepTemp(ref htmlContent, "DST"    , tratamientoTermico.PaisDestino);
            RepTemp(ref htmlContent, "LOTCOM" , tratamientoTermico.LoteComercial);

            try
            {
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.html", certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent);writer.Close();

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

                tratamientoTermico.certkey = certkey;
                dcSoftwareCalidad.SubmitChanges();

                CAL_CertificadoControlVersion controlVersion = new CAL_CertificadoControlVersion()
                {
                    IdTipoCertificado = 10,
                    IdCertificado = tratamientoTermico.IdCCGTratamientoTermico,
                    certkey = tratamientoTermico.certkey,
                    Habilitado = true,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr(),
                    UserIns = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = tratamientoTermico.IdCCGTratamientoTermico, certkey = certkey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT_TRATAMIENTOTERMICO_{0}_{1}.txt", tratamientoTermico.IdCCGTratamientoTermico, certkey)), FileMode.OpenOrCreate, FileAccess.Write);
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
            CAL_CCGTratamientoTermico tratamientoTermico = dcSoftwareCalidad.CAL_CCGTratamientoTermico.SingleOrDefault(X => X.IdCCGTratamientoTermico == id && X.certkey == certkey && X.Habilitado == true);
            if (tratamientoTermico == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", tratamientoTermico.certkey));
                return File(pdf, "application/pdf", String.Format("Certificado de Tratamiento Térmico {0}-{1}.pdf", tratamientoTermico.NCertificado, tratamientoTermico.FechaHoraIns.Year));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CCGTratamientoTermico tratamientoTermico = dcSoftwareCalidad.CAL_CCGTratamientoTermico.SingleOrDefault(X => X.IdCCGTratamientoTermico == id && X.Habilitado == true);
            if (tratamientoTermico == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                tratamientoTermico.Habilitado = false;
                tratamientoTermico.UserUpd = User.Identity.Name;
                tratamientoTermico.FechaHoraUpd = DateTime.Now;
                tratamientoTermico.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El certificado {0} ha sido eliminado", tratamientoTermico.IdCCGTratamientoTermico);
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
            CAL_CCGTratamientoTermico tratamientoTermico = dcSoftwareCalidad.CAL_CCGTratamientoTermico.SingleOrDefault(X => X.IdCCGTratamientoTermico == id && X.Habilitado == true);
            if (tratamientoTermico == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            List<CAL_CertificadoControlVersion> list = dcSoftwareCalidad.CAL_CertificadoControlVersion.Where(X => X.IdCertificado == tratamientoTermico.IdCCGTratamientoTermico && X.IdTipoCertificado == 10 && X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(378),
                                                      CheckPermiso(377),
                                                      CheckPermiso(379),
                                                      CheckPermiso(380));
            ViewData["tratamientoTermico"] = tratamientoTermico;
            return View(list);
        }

        public ActionResult DescargarPDF_ControlVersion(int id, string certkey)
        {
            CheckPermisoAndRedirect(377);
            CAL_CCGTratamientoTermico tratamientoTermico = dcSoftwareCalidad.CAL_CCGTratamientoTermico.SingleOrDefault(X => X.IdCCGTratamientoTermico == id && X.Habilitado == true);
            if (tratamientoTermico == null)
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
                return File(pdf, "application/pdf", String.Format("Certificado de Tratamiento Térmico {0}-{1} V{2:dd-MM-yy HH;mm;ss}.pdf", tratamientoTermico.NCertificado, tratamientoTermico.FechaHoraIns.Year, controlVersion.FechaHoraIns));
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