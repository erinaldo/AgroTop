using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class PrecioIngreso
    {
        public decimal Total
        {
            get 
            {
                return TotalMonedaConvenio(this.Cantidad, this.PrecioUnidad, this.SobrePrecioTotal, this.DescuentoTotal, this.Moneda);
            }
        }

        public static decimal TotalMonedaConvenio(int cantidad, decimal precioUnidad, decimal sobrePrecioTotal, decimal descuentoTotal, Moneda moneda)
        {
            return Math.Round(cantidad * precioUnidad + sobrePrecioTotal - descuentoTotal, moneda.Decimales);
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

        public decimal TotalNetoSinBono()
        {
            var valor = Math.Abs(this.BonoTotal);
            var factor = (this.BonoUnidad > 0 ? -1 : 1);
            return (this.TotalNeto ?? 0M) + (valor * factor);
        }

        //public decimal TotalMonedaPago()
        //{
        //    //if (IdMonedaPago() == 1) //peso
        //    //    return this.TotalNeto ?? 0M;
        //    //else
        //        return this.PrecioUnidad * this.Cantidad;
        //}
    }
}