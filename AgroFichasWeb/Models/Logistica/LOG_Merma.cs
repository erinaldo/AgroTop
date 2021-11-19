using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class LOG_Merma
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdCultivo == 0)
                yield return new RuleViolation("El cultivo es requerido", "IdCultivo");
            if (this.Precio == 0)
                yield return new RuleViolation("El precio es requerido", "Precio");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}