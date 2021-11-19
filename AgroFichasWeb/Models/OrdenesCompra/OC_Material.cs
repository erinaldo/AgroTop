using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class OC_Material
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdProyecto == 0)
                yield return new RuleViolation("El proyecto es requerido", "IdProyecto");

            if (this.IdEmpresa == 0)
                yield return new RuleViolation("La empresa es requerida", "IdEmpresa");

            if (String.IsNullOrEmpty(this.CodigoMaterial))
                yield return new RuleViolation("El código es requerido", "CodigoMaterial");

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