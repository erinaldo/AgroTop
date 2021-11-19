using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class InformeViewModel
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public DateTime                  dateTime          { get; set; }
        public int                       IdTurno           { get; set; }
        public List<CAL_OrdenProduccion> OrdenesProduccion { get; set; }
        public List<CAL_Pale>            Pales             { get; set; }
        public CAL_Turno                 Turno             { get; set; }
        
        public List<CAL_DetalleOrdenProduccion> GetDetalleOrdenProduccion(int IdOrdenProduccion)
        {
            return dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(X => X.IdOrdenProduccion == IdOrdenProduccion).ToList();
        }
    }
}