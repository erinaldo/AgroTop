using RtfPipe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models.TrazaTop.Documentos
{
    public class DocumentoContrato
    {
        public AgroFichasDBDataContext context = new AgroFichasDBDataContext();

        public int IdSolicitudContrato { get; set; }

        [Obsolete]
        public string CrearDoctoContrato(string html, int idTipoDoctoContrato, int correlativo, string userIns, DateTime fechaHoraIns, string ipIns)
        {
            SolicitudContrato solicitudContrato = context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == this.IdSolicitudContrato);

            string certkey = Guid.NewGuid().ToString();
            string filename1 = string.Format("{0}.html", certkey);
            string filename2 = string.Format("{0}.pdf", certkey);
            string path = string.Format(@"{0}\pdfs\liquidaciones\contratos\{1}", ConfigurationSettings.AppSettings.Get("App_Data"), solicitudContrato.Contrato.NumeroContrato);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath1 = string.Format(@"{0}\{1}", path, filename1);
            string fullPath2 = string.Format(@"{0}\{1}", path, filename2);

            FileStream fileStream = new FileStream(fullPath1, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine(html); 
            streamWriter.Close();

            byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()
            {
                Orientation = NReco.PdfGenerator.PageOrientation.Portrait,
                Margins = new NReco.PdfGenerator.PageMargins()
                {
                    Bottom = 15,
                    Left   = 15,
                    Right  = 15,
                    Top    = 15
                },
                Size = NReco.PdfGenerator.PageSize.Default,
                Zoom = 1.5f,
                CustomWkHtmlArgs = "--encoding utf8",
            }).GeneratePdf(html);
            System.IO.File.WriteAllBytes(fullPath2, pdfBytes);

            // Invalido todos los Documentos anteriores asociados a este Contrato
            IQueryable<DoctoContrato> doctoContratos = context.DoctoContrato.Where(dc => dc.IdContrato == solicitudContrato.IdContrato && dc.Correlativo == correlativo);
            foreach (DoctoContrato doctoContrato1 in doctoContratos)
            {
                doctoContrato1.DoctoValido     = false;
                doctoContrato1.RutaArchivoNulo = doctoContrato1.RutaArchivo;
                context.SubmitChanges();
            }
            // Creo un nuevo Documento
            DoctoContrato doctoContrato = new DoctoContrato()
            {
                IdTipoDoctoContrato = idTipoDoctoContrato,
                IdContrato          = solicitudContrato.IdContrato.Value,
                Correlativo         = correlativo,
                RutaArchivo         = string.Format("~/App_Data/pdfs/liquidaciones/contratos/{0}/{1}", solicitudContrato.Contrato.NumeroContrato, filename2),
                DoctoValido         = true,
                UserIns             = userIns,
                FechaHoraIns        = fechaHoraIns,
                IpIns               = ipIns
            };
            context.DoctoContrato.InsertOnSubmit(doctoContrato);
            context.SubmitChanges();

            return certkey;
        }

        public bool ExistePlanilla(bool isCierrePrecio = false)
        {
            return GetPlanillaContrato(isCierrePrecio) != null;
        }

        public bool ExistePlanillaAnexo()
        {
            return GetPlanillaContratoAnexo() != null;
        }

        public int GetAñoContrato(DateTime dateTime)
        {
            SolicitudContrato solicitudContrato = context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == this.IdSolicitudContrato);
            if (dateTime.Year == solicitudContrato.Temporada1.Inicio)
                return dateTime.Year + 1;
            else
                return dateTime.Year;
        }

        public string GetComuna(Comuna comuna)
        {
            string s = "";
            if (comuna != null)
            {
                s = comuna.Nombre;
            }
            else
            {
                s = "";
            }

            return s;
        }

        public string GetCuentaBancaria(CuentaBancaria cuentaBancaria)
        {
            string s = "";
            if (cuentaBancaria != null)
            {
                if (cuentaBancaria.Banco != null)
                {
                    s = string.Format("Cta {0} {1}, {2}", cuentaBancaria.TipoCuentaBancaria.Nombre, cuentaBancaria.NumeroCuenta, cuentaBancaria.Banco.Nombre);
                }
            }
            else
            {
                s = "";
            }

            return s;
        }

        public PlanillaContrato GetPlanillaContrato(bool isCierrePrecio = false)
        {
            SolicitudContrato solicitudContrato = GetSolicitudContrato();
            return context.PlanillaContrato.Where(pc => pc.IdTemporada == solicitudContrato.IdTemporada &&  (isCierrePrecio ? pc.IdTipoContrato == 4 : pc.IdTipoContrato == solicitudContrato.Contrato.IdTipoContrato)).OrderByDescending(pc => pc.IdPlanillaContrato).FirstOrDefault();
        }

        public PlanillaContrato GetPlanillaContratoAnexo()
        {
            SolicitudContrato solicitudContrato = GetSolicitudContrato();
            return context.PlanillaContrato.Where(pc => pc.IdTemporada == solicitudContrato.IdTemporada && pc.IdTipoContrato == null && pc.IdCultivo == solicitudContrato.IdCultivo).OrderByDescending(pc => pc.IdPlanillaContrato).FirstOrDefault();
        }

        public string GetPlanillaHTML(bool isCierrePrecio = false)
        {
            PlanillaContrato planillaContrato = GetPlanillaContrato(isCierrePrecio);

            // Convert String to MemoryStream
            byte[] byteArray = Encoding.UTF8.GetBytes(planillaContrato.DocumentoM);
            MemoryStream stream = new MemoryStream(byteArray);

            // Convert MemoryStream to String
            StreamReader reader = new StreamReader(stream);

            RtfSource source = new RtfSource(reader);
            return RtfPipe.Rtf.ToHtml(source);
        }

        public string GetPlanillaHTMLAnexo()
        {
            PlanillaContrato planillaContrato = GetPlanillaContratoAnexo();

            // Convert String to MemoryStream
            byte[] byteArray = Encoding.UTF8.GetBytes(planillaContrato.DocumentoM);
            MemoryStream stream = new MemoryStream(byteArray);

            // Convert MemoryStream to String
            StreamReader reader = new StreamReader(stream);

            RtfSource source = new RtfSource(reader);
            return RtfPipe.Rtf.ToHtml(source);
        }

        public SolicitudContrato GetSolicitudContrato()
        {
            return context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == this.IdSolicitudContrato);
        }

        public string GetSolicitudContratoVariedad()
        {
            string s = "";
            IQueryable<SolicitudContratoVariedad> solicitudContratoVariedades = context.SolicitudContratoVariedad.Where(scv => scv.IdSolicitudContrato == this.IdSolicitudContrato);
            foreach (SolicitudContratoVariedad solicitudContratoVariedad in solicitudContratoVariedades)
            {
                if (!string.IsNullOrEmpty(s))
                    s += ", " + solicitudContratoVariedad.Variedad;
                else
                    s = solicitudContratoVariedad.Variedad;
            }

            return s;
        }
    }
}