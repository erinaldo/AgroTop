using System;
using System.Collections.Generic;
using System.Linq;

namespace Agrotop.Form.Models
{
    public partial class IncorporacionCamiongo
    {
        //private AgrotopDBDataContext dc = new AgrotopDBDataContext();

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {

            if (string.IsNullOrEmpty(this.RUT))
                yield return new RuleViolation("El RUT es requerido", "rut");

            if (string.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El nombre es requerido", "name");

            if (string.IsNullOrEmpty(this.Email))
                yield return new RuleViolation("El Email es requerido", "email");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}