using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Siembra
    {
        public void SetDefaults()
        {

        }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            return GetRuleViolations(new List<SelectorPotreroViewModel>());
        }

        public IEnumerable<RuleViolation> GetRuleViolations(List<SelectorPotreroViewModel> potreros)
        {
            if (this.IdVariedad <= 0)
                yield return new RuleViolation("La Variedad es requerida.", "IdVariedad");

            if (this.Dosis <= 0)
                yield return new RuleViolation("La Dosis es requerida.", "Dosis");

            if (this.IdTipoSiembra <= 0)
                yield return new RuleViolation("El Tipo de Siembra es requerido.", "IdTipoSiembra");

            if (this.IdCultivoAnterior <= 0)
                yield return new RuleViolation("El Cultivos Anterior es requerido.", "IdCultivoAnterior");

            if (this.RendimientoEstimado.HasValue && this.RendimientoEstimado < 0)
                yield return new RuleViolation("El Rendimiento Estimado no es válido.", "RendimientoEstimado");

            if (potreros.Where(p => p.Seleccionado).Count() == 0)
                yield return new RuleViolation("Seleccione al menos un potrero.", "Potreros");

            yield break;
        }

        public string DescripcionPotreros()
        {
            return string.Join(", ", this.SiembraPotrero.Select(p => p.Potrero.Nombre));
        }
        public int Superficie()
        {
            return this.SiembraPotrero.Sum(sp => sp.Potrero.Superficie);
        }

    }
}