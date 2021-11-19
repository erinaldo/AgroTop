using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectProducto
    {
        public int IdProducto { get; set; }

        public string Nombre { get; set; }

        public int IdDetalleOrdenProduccion { get; set; }

        public SelectFamiliaProducto FamiliaProducto { get; set; }
    }
}