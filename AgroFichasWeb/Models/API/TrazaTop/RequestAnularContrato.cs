using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class RequestAnularContrato
    {
        [Required(ErrorMessage = "El IdSolicitudContrato es requerido")]
        public int IdSolicitudContrato { get; set; }
    }
}