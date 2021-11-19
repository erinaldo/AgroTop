using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Moneda
    {
        string formato2 = "";
        public string Formato2
        {
            get
            {
                if (formato2 == "")
                    formato2 = this.Formato.Replace(this.Simbolo, "").Trim();

                return formato2;
            }
        }

        public bool ValidarDecimales(decimal? valor)
        {
            if (!valor.HasValue)
                return true;

            return valor == Math.Round(valor.Value, this.Decimales);
        }

        public bool ValidarDecimalesPrecio(decimal? valor)
        {
            if (!valor.HasValue)
                return true;

            return valor == Math.Round(valor.Value, this.DecimalesPrecio);
        }

        public static Moneda MonedaDefault(AgroFichasDBDataContext dc)
        {
            return dc.Moneda.Single(m => m.IdMoneda == 1);
        }
    }
}