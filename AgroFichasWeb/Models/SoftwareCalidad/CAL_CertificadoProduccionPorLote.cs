using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_CertificadoProduccionPorLote
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int IdOrdenProduccion { get; set; }
        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetLotes(int? IdOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                     where X.IdTipoOrdenProduccion == 2
                                                     && X.Habilitado
                                                     && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                     orderby X.LoteComercial
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdOrdenProduccion == IdOrdenProduccion && IdOrdenProduccion != null),
                                                         Text = X.LoteComercial,
                                                         Value = X.IdOrdenProduccion.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetLotesPallets(int? IdOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                     where X.IdTipoOrdenProduccion == 1
                                                     && X.Habilitado
                                                     && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                     orderby X.LoteComercial
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdOrdenProduccion == IdOrdenProduccion && IdOrdenProduccion != null),
                                                         Text = X.LoteComercial,
                                                         Value = X.IdOrdenProduccion.ToString()
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
            if (this.IdDetalleOrdenProduccion == 0)
                yield return new RuleViolation("El producto es requerido", "IdDetalleOrdenProduccion");

            if (string.IsNullOrEmpty(this.NCertificado))
                yield return new RuleViolation("El número de certificado es requerido", "NCertificado");

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