using AgroFichasWeb.Models;
using AgroFichasWeb.Models.SoftwareCalidad;
using System.Collections.Generic;
using System.Linq;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class PaleViewModel
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALIdentificacionPale      CALIdentificacionPale  { get; set; }
        public CAL_GrupoEnvasador         GrupoEnvasador         { get; set; }
        public CAL_DetalleOrdenProduccion DetalleOrdenProduccion { get; set; }
        public CAL_EspesorProducto        EspesorProducto        { get; set; }
        public CAL_OrdenProduccion        OrdenProduccion        { get; set; }
        public CAL_Pale                   Pale                   { get; set; }
        public CAL_Subproducto            Producto               { get; set; }
        public CAL_Saco                   Saco                   { get; set; }
        public CAL_Turno                  Turno                  { get; set; }
        public CAL_AnalisisPale           AnalisisPale           { get; set; }
        public List<CAL_AnalisisPaleTest> AnalisisPaleTestList   { get; set; }
        public CAL_Turno                  TurnoAnalisisPale      { get; set; }
        public CAL_Turno2                 Turno2                 { get; set; }

        public List<CAL_Envasador> GetEnvasadores(int IdGrupoEnvasador)
        {
            return dcSoftwareCalidad.CAL_Envasador.Where(X => X.IdGrupoEnvasador == IdGrupoEnvasador).ToList();
        }
    }
}