using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_Saco
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetTipoArteSacos(int? IdTipoArteSaco)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CAL_TipoArteSaco
                                                     where X.Habilitado == true
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoArteSaco == IdTipoArteSaco && IdTipoArteSaco != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdTipoArteSaco.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetTipoColoresHiloSaco(int? IdTipoColorHiloSaco)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CAL_TipoColorHiloSaco
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoColorHiloSaco == IdTipoColorHiloSaco && IdTipoColorHiloSaco != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdTipoColorHiloSaco.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetTipoSacos(int? IdTipoSaco)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CAL_TipoSaco
                                                     where X.Habilitado == true
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoSaco == IdTipoSaco && IdTipoSaco != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdTipoSaco.ToString()
                                                     };
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
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El nombre es requerido", "Nombre");
            if (this.IdTipoSaco == 0)
                yield return new RuleViolation("El tipo de saco es requerido", "IdTipoSaco");
            if (this.IdTipoArteSaco == 0)
                yield return new RuleViolation("El tipo de arte de saco es requerido", "IdTipoArteSaco");
            if (this.IdTipoColorHiloSaco == 0)
                yield return new RuleViolation("El color de hilo de saco es requerido", "IdTipoColorHiloSaco");

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