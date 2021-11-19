using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class Barco
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetCarriers(int? IdCarrier)
        {
            IEnumerable<SelectListItem> selectList = (from X in dc.Carrier
                                                      where X.Habilitado == true
                                                      orderby X.Nombre
                                                      select new SelectListItem
                                                      {
                                                          Selected = (X.IdCarrier == IdCarrier && IdCarrier != null),
                                                          Text = X.Nombre,
                                                          Value = X.IdCarrier.ToString()
                                                      });
            return selectList;
        }
        #endregion
        #region 5. Validaciones
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El nombre es requerido", "Nombre");
            if (this.IdCarrier == 0)
                yield return new RuleViolation("El carrier es requerido", "IdCarrier");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
    }
}