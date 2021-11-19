using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class OPR_CapacidadLineaProduccion
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdLineaProduccion == 0)
                yield return new RuleViolation("La línea de producción es requerida", "IdLineaProduccion");

            if (this.Valor == 0)
                yield return new RuleViolation("El valor (kg/hr) es requerido", "Valor");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}