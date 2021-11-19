using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models.TrazaTop
{
    public class Notificacion
    {
        public static bool EnviarNotificacion(MensajeTipo mensajeTipo, out string errMsg)
        {
            errMsg = "";

            try
            {
                string template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificaasesor_template.html"), Encoding.UTF8);
                RepTemp(ref template, "TIPO",         mensajeTipo.Tipo);
                RepTemp(ref template, "NOMBREASESOR", mensajeTipo.Nombre);
                RepTemp(ref template, "MENSAJE",      mensajeTipo.Mensaje);

                MailMessage objMM = new MailMessage();
                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Empresas Agrotop");
                objMM.To.Add(mensajeTipo.Email);
                objMM.Subject = string.Format("Error en Verificación de {1} - Solicitud de Contrato #{0}", mensajeTipo.IdSolicitudContrato, mensajeTipo.Tipo);
                objMM.IsBodyHtml = true;
                objMM.Body = template;

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);

                return true;
            }
            catch (Exception exception)
            {
                errMsg = exception.ToString();
                return false;
            }
        }

        public static bool EnviarNotificacionContrato(SolicitudContrato solicitudContrato, out string errMsg)
        {
            errMsg = "";

            try
            {
                string template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificaasesorcontrato_template.html"), Encoding.UTF8);
                RepTemp(ref template, "NOMBREASESOR"    , solicitudContrato.NombreAsesor);
                RepTemp(ref template, "CONTRATON"       , solicitudContrato.Contrato.NumeroContrato);
                RepTemp(ref template, "CONTRATONFM"     , solicitudContrato.IdSolicitudContrato.ToString());
                RepTemp(ref template, "CULTIVO"         , solicitudContrato.Cultivo);
                RepTemp(ref template, "PRECIOCIERRE"    , solicitudContrato.PrecioCierre.ToString());
                RepTemp(ref template, "TONELADASCIERRE" , solicitudContrato.ToneladasCierre.ToString());
                RepTemp(ref template, "TIPOCONTRATO"    , solicitudContrato.TipoContrato);
                RepTemp(ref template, "COMUNAORIGEN"    , solicitudContrato.ComunaOrigen);
                RepTemp(ref template, "SUCURSALENTREGA" , solicitudContrato.SucursalEntrega);
                RepTemp(ref template, "HECTAREAS"       , solicitudContrato.Hectareas.ToString());
                RepTemp(ref template, "TONELADASTOTALES", solicitudContrato.ToneladasTotales.ToString());
                RepTemp(ref template, "PREDIO"          , solicitudContrato.Predio);

                MailMessage objMM = new MailMessage();
                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Empresas Agrotop");
                objMM.To.Add(solicitudContrato.EmailAsesor);
                objMM.Subject = string.Format("Notificación de Creación de Contrato Nº {0}", solicitudContrato.Contrato.NumeroContrato);
                objMM.IsBodyHtml = true;
                objMM.Body = template;

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);

                return true;
            }
            catch (Exception exception)
            {
                errMsg = exception.ToString();
                return false;
            }
        }

        private static void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }
    }
}