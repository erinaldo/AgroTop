using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_ReprocesoSacosDañados
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgrofichas = new AgroFichasDBDataContext();
        #endregion
        #region 2. Propiedades
        public int IdOrdenProduccionReproceso { get; set; }
        public int IdProducto { get; set; }
        public IEnumerable<string> SacosDañados { get; set; }
        #endregion
        #region 3. Funciones
        public CAL_OrdenProduccion GetLote(int IdOrdenProduccion)
        {
            return dcSoftwareCalidad.CAL_OrdenProduccion.Single(X => X.IdOrdenProduccion == IdOrdenProduccion);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetLotes(int? IdOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                     where X.Habilitado
                                                     && X.Habilitado
                                                     && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                     && !X.Terminada
                                                     orderby X.LoteComercial
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdOrdenProduccion == IdOrdenProduccion && IdOrdenProduccion != null),
                                                         Text = X.LoteComercial,
                                                         Value = X.IdOrdenProduccion.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetPlantasProductivas()
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgrofichas.PlantaProduccion
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Text = X.Nombre,
                                                         Value = X.IdPlantaProduccion.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetProductosDañados(int? IdOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = (from X in dcSoftwareCalidad.CAL_GetDetalleLoteSacosDañados()
                                                      where X.IdOrdenProduccion == this.IdOrdenProduccionOrigen
                                                      && X.Reprocesado.Value == false
                                                      select new SelectListItem
                                                      {
                                                          Value = string.Format("{0}", X.IdProducto),
                                                          Text = string.Format("{0}", X.Producto)
                                                      }).DistinctBy(Y => Y.Text);
            return selectList;
        }

        public IEnumerable<SelectListItem> GetSacosDañados(int? IdDetalleOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = (from A in dcSoftwareCalidad.CAL_DespachoPale
                                                      join B in dcSoftwareCalidad.CAL_Pale on A.IdPale equals B.IdPale
                                                      join C in dcSoftwareCalidad.CAL_DetalleOrdenProduccion on B.IdDetalleOrdenProduccion equals C.IdDetalleOrdenProduccion
                                                      where A.Habilitado == true 
                                                      && B.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion
                                                      && A.SacosDañados > 0
                                                      orderby B.IdPale
                                                      select new SelectListItem
                                                      {
                                                          Text = string.Format("Pallet {0}: {1} {2}", B.IdPale, A.SacosDañados, (A.SacosDañados > 1 ? "Dañados" : "Dañado")),
                                                          Value = B.IdPale.ToString()
                                                      });
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

            if (actionName == "CrearReprocesoSacosDañados" && this.IdProducto == 0)
                yield return new RuleViolation("La familia de productos es requerido", "IdProducto");

            if (actionName == "CrearReprocesoSacosDañados" && this.SacosDañados == null)
                yield return new RuleViolation("Los sacos dañados son requeridos", "SacosDañados");

            if (actionName == "CrearReprocesoSacosDañados" && this.SacosDañados != null && this.SacosDañados.Count() == 0)
                yield return new RuleViolation("Los sacos dañados son requeridos", "SacosDañados");

            if (actionName == "CrearReprocesoSacosDañados" && this.TotalSacosDañados == 0)
                yield return new RuleViolation("El total sacos dañados es requerido", "TotalSacosDañados");

            if (actionName == "CrearReprocesoSacosDañados" && this.IdOrdenProduccionReproceso == 0)
                yield return new RuleViolation("El lote comercial de reproceso es requerido", "IdOrdenProduccionReproceso");

            if (actionName == "CrearReprocesoSacosDañados" && this.IdOrdenProduccionOrigen == this.IdOrdenProduccionReproceso)
                yield return new RuleViolation("El reproceso no puede ser igual al del pallet", "IdOrdenProduccionReproceso");

            if (actionName == "CrearReprocesoSacosDañados" && this.IdDetalleOrdenProduccion == 0)
                yield return new RuleViolation("El producto es requerido", "IdOrdenProduccionReproceso");

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