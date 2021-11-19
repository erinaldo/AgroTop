using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class RequestContrato
    {
        [Required(ErrorMessage = "El IdEmpresa es requerido")]
        public int IdEmpresa { get; set; }

        [Required(ErrorMessage = "El IdGastosTransportePara es requerido")]
        public int IdGastosTransportePara { get; set; }

        [Required(ErrorMessage = "El NumeroContrato es requerido")]
        public string NumeroContrato { get; set; }

        [Required(ErrorMessage = "El IdSolicitudContrato es requerido")]
        public int IdSolicitudContrato { get; set; }
    }
}