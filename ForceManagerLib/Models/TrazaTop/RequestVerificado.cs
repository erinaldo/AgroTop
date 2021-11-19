using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ForceManagerLib.Models.TrazaTop
{
    public class RequestVerificado
    {
        public int IdSolicitudContrato { get; set; }

        public int IdEstado { get; set; }

        public string Nombre { get; set; }
    }
}