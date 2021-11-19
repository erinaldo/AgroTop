using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class  PlantaProduccion
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public int IdPlantaProduccionSelect { get; set; }

        public int IdCultivoSelect { get; set; }

        public IEnumerable<SelectListItem> GetComuna(int? IdComuna)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.Comuna
                                                     orderby X.Nombre ascending
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdComuna == IdComuna && IdComuna != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdComuna.ToString()
                                                     };
            return selectList;
        }

        #region Validaciones
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("Nombre Inválida", "Nombre");

            if(IdComuna == 0)
                yield return new RuleViolation("Seleccione una Comuna", "IdComuna");
            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
    }
}