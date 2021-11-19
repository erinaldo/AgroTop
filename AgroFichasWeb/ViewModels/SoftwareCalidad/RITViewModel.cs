using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class RITViewModel
    {
        private AgroFichasDBDataContext      dcAgroFichas      = new AgroFichasDBDataContext();
        private OperacionesDBDataContext     dcOperaciones     = new OperacionesDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CAL_RIT                                 RegistroInspeccionTransporte        { get; set; }
        public CAL_RITContenedor                       Contenedor                          { get; set; }
        public CAL_OrdenProduccion                     OrdenProduccion                     { get; set; }
        public List<CAL_RITParametroRevisarContenedor> ParametroRevisarList                { get; set; }

        public LOG_Transportista GetTransportista(int IdTransportista)
        {
            return dcAgroFichas.LOG_Transportista.SingleOrDefault(X => X.IdTransportista == IdTransportista && X.Habilitado == true);
        }
    }
}