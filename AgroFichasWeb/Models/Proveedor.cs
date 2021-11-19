using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Proveedor
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El Nombre es requerido", "Nombre");

            if (String.IsNullOrEmpty(this.Email))
                yield return new RuleViolation("El Email es requerido", "Email");
            
            if (String.IsNullOrEmpty(this.Telefono1))
                yield return new RuleViolation("El Teléfono es requerido", "Telefono1");
            
            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}