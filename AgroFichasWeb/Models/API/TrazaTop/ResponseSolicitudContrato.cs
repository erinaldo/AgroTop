using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class ResponseSolicitudContrato
    {
        public int IdSolicitudContrato { get; set; }

        public string Rut { get; set; }

        public string NombreProveedor { get; set; }

        public string Cultivo { get; set; }

        public int PrecioCierre { get; set; }

        public int ToneladasCierre { get; set; }

        public string TipoContrato { get; set; }

        public string ComunaOrigen { get; set; }

        public string SucursalEntrega { get; set; }

        public int Hectareas { get; set; }

        public int ToneladasTotales { get; set; }

        public string NombreAsesor { get; set; }

        public string EmailAsesor { get; set; }

        public string Predio { get; set; }

        public bool VerificadoCRM { get; set; }

        public bool VerificadoFichas { get; set; }

        public bool ContratoCreado { get; set; }

        public bool CierreCreado { get; set; }

        public bool PDFCreado { get; set; }

        public int? IdTemporada { get; set; }

        public string Variedad { get; set; }

        public List<ResponseSolicitudContratoVariedad> ResponseSolicitudContratoVariedad { get; set; }
    }
}