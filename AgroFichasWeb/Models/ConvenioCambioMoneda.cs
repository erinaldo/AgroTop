using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class ConvenioCambioMoneda
    {
        public List<UsoConvenioCambioMoneda> Usos { get; set; }
        public decimal CantidadUtilizada
        {
            get
            {
                if (Usos != null && Usos.Count > 0)
                    return Usos.Select(u => u.CantidadUtilizada).Sum();
                else
                    return 0;
            }
        }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdContrato <= 0)
                yield return new RuleViolation("El Contrato es requerido", "IdContrato");
            if (this.IdMonedaDestino <= 0)
                yield return new RuleViolation("La Moneda de destino es requerida", "IdMonedaDestino");
            if (this.IdMonedaOrigen <= 0)
                yield return new RuleViolation("La Moneda de destino es requerida", "IdMonedaOrigen");
            if (this.Cantidad <= 0)
                yield return new RuleViolation("La Cantidad no es válida", "Cantidad");
            if (this.PrecioUnidad <= 0)
                yield return new RuleViolation("El Precio no es válido", "PrecioUnidad");
            
            if (this.Cantidad > 0)
            {
                if (this.Cantidad != Math.Round(this.Cantidad, 2))
                    yield return new RuleViolation(String.Format("La Cantidad a Cambiar debe tener a lo más {0} decimal(es)", 2), "PrecioUnidad");
            }

            if (this.PrecioUnidad > 0)
            {
                if (this.PrecioUnidad != Math.Round(this.PrecioUnidad, 2))
                    yield return new RuleViolation(String.Format("La Tasa de Cambio debe tener a lo más {0} decimal(es)", 2), "PrecioUnidad");
            }

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public static int NextPrioridad(AgroFichasDBDataContext dc, int idContrato)
        {
            int? p = (from cp in dc.ConvenioCambioMoneda
                      where cp.IdContrato == idContrato
                      select (int?)cp.Prioridad).Max();

            return (p ?? 0) + 1;
        }


    }

    public class UsoConvenioCambioMoneda
    {
        public PrecioIngreso PrecioIngreso { get; set; }
        public decimal CantidadUtilizada { get; set; }
    }
}