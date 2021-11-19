﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_ParametroAnalisisProducto
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetParametrosAnalisis(int? IdParametroAnalisis)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_ParametroAnalisis
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdParametroAnalisis == IdParametroAnalisis && IdParametroAnalisis != null),
                                                         Text = string.Format("{0} ({1})", X.Nombre, X.UM),
                                                         Value = X.IdParametroAnalisis.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetProductos(int? IdProducto)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_Producto
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdProducto == IdProducto && IdProducto != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdProducto.ToString()
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
            if (this.IdParametroAnalisis == 0)
                yield return new RuleViolation("El parámetro de análisis es requerido", "IdParametroAnalisis");
            if (this.IdProducto == 0)
                yield return new RuleViolation("El producto es requerido", "IdProducto");
            if (this.Orden == 0)
                yield return new RuleViolation("El orden es requerido", "Orden");

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