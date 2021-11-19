using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class RequestAgricultor
    {
        [Required(ErrorMessage = "El IdAgricultor es requerido")]
        public int IdAgricultor { get; set; }

        [Required(ErrorMessage = "El Rut es requerido")]
        public string Rut { get; set; }

        [Required(ErrorMessage = "El Nombre es requerido")]
        public string Nombre { get; set; }

        public string RutRepresentate { get; set; }

        public string NombreRepresentate { get; set; }

        public int? IdRegion { get; set; }

        public int? IdProvincia { get; set; }

        public int? IdComuna { get; set; }

        public string Direccion { get; set; }

        [Required(ErrorMessage = "El Email es requerido")]
        public string Email { get; set; }

        public string RolAvaluo { get; set; }

        public int? InscripcionFS { get; set; }

        public int? InscripcionNum { get; set; }

        public DateTime? InscripcionAno { get; set; }

        public bool? CoberturaSeguro { get; set; }

        public int? IdTituloExplotacion { get; set; }

        public int IdSolicitudContrato { get; set; }
    }
}