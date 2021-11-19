using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class CAL_GetTestAnalisisMetalPesadoResult
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public string Formatter(decimal ValidValue, string UM, string FormatString)
        {
            if (ValidValue != 0 && ValidValue != 999999)
            {
                if (UM == "%")
                    return (ValidValue / 100m).ToString(FormatString);
                else
                    return (ValidValue.ToString(FormatString));
            }
            else
                return "(No tiene)";
        }
        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones

        #endregion
    }
}