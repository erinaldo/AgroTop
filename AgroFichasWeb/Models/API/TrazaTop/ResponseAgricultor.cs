using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class ResponseAgricultor
    {
        public int IdAgricultor { get; set; }

        public string Rut { get; set; }

        public string Nombre { get; set; }

        public string RutRepresentate { get; set; }

        public string NombreRepresentate { get; set; }

        public int? IdRegion { get; set; }

        public string NombreRegion { get; set; }

        public int? IdProvincia { get; set; }

        public string NombreProvincia { get; set; }

        public int? IdComuna { get; set; }

        public string NombreComuna { get; set; }

        public string Direccion { get; set; }

        public string Email { get; set; }

        public string RolAvaluo { get; set; }

        public int? InscripcionFS { get; set; }

        public int? InscripcionNum { get; set; }

        public DateTime? InscripcionAno { get; set; }

        public bool? CoberturaSeguro { get; set; }

        public int? IdTituloExplotacion { get; set; }

        public string NombreTituloExplotacion { get; set; }

        public int IdSolicitudContrato { get; set; }

        public string EstadoSolicitudContrato { get; set; }

        public string ColorSolicitudContrato { get; set; }
    }
}