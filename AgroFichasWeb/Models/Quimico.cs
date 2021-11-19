using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class Quimico
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

        public static IEnumerable<SelectListItem> SelectList(int idTipoRecomendacion, int? idQuimico)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, idTipoRecomendacion, idQuimico);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int idTipoRecomendacion, int? idQuimico)
        {
            return from s in dc.Quimico
                   where s.IdTipoRecomendacion == idTipoRecomendacion 
                      && (s.Habilitado || (idQuimico.HasValue && idQuimico.Value == idQuimico))
                   orderby s.Nombre
                   select new SelectListItem
                   {
                       Selected = (s.IdQuimico == idQuimico && idQuimico != null),
                       Text = s.Nombre,
                       Value = s.IdQuimico.ToString()
                   };
        }

        public static List<Quimico> GetAll(AgroFichasDBDataContext dc)
        {
            if (dc == null)
                dc = new AgroFichasDBDataContext();

            return (from q in dc.Quimico
                    orderby q.Nombre
                    select q).ToList();
        }
    }
}