using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_PresetPepsiCoIncMicrobiologicalParameter
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetFichasTecnicas(int? IdFichaTecnica)
        {
            IEnumerable<SelectListItem> selectList = (from X in dc.CAL_FT
                                                      where X.Habilitado == true
                                                      orderby X.Codigo
                                                      select new SelectListItem
                                                      {
                                                          Selected = (X.IdFichaTecnica == IdFichaTecnica && IdFichaTecnica != null),
                                                          Text = string.Format("{0} {1:dd/MM/yyyy}", X.Codigo, X.FechaHoraIns),
                                                          Value = X.IdFichaTecnica.ToString()
                                                      });
            return selectList;
        }

        public IEnumerable<SelectListItem> GetParametros(int? IdParametroMicrobiologia)
        {
            IEnumerable<SelectListItem> selectList = (from X in dc.CAL_ParametroMicrobiologia
                                                      where X.Habilitado == true
                                                      orderby X.Nombre
                                                      select new SelectListItem
                                                      {
                                                          Selected = (X.IdParametroMicrobiologia == IdParametroMicrobiologia && IdParametroMicrobiologia != null),
                                                          Text = string.Format("{0} ({1})", X.Nombre, X.UM),
                                                          Value = X.IdParametroMicrobiologia.ToString()
                                                      });
            return selectList;
        }
        #endregion
        #region 5. Validaciones
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdFichaTecnica == 0)
                yield return new RuleViolation("La ficha técnica es requerida", "IdFichaTecnica");
            if (this.IdParametroMicrobiologia == 0)
                yield return new RuleViolation("El parámetro es requerido", "IdParametroAnalisis");
            if (String.IsNullOrEmpty(this.Specification))
                yield return new RuleViolation("La Specification es requerida", "Specification");
            if (String.IsNullOrEmpty(this.IndiaMethod))
                yield return new RuleViolation("El India Method es requerido", "IndiaMethod");
            if (String.IsNullOrEmpty(this.IndiaFreq))
                yield return new RuleViolation("El India Freq es requerido", "IndiaFreq");
            if (String.IsNullOrEmpty(this.AvenatopMethod))
                yield return new RuleViolation("El Avenatop Method es requerido", "AvenatopMethod");
            if (String.IsNullOrEmpty(this.AvenatopFreq))
                yield return new RuleViolation("El Avenatop Freq es requerido", "AvenatopFreq");
            if (this.SortOrder == 0)
                yield return new RuleViolation("El orden es requerido", "SortOrder");

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