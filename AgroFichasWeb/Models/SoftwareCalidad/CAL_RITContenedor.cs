using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_RITContenedor
    {
        #region 1. DataContexts
        AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public LOG_Transportista GetTransportista(int? IdTransportista)
        {
            return dcAgroFichas.LOG_Transportista.Single(X => X.IdTransportista == IdTransportista);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetTransportistas(int? IdTransportista)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.LOG_Transportista
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTransportista == this.IdTransportista && IdTransportista != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdTransportista.ToString()
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
            if (String.IsNullOrEmpty(this.NContenedor))
                yield return new RuleViolation("El nombre del contenedor es requerido", "NContenedor");
            if (this.IdTransportista == 0)
                yield return new RuleViolation("El transportista es requerido", "IdTransportista");
            if (this.IdPlanta == 0)
                yield return new RuleViolation("La planta de producción es requerida", "IdPlanta");

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