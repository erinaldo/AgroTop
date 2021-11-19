using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Operaciones
{
    public class LineaProduccionViewModel
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public LineaProduccionViewModel() {
            this.ListaEquiposPorLineaProduccion = new List<EquiposPorLineaProduccion>();
        }

        #region PROPIEDADES

        #region MENSAJES

        public string MensajeError { get; set; }
        public string MensajeExito { get; set; }

        #endregion

        #region PERMISOS
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool PuedeAsociarEquipos { get; set; }
        public bool PuedeVerEquipos { get; set; }
        #endregion

        /// <summary>
        /// Lista todos las líneas de producción y sus equipos asociados
        /// </summary>
        public List<EquiposPorLineaProduccion> ListaEquiposPorLineaProduccion { get; set; }
        /// <summary>
        /// Lista todos los equipos asociados a una línea de producción
        /// </summary>
        public List<OPR_GetEquiposAsociadosPorLineaProduccionResult> EquiposAsociadosPorLineaProduccion { get; set; }
        public string IdEquipos { get; set; }
        public OPR_LineaProduccion LineaProduccion { get; set; }
        #endregion
    }

    public class EquiposPorLineaProduccion
    {
        public OPR_LineaProduccion LineaProduccion { get; set; }
        public List<OPR_Equipo> Equipos { get; set; }
    }
}