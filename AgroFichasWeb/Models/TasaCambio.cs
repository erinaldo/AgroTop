using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class TasaCambio
    {
        public bool IsNew { get; set; }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            var dc = new AgroFichasDBDataContext();

            if (this.Valor <= 0)
                yield return new RuleViolation("El Valor no es válido", "Valor");

            var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
            if (!moneda.ValidarDecimales(this.Valor))
                yield return new RuleViolation(String.Format("El valor en {1} debe tener a lo más {0} decimal(es)", moneda.Decimales, moneda.Simbolo), "Valor");
            
            if (this.IsNew && this.Fecha >= DateTime.Today.AddYears(-100))
            {
                var existente = dc.TasaCambio.SingleOrDefault(tc => tc.IdMoneda == this.IdMoneda && tc.Fecha == this.Fecha);
                if (existente != null)
                    yield return new RuleViolation("Ya existe una tasa de cambio para estar fecha y moneda", "Fecha");
            }


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

        public static decimal GetValorTasaCambio(AgroFichasDBDataContext dc, DateTime fecha, int idMoneda)
        {
            return GetTasaCambio(dc, fecha, idMoneda).Valor;
        }

        public static TasaCambio GetTasaCambio(AgroFichasDBDataContext dc, DateTime fecha, int idMoneda)
        {
            var pre = dc.TasaCambio.Where(t => t.IdMoneda == idMoneda && t.Fecha <= fecha).OrderByDescending(t => t.Fecha).FirstOrDefault();
            if (pre != null)
                return pre;

            var post = dc.TasaCambio.Where(t => t.IdMoneda == idMoneda && t.Fecha > fecha).OrderBy(t => t.Fecha).FirstOrDefault();
            if (post != null)
                return post;

            return new TasaCambio()
            {
                IdMoneda = idMoneda,
                Fecha = fecha,
                Valor = 0
            };
        }
    }
}