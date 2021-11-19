using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class AsociarParametrosAnalisisProductoViewModel
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public List<vw_CAL_ParametroAnalisisProducto> Productos { get; set; }

        public List<CAL_ParametroAnalisisProducto> GetParametrosAnalisis(int IdProducto)
        {
            return dcSoftwareCalidad.CAL_ParametroAnalisisProducto.Where(X => X.IdProducto == IdProducto && X.CAL_ParametroAnalisis.Habilitado).OrderBy(X => X.Orden).ToList();
        }
    }
}