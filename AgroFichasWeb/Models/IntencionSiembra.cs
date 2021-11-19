using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class IntencionSiembra
    {
        public void SetDefaults()
        {
            if (this.Observaciones == null)
                this.Observaciones = "";

            if (this.PuntoEntrega == null)
                this.PuntoEntrega = "";
        }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            var dc = new AgroFichasDBDataContext();

            if (this.IdAgricultor <= 0)
                yield return new RuleViolation("El Agricultor es requerido", "IdAgricultor");

            if (this.IdCultivo <= 0)
                yield return new RuleViolation("El Cultivo es requerido", "IdCultivo");

            if (this.IdComuna <= 0)
                yield return new RuleViolation("La Comuna es requerida", "IdComuna");

            if (this.Superficie <= 0)
                yield return new RuleViolation("La Superficie es no es válida", "Superficie");

            if (this.Cantidad <= 0)
                yield return new RuleViolation("La Cantidad es no es válida", "Cantidad");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}