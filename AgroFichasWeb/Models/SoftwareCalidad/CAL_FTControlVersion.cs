using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_FTControlVersion
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetItems(int? IdItem)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_FTControlVersionItem
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdItem == IdItem && IdItem != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdItem.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetMotivos(int? IdMotivo)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_FTControlVersionMotivo
                                                     where X.Habilitado == true
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdMotivo == IdMotivo && IdMotivo != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdMotivo.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetSolicitantes(int? IdSolicitante)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_FTControlVersionSolicitante
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdSolicitante == IdSolicitante && IdSolicitante != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdSolicitante.ToString()
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
            int Version = 0;
            try
            {
                Version = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == this.IdFichaTecnica).Max(X => X.Version);
            }
            catch { }


            //if (this.Version < Version)
            //    yield return new RuleViolation("La versión no debe ser menor a la actual en la ficha técnica", "Version");

            if (this.Version == 0)
                yield return new RuleViolation("La versión es requerida", "Version");

            if (string.IsNullOrEmpty(this.Cambios))
                yield return new RuleViolation("Los cambios son requeridos", "Cambios");

            if (this.IdItem == 0)
                yield return new RuleViolation("El ítem de control de versión es requerido", "IdItem");

            if (this.IdMotivo == 0)
                yield return new RuleViolation("El motivo de cambio de control de versión es requerido", "IdMotivo");

            if (this.IdSolicitante == 0)
                yield return new RuleViolation("El solicitante de cambio de control de versión es requerido", "IdSolicitante");

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