using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroFichasWeb.Models
{
    public partial class LOG_Camion
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdTransportista == 0)
                yield return new RuleViolation("Debe seleccionar un transportista de la lista", "IdTransportista");
            if (String.IsNullOrEmpty(this.Patente))
                yield return new RuleViolation("La patente es requerida", "Patente");
            if (this.Patente != null && this.Patente.Length < 6)
                yield return new RuleViolation("La patente es inválida", "Patente");
            if (this.IdMarca == 0)
                yield return new RuleViolation("Debe seleccionar una marca de la lista", "IdMarca");
            if (this.IdTipoCamion == 0)
                yield return new RuleViolation("Debe seleccionar un tipo de camión de la lista", "IdTipoCamion");
            if (this.IdTipoDescarga == 0)
                yield return new RuleViolation("Debe seleccionar un tipo de descarga de la lista", "IdTipoDescarga");
            if (this.TaraMaxima == 0)
                yield return new RuleViolation("El tara máxima es requerida", "TaraMaxima");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}
