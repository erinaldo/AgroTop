using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class OPR_Silo
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdTipoSilo == 0)
                yield return new RuleViolation("El tipo de silo es requerido", "IdTipoSilo");

            if (String.IsNullOrWhiteSpace(this.Descripcion))
                yield return new RuleViolation("La descripción es requerida", "Descripcion");

            if (this.AlturaBase == 0 && this.IdTipoSilo == 1)
                yield return new RuleViolation("La altura base es requerida", "AlturaBase");

            if (this.AlturaCono == 0)
                yield return new RuleViolation("La altura cono es requerida", "AlturaCono");

            if (this.IdDensidad == 0)
                yield return new RuleViolation("La densidad es requerida", "IdDensidad");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}