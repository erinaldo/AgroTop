using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.SoftwareCalidad
{
    public class CALUtils
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public static string GetGuid(int IdTurno, int IdDetalleOrdenProduccion)
        {
            return string.Format("Turno{0}{1}", IdDetalleOrdenProduccion, IdTurno);
        }
        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones

        #endregion
    }
}