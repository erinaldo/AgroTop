using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class OPR_RegistroEnvasado
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdProducto == 0)
                yield return new RuleViolation("El producto es requerido", "IdProducto");

            if (this.IdCliente == 0)
                yield return new RuleViolation("La cliente es requerido", "IdCliente");

            if (String.IsNullOrEmpty(this.Lote))
                yield return new RuleViolation("El Lote es requerido", "Lote");

            if (this.EnvasadoSac < 0)
                yield return new RuleViolation("El envasado (sac) no puede ser negativo", "EnvasadoSac");

            if (this.EnvasadoSac < 0)
                yield return new RuleViolation("El peso no puede ser negativo", "Peso");

            if (this.Retenido < 0)
                yield return new RuleViolation("Retenido no puede ser negativo", "Retenido");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}