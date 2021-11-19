using AgroFichasWeb.Models;
using AgroFichasWeb.Models.SoftwareCalidad;
using System.Collections.Generic;
using System.Linq;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class ReprocesoViewModel
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext      dcAgroFichas      = new AgroFichasDBDataContext();

        public CAL_ReprocesoPallets         CALReprocesoPallets         { get; set; }
        public CAL_Pale                     CALPale                     { get; set; }
        public CAL_DetalleOrdenProduccion   CALDetalleOrdenProduccion   { get; set; }
        public CAL_OrdenProduccion          CALOrdenProduccion          { get; set; }
        public Cliente                      Cliente                     { get; set; }

        #region *** Funciones ***
        public CAL_OrdenProduccion GetLote(int IdOrdenProduccion)
        {
            return dcSoftwareCalidad.CAL_OrdenProduccion.Single(X => X.IdOrdenProduccion == IdOrdenProduccion);
        }

        public Cliente GetCliente(int IdCliente)
        {
            return dcAgroFichas.Cliente.Single(X => X.IdCliente == IdCliente);
        }

        public CAL_Subproducto GetSubproducto(int IdSubproducto)
        {

            return dcSoftwareCalidad.CAL_Subproducto.Single(X => X.IdSubproducto == IdSubproducto);
        }

        public CAL_TipoOrdenProduccion GetEnvase(int IdOrdenProduccion)
        {
            CAL_OrdenProduccion OP = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == IdOrdenProduccion && X.Habilitado == true);
            return dcSoftwareCalidad.CAL_TipoOrdenProduccion.Single(X => X.IdTipoOrdenProduccion == OP.IdTipoOrdenProduccion);
        }

        #endregion
    }
}