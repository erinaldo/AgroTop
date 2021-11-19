using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class vw_CAL_OrdenProduccionDespacho
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public string GetCssStyleAutorizado(bool? value)
        {
            if (value.HasValue)
            {
                if (value.Value)
                {
                    return "cal-Autorizado";
                }
                else
                {
                    return "cal-NoAutorizado";
                }
            }
            else
            {
                return "cal-NoAutorizado";
            }
        }
        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones

        #endregion
    }
}