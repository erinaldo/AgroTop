using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agrotop.ReporteControlPlanta
{
    class Program
    {
        static AgroFichasDBDataContext context = new AgroFichasDBDataContext();

        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine(".oPYo.                                                         .oo                       o                ");
            Console.WriteLine("8.                                                            .P 8                       8                ");
            Console.WriteLine("`boo   ooYoYo. .oPYo. oPYo. .oPYo. .oPYo. .oPYo. .oPYo.      .P  8 .oPYo. oPYo. .oPYo.  o8P .oPYo. .oPYo. ");
            Console.WriteLine(".P     8' 8  8 8    8 8  `' 8oooo8 Yb..   .oooo8 Yb..       oPooo8 8    8 8  `' 8    8   8  8    8 8    8 ");
            Console.WriteLine("8      8  8  8 8    8 8     8.       'Yb. 8    8   'Yb.    .P    8 8    8 8     8    8   8  8    8 8    8 ");
            Console.WriteLine("`YooP' 8  8  8 8YooP' 8     `Yooo' `YooP' `YooP8 `YooP'   .P     8 `YooP8 8     `YooP'   8  `YooP' 8YooP' ");
            Console.WriteLine(":.....:..:..:..8 ....:..:::::.....::.....::.....::.....:::..:::::..:....8 ..:::::.....:::..::.....:8 ....:");
            Console.WriteLine(":::::::::::::::8 ::::::::::::::::::::::::::::::::::::::::::::::::::::ooP'.:::::::::::::::::::::::::8 :::::");
            Console.WriteLine(":::::::::::::::..::::::::::::::::::::::::::::::::::::::::::::::::::::...:::::::::::::::::::::::::::..:::::");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Reporte Semanal");
            Console.WriteLine("Camiones en Planta");
            Console.WriteLine("v1.0");

            try
            {
                var encabezado = context.rpt_CTR_EncabezadoControlSemanalPorEmpresa().FirstOrDefault();
                var empresas = context.Empresa.ToList();

                foreach (var empresa in empresas)
                {
                    string htmlContent = System.IO.File.ReadAllText(Path.GetFullPath(string.Format(@"{0}\informe_semanal_camiones_planta.html", Properties.Settings.Default.AppData)), Encoding.UTF8);
                    Utility.RepTemp(ref htmlContent, "FECHA"     , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
                    Utility.RepTemp(ref htmlContent, "AÑO"       , encabezado.PrimerDiaSemanaPasada.Value.Year.ToString());
                    Utility.RepTemp(ref htmlContent, "SEMANA"    , encabezado.NumeroSemana.Value.ToString());
                    Utility.RepTemp(ref htmlContent, "EMPRESA"   , empresa.Nombre);
                    Utility.RepTemp(ref htmlContent, "FECHADESDE", string.Format("{0:dd/MM/yy}", encabezado.PrimerDiaSemanaPasada));
                    Utility.RepTemp(ref htmlContent, "FECHAHASTA", string.Format("{0:dd/MM/yy}", encabezado.UltimoDiaSemanaPasada));

                    StringBuilder builder = new StringBuilder();

                    var detalle = context.rpt_CTR_DetalleControlSemanalPorEmpresa(empresa.IdEmpresa);
                    foreach (var d in detalle)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format( "    <td>{0}</td>", d.Producto));
                        builder.AppendLine(string.Format( "    <td>{0}</td>", d.Envase));
                        builder.AppendLine(string.Format( "    <td>{0:dd/MM/yy HH:mm:ss}</td>", d.FechaLlegada));
                        builder.AppendLine(string.Format( "    <td>{0:dd/MM/yy HH:mm:ss}</td>", d.FechaPesajeInicial));
                        builder.AppendLine(string.Format( "    <td>{0:dd/MM/yy HH:mm:ss}</td>", d.FechaPesajeFinal));
                        builder.AppendLine(string.Format( "    <td>{0:dd/MM/yy HH:mm:ss}</td>", d.FechaSalida));
                        builder.AppendLine(string.Format(@"    <td>{0:d\.hh\:mm\:ss}</td>", TimeSpan.FromMinutes(d.IngresoPlanta.Value)));
                        builder.AppendLine(string.Format(@"    <td>{0:d\.hh\:mm\:ss}</td>", TimeSpan.FromMinutes(d.Romana.Value)));
                        builder.AppendLine(string.Format(@"    <td>{0:d\.hh\:mm\:ss}</td>", TimeSpan.FromMinutes(d.SalidaPlanta.Value)));
                        builder.AppendLine(string.Format(@"    <td>{0:d\.hh\:mm\:ss}</td>", TimeSpan.FromMinutes(d.EstadiaEnPlanta.Value)));
                        builder.AppendLine("</tr>");
                    }

                    Utility.RepTemp(ref htmlContent, "CAMIONESESPERA", builder.ToString());
                    builder = new StringBuilder();

                    var resumen = context.rpt_CTR_ResumenControlSemanalPorEmpresa(empresa.IdEmpresa);
                    foreach (var r in resumen)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format( "    <td>{0}</td>", r.Producto));
                        builder.AppendLine(string.Format( "    <td>{0}</td>", r.Envase));
                        builder.AppendLine(string.Format( "    <td>{0}</td>", r.NControles));
                        builder.AppendLine(string.Format(@"    <td>{0:d\.hh\:mm\:ss}</td>", TimeSpan.FromMinutes(r.IngresoPlanta.Value)));
                        builder.AppendLine(string.Format(@"    <td>{0:d\.hh\:mm\:ss}</td>", TimeSpan.FromMinutes(r.Romana.Value)));
                        builder.AppendLine(string.Format(@"    <td>{0:d\.hh\:mm\:ss}</td>", TimeSpan.FromMinutes(r.SalidaPlanta.Value)));
                        builder.AppendLine(string.Format(@"    <td>{0:d\.hh\:mm\:ss}</td>", TimeSpan.FromMinutes(r.Promedio.Value)));
                        builder.AppendLine("</tr>");
                    }

                    Utility.RepTemp(ref htmlContent, "RESUMENESPERA", builder.ToString());

                    var filename = string.Format("reporte_semana_{0}_{1}_{2}", encabezado.NumeroSemana, encabezado.PrimerDiaSemanaPasada.Value.Year, empresa.Nombre.ToLower().Replace(" ", "_"));

                    FileStream fileStream = new FileStream(string.Format(@"{0}\pdf\{1}.html", Properties.Settings.Default.AppData, filename), FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fileStream);
                    writer.WriteLine(htmlContent); writer.Close();

                    var path = string.Format(@"{0}\pdf\{1}.pdf", Properties.Settings.Default.AppData, filename);
                    var bytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(htmlContent);
                    System.IO.File.WriteAllBytes(path, bytes);
                }

                MailHelper.Sendmail(encabezado);
            }
            catch (Exception e)
            {
                // Example #2: Write one string to a text file.
                string text = "A class is the most powerful data type in C#. Like a structure, " +
                               "a class defines the data and behavior of the data type. ";
                // WriteAllText creates a file, writes the specified string to the file,
                // and then closes the file.    You do NOT need to call Flush() or Close().
                System.IO.File.WriteAllText(Properties.Settings.Default.LogFolder + @"\" + System.Guid.NewGuid().ToString() + ".txt", e.ToString());
            }
        }
    }
}
