using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class ResponseSolicitudContratoVariedad
    {
        public int IdSolicitudContrato { get; set; }

        public int IdVariedad { get; set; }

        public string Variedad { get; set; }

        public int Hectareas { get; set; }

        public int Toneladas { get; set; }
    }
}