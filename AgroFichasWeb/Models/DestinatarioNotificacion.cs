using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public class DestinatarioNotificacion
    {
        public string Rol { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public bool Optional { get; set; }
        public bool Seleccionado { get; set; }
    }
}