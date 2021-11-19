using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class OPR_RegistroGeneracionMaxisacos
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.Despedradora < 0)
                yield return new RuleViolation("La despedradora no puede ser negativo", "Despedradora");

            if (this.Desperdicio < 0)
                yield return new RuleViolation("El desperdicio no puede ser negativo", "Desperdicio");

            if (this.Sortex < 0)
                yield return new RuleViolation("El sortex no puede ser negativo", "Sortex");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}