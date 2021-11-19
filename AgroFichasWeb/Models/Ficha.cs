using AgroFichasLib;
using AgroFichasWeb.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Ficha : IFichaNotificable
    {
        public void SetDefaults()
        {
            if (this.Observaciones == null)
                this.Observaciones = "";
        }
        public int ID
        {
            get { return this.IdFicha; }
        }

        public string NombrePredio
        {
            get
            {
                return this.Predio.Nombre;
            }
        }

        public string NombreAgricultor
        {
            get
            {
                return this.Predio.Agricultor.Nombre;
            }
        }

        public string EmailAgricultor
        {
            get
            {
                return this.Predio.Agricultor.Email;
            }
        }

        public List<string> GetDestinatariosFicha()
        {
            var dc = new AgroFichasDBDataContext();
            return dc.GetDestinatariosFicha(this.IdFicha).Select(d => d.Email).ToList();
        }


        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            return GetRuleViolations(new List<SelectorPotreroViewModel>());
        }

        public IEnumerable<RuleViolation> GetRuleViolations(List<SelectorPotreroViewModel> potreros)
        {
            if (potreros.Where(p => p.Seleccionado).Count() == 0)
                yield return new RuleViolation("Seleccione al menos un potrero.", "Potreros");

            yield break;
        }

        public string DescripcionPotreros()
        {
            return string.Join(", ", this.FichaPotrero.Select(p => p.Potrero.Nombre));
        }

        public bool TieneVariedades(int[] idsVariedades)
        {
            if (idsVariedades.Length == 0) //Empty filter means no filter
                return true;

            //Para cada potrero de esta ficha, revisamos si está sembrado con una de las variedades solicitadas
            foreach (var fp in this.FichaPotrero)
            {
                var variedad = fp.Potrero.VariedadTemporada(this.IdTemporada);
                if (variedad != null && idsVariedades.Contains(variedad.IdVariedad))
                    return true;
            }

            return false;
        }

        public string Hash
        {
            get
            {
                return AgroFichasLib.Ficha.Hash(this.IdFicha, this.IdPredio, this.UserIns);
            }
        }

        public string Hash2
        {
            get
            {
                return AgroFichasLib.Ficha.Hash2(this.IdFicha, this.IdPredio);
            }
        }

        public string PdfUrl
        {
            get
            {
                return $"{ConfigurationManager.AppSettings["WebsiteUrl"]}/documents/ficha/{this.IdFicha}?h={this.Hash}";
            }
        }

        public string SendUrl
        {
            get
            {

                return string.Format($"{ConfigurationManager.AppSettings["ApiBaseUrl"]}/agroapi/sendficha/{this.IdFicha}?h={this.Hash2}");
            }
        }

        public void NotifyCreator()
        {
            try
            {
                var mailer = new FichaMailer();
                mailer.NotifyCreator(this);
            }
            catch (Exception ex)
            {
                var s = "";
                s += "DateTime: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "\r\n";
                s += "IdFicha: " + this.IdFicha + "\r\n";
                s += ex.ToString();
                System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/App_Data/notifications/errors/") + System.Guid.NewGuid() + ".txt", s);
            }
        }

        public string GetPdf(string filepath)
        {
            var baseFont = BaseFont.CreateFont();
            var bold = new Font(baseFont);
            bold.SetStyle(Font.BOLD);

            var context = HttpContext.Current;
            var doc = new Document();
            PdfWriter.GetInstance(doc, new FileStream(filepath, FileMode.Create));
            doc.Open();

            PdfPTable tableh = new PdfPTable(new float[] { 0.25f, 0.75f });
            tableh.DefaultCell.Border = 0;
            tableh.DefaultCell.Padding = 0;

            tableh.AddCell(Image.GetInstance(context.Server.MapPath("~/content/images/logo_pdf.png")));
            tableh.AddCell(DocTitle("Asistencia Técnica en Terreno #" + this.IdFicha));

            doc.Add(tableh);

            PdfPTable table = new PdfPTable(new float[] { 0.25f, 0.75f });
            table.SpacingBefore = 30;
            table.DefaultCell.Padding = 6;
            table.DefaultCell.BorderColor = new BaseColor(127, 127, 127);

            table.AddCell(SectionTitle("General", 2));
            table.AddCell(Label("Predio"));
            table.AddCell(this.Predio.Nombre);
            table.AddCell(Label("Comuna"));
            table.AddCell(this.Predio.Comuna.Nombre);
            table.AddCell(Label("Agricultor"));
            table.AddCell(this.Predio.Agricultor.Nombre);
            table.AddCell(Label("Rut"));
            table.AddCell(this.Predio.Agricultor.Rut);
            table.AddCell(Label("Email"));
            table.AddCell(this.Predio.Agricultor.Email);
            table.AddCell(Label("Fecha"));
            table.AddCell(this.Fecha.ToString("dd MMMM yyyy"));

            table.AddCell(Label("Responsable Agrotop"));
            var dc = new AgroFichasDBDataContext();
            var usuario = dc.SYS_User.SingleOrDefault(u => u.UserName == this.UserIns);
            if (usuario != null)
                table.AddCell(usuario.FullName);
            else
                table.AddCell(this.UserIns); doc.Add(table);

            
            PdfPTable table2 = new PdfPTable(new float[] { 0.25f, 0.75f });
            table2.SpacingBefore = 30;
            table2.DefaultCell.Padding = 6;
            table2.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY;

            table2.AddCell(SectionTitle("Visita", 2));
            table2.AddCell(Label("Potreros"));

            var pots = new PdfPTable(3);
            pots.DefaultCell.Border = 0;
            foreach (var fp in this.FichaPotrero)
            {
                string siembra = "";
                var datoSiembra = fp.Potrero.SiembraPotrero.FirstOrDefault(sp => sp.IdTemporada == this.IdTemporada);
                if (datoSiembra != null)
                    siembra = datoSiembra.Siembra.Variedad.Cultivo.Nombre + " " + datoSiembra.Siembra.Variedad.Nombre;

                string supeficie = "";
                if (fp.Potrero.Superficie > 0)
                    supeficie = fp.Potrero.Superficie.ToString("#,##0 há");

                pots.AddCell(fp.Potrero.Nombre);
                pots.AddCell(siembra);
                pots.AddCell(supeficie);
            }


            table2.AddCell(pots);

            table2.AddCell(Label("Etapa"));
            table2.AddCell(this.TipoFicha.Nombre);

            table2.AddCell(Label("Observaciones"));
            table2.AddCell(this.Observaciones);

            doc.Add(table2);

            var quimicos = this.Recomendacion.Where(r => r.Quimico.IdTipoRecomendacion != 3);
            if (quimicos.Count() > 0)
            {
                PdfPTable table3 = new PdfPTable(4);
                table3.SpacingBefore = 30;
                table3.DefaultCell.Padding = 6;
                table3.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY;

                table3.AddCell(SectionTitle("Recomendaciones", 4));
                table3.AddCell(Label("Tipo"));
                table3.AddCell(Label("Producto"));
                table3.AddCell(Label("Dósis"));
                table3.AddCell(Label("Fecha Aplicación"));

                foreach (var rec in quimicos)
                {
                    table3.AddCell(rec.Quimico.TipoRecomendacion.Nombre);
                    table3.AddCell(rec.Quimico.Nombre);
                    table3.AddCell(rec.Dosis.ToString("#,##0.#### ") + rec.UM.Nombre);
                    table3.AddCell(rec.FechaAplicacion.Value.ToString("dd MMMM"));
                }

                doc.Add(table3);
            }

            var fertilizantes = this.Recomendacion.Where(r => r.Quimico.IdTipoRecomendacion == 3);
            if (fertilizantes.Count() > 0)
            {
                PdfPTable table3 = new PdfPTable(8);
                table3.SpacingBefore = 30;
                table3.DefaultCell.Padding = 6;
                table3.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY;

                table3.AddCell(SectionTitle("Fertilizantes", 8));
                table3.AddCell(Label("N"));
                table3.AddCell(Label("P2O5"));
                table3.AddCell(Label("KO2"));
                table3.AddCell(Label("MgO"));
                table3.AddCell(Label("S"));
                table3.AddCell(Label("B"));
                table3.AddCell(Label("Zn"));
                table3.AddCell(Label("CaO"));

                foreach (var rec in fertilizantes)
                {
                    table3.AddCell(rec.FerN.ToString("#,##0.####"));
                    table3.AddCell(rec.FerP2O5.ToString("#,##0.####"));
                    table3.AddCell(rec.FerKO2.ToString("#,##0.####"));
                    table3.AddCell(rec.FerMgO.ToString("#,##0.####"));
                    table3.AddCell(rec.FerS.ToString("#,##0.####"));
                    table3.AddCell(rec.FerB.ToString("#,##0.####"));
                    table3.AddCell(rec.FerZn.ToString("#,##0.####"));
                    table3.AddCell(rec.FerCaO.ToString("#,##0.####"));
                }

                doc.Add(table3);
            }

            if (this.FotoFicha.Count() > 0)
            {
                PdfPTable table4 = new PdfPTable(1)
                {
                    KeepTogether = true,
                    SpacingBefore = 30
                };

                table4.DefaultCell.Padding = 6;
                table4.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY;

                table4.AddCell(SectionTitle("Fotografías", 1));

                foreach (var foto in this.FotoFicha)
                {

                    Image pic = Image.GetInstance(foto.FotoPath);
                    pic.ScaleToFit(400, 320);

                    var innerCell = new PdfPCell(pic)
                    {
                        FixedHeight = 320f,
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE
                    };
                    table4.AddCell(innerCell);
                    table4.AddCell(foto.Observaciones);
                }

                doc.Add(table4);
            }


            doc.Close();



            return filepath;
        }

        private PdfPCell Label(string text)
        {
            var baseFont = BaseFont.CreateFont();
            var bold = new Font(baseFont);
            bold.SetStyle(Font.BOLD);

            return new PdfPCell(new Phrase(text, bold)) 
            { 
                Padding = 6,
                BorderColor = new BaseColor(127, 127, 127),
                BackgroundColor = new BaseColor(235, 235, 235)
            };
        }

        private PdfPCell DocTitle(string text)
        {
            var baseFont = BaseFont.CreateFont();
            var bold = new Font(baseFont);
            bold.SetStyle(Font.BOLD);
            bold.Size = 18;

            return new PdfPCell(new Phrase(text, bold))
            {
                Padding = 6,
                Border = 0
            };
        }

        private PdfPCell SectionTitle(string text, int colspan)
        {
            var baseFont = BaseFont.CreateFont();
            var bold = new Font(baseFont);
            bold.SetStyle(Font.BOLD);

            return new PdfPCell(new Phrase(text, bold))
            {
                Padding = 6,
                Colspan = colspan,
                BorderColor = new BaseColor(127, 127, 127),
                BorderWidthBottom = 2,
                BorderColorBottom =  new BaseColor(230, 24, 26),
                BackgroundColor = new BaseColor(235, 235, 235)
            };
        }
    }
}