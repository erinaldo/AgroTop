using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_InsumoSalida
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public string GetCssStyleAutorizado(bool value)
        {
            if (value)
            {
                return "cal-Autorizado";
            }
            else
            {
                return "cal-NoAutorizado";
            }
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetInsumos(int? IdInsumo)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CAL_Insumo
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdInsumo == IdInsumo && IdInsumo != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdInsumo.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetProveedores(int? IdProveedor)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CAL_Proveedor
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdProveedor == IdProveedor && IdProveedor != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdProveedor.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetProveedoresProvenientesDeEntradas(int? IdProveedor)
        {
            IEnumerable<SelectListItem> selectList = (from P0 in dc.CAL_Proveedor
                                                      join I0 in dc.CAL_InsumoEntrada on P0.IdProveedor equals I0.IdProveedor
                                                      where I0.IdInsumo == this.IdInsumo
                                                      orderby P0.Nombre
                                                      select new SelectListItem
                                                      {
                                                          Selected = (P0.IdProveedor == IdProveedor && IdProveedor != null),
                                                          Text = P0.Nombre,
                                                          Value = P0.IdProveedor.ToString()
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
            if (this.IdProveedor == 0)
                yield return new RuleViolation("El proveedor es requerido", "IdProveedor");

            if (this.Cantidad == 0)
                yield return new RuleViolation("La cantidad es requerida", "Cantidad");

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