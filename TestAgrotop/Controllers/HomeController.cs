using RtfPipe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using TestAgrotop.Models;

namespace TestAgrotop.Controllers
{
    public class HomeController : Controller
    {
        AgroFichasDBDataContext context = new AgroFichasDBDataContext();

        public ActionResult Index()
        {
            string html, text;
            // Open the text file using a stream reader.
            using (var sr = new StreamReader(Server.MapPath("~/App_Data/PLANTILLA_ACUERDO.rtf")))
            {
                RtfSource source = new RtfSource(sr);
                html = RtfPipe.Rtf.ToHtml(source);
            }
            CultureInfo cultureInfo             = new CultureInfo("es-CL");
            Agricultor agricultor               = context.Agricultor.Single(a => a.IdAgricultor == 1847);
            Empresa empresa                     = context.Empresa.Single(e => e.IdEmpresa == 2);
            SolicitudContrato solicitudContrato = context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == 5777);
            Temporada temporada                 = context.Temporada.Single(t => t.IdTemporada == solicitudContrato.IdTemporada);

            AcuerdoComercial acuerdoComercial = new AcuerdoComercial()
            { 
                AñoContrato        = DateTime.Now.Year,
                Comuna             = solicitudContrato.ComunaOrigen,
                Correo             = agricultor.Email,
                Cultivo            = solicitudContrato.Cultivo.ToUpper(),
                Domicilio          = agricultor.Direccion,
                Empresa            = empresa.Nombre.ToUpper(),
                EmpresaRazonSocial = empresa.RazonSocial,
                EmpresaRut         = empresa.Rut,
                FechaContrato      = DateTime.Now,
                GastosTransportes  = "PRODUCTOR",
                NumeroContrato     = "123-20",
                Predio             = solicitudContrato.Predio,
                Proveedor          = agricultor.Nombre,
                Representante      = agricultor.NombreRepresentate,
                Rut                = solicitudContrato.Rut,
                RutRepresentante   = agricultor.RutRepresentate,
                Superficie         = solicitudContrato.Hectareas,
                Temporada          = temporada.NombreCorto,
                Toneladas          = solicitudContrato.ToneladasTotales,
                Ubicacion          = solicitudContrato.ComunaOrigen,
                //Variedad           = solicitudContrato.Variedad
            };

            html = html.Replace("[[AñoContrato]]"       , acuerdoComercial.AñoContrato.ToString());
            html = html.Replace("[[Comuna]]"            , acuerdoComercial.Comuna);
            html = html.Replace("[[Correo]]"            , acuerdoComercial.Correo);
            html = html.Replace("[[Cultivo]]"           , acuerdoComercial.Cultivo);
            html = html.Replace("[[Domicilio]]"         , acuerdoComercial.Domicilio);
            html = html.Replace("[[Empresa]]"           , acuerdoComercial.Empresa);
            html = html.Replace("[[EmpresaRazonSocial]]", acuerdoComercial.EmpresaRazonSocial);
            html = html.Replace("[[EmpresaRut]]"        , acuerdoComercial.EmpresaRut);
            html = html.Replace("[[FechaContrato]]"     , string.Format("{0:dd} de {0:MMMM} de {0:yyyy}", acuerdoComercial.FechaContrato).ToString(cultureInfo));
            html = html.Replace("[[GastosTransportes]]" , acuerdoComercial.GastosTransportes);
            html = html.Replace("[[NumeroContrato]]"    , acuerdoComercial.NumeroContrato);
            html = html.Replace("[[Predio]]"            , acuerdoComercial.Predio);
            html = html.Replace("[[Proveedor]]"         , acuerdoComercial.Proveedor);
            html = html.Replace("[[Representante]]"     , acuerdoComercial.Representante);
            html = html.Replace("[[Rut]]"               , acuerdoComercial.Rut);
            html = html.Replace("[[RutRepresentante]]"  , acuerdoComercial.RutRepresentante);
            html = html.Replace("[[Superficie]]"        , acuerdoComercial.Superficie.ToString());
            html = html.Replace("[[Temporada]]"         , acuerdoComercial.Temporada);
            html = html.Replace("[[Toneladas]]"         , acuerdoComercial.Toneladas.ToString());
            html = html.Replace("[[Ubicacion]]"         , acuerdoComercial.Ubicacion);
            html = html.Replace("[[Variedad]]"          , acuerdoComercial.Variedad);

            return Content(html);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}