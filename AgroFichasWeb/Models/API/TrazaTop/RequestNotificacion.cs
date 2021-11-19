using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class RequestNotificacion
    {
        public string Mensaje { get; set; }

        public int IdSolicitudContrato { get; set; }

        public string Tipo { get; set; }
    }
}