using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_ResponsableArea
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public SYS_User GetUser(int UserID)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserID == UserID);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetAreas(int? IdArea)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_Area
                                                     orderby X.IdArea
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdArea == IdArea && IdArea != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdArea.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetResponsables(int? UserID)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.SYS_User
                                                     where X.Disabled == false
                                                     orderby X.FullName
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.UserID == UserID && UserID != null),
                                                         Text = X.FullName,
                                                         Value = X.UserID.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetPlantaProduccion(int? IdPlantaProduccion)
        {
            IEnumerable<SelectListItem> selectList = from pp in dcAgroFichas.PlantaProduccion
                                                     select new SelectListItem
                                                     {
                                                         Value = pp.IdPlantaProduccion.ToString(),
                                                         Text = pp.Nombre,
                                                         Selected = (pp.IdPlantaProduccion == IdPlantaProduccion && IdPlantaProduccion != null)
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
            if (this.IdResponsableArea == 0)
                yield return new RuleViolation("El responsable de área es requerido", "IdResponsableArea");
            if (this.IdArea == 0)
                yield return new RuleViolation("El Área es requerido", "IdArea");

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