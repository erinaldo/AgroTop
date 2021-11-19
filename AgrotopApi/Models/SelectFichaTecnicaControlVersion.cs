using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectFichaTecnicaControlVersion
    {
        public int Version { get; set; }

        public string Item { get; set; }

        public string Cambios { get; set; }

        public string Motivo { get; set; }

        public string Solicitante { get; set; }

        public string Fecha { get; set; }
    }
}