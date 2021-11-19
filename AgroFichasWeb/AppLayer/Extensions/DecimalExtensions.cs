using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer.Extensions
{
    public  static class DecimalExtensions
    {
        public static decimal? DivideByInt(this decimal? numerador, int? denominador)
        {
            decimal num = numerador ?? 0;
            int den = denominador ?? 0;
            return num.DivideByInt(den);
        }

        public static decimal? DivideByInt(this decimal numerador, int denominador)
        {
            if (denominador != 0)
                return numerador / denominador;
            else
                return null;
        }
    }
}