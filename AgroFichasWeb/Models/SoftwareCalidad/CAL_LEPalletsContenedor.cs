using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AgroFichasWeb.Models.Response;
using MoreLinq;

namespace AgroFichasWeb.Models
{
    public partial class CAL_LEPalletsContenedor
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public decimal EditPesoNeto { get; set; }
        public decimal EditPesoBruto { get; set; }
        #endregion
        #region 3. Funciones

        public IEnumerable<SelectListItem> GetContenedores(int IdOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = (from X in dcSoftwareCalidad.CAL_GetContenedoresDisponiblesParaListaEmpaqueEnPallets(IdOrdenProduccion)
                                                      where X.Registrado == 0
                                                      orderby X.NContenedor
                                                      select new SelectListItem
                                                      {
                                                          Text = X.NContenedor.ToUpperInvariant(),
                                                          Value = X.IdContenedor.ToString()
                                                      });
            return selectList;
        }

        public List<SelectContenedoresListaEmpaquePallets> GetContenedoresList(int IdOrdenProduccion)
        {
            List<SelectContenedoresListaEmpaquePallets> selectList = (from X in dcSoftwareCalidad.CAL_GetContenedoresDisponiblesParaListaEmpaqueEnPallets(IdOrdenProduccion)
                                                      where X.Registrado == 0
                                                      orderby X.NContenedor
                                                      select new SelectContenedoresListaEmpaquePallets
                                                      {
                                                          Text = X.NContenedor.ToUpperInvariant(),
                                                          Value = X.IdContenedor.ToString()
                                                      }).ToList();
            return selectList;
        }


        public string GetSubproducto(int IdOrdenProduccion, int IdContenedor)
        {
            //CAL_Subproducto subproducto = (from X in dcSoftwareCalidad.CAL_Subproducto
            //                               join Y in dcSoftwareCalidad.CAL_DetalleOrdenProduccion on X.IdSubproducto equals Y.IdSubproducto
            //                               join Z in dcSoftwareCalidad.CAL_OrdenProduccion on Y.IdOrdenProduccion equals Z.IdOrdenProduccion
            //                               where Z.IdOrdenProduccion == IdOrdenProduccion
            //                                       && Z.Habilitado
            //                                       && (Z.Autorizado.HasValue && Z.Autorizado.Value)
            //                                       && !Z.Terminada
            //                               select X).FirstOrDefault();

            //return subproducto.Nombre;
            List<CAL_GetProductosYSacosParaListaEmpaque_PalletsResult> productos = dcSoftwareCalidad.CAL_GetProductosYSacosParaListaEmpaque_Pallets(IdOrdenProduccion, IdContenedor).ToList();
            string s = "";
            foreach (var producto in productos)
            {
                s += string.Format("{0} ({1} sacos)<br>", producto.Subproducto, producto.Sacos);
            }
            return s;
        }

        #endregion
        #region 4. SelectLists

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
            if ((controllerName.ToLower() != "callepallets" && actionName.ToLower() != "crear")){

                if (this.IdContenedor == 0)
                    yield return new RuleViolation("El contenedor es requerido", "IdContenedor");

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