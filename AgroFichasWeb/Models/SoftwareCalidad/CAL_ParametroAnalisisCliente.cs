using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class CAL_ParametroAnalisisCliente
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public string GetCssStyleMostrarParametroAnalisis(bool value)
        {
            if (value)
            {
                return "cal-Mostrar";
            }
            else
            {
                return "cal-NoMostrar";
            }
        }

        public string GetCssStyleRequerido(bool value)
        {
            if (value)
            {
                return "cal-Requerido";
            }
            else
            {
                return "cal-NoRequerido";
            }
        }
        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones

        #endregion
    }
}