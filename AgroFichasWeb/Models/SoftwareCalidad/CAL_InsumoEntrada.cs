using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_InsumoEntrada
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public List<CAL_InsumoCausaRechazo> GetCausaRechazos()
        {
            return (from I0 in dc.CAL_InsumoEntradaRechazado
                    join C0 in dc.CAL_InsumoCausaRechazo on I0.IdCausaRechazo equals C0.IdCausaRechazo
                    where I0.IdInsumoEntrada == this.IdInsumoEntrada
                    select C0).ToList();
        }

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

        public string GetCssStyleEstado(int IdEstado)
        {
            switch (IdEstado)
            {
                case 1:
                    return "<span class=\"label label-warning\">{0}</span>";
                case 2:
                    return "<span class=\"label label-success\">{0}</span>";
                case 3:
                    return "<span class=\"label label-danger\">{0}</span>";
                default:
                    throw new Exception("Estado desconocido");
            }
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetCausaReingreso(int? IdCausaReingreso)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CAL_InsumoCausaReingreso
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCausaReingreso == IdCausaReingreso && IdCausaReingreso != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdCausaReingreso.ToString()
                                                     };
            return selectList;
        }

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

            if (this.FechaCompra == null)
                yield return new RuleViolation("La fecha de compra es requerido", "FechaCompra");

            if (this.OrdenCompra == 0)
                yield return new RuleViolation("La orden de compra es requerida", "OrdenCompra");

            if (this.FechaLlegada == null)
                yield return new RuleViolation("La fecha de llegada es requerida", "FechaLlegada");

            if (this.Cantidad == 0)
                yield return new RuleViolation("La cantidad es requerida", "Cantidad");

            if (this.Reingreso.HasValue && this.Reingreso.Value && this.IdCausaReingreso == null)
            {
                yield return new RuleViolation("Debe seleccionar la causa de reingreso", "IdCausaReingreso");
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