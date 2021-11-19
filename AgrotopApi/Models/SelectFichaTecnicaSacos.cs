using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectFichaTecnicaSacos
    {
        public int IdSaco { get; set; }

        public string Nombre { get; set; }

        public decimal Peso { get; set; }

        public string Descripcion { get; set; }

        public string ColorHilo { get; set; }
    }
}