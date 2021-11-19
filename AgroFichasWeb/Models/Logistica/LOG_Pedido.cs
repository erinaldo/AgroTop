using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroFichasWeb.Models
{
    public partial class LOG_Pedido
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdCultivo == 0)
                yield return new RuleViolation("Debe seleccionar un cultivo de la lista", "IdCultivo");
            if (this.IdTipoPedido == 0)
                yield return new RuleViolation("Debe seleccionar un tipo de pedido de la lista", "IdTipoPedido");
            if (this.IdTipoPedido == 3 && this.IdEnvase == null)
                yield return new RuleViolation("Debe seleccionar un tipo de envase de la lista", "IdEnvase");
            if (this.CantidadUnitariaKg <= 0)
                yield return new RuleViolation("La cantidad a solicitar debe ser mayor a 0", "CantidadUnitariaKg");
            if (this.Origen > 0 && this.Destino == null)
                yield return new RuleViolation("Debe seleccionar un destino", "Destino");
            if (this.Origen == null && this.Destino > 0)
                yield return new RuleViolation("Debe seleccionar un origen", "Origen");
            if (this.OtroOrigen != "" && this.OtroDestino == "")
                yield return new RuleViolation("Debe escribir un destino", "OtroDestino");
            if (this.OtroOrigen == "" && this.OtroDestino != "")
                yield return new RuleViolation("Debe escribir un origen", "OtroOrigen");
            if (this.IdEstado == 0)
                yield return new RuleViolation("Debe seleccionar un estado de la lista", "IdEstado");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}
