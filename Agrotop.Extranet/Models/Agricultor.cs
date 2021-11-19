using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public partial class Agricultor
    {

        public List<int> IdsHijos()
        {
            return (from ar in this.AgricultorRelacionado1 select ar.IdAgricultorHijo).ToList();
            //return (from ar in AgricultorRelacionado
            //        where ar.IdAgricultorPadre == this.IdAgricultor
            //          && ar.Agricultor.Habilitado
            //          && ar.Agricultor1.Habilitado
            //        select ar.IdAgricultorPadre).ToList();
        }

        public static string NomarlizarRut(string rut)
        {
            return rut.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();
        }

        public static string HashPassword(string password)
        {
            if (password == "")
                return "";

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = md5.ComputeHash(Encoding.Default.GetBytes("ask(sdMARCO)anbuw&((#=)" + password));

            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2"));

            return sb.ToString();
        }

        public PasswordResetRequest CreatePasswordResetRequest(string ip)
        {
            var pRequest = new PasswordResetRequest();

            pRequest.ParamKey = Guid.NewGuid().ToString().Replace("-", "");
            
            if (this.Habilitado)
            {
                pRequest.IdAgricultor = this.IdAgricultor;
                pRequest.Valid = true;
                pRequest.ExpirationDateTime = DateTime.Now.AddHours(24);
            }
            else
            {
                pRequest.IdAgricultor = null;
                pRequest.Valid = false;
                pRequest.ExpirationDateTime = DateTime.Now.AddHours(-1);
            }

            pRequest.Email = this.Email;
            pRequest.EmailSent = false;
            pRequest.DateTimeEmailSent = null;
            pRequest.EmailSentComment = "";
            pRequest.Source = "PasswordResetRequest";
            pRequest.DateTimeIns = DateTime.Now;
            pRequest.IpIns = ip;
            pRequest.Used = false;
            pRequest.DateTimeUsed = null;
            pRequest.IpUsed = "";

            var dc = new AgrotopDBDataContext();
            dc.PasswordResetRequest.InsertOnSubmit(pRequest);
            dc.SubmitChanges();

            if (pRequest.Valid)
                SendPasswordResetEmail(dc, pRequest);

            return pRequest;
        }

        private void SendPasswordResetEmail(AgrotopDBDataContext dc, PasswordResetRequest pRequest)
        {

            try
            {
                var resetLink = string.Format("<a href=\"{0}\">{0}</a>", "http://clientes.empresasagrotop.cl/account/reset/" + pRequest.ParamKey);

                MailMessage objMM = new MailMessage();

                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                objMM.To.Add(pRequest.Email);
                objMM.Subject = string.Format("Creación de contraseña clientes Empresas Agrotop");
                objMM.IsBodyHtml = true;

                string Template = null;
                Template = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/pwdresetrequest_template.html"), Encoding.UTF7);
                RepTemp(ref Template, "NOMBRE", this.Nombre);
                RepTemp(ref Template, "RESETLINK", resetLink);

                objMM.Body = Template;

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);

                pRequest.EmailSent = true;
                pRequest.DateTimeEmailSent = DateTime.Now;
                pRequest.EmailSentComment = "OK";

            }
            catch (Exception ex)
            {
                pRequest.EmailSent = false;
                pRequest.DateTimeEmailSent = DateTime.Now;
                pRequest.EmailSentComment = (ex.Message.Length <= 200) ? ex.Message : ex.Message.Substring(0, 200);
            }
            finally
            {
                dc.SubmitChanges();
            }

        }

        private static void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }


    }
}