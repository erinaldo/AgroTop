using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class RequestVerificadoFichas
    {
        [Required(ErrorMessage = "El IdSolicitudContrato es requerido")]
        public int IdSolicitudContrato { get; set; }

        [Required(ErrorMessage = "El VerificadoFichas es requerido")]
        public bool VerificadoFichas { get; set; }
    }
}