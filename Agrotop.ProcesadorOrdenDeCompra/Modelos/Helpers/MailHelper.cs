using Agrotop.PROCEA.Modelos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Agrotop.PROCEA
{
    class MailHelper
    {
        public static void SendNotificacionError(OrdenCompra ordenCompra, string errorMsg, AgrofichasDBDataContext dc, NLog.Logger logger, Exception ex = null)
        {
            try
            {
                MailMessage objMM = new MailMessage();
                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                objMM.To.Add("frodriguez@saprosem.cl");
                objMM.Bcc.Add("csepulveda@saprosem.cl");
                objMM.Bcc.Add("jfernandez@granotop.cl");
                objMM.Subject = "[App] Error en el Procesador de ODC";

                String Template = System.IO.File.ReadAllText(Path.GetFullPath(Properties.Settings.Default.EmailTemplatesFolder + "/procesador_orden_de_compra_error.html"), System.Text.Encoding.UTF8);

                if (ordenCompra != null)
                {
                    Utility.RepTemp(ref Template, "PROYECTO", DatabaseHelper.GetProyecto(ordenCompra.IdProyecto, dc).Descripcion);
                    Utility.RepTemp(ref Template, "EMPRESA", DatabaseHelper.GetEmpresa(ordenCompra.IdProyecto, ordenCompra.IdLiquidacion, dc).Nombre);
                    Utility.RepTemp(ref Template, "LIQUIDACION", string.Format("#{0}", ordenCompra.IdLiquidacion));
                    Utility.RepTemp(ref Template, "ARCHIVO", ordenCompra.Filename);
                }
                else
                {
                    Utility.RepTemp(ref Template, "PROYECTO", "n/d");
                    Utility.RepTemp(ref Template, "LIQUIDACION", "n/d");
                    Utility.RepTemp(ref Template, "ARCHIVO", "n/d");
                }
                
                Utility.RepTemp(ref Template, "ERRORMESSAGE", errorMsg);

                // Stack Trace del Error
                if (ex != null)
                {
                    Utility.RepTemp(ref Template, "STACKTRACE", ex.ToString());
                }
                else
                {
                    Utility.RepTemp(ref Template, "STACKTRACE", "n/d");
                }

                Utility.RepTemp(ref Template, "DATETIME", string.Format("{0:dd-MM-yyyy hh:mm}", DateTime.Now));
                objMM.IsBodyHtml = true;
                objMM.Body = Template;

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);

                logger.Info("Mensaje OK");
            }
            catch (Exception exc)
            {
                logger.Error(exc.GetBaseException());
            }
        }

        public static void SendNotificacionOK(OrdenCompra ordenCompra, string code, AgrofichasDBDataContext dc, NLog.Logger logger)
        {
            try
            {
                MailMessage objMM = new MailMessage();
                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                objMM.To.Add("frodriguez@saprosem.cl");
                objMM.Bcc.Add("csepulveda@saprosem.cl");
                objMM.Bcc.Add("jfernandez@granotop.cl");
                objMM.Subject = "[App] ODC Procesada";

                String Template = System.IO.File.ReadAllText(Path.GetFullPath(Properties.Settings.Default.EmailTemplatesFolder + "/procesador_orden_de_compra_procesada.html"), System.Text.Encoding.UTF8);

                Utility.RepTemp(ref Template, "PROYECTO", DatabaseHelper.GetProyecto(ordenCompra.IdProyecto, dc).Descripcion);
                Utility.RepTemp(ref Template, "EMPRESA", DatabaseHelper.GetEmpresa(ordenCompra.IdProyecto, ordenCompra.IdLiquidacion, dc).Nombre);
                Utility.RepTemp(ref Template, "LIQUIDACION", string.Format("#{0}", ordenCompra.IdLiquidacion));
                Utility.RepTemp(ref Template, "ARCHIVO", ordenCompra.Filename);
                Utility.RepTemp(ref Template, "OC", code);
                Utility.RepTemp(ref Template, "DATETIME", string.Format("{0:dd-MM-yyyy hh:mm}", DateTime.Now));
                objMM.IsBodyHtml = true;
                objMM.Body = Template;

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);

                logger.Info("Email OK");
            }
            catch (Exception exc)
            {
                logger.Error(exc.GetBaseException());
            }
        }
    }
}
