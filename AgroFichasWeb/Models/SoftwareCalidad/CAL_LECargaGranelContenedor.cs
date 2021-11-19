using AgroFichasWeb.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_LECargaGranelContenedor
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public decimal EditPesoNeto { get; set; }
        public decimal EditPesoBruto { get; set; }
        #endregion
        #region 3. Funciones
        public IEnumerable<SelectListItem> GetContenedores(int? IdDespachoCargaGranel)
        {
            int IdDespachoCargaGranelSelect = IdDespachoCargaGranel ?? 0;
            IEnumerable<SelectListItem> selectList = (from X in dcSoftwareCalidad.CAL_GetContenedoresDisponiblesParaListaEmpaqueEnCargaGranel(this.IdLECargaGranel, IdDespachoCargaGranelSelect)
                                                      orderby X.NContenedor
                                                      select new SelectListItem
                                                      {
                                                          Selected = (X.IdDespachoCargaGranel == IdDespachoCargaGranel && IdDespachoCargaGranel != null),
                                                          Text = X.NContenedor.ToUpperInvariant(),
                                                          Value = X.IdDespachoCargaGranel.ToString()
                                                      });
            return selectList;
        }

        public List<SelectContenedoresListaEmpaqueGranel> GetContenedoresList(int? IdDespachoCargaGranel)
        {
            int IdDespachoCargaGranelSelect = IdDespachoCargaGranel ?? 0;
            List<SelectContenedoresListaEmpaqueGranel> selectList = (from X in dcSoftwareCalidad.CAL_GetContenedoresDisponiblesParaListaEmpaqueEnCargaGranel(this.IdLECargaGranel, IdDespachoCargaGranelSelect)
                                                      orderby X.NContenedor
                                                      select new SelectContenedoresListaEmpaqueGranel
                                                      {
                                                          Text = X.NContenedor.ToUpperInvariant(),
                                                          Value = X.IdDespachoCargaGranel.ToString()
                                                      }).ToList();
            return selectList;
        }


        #endregion
        #region 4. SelectLists
        public List<SelectListItem> GetListaEmpaqueCargaGranel()
        {
            List<SelectListItem> list = (from X in dcSoftwareCalidad.CAL_LECargaGranel
                                             where X.Habilitado == true
                                             orderby X.IdLECargaGranel
                                             select new SelectListItem
                                             {
                                                 Value = X.IdLECargaGranel.ToString(),
                                                 Text = string.Format("Lista de Empaque Nº {0} - {1} - Nº Factura {2}", X.IdLECargaGranel, X.CAL_OrdenProduccion.LoteComercial, X.NFactura)
                                             }).ToList();
            return list;
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
            if ((controllerName.ToLower() != "cal_lecargagranel" && actionName.ToLower() != "crear"))
            {
                if (this.IdDespachoCargaGranel == 0)
                    yield return new RuleViolation("El contenedor es requerido", "IdDespachoCargaGranel");

                if (this.NGuiaDespacho == 0)
                    yield return new RuleViolation("La guía de despacho es requerida", "NGuiaDespacho");

                if (string.IsNullOrEmpty(this.SelloLinea))
                    yield return new RuleViolation("El sello de línea es requerido", "SelloLinea");

                if (this.PesoNeto == 0)
                    yield return new RuleViolation("El peso neto del contenedor es requerido", "PesoNeto");

                if (this.Tara == 0)
                    yield return new RuleViolation("La tara del contenedor es requerida", "Tara");

                yield break;
            }

        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
    }
}