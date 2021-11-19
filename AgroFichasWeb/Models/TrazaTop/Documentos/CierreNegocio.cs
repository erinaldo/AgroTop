using System;
using System.Linq;

namespace AgroFichasWeb.Models.TrazaTop.Documentos
{
    public class CierreNegocio : DocumentoContrato
    {
        public CierreNegocio(int idSolicitudContrato)
        {
            this.IdSolicitudContrato = idSolicitudContrato;

            // Descargo de responsabilidad:
            // Como en Force Manager no se discrima el Tipo de Moneda se asume siempre que será Peso CLP.

            SolicitudContrato solicitudContrato = context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == idSolicitudContrato);
            Models.Contrato contrato            = context.Contrato.Single(c => c.IdContrato == solicitudContrato.IdContrato);
            Agricultor agricultor               = context.Agricultor.Single(a => a.IdAgricultor == solicitudContrato.IdAgricultor);
            CuentaBancaria cuentaBancaria       = context.CuentaBancaria.SingleOrDefault(cb => cb.IdAgricultor == agricultor.IdAgricultor);
            Moneda moneda                       = context.Moneda.Single(m => m.IdMoneda == 1);
            Empresa empresa                     = context.Empresa.Single(e => e.IdEmpresa == solicitudContrato.IdEmpresa);

            this.NumeroCierre       = contrato.NumeroContrato;
            this.FechaCierre        = DateTime.Now;
            this.Proveedor          = agricultor.Nombre;
            this.Representante      = agricultor.NombreRepresentate;
            this.RutRepresentante   = agricultor.RutRepresentate;
            this.Correo             = agricultor.Email;
            this.Telefono           = agricultor.Fono1;
            this.DatosBancarios     = GetCuentaBancaria(cuentaBancaria);
            this.Ciudad             = GetComuna(agricultor.Comuna);
            this.Moneda             = moneda.Nombre;
            this.Unidad             = moneda.Simbolo;
            this.Cultivo            = solicitudContrato.Cultivo;
            this.Rut                = solicitudContrato.Rut;
            this.Comuna             = solicitudContrato.ComunaOrigen;
            this.Predio             = solicitudContrato.Predio;
            this.Superficie         = string.Format("{0}", solicitudContrato.Hectareas);
            this.Toneladas          = string.Format("{0}", solicitudContrato.ToneladasTotales);
            this.PrecioFichas       = string.Format("{0}", solicitudContrato.PrecioCierre);
            this.LugarEntrega       = solicitudContrato.SucursalEntrega;
            this.Asesor             = solicitudContrato.NombreAsesor;
            this.Logotipo           = empresa.Logotipo;
            this.EmpresaRazonSocial = empresa.RazonSocial;
            this.EmpresaRut         = empresa.Rut;
            this.Empresa            = empresa.Nombre;
        }

        public string Asesor { get; set; }

        public string Ciudad { get; set; }

        public string Comuna { get; set; }

        public string Correo { get; set; }

        public string Cultivo { get; set; }

        public string DatosBancarios { get; set; }

        public string Empresa { get; set; }

        public string EmpresaRazonSocial { get; set; }

        public string EmpresaRut { get; set; }

        public DateTime FechaCierre { get; set; }

        public string Logotipo { get; set; }

        public string LugarEntrega { get; set; }

        public string Moneda { get; set; }

        public string NumeroCierre { get; set; }

        public string PrecioFichas { get; set; }

        public string Predio { get; set; }

        public string Proveedor { get; set; }

        public string Representante { get; set; }

        public string Rut { get; set; }

        public string RutRepresentante { get; set; }

        public string Superficie { get; set; }

        public string Telefono { get; set; }

        public string Toneladas { get; set; }

        public string Unidad { get; set; }

        [Obsolete]
        public string CrearPDF(string userIns, DateTime fechaHoraIns, string ipIns)
        {
            string html = GetPlanillaHTML();

            html = html.Replace("[[NumeroCierre]]"      , this.NumeroCierre);
            html = html.Replace("[[FechaCierre]]"       , string.Format(new System.Globalization.CultureInfo("es-CL"), "{0:dd} de {0:MMMM} de {0:yyyy}", this.FechaCierre));
            html = html.Replace("[[Proveedor]]"         , this.Proveedor);
            html = html.Replace("[[Representante]]"     , this.Representante);
            html = html.Replace("[[RutRepresentante]]"  , this.RutRepresentante);
            html = html.Replace("[[Correo]]"            , this.Correo);
            html = html.Replace("[[Telefono]]"          , this.Telefono);
            html = html.Replace("[[DatosBancarios]]"    , this.DatosBancarios);
            html = html.Replace("[[Ciudad]]"            , this.Ciudad);
            html = html.Replace("[[Moneda]]"            , this.Moneda);
            html = html.Replace("[[Unidad]]"            , this.Unidad);
            html = html.Replace("[[Cultivo]]"           , this.Cultivo.ToUpper());
            html = html.Replace("[[Rut]]"               , this.Rut);
            html = html.Replace("[[Comuna]]"            , this.Comuna);
            html = html.Replace("[[Predio]]"            , this.Predio);
            html = html.Replace("[[Superficie]]"        , this.Superficie);
            html = html.Replace("[[Toneladas]]"         , this.Toneladas);
            html = html.Replace("[[PrecioFichas]]"      , this.PrecioFichas);
            html = html.Replace("[[LugarEntrega]]"      , this.LugarEntrega);
            html = html.Replace("[[Asesor]]"            , this.Asesor);
            html = html.Replace("[[Logotipo]]"          , string.Format("<img src=\"{0}\" alt=\"{1}\" width=\"100\">", this.Logotipo, this.Empresa));
            html = html.Replace("[[EmpresaRazonSocial]]", this.EmpresaRazonSocial);
            html = html.Replace("[[EmpresaRut]]"        , this.EmpresaRut);
            html = html.Replace("[[Empresa]]"           , this.Empresa);

            return CrearDoctoContrato(html, 1, 0, userIns, fechaHoraIns, ipIns);
        }

        public string CrearHTML()
        {
            string html = GetPlanillaHTML();

            html = html.Replace("[[NumeroCierre]]"      , this.NumeroCierre);
            html = html.Replace("[[FechaCierre]]"       , string.Format(new System.Globalization.CultureInfo("es-CL"), "{0:dd} de {0:MMMM} de {0:yyyy}", this.FechaCierre));
            html = html.Replace("[[Proveedor]]"         , this.Proveedor);
            html = html.Replace("[[Representante]]"     , this.Representante);
            html = html.Replace("[[RutRepresentante]]"  , this.RutRepresentante);
            html = html.Replace("[[Correo]]"            , this.Correo);
            html = html.Replace("[[Telefono]]"          , this.Telefono);
            html = html.Replace("[[DatosBancarios]]"    , this.DatosBancarios);
            html = html.Replace("[[Ciudad]]"            , this.Ciudad);
            html = html.Replace("[[Moneda]]"            , this.Moneda);
            html = html.Replace("[[Unidad]]"            , this.Unidad);
            html = html.Replace("[[Cultivo]]"           , this.Cultivo.ToUpper());
            html = html.Replace("[[Rut]]"               , this.Rut);
            html = html.Replace("[[Comuna]]"            , this.Comuna);
            html = html.Replace("[[Predio]]"            , this.Predio);
            html = html.Replace("[[Superficie]]"        , this.Superficie);
            html = html.Replace("[[Toneladas]]"         , this.Toneladas);
            html = html.Replace("[[PrecioFichas]]"      , this.PrecioFichas);
            html = html.Replace("[[LugarEntrega]]"      , this.LugarEntrega);
            html = html.Replace("[[Asesor]]"            , this.Asesor);
            html = html.Replace("[[Logotipo]]"          , string.Format("<img src=\"{0}\" alt=\"{1}\" width=\"100\">", this.Logotipo, this.Empresa));
            html = html.Replace("[[EmpresaRazonSocial]]", this.EmpresaRazonSocial);
            html = html.Replace("[[EmpresaRut]]"        , this.EmpresaRut);
            html = html.Replace("[[Empresa]]"           , this.Empresa);

            return html;
        }
    }
}