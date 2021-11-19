using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_CertificadoTechnicalCertificate
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int? IdAnalisisPeriodicoMetalesPesados { get; set; }
        public int? IdAnalisisPeriodicoMicotoxinas { get; set; }
        public int? IdAnalisisPeriodicoMicrobiologia { get; set; }
        public int? IdAnalisisPeriodicoNutricional { get; set; }
        public int? IdAnalisisPeriodicoPesticidas { get; set; }
        public int IdOrdenProduccion { get; set; }
        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetLotes(int? IdOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_GetLotesPepsiCo()
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

            if (!this.IdLEPallets.HasValue || this.IdLEPallets == 0)
                yield return new RuleViolation("La lista de empaque es requerida", "IdLEPallets");

            if (string.IsNullOrEmpty(this.CertificateNumber))
                yield return new RuleViolation("El número de certificado es requerido", "CertificateNumber");

            if (string.IsNullOrEmpty(this.FinishedProcessCode))
                yield return new RuleViolation("El código del proceso es requerido", "FinishedProcessCode");

            if (string.IsNullOrEmpty(this.Vessel))
                yield return new RuleViolation("El buque es requerido", "Vessel");

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