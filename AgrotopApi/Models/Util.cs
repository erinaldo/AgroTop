using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace AgrotopApi.Models
{
    public class Util
    {
        public static void RepTempAngularStyle(ref string template, string key, string value)
        {
            template = template.Replace("{{" + key + "}}", value);
        }

        public static void SendMail(string destinatarios, string body, string subject)
        {
            MailMessage objMM = new MailMessage();

            objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
            objMM.To.Add(destinatarios);
            objMM.Bcc.Add("ti@empresasagrotop.cl");
            objMM.Subject = subject;
            objMM.IsBodyHtml = true;

            objMM.Body = body;

            var objSmtp = new SmtpClient();
            objSmtp.Send(objMM);
        }
    }
}