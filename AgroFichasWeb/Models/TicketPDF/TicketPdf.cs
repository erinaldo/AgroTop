using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public class TicketPdf
    {
        public bool Imprimir { get; set; }
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public string RazonSocial { get; set; }
        public string Comuna { get; set; }
        public string Actividad { get; set;}
        public string Ciudad { get; set; }
        public string Rut { get; set; }
        public string Fono { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
        public string Patente { get; set; }
        public string TipoCamion { get; set; }
        public string Empresa { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string Chofer { get; set; }
        public string Transportista { get; set; }
        public string TipoOperacion { get; set; }
        public string NumeroGuia { get; set; }
        public string Observacion { get; set; }

        public decimal? PesoFinal { get; set; }
        public decimal? PesoInicial { get; set; }

        public Object TicketMain()
        {

            Document doc = new Document();

            doc.SetPageSize(PageSize.NOTE);

            doc.SetMargins(70f, 70f, 30f, 70f);

            MemoryStream memStream = new MemoryStream();



            PdfWriter wri = PdfWriter.GetInstance(doc, memStream);




            doc.Open();



            //Datos del documento
            doc.AddCreationDate();


            GenerarCabecera(doc);

            GenerarCuerpo(doc);

            GenerarFinal(doc);




            doc.Close();

            byte[] byteStream = memStream.ToArray();

            memStream = new MemoryStream();

            memStream.Write(byteStream, 0, byteStream.Length);

            memStream.Position = 0;
            // return new FileStreamResult(memStream, "application/pdf");
            if (this.Imprimir)
            {
                return new FileStreamResult(memStream, "application/pdf");
            }
            else
            {
                return memStream;

            }
        }

        private void GenerarCabecera(Document doc)
        {
            //fuente
            var fuente = FontFactory.GetFont(FontFactory.HELVETICA, 8f, BaseColor.BLACK);


            //Fecha impresion
            var parrafo = new Paragraph("Fecha impresión: " + DateTime.Today.ToString("dd'/'MM'/'yyyy"), fuente)
            {
                Alignment = Element.ALIGN_RIGHT
            };

            doc.Add(parrafo);

            var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA, 12f, BaseColor.BLACK);


            parrafo = new Paragraph("BOLETO PESAJE COMERCIAL N°  80.645", fuenteTitulo)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 10f
            };

            doc.Add(parrafo);




        }

        private void GenerarCuerpo(Document doc)
        { 
            //fuente
            var fuente = FontFactory.GetFont(FontFactory.HELVETICA, 8f, BaseColor.BLACK);

            //variables
            var nada = new PdfPCell(new Phrase(" "))
            {
                Border = 0
            };


            var table = new PdfPTable(4);

            var width = new float[] { 2.5f, 8.0f, 2.5f, 4.0f };
            table.HorizontalAlignment = Element.ALIGN_CENTER;
            table.WidthPercentage = 95;
            table.SetWidths(width);

            table.AddCell(nada);
            table.AddCell(nada);
            table.AddCell(nada);
            table.AddCell(nada);

            //Fila 1 ----------------------------------------------------------
            var columna = new PdfPCell(new Phrase("Razón Social: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.RazonSocial, fuente))
            {
                Border = 0,
                Colspan = 1
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Comuna: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Comuna, fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            //Fila 2 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Actividad: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Actividad, fuente))
            {
                Border = 0,
                Colspan = 1
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Ciudad: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Ciudad, fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            //Fila3 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Rut: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Rut, fuente))
            {
                Border = 0,
                Colspan = 1
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Fono: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(" ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            //Fila4 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Dirección: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Direccion, fuente))
            {
                Border = 0,
                Colspan = 1
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Email: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Email, fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            doc.Add(table);

            var linea = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(2f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 5f));
            doc.Add(linea);

            //Segunda tabla 

            table = new PdfPTable(4);

            width = new float[] { 8.0f, 8.0f, 8.0f, 8.0f };
            table.HorizontalAlignment = Element.ALIGN_CENTER;
            table.WidthPercentage = 95;
            table.SetWidths(width);


            //Fila 1 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Patente: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Patente, fuente))
            {
                Border = 0,
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Tipo de Camión: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.TipoCamion, fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            //Fila 2 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Empresa: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Empresa, fuente))
            {
                Border = 0,
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Fecha/hora de Entrada: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.FechaEntrada.ToString("dd'/'MM'/'yyyy HH:mm:ss"), fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            //Fila 3 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Chofer: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Chofer, fuente))
            {
                Border = 0,
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Fecha/hora de Salida: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);
            if(this.FechaSalida == null)
            {
                columna = new PdfPCell(new Phrase(this.FechaSalida.ToString(), fuente))
                {
                    Border = 0
                };
                table.AddCell(columna);
            }
            else
            {
                DateTime salida = (DateTime) this.FechaSalida;
                columna = new PdfPCell(new Phrase(salida.ToString("dd'/'MM'/'yyyy HH:mm:ss"), fuente))
                {
                    Border = 0
                };
                table.AddCell(columna);
            }


            //Fila 4 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Transportista: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Transportista, fuente))
            {
                Border = 0,
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Tipo Operación: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.TipoOperacion, fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            //Fila 5 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Usuario: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("ROMANA", fuente))
            {
                Border = 0,
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("N° Guía: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.NumeroGuia, fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            //Fila 6 ----------------------------------------------------------
            columna = new PdfPCell(new Phrase("Observación: ", fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase(this.Observacion, fuente))
            {
                Border = 0,
            };
            table.AddCell(columna);

            columna = new PdfPCell(nada)
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(nada)
            {
                Border = 0
            };
            table.AddCell(columna);

            doc.Add(table);

        }

        private void GenerarFinal(Document doc)
        {


            //fuente
            var fuente = FontFactory.GetFont(FontFactory.HELVETICA, 10f, BaseColor.BLACK);

            //variables
            var nada = new PdfPCell(new Phrase(" "))
            {
                Border = 0
            };


            var table = new PdfPTable(4);

            var width = new float[] { 1f, 2f, 2f,2f };
            table.WidthPercentage = 95;
            table.SetWidths(width);
            table.HorizontalAlignment = Element.ALIGN_CENTER;

            //Fila 1 ----------------------------------------------------------
            decimal pesoInicial = 0;
            decimal pesoFinal = 0;
            if (this.PesoInicial != null) pesoInicial = (decimal)this.PesoInicial; 
            if (this.PesoFinal != null) pesoFinal = (decimal)this.PesoFinal; 
            table.AddCell(nada);
            var columna = new PdfPCell(new Phrase("Peso Tara: " + pesoInicial.ToString("N0"), fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            columna = new PdfPCell(new Phrase("Peso Bruto: " + (pesoFinal.ToString("N0")), fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            decimal resultado = 0;
            if(this.PesoFinal > this.PesoInicial)
            {
                resultado = (decimal) (this.PesoFinal - this.PesoInicial);
            }
            else
            {
                resultado = (decimal) (this.PesoInicial - this.PesoFinal??0);
            }
            columna = new PdfPCell(new Phrase("Peso Neto: " + resultado.ToString("N0"), fuente))
            {
                Border = 0
            };
            table.AddCell(columna);

            doc.Add(table);
        }



    }
}