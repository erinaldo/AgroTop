using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Predio
    {
        public void SetDefaults()
        {

        }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El Nombre es requerido", "Nombre");

            if (this.IdComuna <= 0)
                yield return new RuleViolation("La Comuna es requerida", "IdComuna");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {

            if (!IsValid)
            {
                var msg = "";
                foreach (var rv in GetRuleViolations())
                    msg += rv.ErrorMessage + " \n";

                throw new ApplicationException("Rule violations prevent saving: " + msg);
            }
        }

    }
}