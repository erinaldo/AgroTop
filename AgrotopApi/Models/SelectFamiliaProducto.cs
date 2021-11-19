using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectFamiliaProducto
    {
        public int IdFamiliaProducto { get; set; }

        public string Nombre { get; set; }

        public int IdEspesor { get; set; }

        public string Espesor { get; set; }
    }
}