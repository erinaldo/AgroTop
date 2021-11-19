using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class SaldoCtaCte
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            var dc = new AgroFichasDBDataContext();

            if (this.IdAgricultor <= 0)
                yield return new RuleViolation("El Agricultor es requerido", "IdAgricultor");
            if (this.IdEmpresa <= 0)
                yield return new RuleViolation("La Empresa es requerida", "IdEmpresa");
            //if (this.MontoCtaCte < 0)
            //    yield return new RuleViolation("La Monto Cuenta Corriente no es válido", "MontoCtaCte");
            //if (this.MontoDocumentado < 0)
            //    yield return new RuleViolation("El Monto Documentado no es válido", "MontoDocumentado");

            if (dc.SaldoCtaCte.SingleOrDefault(s => s.IdEmpresa == this.IdEmpresa && s.IdAgricultor == this.IdAgricultor && s.IdSaldoCtaCte != this.IdSaldoCtaCte) != null)
                yield return new RuleViolation("Ya existe un saldo para este agricultor en esta empresa", "IdAgricultor");

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