using AgroFichasWeb.Models.API.TrazaTop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.TrazaTop
{
    public class MensajeTipo : RequestNotificacion
    {
        public string Nombre { get; set; }

        public string Email { get; set; }
    }
}