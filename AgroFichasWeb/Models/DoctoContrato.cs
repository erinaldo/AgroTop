using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class DoctoContrato
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdTipoDoctoContrato == 0)
                yield return new RuleViolation("El tipo docto es requerido", "IdTipoDoctoContrato");

            if (this.IdContrato == 0)
                yield return new RuleViolation("El contrato es requerido", "IdContrato");

            if (this.IdTipoDoctoContrato == 2 && this.Correlativo == 0)
                yield return new RuleViolation("El correlativo es requerido", "Correlativo");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}