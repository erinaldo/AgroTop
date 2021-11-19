using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroFichasWeb.Models
{
    public partial class LOG_Requerimiento
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdEmpresa == 0)
                yield return new RuleViolation("Debe seleccionar una empresa de la lista", "IdEmpresa");
            if (this.IdTipoMovimiento == 0)
                yield return new RuleViolation("Debe seleccionar un tipo de movimiento de la lista", "IdTipoMovimiento");
            if (String.IsNullOrEmpty(this.Glosa))
                yield return new RuleViolation("La glosa es requerida", "Glosa");
            //if (this.IdRequerimiento == 0 && (DateTime.Now > this.FechaInicio))
            //    yield return new RuleViolation("La fecha de inicio debe ser mayor o igual que la del día de hoy " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt") + "", "FechaInicio");
            //if (this.IdRequerimiento == 0 && (DateTime.Now > this.FechaVencimiento))
            //    yield return new RuleViolation("La fecha de término debe ser mayor o igual que la del día de hoy " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt") + "", "FechaVencimiento");
            if (this.IdRequerimiento == 0 && (this.FechaInicio > this.FechaVencimiento))
                yield return new RuleViolation("La fecha de inicio no debe ser mayor que la fecha de término", "FechaInicio");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}
