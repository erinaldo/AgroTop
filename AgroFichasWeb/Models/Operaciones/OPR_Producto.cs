using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class OPR_Producto
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdTipoProducto == 0)
                yield return new RuleViolation("El tipo de producto es requerido", "IdTipoProducto");

            if (String.IsNullOrEmpty(this.Descripcion))
                yield return new RuleViolation("La descripción es requerida", "Descripcion");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}