using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Telesign;

namespace AgroFichasWeb.Controllers.ControlTiempo
{
    [WebsiteAuthorize]
    public class ControlTiempoController : BaseApplicationController
    {
        //
        // GET: /ControlTiempo/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ControlTiempoController()
        {
            SetCurrentModulo(9);
        }

        public ActionResult Index(int? IdEmpresa, int? IdProducto, int? IdEnvase, int? IdEstado, DateTime? FechaDesde, DateTime? FechaHasta, int? IdAntiguedad, int? IdPlantaProduccion, string key = "")
        {
            CheckPermisoAndRedirect(224);

            CTR_ControlTiempo controlTiempo = new CTR_ControlTiempo();
            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = controlTiempo.GetPlantaProduccion(IdPlantaProduccionSelect, CurrentUser.UserID);

            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            var IdEmpresaSelect = IdEmpresa ?? 0;
            var IdProductoSelect = IdProducto ?? 0;
            var IdEnvaseSelect = IdEnvase ?? 0;
            var IdEstadoSelect = IdEstado ?? 0;
            var IdAntiguedadSelect = IdAntiguedad ?? 0;

            var FechaDesdeSelect = (FechaDesde.HasValue ? FechaDesde.Value : DateTime.Now.AddMonths(-1));
            var FechaHastaSelect = (FechaHasta.HasValue ? FechaHasta.Value : DateTime.Now);
            List<CTR_ControlTiempo> controlesTiempo = new List<CTR_ControlTiempo>();
            if (IdEstadoSelect != 9)
            {
                 controlesTiempo = (from X in dc.CTR_ControlTiempo
                                                           where (IdEmpresaSelect == 0 || X.IdEmpresa == IdEmpresaSelect) &&
                                                                 (IdProductoSelect == 0 || X.IdProducto == IdProductoSelect) &&
                                                                 (IdEnvaseSelect == 0 || X.IdEnvase == IdEnvaseSelect) &&
                                                                 (IdEstadoSelect == 0 || X.IdEstado == IdEstadoSelect) &&
                                                                 (X.IdPlantaProduccion == IdPlantaProduccionSelect) &&
                                                                 ((X.FechaHoraIns >= FechaDesdeSelect && X.FechaHoraIns <= FechaHastaSelect)) &&
                                                                 (key == "" ||
                                                                  X.IdControlTiempo.ToString().Contains(key) ||
                                                                  X.RutTransportista.Contains(key) ||
                                                                  X.NombreTransportista.Contains(key) ||
                                                                  X.NombreChofer.Contains(key) ||
                                                                  X.Patente.Contains(key) ||
                                                                  X.NumeroGuia.ToString().Contains(key))
                                                           && X.Habilitado == true
                                                           select X).ToList();
            }
            else
            {
                 controlesTiempo = (from X in dc.CTR_ControlTiempo
                                                           where (IdEmpresaSelect == 0 || X.IdEmpresa == IdEmpresaSelect) &&
                                                                 (IdProductoSelect == 0 || X.IdProducto == IdProductoSelect) &&
                                                                 (IdEnvaseSelect == 0 || X.IdEnvase == IdEnvaseSelect) &&
                                                                 (IdEstadoSelect == 0 || X.IdEstado != 4) &&
                                                                 (X.IdPlantaProduccion == IdPlantaProduccionSelect) &&
                                                                 ((X.FechaHoraIns >= FechaDesdeSelect && X.FechaHoraIns <= FechaHastaSelect)) &&
                                                                 (key == "" ||
                                                                  X.IdControlTiempo.ToString().Contains(key) ||
                                                                  X.RutTransportista.Contains(key) ||
                                                                  X.NombreTransportista.Contains(key) ||
                                                                  X.NombreChofer.Contains(key) ||
                                                                  X.Patente.Contains(key) ||
                                                                  X.NumeroGuia.ToString().Contains(key))
                                                           && X.Habilitado == true
                                                           select X).ToList();
            }


            if (IdAntiguedadSelect != 0)
            {
                if (IdAntiguedadSelect == 1)
                {
                    controlesTiempo = controlesTiempo.Where(X => X.IdEstado != 4).OrderBy(X => X.FechaLlegada).ToList();
                }
                else if (IdAntiguedadSelect == 2)
                {
                    controlesTiempo = controlesTiempo.Where(X => X.IdEstado != 4).OrderByDescending(X => X.FechaLlegada).ToList();
                }
                else
                {
                    controlesTiempo = controlesTiempo.OrderByDescending(X => X.IdControlTiempo).ToList();
                }
            }
            else
            {
                controlesTiempo = controlesTiempo.OrderByDescending(X => X.IdControlTiempo).ToList();
            }


            controlTiempo.IdEmpresa = IdEmpresaSelect;
            controlTiempo.IdProducto = IdProductoSelect;
            controlTiempo.IdEstado = IdEstadoSelect;
            controlTiempo.IdAntiguedad = IdAntiguedadSelect;
            controlTiempo.FechaDesde = FechaDesdeSelect;
            controlTiempo.FechaHasta = FechaHastaSelect;
            controlTiempo.IdPlantaProduccion = IdPlantaProduccionSelect;

            ViewData["controlTiempo"] = controlTiempo;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(225),
                                                      CheckPermiso(224),
                                                      CheckPermiso(226),
                                                      CheckPermiso(227));
            ViewData["PlantasProduccion"] = plantas;

            return View(controlesTiempo);
        }

        public ActionResult Detalle(int id)
        {
            CheckPermisoAndRedirect(224);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            return View("detalle", controlTiempo);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(226);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            ViewData["UserID"] = CurrentUser.UserID;
            ViewBag.Romana = (from c in dc.CTR_MantenedorRomana where (c.EsPlanta == true && c.Nombre == controlTiempo.PlantaProduccion.Nombre && c.Vigente == true) select c).SingleOrDefault();
            return View("editar", controlTiempo);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(226);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(controlTiempo, new string[] { "IdPlantaProduccion", "IdEmpresa", "RutTransportista", "NombreTransportista", "Patente", "NumeroGuia", "IdProducto", "IdCliente", "NombreChofer", "TelefonoChofer", "TipoCamion", "EmailChofer", "RutChofer", "DUS", "Reserva", "Observaciones" });

                    if (string.IsNullOrEmpty(controlTiempo.TelefonoChofer))
                        controlTiempo.TelefonoChofer = "";
                    if (string.IsNullOrEmpty(controlTiempo.EmailChofer))
                        controlTiempo.EmailChofer = "";
                    if (string.IsNullOrEmpty(controlTiempo.DUS))
                        controlTiempo.DUS = "";
                    if (string.IsNullOrEmpty(controlTiempo.Reserva))
                        controlTiempo.Reserva = "";
                    if (string.IsNullOrEmpty(controlTiempo.Observaciones))
                        controlTiempo.Observaciones = "";

                    controlTiempo.FechaHoraUpd = DateTime.Now;
                    controlTiempo.IpUpd = RemoteAddr();
                    controlTiempo.UserUpd = User.Identity.Name;
                    dc.SubmitChanges();
                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Control tiempo N° " + controlTiempo.IdControlTiempo + " ha sido editado correctamente" });
                }
                catch
                {
                    var rv = controlTiempo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }

            }
            ViewData["UserID"] = CurrentUser.UserID;
            return View("editar",controlTiempo);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(227);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                controlTiempo.Habilitado = false;
                controlTiempo.UserUpd = User.Identity.Name;
                controlTiempo.FechaHoraUpd = DateTime.Now;
                controlTiempo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El control de tiempo #{0} ha sido eliminado", controlTiempo.IdControlTiempo);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult EnviarSMS(int id)
        {
            CheckPermisoAndRedirect(224);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            return View("EnviarSMS", controlTiempo);
        }

        [HttpPost]
        public ActionResult EnviarSMS(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(224);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(controlTiempo, new string[] { "UserID" });

                    // Capturando el destinatario
                    SYS_User user = dc.SYS_User.Single(X => X.UserID == controlTiempo.UserID);

                    // Configuración del SMS a enviar
                    string customerId = ConfigurationManager.AppSettings["TeleSign_customerId"];
                    string apiKey = ConfigurationManager.AppSettings["TeleSign_apiKey"];
                    string phoneNumber = user.Telefono;
                    string message = controlTiempo.GetPlantilla();
                    string messageType = "ARN";

                    try
                    {
                        MessagingClient messagingClient = new MessagingClient(customerId, apiKey);
                        RestClient.TelesignResponse telesignResponse = messagingClient.Message(phoneNumber, message, messageType);

                        // Creando registro de alerta
                        CTR_RegistroAlerta registroAlerta = new CTR_RegistroAlerta()
                        {
                            IdControlTiempo = controlTiempo.IdControlTiempo,
                            Nombre = user.FullName,
                            Telefono = (user.Telefono ?? ""),
                            Mensaje = message,
                            Tipo = "SMS",
                            Estado = "Pendiente",
                            UserIns = CurrentUser.UserName,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        };

                        if (telesignResponse.OK)
                        {
                            registroAlerta.Estado = string.Format("{0} OK", telesignResponse.StatusCode);
                        }
                        else
                        {
                            registroAlerta.Estado = string.Format("{0} Error", telesignResponse.StatusCode);
                        }

                        dc.CTR_RegistroAlerta.InsertOnSubmit(registroAlerta);
                        dc.SubmitChanges();

                        string okMsg = string.Format("SMS enviado a {0} {1}", user.Telefono, user.FullName);
                        return RedirectToAction("Index", new { errMsg = "", okMsg = okMsg });
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("Detalle", new { id = controlTiempo.IdControlTiempo, msgerr = ex.Message, msgok = "" });
                    }
                }
                catch
                {
                    var rv = controlTiempo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("EnviarSMS", controlTiempo);
        }

        public ActionResult EnviarEmail(int id)
        {
            CheckPermisoAndRedirect(224);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            return View("EnviarEmail", controlTiempo);
        }

        [HttpPost]
        public ActionResult EnviarEmail(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(224);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(controlTiempo, new string[] { "UserID" });

                    // Capturando el destinatario
                    SYS_User user = dc.SYS_User.Single(X => X.UserID == controlTiempo.UserID);

                    try
                    {
                        MailMessage objMM = new MailMessage();
                        objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                        objMM.To.Add(user.Email);
                        //objMM.Bcc.Add("jfernandez@granotop.cl");
                        objMM.Subject = string.Format("[App] Camión {0} en espera/parado", controlTiempo.Patente);

                        string baseTemplate = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/App_Data/notificacontroltiempo_template.html"), Encoding.UTF7);
                        Util.RepTemp(ref baseTemplate, "MENSAJE", controlTiempo.GetPlantilla());
                        Util.RepTemp(ref baseTemplate, "HORAENVIO", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                        objMM.IsBodyHtml = true;
                        objMM.Body = baseTemplate;

                        SmtpClient objSmtp = new SmtpClient();
                        objSmtp.Send(objMM);

                        CTR_RegistroAlerta registroAlerta = new CTR_RegistroAlerta()
                        {
                            IdControlTiempo = controlTiempo.IdControlTiempo,
                            Nombre = user.FullName,
                            Telefono = (user.Telefono ?? ""),
                            Mensaje = controlTiempo.GetPlantilla(),
                            Tipo = "Email",
                            Estado = "OK",
                            UserIns = CurrentUser.UserName,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        };
                        dc.CTR_RegistroAlerta.InsertOnSubmit(registroAlerta);
                        dc.SubmitChanges();

                        string okMsg = string.Format("Email enviado a {0} ({1})", user.FullName, user.Email);
                        return RedirectToAction("Index", new { errMsg = "", okMsg = okMsg });
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Detalle", new { id = controlTiempo.IdControlTiempo, msgerr = e.Message, msgok = "" });
                    }
                }
                catch
                {
                    var rv = controlTiempo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("EnviarEmail", controlTiempo);
        }

        public ActionResult GenerarTicket(int id)
        {
            CheckPermisoAndRedirect(233);

            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            if (string.IsNullOrEmpty(controlTiempo.EmailChofer)) { return RedirectToAction("EmailResponse", new { errMsg = "El chofer no tiene un correo electrónico asociado", okMsg = "" }); }

            List<CTR_ControlTiempo_Detalle> detalle = (from c in dc.CTR_ControlTiempo_Detalle where c.IdControlTiempo == id select c).ToList();
            string numeros = "";
            if (detalle.Count > 0)
            {
                foreach (var item in detalle)
                {
                    if (!numeros.Contains(item.NumeroGuia.ToString()))
                    {
                        if (numeros.Length == 0)
                        {
                            numeros += item.NumeroGuia.ToString();
                        }
                        else
                        {
                            numeros = numeros + " - " + item.NumeroGuia.ToString();
                        }
                    }
                }
            }
            else
            {
                numeros = "No existen números asignados";
            }

            MailMessage mail = new MailMessage();
            TicketPdf ticketPdf = new TicketPdf
            {
                Imprimir = false,
                RazonSocial = controlTiempo.Empresa.RazonSocial,
                Comuna = controlTiempo.PlantaProduccion.Comuna.Nombre,
                Actividad = controlTiempo.Empresa.Actividad,
                Ciudad = controlTiempo.PlantaProduccion.Comuna.Provincia.Nombre,
                Rut = controlTiempo.Empresa.Rut,
                Fono = controlTiempo.TelefonoChofer,
                Direccion = controlTiempo.Empresa.Direccion,
                Email = controlTiempo.Empresa.Email,
                Patente = controlTiempo.Patente,
                TipoCamion = controlTiempo.TipoCamion,
                Empresa = controlTiempo.NombreTransportista,
                FechaEntrada = controlTiempo.FechaLlegada,
                FechaSalida = controlTiempo.FechaPesajeFinal,
                Chofer = controlTiempo.RutChofer,
                Transportista = controlTiempo.NombreChofer,
                TipoOperacion = "",
                NumeroGuia = numeros,
                Observacion = controlTiempo.Observaciones,
                PesoFinal = controlTiempo.PesoFinal,
                PesoInicial = controlTiempo.PesoInicial
               
            };
            var file = ticketPdf.TicketMain();
            mail.From = new MailAddress("emailstests2021@gmail.com", "Notificación ticket");
            mail.To.Add(controlTiempo.EmailChofer);
            mail.Subject = "Este es un correo de prueba";
            mail.Body = "Este es un cuerpo de prueba";
            System.Net.Mail.Attachment attachment;
            Stream stream = (Stream)file;
            attachment = new System.Net.Mail.Attachment(stream, "Ticket.pdf", "application/pdf");
            mail.Attachments.Add(attachment);

            System.Net.Mail.SmtpClient objSmtp = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("emailstests2021@gmail.com", "FinalFantasy9")

            };
            objSmtp.Send(mail);

            return RedirectToAction("EmailResponse", new { errMsg = "", okMsg = "El correo electrónico se ha enviado correctamente" });
        }

        public ActionResult EmailResponse()
        {
            return View();
        }

    }
}