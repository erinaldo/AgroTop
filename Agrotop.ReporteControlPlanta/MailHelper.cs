using System;
using System.IO;
using System.Net.Mail;

namespace Agrotop.ReporteControlPlanta
{
    class MailHelper
    {
        public static void Sendmail(rpt_CTR_EncabezadoControlSemanalPorEmpresaResult rpt)
        {
            try
            {
                MailMessage objMM = new MailMessage();
                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                //objMM.To.Add("facturacion@oleotop.cl");
                objMM.To.Add("ti@empresasagrotop.cl");
                objMM.Subject = string.Format("Reporte Semana {0} {1}", rpt.NumeroSemana, rpt.PrimerDiaSemanaPasada.Value.Year);

                string s = "Adjunto reportes,<br><br>";
                s += "<a href='http://sys.empresasagrotop.cl/reporte-semanal/reporte_semana_{0}_{1}_avenatop.html'>Avenatop</a><br>";
                s += "<a href='http://sys.empresasagrotop.cl/reporte-semanal/reporte_semana_{0}_{1}_granotop.html'>Granotop</a><br>";
                s += "<a href='http://sys.empresasagrotop.cl/reporte-semanal/reporte_semana_{0}_{1}_industrial_community_international.html'>Industrial Community International</a><br>";
                s += "<a href='http://sys.empresasagrotop.cl/reporte-semanal/reporte_semana_{0}_{1}_oleotop.html'>Oleotop</a><br>";
                s += "<a href='http://sys.empresasagrotop.cl/reporte-semanal/reporte_semana_{0}_{1}_saprosem.html'>Saprosem</a><br>";

                objMM.IsBodyHtml = true;
                objMM.Body = string.Format(s, rpt.NumeroSemana, rpt.PrimerDiaSemanaPasada.Value.Year);

                FileStream fileStream = new FileStream(string.Format(@"{0}\pdf\maillog_{1}.html", Properties.Settings.Default.AppData, System.Guid.NewGuid().ToString()), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(objMM.Body); writer.Close();

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);
            }
            catch (Exception ex)
            {
                FileStream fileStream = new FileStream(string.Format(@"{0}\pdf\errorlog_{1}.TXT", Properties.Settings.Default.AppData, System.Guid.NewGuid().ToString()), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(ex.ToString()); writer.Close();
            }
        }
    }
}
