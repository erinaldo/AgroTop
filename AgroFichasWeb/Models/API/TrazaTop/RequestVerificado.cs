using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class RequestVerificado
    {
        [Required(ErrorMessage = "El IdSolicitudContrato es requerido")]
        public int IdSolicitudContrato { get; set; }

        [Required(ErrorMessage = "El IdEstado es requerido")]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "El Estado es requerido")]
        public string Nombre { get; set; }
    }
}