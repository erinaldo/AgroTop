using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class CAL_FTControlVersionMotivo
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (string.IsNullOrEmpty(this.Descripcion))
                yield return new RuleViolation("La descripción es requerida", "Descripcion");

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