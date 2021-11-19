using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroFichasWeb.Models
{
    public partial class LOG_Transportista
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.RUT))
                yield return new RuleViolation("El RUT es requerido", "RUT");
            if (Rut.ValidaRut(this.RUT) == false)
                yield return new RuleViolation("El RUT es inválido", "RUT");
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El nombre es requerido", "Nombre");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}
