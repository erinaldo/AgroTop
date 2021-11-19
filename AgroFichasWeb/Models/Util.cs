using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models
{
    public class Util
    {
        public static string HttpBasePath()
        {
            HttpRequest request = HttpContext.Current.Request;
            string s = "http://" + request.ServerVariables["HTTP_HOST"] + request.ApplicationPath;
            if (!s.EndsWith("/"))
            {
                s += "/";
            }
            return s;
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

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