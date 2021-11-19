using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_ReprocesoPallets
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private OperacionesDBDataContext dcOperaciones = new OperacionesDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int IdSubproductoOrigen { get; set; }
        #endregion
        #region 3. Funciones
        public Cliente GetCliente(int IdCliente)
        {
            return dcAgroFichas.Cliente.Single(X => X.IdCliente == IdCliente);
        }

        public CAL_DetalleOrdenProduccion GetDetallePale(int IdPale)
        {
            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == IdPale);
            return dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Single(X => X.IdDetalleOrdenProduccion == pale.IdDetalleOrdenProduccion);
        }

        public CAL_TipoOrdenProduccion GetEnvase(int IdOrdenProduccion)
        {
            CAL_OrdenProduccion OP = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == IdOrdenProduccion && X.Habilitado == true);
            return dcSoftwareCalidad.CAL_TipoOrdenProduccion.Single(X => X.IdTipoOrdenProduccion == OP.IdTipoOrdenProduccion);
        }

        public CAL_OrdenProduccion GetLote(int IdOrdenProduccion)
        {
            return dcSoftwareCalidad.CAL_OrdenProduccion.Single(X => X.IdOrdenProduccion == IdOrdenProduccion);
        }

        public CAL_Subproducto GetSubproducto(int IdSubproducto)
        {

            return dcSoftwareCalidad.CAL_Subproducto.Single(X => X.IdSubproducto == IdSubproducto);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetLotes(int? IdOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                     orderby X.LoteComercial
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdOrdenProduccion == IdOrdenProduccion && IdOrdenProduccion != null),
                                                         Text = X.LoteComercial,
                                                         Value = X.IdOrdenProduccion.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetSubproductos(int? IdOrdenProduccion, int? IdSubproducto)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_Subproducto
                                                     join Y in dcSoftwareCalidad.CAL_DetalleOrdenProduccion on X.IdSubproducto equals Y.IdSubproducto
                                                     where Y.IdOrdenProduccion == IdOrdenProduccion
                                                     orderby X.IdSubproducto
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdSubproducto == IdSubproducto && IdSubproducto != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdSubproducto.ToString()
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