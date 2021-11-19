using ForceManagerLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForceManagerSync
{
    class Reporte
    {
        public void CreatePdf(List<Account> accounts_no_asociados)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Account account in accounts_no_asociados)
                {
                    stringBuilder.AppendLine("<tr>");
                    stringBuilder.AppendLine(string.Format("<td>{0}</td>", account.vatNumber ?? ""));
                    stringBuilder.AppendLine(string.Format("<td>{0}</td>", account.name ?? ""));
                    stringBuilder.AppendLine("</tr>");
                }

                string htmlContent = System.IO.File.ReadAllText(Path.GetFullPath(string.Format(@"{0}\forcemanager_template.html", Properties.Settings.Default.AppData)), Encoding.UTF8);
                RepTemp(ref htmlContent, "FECHA", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
                RepTemp(ref htmlContent, "AGRICULTORES", stringBuilder.ToString());
                RepTemp(ref htmlContent, "AÑO", DateTime.Now.Year.ToString());

                var path = string.Format(@"{0}\pdf\{1}.pdf", Properties.Settings.Default.AppData, Guid.NewGuid());
                var bytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(htmlContent);
                System.IO.File.WriteAllBytes(path, bytes);
            }
            catch (Exception ex)
            {
                string pdf_error_key = string.Format("pdf_error_{0}", System.Guid.NewGuid());
                System.IO.File.WriteAllText(string.Format(@"error/{0}.txt", pdf_error_key), ex.ToString());
            }
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }
    }
}