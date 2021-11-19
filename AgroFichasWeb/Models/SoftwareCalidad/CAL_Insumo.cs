using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_Insumo
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

        public string GetCssStyleStock(int value)
        {
            if (value <= 10000)
            {
                return "<span class=\"label label-danger\">{0}</span>";
            }
            else if (value <= 20000)
            {
                return "<span class=\"label label-warning\">{0}</span>";
            }
            else if (value > 20000)
            {
                return "<span class=\"label label-success\">{0}</span>";
            }
            else
            {
                return "<span class=\"label label-success\">{0}</span>";
            }
        }

        public int GetStockRechazado()
        {
            var stockRechazado = (from I0 in dc.CAL_InsumoEntrada
                                  where I0.IdInsumo == this.IdInsumo
                                  && I0.IdEstado == 3
                                  select I0).ToList();
            if (stockRechazado.Count > 0)
                return stockRechazado.Sum(X => X.Cantidad);
            else
                return 0;
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetTipoInsumos(int? IdTipoInsumo)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CAL_TipoInsumo
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoInsumo == IdTipoInsumo && IdTipoInsumo != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdTipoInsumo.ToString()
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
            if (this.IdTipoInsumo == 0)
                yield return new RuleViolation("El tipo es requerido", "IdTipoInsumo");

            if (string.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El nombre es requerido", "Nombre");

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