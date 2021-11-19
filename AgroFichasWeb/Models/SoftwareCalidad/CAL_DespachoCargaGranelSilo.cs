using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class CAL_DespachoCargaGranelSilo
    {
        #region 1. DataContexts
        private OperacionesDBDataContext dcOperaciones = new OperacionesDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public string GetSilo(int? IdSilo)
        {
            OPR_Silo oPR_Silo = (from X in dcOperaciones.OPR_Silo
                                 where X.IdSilo == IdSilo.Value
                                 && X.Habilitado == true
                                 select X).Single();

            return oPR_Silo.Descripcion.ToUpper();
        }
        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones

        #endregion
    }
}