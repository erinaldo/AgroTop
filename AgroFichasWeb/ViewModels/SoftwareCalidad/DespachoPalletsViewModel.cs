using AgroFichasWeb.Models;
using AgroFichasWeb.Models.SoftwareCalidad;
using System.Collections.Generic;
using System.Linq;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class DespachoPalletsViewModel
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public List<CAL_GetOrdenProduccionAnalisisCompletoResult>      AnalisisCompleto                 { get; set; }
        public List<CAL_GetPorductosAvanceCargaPaleResult>             AvanceCargaPallet                { get; set; }
        public List<CAL_GetAvanceCargaPalletsPorContenedorResult>      ContenedoresAvanceCargaPale      { get; set; }
        public List<CAL_GetAvanceCargaTotalPalletsPorContenedorResult> ContenedoresAvanceCargaPaleTotal { get; set; }
        public List<CAL_GetPalletsPorContenedorResult>                 PalletsPorContenedor             { get; set; }

        #region *** Funciones ***
        public Cliente GetCliente(int IdCliente)
        {
            return dcAgroFichas.Cliente.SingleOrDefault(X => X.IdCliente == IdCliente);
        }

        #endregion

        #region *** Vistas ***

        public string GetCssStyleAutorizado(bool? value)
        {
            if (value.HasValue)
            {
                if (value.Value)
                {
                    return "cal-Autorizado";
                }
                else
                {
                    return "cal-NoAutorizado";
                }
            }
            else
            {
                return "cal-NoAutorizado";
            }
        }

        #endregion
    }
}