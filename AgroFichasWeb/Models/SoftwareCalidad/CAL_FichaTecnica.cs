using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_FichaTecnica
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        [Required(ErrorMessage = "Por favor seleccione el archivo")]
        public HttpPostedFileBase PostedFile { get; set; }
        public DictionaryEntry doc { get; set; }
        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetSubproducto(int? IdSubproducto)
        {
            IEnumerable<SelectListItem> selectList = (from X in dcSoftwareCalidad.CAL_DetalleOrdenProduccion
                                                      where X.IdOrdenProduccion == this.IdOrdenProduccion
                                                      orderby X.CAL_Subproducto.Nombre
                                                      select new SelectListItem
                                                      {
                                                          Selected = (X.IdSubproducto == IdSubproducto && IdSubproducto != null),
                                                          Text = X.CAL_Subproducto.Nombre,
                                                          Value = X.IdSubproducto.ToString()
                                                      }).Distinct();
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
            if (this.IdSubproducto == 0)
                yield return new RuleViolation("El producto es requerido", "IdSubproducto");
            if (this.Version == 0)
                yield return new RuleViolation("La versión es requerida", "Version");

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