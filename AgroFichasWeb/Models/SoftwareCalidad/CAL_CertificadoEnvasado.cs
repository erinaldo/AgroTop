using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_CertificadoEnvasado
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
        public int? IdMetodologia { get; set; }
        public int? IdMetodologias { get; set; }
        public int? IdMetodologiaSelect { get; set; }
        public int IdOrdenProduccion { get; set; }
        #endregion
        #region 3. Funciones
        public List<CAL_Metodologia> GetMetodologias()
        {
            return dcSoftwareCalidad.CAL_Metodologia.Where(X => X.Habilitado == true).OrderBy(X => X.Analisis).ToList();
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetAnalisisPeriodicoMetalesPesados(int? IdAnalisisPeriodicoMetalesPesados)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_GetAnalisisPeriodicosPorDetalleOrdenProduccionYTipoAnalisis(this.IdDetalleOrdenProduccion, 1)
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdAnalisisPeriodico == IdAnalisisPeriodicoMetalesPesados && IdAnalisisPeriodicoMetalesPesados != null),
                                                         Text = string.Format("{0:dd/MM/yy HH:mm} - Lote {1} - {2} ({3}) - Frec: {4} ({5} {6})", X.FechaAnalisis, X.LoteComercial, X.ProductoNombre, X.SubproductoNombre, X.Frecuencia, X.CntDias, (X.CntDias > 1 ? "días" : "día")),
                                                         Value = X.IdAnalisisPeriodico.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetAnalisisPeriodicoMicotoxinas(int? IdAnalisisPeriodicoMicotoxinas)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_GetAnalisisPeriodicosPorDetalleOrdenProduccionYTipoAnalisis(this.IdDetalleOrdenProduccion, 2)
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdAnalisisPeriodico == IdAnalisisPeriodicoMicotoxinas && IdAnalisisPeriodicoMicotoxinas != null),
                                                         Text = string.Format("{0:dd/MM/yy HH:mm} - Lote {1} - {2} ({3}) - Frec: {4} ({5} {6})", X.FechaAnalisis, X.LoteComercial, X.ProductoNombre, X.SubproductoNombre, X.Frecuencia, X.CntDias, (X.CntDias > 1 ? "días" : "día")),
                                                         Value = X.IdAnalisisPeriodico.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetAnalisisPeriodicoMicrobiologia(int? IdAnalisisPeriodicoMicrobiologia)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_GetAnalisisPeriodicosPorDetalleOrdenProduccionYTipoAnalisis(this.IdDetalleOrdenProduccion, 3)
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdAnalisisPeriodico == IdAnalisisPeriodicoMicrobiologia && IdAnalisisPeriodicoMicrobiologia != null),
                                                         Text = string.Format("{0:dd/MM/yy HH:mm} - Lote {1} - {2} ({3}) - Frec: {4} ({5} {6})", X.FechaAnalisis, X.LoteComercial, X.ProductoNombre, X.SubproductoNombre, X.Frecuencia, X.CntDias, (X.CntDias > 1 ? "días" : "día")),
                                                         Value = X.IdAnalisisPeriodico.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetAnalisisPeriodicoNutricional(int? IdAnalisisPeriodicoNutricional)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_GetAnalisisPeriodicosPorDetalleOrdenProduccionYTipoAnalisis(this.IdDetalleOrdenProduccion, 4)
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdAnalisisPeriodico == IdAnalisisPeriodicoNutricional && IdAnalisisPeriodicoNutricional != null),
                                                         Text = string.Format("{0:dd/MM/yy HH:mm} - Lote {1} - {2} ({3}) - Frec: {4} ({5} {6})", X.FechaAnalisis, X.LoteComercial, X.ProductoNombre, X.SubproductoNombre, X.Frecuencia, X.CntDias, (X.CntDias > 1 ? "días" : "día")),
                                                         Value = X.IdAnalisisPeriodico.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetAnalisisPeriodicoPesticidas(int? IdAnalisisPeriodicoPesticidas)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_GetAnalisisPeriodicosPorDetalleOrdenProduccionYTipoAnalisis(this.IdDetalleOrdenProduccion, 5)
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdAnalisisPeriodico == IdAnalisisPeriodicoPesticidas && IdAnalisisPeriodicoPesticidas != null),
                                                         Text = string.Format("{0:dd/MM/yy HH:mm} - Lote {1} - {2} ({3}) - Frec: {4} ({5} {6})", X.FechaAnalisis, X.LoteComercial, X.ProductoNombre, X.SubproductoNombre, X.Frecuencia, X.CntDias, (X.CntDias > 1 ? "días" : "día")),
                                                         Value = X.IdAnalisisPeriodico.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetLotes(int? IdOrdenProduccion)
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

        public IEnumerable<SelectListItem> GetMetodologias(int? IdMetodologia)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_Metodologia
                                                     where X.Habilitado == true
                                                     orderby X.Analisis
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdMetodologia == IdMetodologia && IdMetodologia != null),
                                                         Text = string.Format("{0}: {1}", X.Analisis, X.Tecnica),
                                                         Value = X.IdMetodologia.ToString()
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

            if (actionName == "Crear" && this.IdOrdenProduccion == 0)
                yield return new RuleViolation("El lote comercial es requerido", "IdOrdenProduccion");

            if (actionName == "Crear" && this.IdDetalleOrdenProduccion == 0)
                yield return new RuleViolation("El producto es requerido", "IdDetalleOrdenProduccion");

            if (this.IdLEPallets == 0)
                yield return new RuleViolation("La lista de empaque es requerida", "IdLEPallets");

            if (string.IsNullOrEmpty(this.NCertificado))
                yield return new RuleViolation("El número de certificado es requerido", "NCertificado");

            if (string.IsNullOrEmpty(this.FechaElaboracion))
                yield return new RuleViolation("La fecha de elaboración es requerida", "FechaElaboracion");

            if (string.IsNullOrEmpty(this.MaterialEmpaque))
                yield return new RuleViolation("El material de empaque es requerido", "MaterialEmpaque");

            if (string.IsNullOrEmpty(this.CodigoProceso))
                yield return new RuleViolation("El código del proceso es requerido", "CodigoProceso");

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