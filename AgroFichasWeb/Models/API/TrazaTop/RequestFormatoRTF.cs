using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class RequestFormatoRTF
    {
        [Required(ErrorMessage = "El Nombre del Documento es requerido")]
        public string DocumentoN { get; set; }

        [Required(ErrorMessage = "La Ruta del Documento es requerida")]
        public string DocumentoR { get; set; }

        [Required(ErrorMessage = "La Observación es requerida")]
        public string Observacion { get; set; }

        [Required(ErrorMessage = "El IdTemporada es requerido")]
        public int IdTemporada { get; set; }

        public int? IdTipoContrato { get; set; }

        public int? IdCultivo { get; set; }

        public string UserIns { get; set; }

        public DateTime FechaHoraIns { get; set; }

        public string IpIns { get; set; }

        public string UserUpd { get; set; }

        public DateTime? FechaHoraUpd { get; set; }

        public string IpUpd { get; set; }
    }
}