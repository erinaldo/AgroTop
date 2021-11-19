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
    public class CALCCGControlPorLoteController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALCCGControlPorLoteController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALCCGControlPorLote
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

            List<CAL_CCGControlPorLote> list = dcSoftwareCalidad.CAL_CCGControlPorLote
                .Where(X => X.Habilitado == true && X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect)
                .OrderByDescending(X => X.FechaHoraIns)
                .OrderByDescending(X => X.IdCCGControlPorLote)
                .ToList();
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
            CAL_CCGControlPorLote controlPorLote = new CAL_CCGControlPorLote();
            return View(controlPorLote);
        }

        [HttpPost]
        public ActionResult Crear(CAL_CCGControlPorLote controlPorLote, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(378);
            if (ModelState.IsValid)
            {
                try
                {
                    CAL_GetFichaTecnicaPorDetalleOrdenProduccion_CargaGranelResult result = dcSoftwareCalidad.CAL_GetFichaTecnicaPorDetalleOrdenProduccion_CargaGranel(controlPorLote.IdDetalleOrdenProduccion).FirstOrDefault();
                    if (result == null)
                        return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

                    // Actualizar Luego
                    controlPorLote.NombreCliente = "";
                    controlPorLote.NContenedores = 0;
                    controlPorLote.CantidadProducto = 0;
                    controlPorLote.ToneladasCertificadas = 0;
                    controlPorLote.Reserva = "";
                    controlPorLote.Naviera = "";
                    controlPorLote.LoteComercial = "";
                    controlPorLote.certkey = "";
                    // Rescatados del Form
                    controlPorLote.CodigoCliente = result.Codigo;
                    controlPorLote.Version = result.Version;
                    controlPorLote.FechaProduccion = controlPorLote.FechaProduccion.ToUpper();
                    controlPorLote.CodigoProceso = controlPorLote.CodigoProceso.ToUpper();
                    controlPorLote.Presentacion = "Granel";
                    controlPorLote.Habilitado = true;
                    controlPorLote.FechaHoraIns = DateTime.Now;
                    controlPorLote.IpIns = RemoteAddr();
                    controlPorLote.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_CCGControlPorLote.InsertOnSubmit(controlPorLote);
                    dcSoftwareCalidad.SubmitChanges();
                    // Actualizado
                    controlPorLote.NombreCliente = controlPorLote.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.GetCliente(controlPorLote.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdCliente).RazonSocial.ToUpper();
                    controlPorLote.LoteComercial = controlPorLote.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.LoteComercial;
                    controlPorLote.NContenedores = (from X in dcSoftwareCalidad.CAL_LECargaGranel
                                                    join Y in dcSoftwareCalidad.CAL_LECargaGranelContenedor on X.IdLECargaGranel equals Y.IdLECargaGranel
                                                    where X.IdLECargaGranel == controlPorLote.IdLECargaGranel
                                                    select Y).Count();
                    controlPorLote.CantidadProducto = (from X in dcSoftwareCalidad.CAL_LECargaGranel
                                                       join Y in dcSoftwareCalidad.CAL_LECargaGranelContenedor on X.IdLECargaGranel equals Y.IdLECargaGranel
                                                       where X.IdLECargaGranel == controlPorLote.IdLECargaGranel
                                                       select Y).Sum(X => X.PesoBruto);
                    controlPorLote.ToneladasCertificadas = Convert.ToDecimal(((from X in dcSoftwareCalidad.CAL_LECargaGranel
                                                                               join Y in dcSoftwareCalidad.CAL_LECargaGranelContenedor on X.IdLECargaGranel equals Y.IdLECargaGranel
                                                                               where X.IdLECargaGranel == controlPorLote.IdLECargaGranel
                                                                               select Y).Sum(X => X.PesoBruto) / 1000));
                    controlPorLote.Reserva = controlPorLote.CAL_LECargaGranel.NReserva;
                    controlPorLote.Naviera = string.Format("{0} {1}", controlPorLote.CAL_LECargaGranel.Carrier.Nombre, controlPorLote.CAL_LECargaGranel.Barco.Nombre);
                    controlPorLote.LoteComercial = controlPorLote.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.LoteComercial;
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = controlPorLote.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View(controlPorLote);
        }

        public ActionResult CrearPDF(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CCGControlPorLote controlPorLote = dcSoftwareCalidad.CAL_CCGControlPorLote.SingleOrDefault(X => X.IdCCGControlPorLote == id && X.Habilitado == true);
            if (controlPorLote == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/control_groats_por_lote_certificado_calidad_template.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA" , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "NO"    , controlPorLote.NCertificado.ToString());
            RepTemp(ref htmlContent, "AÑO"   , DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "CODCLI", controlPorLote.CodigoCliente);
            RepTemp(ref htmlContent, "VER"   , controlPorLote.Version.ToString());
            RepTemp(ref htmlContent, "NOMCLI", controlPorLote.NombreCliente);
            RepTemp(ref htmlContent, "NOCONT", controlPorLote.NContenedores.ToString());
            RepTemp(ref htmlContent, "CNTPRO", controlPorLote.CantidadProducto.ToString("N0"));
            RepTemp(ref htmlContent, "TNCERT", controlPorLote.ToneladasCertificadas.ToString("N2"));
            RepTemp(ref htmlContent, "RSV"   , controlPorLote.Reserva);
            RepTemp(ref htmlContent, "NAV"   , controlPorLote.Naviera);
            RepTemp(ref htmlContent, "LOTCOM", controlPorLote.LoteComercial);
            RepTemp(ref htmlContent, "FECPRO", controlPorLote.FechaProduccion);
            RepTemp(ref htmlContent, "CODPRO", controlPorLote.CodigoProceso);
            RepTemp(ref htmlContent, "PST"   , controlPorLote.Presentacion);

            StringBuilder builder = new StringBuilder();
            List<CAL_DespachoCargaGranel> despachoCargaGranelList = (from L0 in dcSoftwareCalidad.CAL_LECargaGranelContenedor
                                                                     join L1 in dcSoftwareCalidad.CAL_LECargaGranel on L0.IdLECargaGranel equals L1.IdLECargaGranel
                                                                     join D0 in dcSoftwareCalidad.CAL_DespachoCargaGranel on L0.IdDespachoCargaGranel equals D0.IdDespachoCargaGranel
                                                                     where L1.IdLECargaGranel == controlPorLote.IdLECargaGranel
                                                                     && D0.IdDetalleOrdenProduccion == controlPorLote.IdDetalleOrdenProduccion
                                                                     select D0).ToList();
            if (despachoCargaGranelList.Count > 0)
            {
                CAL_DespachoCargaGranel header = despachoCargaGranelList.FirstOrDefault();
                List<CAL_GetControlGroatsPorContenedorResult> parametros = dcSoftwareCalidad.CAL_GetControlGroatsPorContenedor(header.IdDetalleOrdenProduccion, controlPorLote.IdLECargaGranel, header.IdContenedor).ToList();

                // Creando el HEADER
                builder.AppendLine("<thead>");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>Fecha</th>");
                builder.AppendLine("<th>Contenedor</th>");
                foreach (var parametro in parametros)
                {
                    builder.AppendLine(string.Format("<th>{0}<br><small style=\"font-size: 10px;\">{1}</small></th>", parametro.Nombre, parametro.UM));
                }
                builder.AppendLine("</tr>");
                builder.AppendLine("</thead>");
                builder.AppendLine("<tbody>");
                foreach (CAL_DespachoCargaGranel despachoCargaGranel in despachoCargaGranelList)
                {
                    List<CAL_GetControlGroatsPorContenedorResult> resultados = dcSoftwareCalidad.CAL_GetControlGroatsPorContenedor(despachoCargaGranel.IdDetalleOrdenProduccion, controlPorLote.IdLECargaGranel, despachoCargaGranel.IdContenedor).ToList();
                    if (resultados.Count > 0)
                    {
                        var primerResultado = resultados.FirstOrDefault();
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td>{0:dd/MM/yyyy}</td>", primerResultado.Fecha));
                        builder.AppendLine(string.Format("<td>{0}</td>", primerResultado.NContenedor));
                        foreach (var resultado in resultados)
                        {
                            if (resultado.UM.ToLower() == "ausencia")
                                builder.AppendLine("<td class=\"text-align-center\">Ausencia</td>");
                            else if (resultado.UM.ToLower() == "negativa/15 min.")
                                builder.AppendLine("<td class=\"text-align-center\">Negativa</td>");
                            else
                                builder.AppendLine(string.Format("<td>{0:N2}</td>", resultado.Value));
                        }
                        builder.AppendLine("</tr>");
                    }
                }
                builder.AppendLine("<tr>");
                builder.AppendLine("<td style=\"text-align: right;\" colspan=\"2\"><strong>Promedios</strong>:</td>");
                List<CAL_GetPromedioControlGroatsPorContenedorResult> promedioResultados = dcSoftwareCalidad.CAL_GetPromedioControlGroatsPorContenedor(controlPorLote.IdDetalleOrdenProduccion, controlPorLote.IdLECargaGranel).ToList();
                foreach (var promedio in promedioResultados)
                {
                    if (promedio.UM.ToLower() == "ausencia")
                    {
                        builder.AppendLine("<td class=\"text-align-center\">Ausencia</td>");
                    }
                    else if (promedio.UM.ToLower() == "negativa/15 min.")
                    {
                        builder.AppendLine("<td class=\"text-align-center\">Negativa</td>");
                    }
                    else
                    {
                        builder.AppendLine(string.Format("<td>{0:N2}</td>", promedio.Promedio.Value));
                    }
                }
                builder.AppendLine("</tr>");
                builder.AppendLine("</tbody>");
                RepTemp(ref htmlContent, "ANALISISSOBRE100GRS", builder.ToString());
            }
            else
            {
                RepTemp(ref htmlContent, "ANALISISSOBRE100GRS", string.Empty);
            }

            try
            {
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.html", certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.pdf", certkey));
                byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()
                {
                    Orientation = NReco.PdfGenerator.PageOrientation.Landscape,
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

                controlPorLote.certkey = certkey;
                dcSoftwareCalidad.SubmitChanges();

                CAL_CertificadoControlVersion controlVersion = new CAL_CertificadoControlVersion()
                {
                    IdTipoCertificado = 11,
                    IdCertificado = controlPorLote.IdCCGControlPorLote,
                    certkey = controlPorLote.certkey,
                    Habilitado = true,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr(),
                    UserIns = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = controlPorLote.IdCCGControlPorLote, certkey = certkey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT_CONTROLPORLOTE_{0}_{1}.txt", controlPorLote.IdCCGControlPorLote, certkey)), FileMode.OpenOrCreate, FileAccess.Write);
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
            CAL_CCGControlPorLote controlPorLote = dcSoftwareCalidad.CAL_CCGControlPorLote.SingleOrDefault(X => X.IdCCGControlPorLote == id && X.certkey == certkey && X.Habilitado == true);
            if (controlPorLote == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", controlPorLote.certkey));
                return File(pdf, "application/pdf", String.Format("Control de Groats Por Lote de Certificado de Calidad {0}-{1}.pdf", controlPorLote.NCertificado, controlPorLote.FechaHoraIns.Year));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CCGControlPorLote controlPorLote = dcSoftwareCalidad.CAL_CCGControlPorLote.SingleOrDefault(X => X.IdCCGControlPorLote == id && X.Habilitado == true);
            if (controlPorLote == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                controlPorLote.Habilitado = false;
                controlPorLote.UserUpd = User.Identity.Name;
                controlPorLote.FechaHoraUpd = DateTime.Now;
                controlPorLote.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El certificado {0} ha sido eliminado", controlPorLote.IdCCGControlPorLote);
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
            CAL_CCGControlPorLote controlPorLote = dcSoftwareCalidad.CAL_CCGControlPorLote.SingleOrDefault(X => X.IdCCGControlPorLote == id && X.Habilitado == true);
            if (controlPorLote == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            List<CAL_CertificadoControlVersion> list = dcSoftwareCalidad.CAL_CertificadoControlVersion.Where(X => X.IdCertificado == controlPorLote.IdCCGControlPorLote && X.IdTipoCertificado == 11 && X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(378),
                                                      CheckPermiso(377),
                                                      CheckPermiso(379),
                                                      CheckPermiso(380));
            ViewData["controlPorLote"] = controlPorLote;
            return View(list);
        }

        public ActionResult DescargarPDF_ControlVersion(int id, string certkey)
        {
            CheckPermisoAndRedirect(377);
            CAL_CCGControlPorLote controlPorLote = dcSoftwareCalidad.CAL_CCGControlPorLote.SingleOrDefault(X => X.IdCCGControlPorLote == id && X.Habilitado == true);
            if (controlPorLote == null)
            {
                return RedirectToAction("ControlVersiones", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            CAL_CertificadoControlVersion controlVersion = dcSoftwareCalidad.CAL_CertificadoControlVersion.SingleOrDefault(X => X.certkey == certkey);
            if (controlVersion == null)
            {
                return RedirectToAction("ControlVersiones", new { errMsg = "No se ha encontrado el control de versión", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", controlVersion.certkey));
                return File(pdf, "application/pdf", String.Format("Control de Groats Por Lote de Certificado de Calidad {0}-{1} V{2:dd-MM-yy HH;mm;ss}.pdf", controlPorLote.NCertificado, controlPorLote.FechaHoraIns.Year, controlVersion.FechaHoraIns));
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