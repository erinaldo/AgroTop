using AgroFichasWeb.Models;
using AgroFichasWeb.Models.SoftwareCalidad;
using System.Collections.Generic;
using System.Linq;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class DespachoCargaGranelViewModel
    {
        private OperacionesDBDataContext dcOperaciones = new OperacionesDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CAL_DetalleOrdenProduccion        DetalleOrdenProduccion              { get; set; }
        public CAL_EspesorProducto               EspesorProducto                     { get; set; }
        public CAL_OrdenProduccion               OrdenProduccion                     { get; set; }
        public CAL_DespachoCargaGranel           DespachoCargaGranel                 { get; set; }
        public CAL_Subproducto                   Producto                            { get; set; }
        public OPR_Turno                         Turno                               { get; set; }
        public List<CAL_DespachoCargaGranelTest> AnalisisDespachoCargaGranelTestList { get; set; }
        public CAL_FT                            FichaTecnica                        { get; set; }
        public CAL_FTControlVersion              ControlVersion                      { get; set; }
    }
}