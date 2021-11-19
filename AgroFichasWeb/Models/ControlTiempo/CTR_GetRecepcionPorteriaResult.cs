using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.ControlTiempo
{
    public partial class CTR_GetRecepcionPorteriaResult
    {
        public string GetCssStyleBloqueado(bool value)
        {
            if (value)
                return "ct-Bloqueado";
            else
                return "";
        }
    }
}