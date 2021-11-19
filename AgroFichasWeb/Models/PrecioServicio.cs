using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class  PrecioServicio
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public int IdSucursalSelect { get; set; }

        public int IdCultivoSelect { get; set; }

        public int IdTipoServicioSelect { get; set; }
        
        public DateTime Fecha1{ get; set; }

        public string ValorForm { get; set; }

        

        public IEnumerable<SelectListItem> GetTipoServicio(int? IdTipoServicio)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.TipoServicio
                                                     orderby X.IdTipoServicio
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoServicio == IdTipoServicio && IdTipoServicio != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdTipoServicio.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetCultivo(int? IdCultivo)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.Cultivo
                                                     orderby X.IdCultivo
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCultivo == IdCultivo && IdCultivo != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdCultivo.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetSucursal(int? IdSucursal)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.Sucursal
                                                     orderby X.IdSucursal
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdSucursal == IdCultivo && IdCultivo != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdSucursal.ToString()
                                                     };
            return selectList;
        }

        #region Validaciones
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (string.IsNullOrEmpty(this.Fecha.ToString()))
                yield return new RuleViolation("Fecha Inválida", "Fecha");

            if (!decimal.TryParse(ValorForm, out decimal aux))
                yield return new RuleViolation("Valor Inválido", "Valor");


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