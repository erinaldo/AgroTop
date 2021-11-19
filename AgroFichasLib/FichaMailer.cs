using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AgroFichasLib
{
    public class FichaMailer
    {
        public void NotifyCreator(IFichaNotificable ficha)
        {
            var dc = new AgroFichaDBDataContext();
            var user = dc.SYS_User.SingleOrDefault(u => u.UserName == ficha.UserIns);
            if (user == null)
                return;

            var logFolder = ConfigurationManager.AppSettings["NotificationsLogFolder"];
            var templateFolder = ConfigurationManager.AppSettings["TemplateLogFolder"];

            var recipientMail = user.Email;
            var recipientName = user.FullName;

            var pdfUrl = $"{ficha.PdfUrl}&nolog=1";
            //var bcc = "cdonoso@woc.cl";

            string xmlFile = Path.Combine(logFolder, $"nf_creator_{ficha.ID}_{DateTime.Now.ToString("yyyyMMddhhmmss")}.xml");
            XmlTextWriter w = new XmlTextWriter(xmlFile, Encoding.UTF8);
            w.WriteStartDocument();
            w.WriteStartElement("root");

            w.WriteStartElement("notification");
            w.WriteAttributeString("date", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            w.WriteAttributeString("type", "creator");

            this.WriteCData(ref w, "idficha", ficha.ID.ToString());
            this.WriteCData(ref w, "recipientMail", recipientMail);
            this.WriteCData(ref w, "recipientName", recipientName);
            this.WriteCData(ref w, "pdfUrl", pdfUrl);
            //this.WriteCData(ref w, "bcc", bcc);

            w.WriteEndElement();
            w.Close();
            w = null;

            string template = "";

            var objMM = new MailMessage();
            {
                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                objMM.To.Add(new MailAddress(recipientMail, recipientName));
                //objMM.Bcc.Add(bcc);

                objMM.Subject = string.Format("Revisión de Asesoría Técnica en Terreno");
                objMM.IsBodyHtml = true;

                template = File.ReadAllText(Path.Combine(templateFolder, "ficha_creator_template.html"), System.Text.Encoding.UTF7);
                RepTemp(ref template, "NOMBRE", ficha.NombreAgricultor);
                RepTemp(ref template, "NROFICHA", ficha.ID.ToString("#,##0"));
                RepTemp(ref template, "PREDIO", ficha.NombrePredio);
                RepTemp(ref template, "SENDURL", ficha.SendUrl);
                RepTemp(ref template, "PDFURL", pdfUrl);
            }
            objMM.Body = template;

            SmtpClient objSmtp = new SmtpClient();
            objSmtp.Send(objMM);
        }

        public void NotifyAgricultor(IFichaNotificable ficha)
        {
            var recipientMail = ficha.EmailAgricultor;
            var recipientName = ficha.NombreAgricultor;

            var logFolder = ConfigurationManager.AppSettings["NotificationsLogFolder"];
            var templateFolder = ConfigurationManager.AppSettings["TemplateLogFolder"];

            var dc = new AgroFichaDBDataContext();
            var pdfUrl = ficha.PdfUrl;
            var bcc = String.Join(",", ficha.GetDestinatariosFicha());

            
            string xmlFile = Path.Combine(logFolder, $"nf_agricultor_{ficha.ID}_{DateTime.Now.ToString("yyyyMMddhhmmss")}.xml");
            XmlTextWriter w = new XmlTextWriter(xmlFile, Encoding.UTF8);
            w.WriteStartDocument();
            w.WriteStartElement("root");

            w.WriteStartElement("notification");
            w.WriteAttributeString("date", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            w.WriteAttributeString("type", "agricultor");

            this.WriteCData(ref w, "idFichaPreSiembra", ficha.ID.ToString());
            this.WriteCData(ref w, "recipientMail", recipientMail);
            this.WriteCData(ref w, "recipientName", recipientName);
            this.WriteCData(ref w, "pdfUrl", pdfUrl);
            this.WriteCData(ref w, "bcc", bcc);

            w.WriteEndElement();
            w.Close();
            w = null;

            string template = "";

            MailMessage objMM = new MailMessage();
            {
                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                objMM.To.Add(new MailAddress(recipientMail, recipientName));
                objMM.Bcc.Add(bcc);

                objMM.Subject = string.Format("Asesoría Técnica en Terreno");
                objMM.IsBodyHtml = true;

                template = File.ReadAllText(Path.Combine(templateFolder, "ficha_template.html"), System.Text.Encoding.UTF7);
                RepTemp(ref template, "NOMBRE", ficha.NombreAgricultor);
                RepTemp(ref template, "PDFURL", pdfUrl);
            }
            objMM.Body = template;

            SmtpClient objSmtp = new SmtpClient();
            objSmtp.Send(objMM);
        }

        private void RepTemp(ref string Template, string Key, string Value)
        {
            Template = Template.Replace("***" + Key + "***", Value);
        }

        private void WriteCData(ref XmlTextWriter w, string Name, string Value)
        {
            w.WriteStartElement(Name);
            w.WriteCData(Value);
            w.WriteEndElement();
        }
    }
}
