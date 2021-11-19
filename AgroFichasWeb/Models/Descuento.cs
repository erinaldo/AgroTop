using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Descuento
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdAgricultor <= 0)
                yield return new RuleViolation("El Agricultor es requerido", "IdAgricultor");
            if (this.IdTipoDescuento <= 0)
                yield return new RuleViolation("El Tipo de desuento es requerido", "IdTipoDescuento");
            if (this.IdMoneda <= 0)
                yield return new RuleViolation("La Moneda es requerida", "IdMoneda");
            if (this.Monto <= 0)
                yield return new RuleViolation("El Monto no es válido", "Monto");
            if (this.IdTipoDescuento  <= 0)
                yield return new RuleViolation("El Precio no es válido", "IdTipoDescuento");

            if (this.IdTipoDescuento == 3 || this.IdTipoDescuento == 4) //Factoring y Anticipo requiere Numero Documento, Precio Unitario y Cantidad
            {
                if (!this.NumeroDocumento.HasValue)
                    yield return new RuleViolation("El Número de Documento es requerido", "NumeroDocumento");
                if (!this.Cantidad.HasValue)
                    yield return new RuleViolation("Los Kg son requeridos", "Cantidad");
                if (!this.PrecioUnidad.HasValue)
                    yield return new RuleViolation("El Precio por Kg es requerido", "PrecioUnidad");
                
                if (this.Cantidad.HasValue && this.Cantidad.Value <= 0)
                    yield return new RuleViolation("Los Kg no son válidos", "Cantidad");
                if (this.PrecioUnidad.HasValue && this.PrecioUnidad.Value <= 0)
                    yield return new RuleViolation("El Precio por Kg  no es válido", "PrecioUnidad");
            }

            if (this.IdTipoDescuento != 3) //Insitución opcional sólo para anticipos
                if (String.IsNullOrWhiteSpace(this.Institucion))
                    yield return new RuleViolation("La Institución es requerida", "Institucion");

            if (IdMoneda > 0 && this.Monto > 0)
            {
                var dc = new AgroFichasDBDataContext();
                var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
                if (this.Monto != Math.Round(this.Monto, moneda.Decimales))
                    yield return new RuleViolation(String.Format("El monto debe tener a lo más {0} decimal(es)", moneda.Decimales), "Monto");
            }


            yield break;
        }


        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}