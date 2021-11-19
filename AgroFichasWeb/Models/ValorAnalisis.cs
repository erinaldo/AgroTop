using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class ValorAnalisis
    {
        public static bool IsSelectList(int idParametroAnalisis)
        {
            int[] ids = { 11, 14, 15, 16, 17, 18, 19, 34, 35, 36, 42, 43, 44, 45, 46, 47 };
            return ids.Contains(idParametroAnalisis);
        }

        public override string ToString()
        {
            return ToString(this.ParametroAnalisis);
        }

        public string ToString(ParametroAnalisis pa)
        {
            return ToString(pa, this.Valor, true);
        }

        public static string ToString(ParametroAnalisis pa, decimal? valor, bool showUM)
        {
            if (valor.HasValue)
            {
                if (ValorAnalisis.IsSelectList(pa.IdParametroAnalisis))
                    return valor.Value == 1 ? "Aceptado" : "Rechazado";
                else
                    return valor.Value.ToString(pa.FormatString) + (showUM ? pa.UM : "");
            }
            else
            {
                return "";
            }
        }

        public static string ToString(ValorAnalisisNulo va, bool showUM)
        {
            return ToString(va.ParametroAnalisis, va.Valor, showUM);
        }
    }
}