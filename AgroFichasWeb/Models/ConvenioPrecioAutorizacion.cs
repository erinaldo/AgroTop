using AgroFichasWeb.ViewModels.Liquidaciones;
using AgroFichasWeb.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class ConvenioPrecioAutorizacion
    {
        public string EstadoAutorizacion
        {
            get
            {
                if (this.Autorizada.HasValue)
                {
                    if (this.Autorizada.Value)
                        return "AUTORIZADO";
                    else
                        return "RECHAZADO";
                }
                else
                {
                    return "EN REVISION";
                }
            }
        }

        public string ColorAutorizacion
        {
            get
            {
                if (this.Autorizada.HasValue)
                {
                    return this.Autorizada.Value ? "#83f03c" : "#ff8e73";
                }
                else
                {
                    return "#FFE640";
                }
            }
        }

        public bool NotificarEnRevision(ControllerContext ctx, out string msg)
        {
            msg = "";
            try
            {
                var dc = new AgroFichasDBDataContext();
                var model = new AutorizarPreciosViewModel(dc, this.IdConvenioPrecioAutorizacion);

                string destinatarios = "";

                var receptores = dc.ReceptoresNotificacionAutorizacionPrecio(this.IdConvenioPrecioAutorizacion).ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar el requerimiento de autorización");

                string receptoresSinAut = "";
                string autorizadores = "";
                var receptoresConAut = new List<ReceptoresNotificacionAutorizacionPrecioResult>();

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;

                    if (receptor.PuedeAutorizar.Value)
                    {
                        receptoresConAut.Add(receptor);
                        autorizadores += (autorizadores != "" ? ", " : "") + receptor.FullName;
                    }
                    else
                    {
                        receptoresSinAut += (receptoresSinAut != "" ? ", " : "") + email;
                    }
                }

                string sucursalesEntrega = @String.Join(", ", model.ConvenioPrecioPorAutorizar.Sucursales.Where(s => s.Seleccionado).Select(s => s.NombreSucursal));

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificarevisionprecio_template.html"), Encoding.UTF7);
                RepTemp(ref baseTemplate, "TEMPORADA", model.ConvenioPrecioPorAutorizar.Contrato.Temporada.Nombre);
                RepTemp(ref baseTemplate, "AGRICULTOR", model.ConvenioPrecioPorAutorizar.Contrato.Agricultor.Nombre);
                RepTemp(ref baseTemplate, "RUT", model.ConvenioPrecioPorAutorizar.Contrato.Agricultor.Rut);
                RepTemp(ref baseTemplate, "CANTIDAD", model.ConvenioPrecioPorAutorizar.Cantidad.ToString("#,##0"));
                RepTemp(ref baseTemplate, "PRECIO", String.Format("{0:#,##0.00##} {1}/Kg", model.ConvenioPrecioPorAutorizar.PrecioUnidad, model.ConvenioPrecioPorAutorizar.Moneda.Simbolo));
                RepTemp(ref baseTemplate, "CONTRATO", this.Contrato.NumeroContrato + " - " + this.Contrato.Empresa.Nombre + " - " + this.Contrato.DescripcionCultivos(", "));
                RepTemp(ref baseTemplate, "SUCURSALES", sucursalesEntrega);
                RepTemp(ref baseTemplate, "IDCONVENIOPRECIO", this.IdConvenioPrecio.ToString());
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "AUTORIZADORES", autorizadores);
                RepTemp(ref baseTemplate, "AJUSTES", ViewHelpers.RenderRazorViewToString(ctx, "~/Views/ConveniosPrecio/DetalleAjustes.cshtml", model.ConvenioPrecioPorAutorizar));
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

                string subject = String.Format("Autorización Requerida para Convenio Precio {0}", model.ConvenioPrecioPorAutorizar.Contrato.Agricultor.Nombre);

                foreach (var autorizador in receptoresConAut)
                {
                    string autorizarLink = string.Format("{0}/{1}", ConfigurationManager.AppSettings["AutPrecioUrl"], model.IdConvenioPrecioAutorizacion);

                    string autorizarHtml = "<p>Puede autorizar este ingreso en el sistema, opción <i>Liquidaciones &gt; Convenios de Precios  &gt; Autorizar</i>.</p>" +
                                           "<p>Para autorizar inmediatemente, utilice el siguiente link: <a href=\"***AUTORIZALINK***\">***AUTORIZALINK***</a></p>";

                    string Template = baseTemplate;
                    RepTemp(ref Template, "SECCIONAUTORIZACION", autorizarHtml.Replace("***AUTORIZALINK***", autorizarLink));

                    SendMail(autorizador.Email, Template, subject);
                }

                if (receptoresSinAut != "")
                {
                    string Template = baseTemplate;
                    RepTemp(ref Template, "SECCIONAUTORIZACION", "");

                    SendMail(receptoresSinAut, Template, subject);
                }

                msg = "Se ha notificado que el convenio de precio está en revisión a los siguientes correos: " + destinatarios;
                return true;
            }
            catch (Exception ex)
            {
                msg = "Ocurrió un error al intentar notificar por correo: " + ex.Message;
                return false;
            }
        }

        public List<DestinatarioNotificacion> DestinatariosNotificacionRechazo()
        {
            var dc = new AgroFichasDBDataContext();
            return (from d in dc.ReceptoresNotificacionConvenioPrecio(this.IdConvenioPrecio)
                    where d.Rol != "Agricultor"
                    select new DestinatarioNotificacion()
                    {
                        Rol = d.Rol,
                        Email = d.Email,
                        Nombre = d.Nombre,
                        Seleccionado = true,
                        Optional = d.Optional ?? false
                    }).ToList();
        }

        public bool NotificarRechazo(ControllerContext ctx, List<DestinatarioNotificacion> receptores, out string msg)
        {
            msg = "";
            try
            {
                var dc = new AgroFichasDBDataContext();
                var model = new AutorizarPreciosViewModel(dc, this.IdConvenioPrecioAutorizacion);

                string destinatarios = "";

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar el rechazo");


                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string sucursalesEntrega = @String.Join(", ", model.ConvenioPrecioPorAutorizar.Sucursales.Where(s => s.Seleccionado).Select(s => s.NombreSucursal));

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificarechazoprecio_template.html"), Encoding.UTF7);
                RepTemp(ref baseTemplate, "TEMPORADA", model.ConvenioPrecioPorAutorizar.Contrato.Temporada.Nombre);
                RepTemp(ref baseTemplate, "AGRICULTOR", model.ConvenioPrecioPorAutorizar.Contrato.Agricultor.Nombre);
                RepTemp(ref baseTemplate, "RUT", model.ConvenioPrecioPorAutorizar.Contrato.Agricultor.Rut);
                RepTemp(ref baseTemplate, "CANTIDAD", model.ConvenioPrecioPorAutorizar.Cantidad.ToString("#,##0"));
                RepTemp(ref baseTemplate, "PRECIO", String.Format("{0:#,##0.00##} {1}/Kg", model.ConvenioPrecioPorAutorizar.PrecioUnidad, model.ConvenioPrecioPorAutorizar.Moneda.Simbolo));
                RepTemp(ref baseTemplate, "CONTRATO", this.Contrato.NumeroContrato + " - " + this.Contrato.Empresa.Nombre + " - " + this.Contrato.DescripcionCultivos(", "));
                RepTemp(ref baseTemplate, "SUCURSALES", sucursalesEntrega);
                RepTemp(ref baseTemplate, "IDCONVENIOPRECIO", this.IdConvenioPrecio.ToString());
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "AJUSTES", ViewHelpers.RenderRazorViewToString(ctx, "~/Views/ConveniosPrecio/DetalleAjustes.cshtml", model.ConvenioPrecioPorAutorizar));
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

                string subject = String.Format("Convenio de Precio Rechazado", model.ConvenioPrecioPorAutorizar.Contrato.Agricultor.Nombre);

                SendMail(destinatarios, baseTemplate, subject);

                msg = "Se ha notificado que el convenio de precio está en revisión a los siguientes correos: " + destinatarios;
                return true;
            }
            catch (Exception ex)
            {
                msg = "Ocurrió un error al intentar notificar por correo: " + ex.Message;
                return false;
            }
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        private void SendMail(string destinatarios, string body, string subject)
        {
            var objMM = new MailMessage
            {
                From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop"),
                Subject = subject,
                IsBodyHtml = true,
                Body = body
            };
            objMM.To.Add(destinatarios);

            var objSmtp = new SmtpClient();
            objSmtp.Send(objMM);
        }
    }
}