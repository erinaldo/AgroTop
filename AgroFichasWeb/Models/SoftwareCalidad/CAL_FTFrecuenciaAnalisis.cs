using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_FTFrecuenciaAnalisis
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public bool Editar { get; set; }
        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetFrecuenciasAnalisis(int? IdFrecuenciaAnalisis)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_FrecuenciaAnalisis
                                                     orderby X.IdFrecuenciaAnalisis
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdFrecuenciaAnalisis == IdFrecuenciaAnalisis && IdFrecuenciaAnalisis != null),
                                                         Text = X.Frecuencia,
                                                         Value = X.IdFrecuenciaAnalisis.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetTiposAnalisis(int? IdTipoAnalisis)
        {
            IEnumerable<SelectListItem> selectList = (from X in dcSoftwareCalidad.CAL_TipoAnalisis
                                                      orderby X.IdTipoAnalisis
                                                      select new SelectListItem
                                                      {
                                                          Selected = (X.IdTipoAnalisis == IdTipoAnalisis && IdTipoAnalisis != null),
                                                          Text = X.Descripcion,
                                                          Value = X.IdTipoAnalisis.ToString()
                                                      });

            List<CAL_FTFrecuenciaAnalisis> cAL_FTFrecuenciaAnalisisList = dcSoftwareCalidad.CAL_FTFrecuenciaAnalisis.Where(X => X.IdFichaTecnica == this.IdFichaTecnica).ToList();
            List<SelectListItem> filtered = selectList.Where(p => !cAL_FTFrecuenciaAnalisisList.Any(p2 => p2.IdTipoAnalisis == int.Parse(p.Value))).ToList();
            if (this.IdTipoAnalisis != 0)
            {
                filtered.Add(new SelectListItem
                {
                    Selected = (this.IdTipoAnalisis == this.IdTipoAnalisis && IdTipoAnalisis != null),
                    Text = this.CAL_TipoAnalisis.Descripcion,
                    Value = this.IdTipoAnalisis.ToString()
                });
            }

            return filtered;
        }
        #endregion
        #region 5. Validaciones
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            string actionName = "";
            string controllerName = "";

            var routeValues = HttpContext.Current.Request.RequestContext.RouteData.Values;
            if (routeValues != null)
            {
                if (routeValues.ContainsKey("action"))
                {
                    actionName = routeValues["action"].ToString();
                }
                if (routeValues.ContainsKey("controller"))
                {
                    controllerName = routeValues["controller"].ToString();
                }
            }

            if (this.IdTipoAnalisis == 0)
                yield return new RuleViolation("El tipo de análisis es requerido", "IdTipoAnalisis");

            if (this.IdFrecuenciaAnalisis == 0)
                yield return new RuleViolation("La frecuencia de análisis es requerida", "IdFrecuenciaAnalisis");

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