using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class ConvenioPrecioItemTabla
    {
        public int GetCantidaUtilizada(AgroFichasDBDataContext dc, out int cantidadDisponible)
        {
            var utilizado = dc.PrecioIngreso.Where(x =>
                x.IdConvenioPrecio == this.IdConvenioPrecio &&
                x.ProcesoIngreso.IdSucursal == this.IdSucursal &&
                x.PrecioUnidad == this.PrecioUnidad).Sum(x => (int?)x.Cantidad) ?? 0;

            cantidadDisponible = this.Cantidad - utilizado;
            return utilizado;
        }


    }
}