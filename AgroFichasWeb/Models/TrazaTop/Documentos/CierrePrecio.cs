using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.TrazaTop.Documentos
{
    public class CierrePrecio : DocumentoContrato
    {
        public CierrePrecio(int idSolicitudContrato)
        {
            this.IdSolicitudContrato = idSolicitudContrato;

            // Descargo de responsabilidad:
            // Como en Force Manager no se discrima el Tipo de Moneda se asume siempre que será Peso CLP.

            SolicitudContrato solicitudContrato = context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == idSolicitudContrato);
            Models.Contrato contrato            = context.Contrato.Single(c => c.IdContrato == solicitudContrato.IdContrato);
            Agricultor agricultor               = context.Agricultor.Single(a => a.IdAgricultor == solicitudContrato.IdAgricultor);
            Empresa empresa                     = context.Empresa.Single(e => e.IdEmpresa == solicitudContrato.IdEmpresa);
            Temporada temporada                 = context.Temporada.Single(t => t.IdTemporada == solicitudContrato.IdTemporada);

            this.AñoContrato        = DateTime.Now;
            this.ComunaOrigen       = solicitudContrato.ComunaOrigen;
            this.EmpresaRazonSocial = empresa.RazonSocial;
            this.FechaContrato      = DateTime.Now;
            this.NumeroContrato     = contrato.NumeroContrato;
            this.PrecioCierre       = solicitudContrato.PrecioCierre;
            this.Proveedor          = agricultor.Nombre;
            this.Temporada          = temporada.NombreCorto;
            this.ToneladasCierre    = solicitudContrato.ToneladasCierre;
            this.ToneladasTotales   = solicitudContrato.ToneladasTotales;
        }

        public DateTime AñoContrato { get; set; }

        public string ComunaOrigen { get; set; }

        public string EmpresaRazonSocial { get; set; }

        public DateTime FechaContrato { get; set; }

        public string NumeroContrato { get; set; }

        public int PrecioCierre { get; set; }

        public string Proveedor { get; set; }

        public string Temporada { get; set; }

        public int ToneladasCierre { get; set; }

        public int ToneladasTotales { get; set; }

        [Obsolete]
        public string CrearPDF(string userIns, DateTime fechaHoraIns, string ipIns)
        {
            string html = GetPlanillaHTML(true);

            html = html.Replace("[[AñoContrato]]"       , GetAñoContrato(this.AñoContrato).ToString());
            html = html.Replace("[[ComunaOrigen]]"      , this.ComunaOrigen);
            html = html.Replace("[[EmpresaRazonSocial]]", this.EmpresaRazonSocial);
            html = html.Replace("[[FechaContrato]]"     , string.Format(new System.Globalization.CultureInfo("es-CL"), "{0:dd} de {0:MMMM} de {0:yyyy}", this.FechaContrato));
            html = html.Replace("[[NumeroContrato]]"    , this.NumeroContrato);
            html = html.Replace("[[PrecioCierre]]"      , this.PrecioCierre.ToString());
            html = html.Replace("[[Proveedor]]"         , this.Proveedor);
            html = html.Replace("[[Temporada]]"         , this.Temporada);
            html = html.Replace("[[ToneladasCierre]]"   , this.ToneladasCierre.ToString());
            html = html.Replace("[[ToneladasTotales]]"  , this.ToneladasTotales.ToString());

            return CrearDoctoContrato(html, 2, 1, userIns, fechaHoraIns, ipIns);
        }

        public string CrearHTML()
        {
            string html = GetPlanillaHTML(true);

            html = html.Replace("[[AñoContrato]]"       , GetAñoContrato(this.AñoContrato).ToString());
            html = html.Replace("[[ComunaOrigen]]"      , this.ComunaOrigen);
            html = html.Replace("[[EmpresaRazonSocial]]", this.EmpresaRazonSocial);
            html = html.Replace("[[FechaContrato]]"     , string.Format(new System.Globalization.CultureInfo("es-CL"), "{0:dd} de {0:MMMM} de {0:yyyy}", this.FechaContrato));
            html = html.Replace("[[NumeroContrato]]"    , this.NumeroContrato);
            html = html.Replace("[[PrecioCierre]]"      , this.PrecioCierre.ToString());
            html = html.Replace("[[Proveedor]]"         , this.Proveedor);
            html = html.Replace("[[Temporada]]"         , this.Temporada);
            html = html.Replace("[[ToneladasCierre]]"   , this.ToneladasCierre.ToString());
            html = html.Replace("[[ToneladasTotales]]"  , this.ToneladasTotales.ToString());

            return html;
        }
    }
}