using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class Variedad
    {
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El Nombre es requerido", "Nombre");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public static IEnumerable<SelectListItem> SelectList(int? idVariedad)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, idVariedad);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int? idVariedad)
        {
            return from s in dc.Variedad
                   orderby s.Cultivo.Nombre, s.Nombre
                   select new SelectListItem
                   {
                       Selected = (s.IdVariedad == idVariedad && idVariedad != null),
                       Text = $"{s.Cultivo.Nombre} > {s.Nombre}",
                       Value = s.IdVariedad.ToString()
                   };
        }

    }
}