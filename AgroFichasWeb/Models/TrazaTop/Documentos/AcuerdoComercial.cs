using System;
using System.Linq;

namespace AgroFichasWeb.Models.TrazaTop.Documentos
{
    public class AcuerdoComercial : DocumentoContrato
    {
        public AcuerdoComercial(int idSolicitudContrato)
        {
            this.IdSolicitudContrato = idSolicitudContrato;

            // Descargo de responsabilidad:
            // Como en Force Manager no se discrima el Tipo de Moneda se asume siempre que será Peso CLP.

            SolicitudContrato solicitudContrato = context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == idSolicitudContrato);
            Models.Contrato contrato            = context.Contrato.Single(c => c.IdContrato == solicitudContrato.IdContrato);
            Agricultor agricultor               = context.Agricultor.Single(a => a.IdAgricultor == solicitudContrato.IdAgricultor);
            Empresa empresa                     = context.Empresa.Single(e => e.IdEmpresa == solicitudContrato.IdEmpresa);
            Temporada temporada                 = context.Temporada.Single(t => t.IdTemporada == solicitudContrato.IdTemporada);

            this.AñoContrato        = DateTime.Now.Year;
            this.Comuna             = solicitudContrato.ComunaOrigen;
            this.Correo             = agricultor.Email;
            this.Cultivo            = solicitudContrato.Cultivo.ToUpper();
            this.Domicilio          = agricultor.Direccion;
            this.Empresa            = empresa.Nombre.ToUpper();
            this.EmpresaRazonSocial = empresa.RazonSocial;
            this.EmpresaRut         = empresa.Rut;
            this.FechaContrato      = DateTime.Now;
            this.GastosTransportePara = solicitudContrato.GastosTransportePara;
            this.NumeroContrato     = contrato.NumeroContrato;
            this.Predio             = solicitudContrato.Predio;
            this.Proveedor          = agricultor.Nombre;
            this.Representante      = agricultor.NombreRepresentate;
            this.Rut                = solicitudContrato.Rut;
            this.RutRepresentante   = agricultor.RutRepresentate;
            this.Superficie         = solicitudContrato.Hectareas;
            this.Temporada          = temporada.NombreCorto;
            this.Toneladas          = solicitudContrato.ToneladasTotales;
            this.Ubicacion          = solicitudContrato.ComunaOrigen;
            this.Variedad           = GetSolicitudContratoVariedad();
            this.Logotipo           = empresa.Logotipo;
        }

        public int AñoContrato { get; set; }

        public string Comuna { get; set; }

        public string Correo { get; set; }

        public string Cultivo { get; set; }

        public string Domicilio { get; set; }

        public string Empresa { get; set; }

        public string EmpresaRazonSocial { get; set; }

        public string EmpresaRut { get; set; }

        public DateTime FechaContrato { get; set; }

        public string GastosTransportePara { get; set; }

        public string Logotipo { get; set; }

        public string NumeroContrato { get; set; }

        public string Predio { get; set; }

        public string Proveedor { get; set; }

        public string Representante { get; set; }

        public string Rut { get; set; }

        public string RutRepresentante { get; set; }

        public int Superficie { get; set; }

        public string Temporada { get; set; }

        public int Toneladas { get; set; }

        public string Ubicacion { get; set; }

        public string Variedad { get; set; }

        [Obsolete]
        public string CrearPDF(string userIns, DateTime fechaHoraIns, string ipIns)
        {
            string html = GetPlanillaHTML();

            html = html.Replace("[[Logotipo]]"            , string.Format("<img src=\"{0}\" alt=\"{1}\" width=\"100\">", this.Logotipo, this.Empresa));
            html = html.Replace("[[AñoContrato]]"         , this.AñoContrato.ToString());
            html = html.Replace("[[Comuna]]"              , this.Comuna);
            html = html.Replace("[[Correo]]"              , this.Correo);
            html = html.Replace("[[Cultivo]]"             , this.Cultivo);
            html = html.Replace("[[Domicilio]]"           , this.Domicilio);
            html = html.Replace("[[Empresa]]"             , this.Empresa);
            html = html.Replace("[[EmpresaRazonSocial]]"  , this.EmpresaRazonSocial);
            html = html.Replace("[[EmpresaRut]]"          , this.EmpresaRut);
            html = html.Replace("[[FechaContrato]]"       , string.Format(new System.Globalization.CultureInfo("es-CL"), "{0:dd} de {0:MMMM} de {0:yyyy}", this.FechaContrato));
            html = html.Replace("[[GastosTransportePara]]", this.GastosTransportePara);
            html = html.Replace("[[NumeroContrato]]"      , this.NumeroContrato);
            html = html.Replace("[[Predio]]"              , this.Predio);
            html = html.Replace("[[Proveedor]]"           , this.Proveedor);
            html = html.Replace("[[Representante]]"       , this.Representante);
            html = html.Replace("[[Rut]]"                 , this.Rut);
            html = html.Replace("[[RutRepresentante]]"    , this.RutRepresentante);
            html = html.Replace("[[Superficie]]"          , this.Superficie.ToString());
            html = html.Replace("[[Temporada]]"           , this.Temporada);
            html = html.Replace("[[Toneladas]]"           , this.Toneladas.ToString());
            html = html.Replace("[[Ubicacion]]"           , this.Ubicacion);
            html = html.Replace("[[Variedad]]"            , this.Variedad);

            return CrearDoctoContrato(html, 1, 0, userIns, fechaHoraIns, ipIns);
        }

        public string CrearHTML()
        {
            string html = GetPlanillaHTML();

            html = html.Replace("[[Logotipo]]"            , string.Format("<img src=\"{0}\" alt=\"{1}\" width=\"100\">", this.Logotipo, this.Empresa));
            html = html.Replace("[[AñoContrato]]"         , this.AñoContrato.ToString());
            html = html.Replace("[[Comuna]]"              , this.Comuna);
            html = html.Replace("[[Correo]]"              , this.Correo);
            html = html.Replace("[[Cultivo]]"             , this.Cultivo);
            html = html.Replace("[[Domicilio]]"           , this.Domicilio);
            html = html.Replace("[[Empresa]]"             , this.Empresa);
            html = html.Replace("[[EmpresaRazonSocial]]"  , this.EmpresaRazonSocial);
            html = html.Replace("[[EmpresaRut]]"          , this.EmpresaRut);
            html = html.Replace("[[FechaContrato]]"       , string.Format(new System.Globalization.CultureInfo("es-CL"), "{0:dd} de {0:MMMM} de {0:yyyy}", this.FechaContrato));
            html = html.Replace("[[GastosTransportePara]]", this.GastosTransportePara);
            html = html.Replace("[[NumeroContrato]]"      , this.NumeroContrato);
            html = html.Replace("[[Predio]]"              , this.Predio);
            html = html.Replace("[[Proveedor]]"           , this.Proveedor);
            html = html.Replace("[[Representante]]"       , this.Representante);
            html = html.Replace("[[Rut]]"                 , this.Rut);
            html = html.Replace("[[RutRepresentante]]"    , this.RutRepresentante);
            html = html.Replace("[[Superficie]]"          , this.Superficie.ToString());
            html = html.Replace("[[Temporada]]"           , this.Temporada);
            html = html.Replace("[[Toneladas]]"           , this.Toneladas.ToString());
            html = html.Replace("[[Ubicacion]]"           , this.Ubicacion);
            html = html.Replace("[[Variedad]]"            , this.Variedad);

            return html;
        }
    }
}