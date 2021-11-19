using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class AsociarParametrosAnalisisClienteViewModel
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public List<vw_CAL_ParametroAnalisisCliente> Clientes { get; set; }

        public List<CAL_ParametroAnalisisCliente> GetParametrosAnalisis(int IdCliente)
        {
            return dcSoftwareCalidad.CAL_ParametroAnalisisCliente.Where(X => X.IdCliente == IdCliente).OrderBy(X => X.Orden).ToList();
        }
    }
}