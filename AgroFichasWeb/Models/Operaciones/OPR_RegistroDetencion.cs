using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class OPR_RegistroDetencion
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdArea == 0)
                yield return new RuleViolation("El área es requerida", "IdArea");

            if (this.IdLineaProduccion == 0)
                yield return new RuleViolation("La línea de producción es requerida", "IdLineaProduccion");

            if (this.IdEquipo == 0 || this.IdEquipo == null)
                yield return new RuleViolation("El equipo es requerido", "IdEquipo");

            if (this.IdTipoDetencion == 0)
                yield return new RuleViolation("El tipo de detención es requerido", "IdTipoDetencion");

            if (this.IdCausaDetencion == 0)
                yield return new RuleViolation("La causa de la detención es requerida", "IdLineaProduccion");

            if (this.HoraDetencion == null)
                yield return new RuleViolation("La hora de detención es requerida", "HoraDetencion");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}