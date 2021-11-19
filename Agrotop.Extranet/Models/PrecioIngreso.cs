using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public partial class PrecioIngreso
    {
        public decimal Total
        {
            get 
            {
                return TotalMonedaConvenio(this.Cantidad, this.PrecioUnidad);
            }
        }

        public static decimal TotalMonedaConvenio(int cantidad, decimal precioUnidad)
        {
            return Math.Round(cantidad * precioUnidad, 5);
        }

        public int IdMonedaPago()
        {
            //Si el inreso se liquida en dólares y el precio está en dólares => liquidamos en dólares
            if (this.IdMoneda == 2 && this.ProcesoIngreso.IdMonedaLiquidacion == 2)
                return 2;
            else
                return 1;
        }

        public decimal TotalMonedaPago()
        {
            if (IdMonedaPago() == 1) //peso
                return this.TotalNeto ?? 0M;
            else
                return this.PrecioUnidad * this.Cantidad;
        }

    }
}