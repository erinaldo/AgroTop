using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models.SoftwareCalidad
{
    public class CALAnalisisPeriodico
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int IdCliente { get; set; }
        public int IdOrdenProduccion { get; set; }
        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetClientes(int? IdCliente)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.vw_CAL_OrdenProduccionCliente
                                                     orderby X.RazonSocial
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCliente == IdCliente && IdCliente != null),
                                                         Text = X.RazonSocial,
                                                         Value = X.IdCliente.ToString()
                                                     };
            return selectList;
        }
        #endregion
        #region 5. Validaciones

        #endregion
    }
}