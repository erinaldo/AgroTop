using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Operaciones
{
    public class RegistroGeneracionMaxisacosViewModel
    {
        public RegistroGeneracionMaxisacosViewModel() { }

        #region PROPIEDADES

        #region MENSAJES
        public string MensajeError { get; set; }
        public string MensajeExito { get; set; }
        #endregion

        #region PERMISOS
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        #endregion

        public List<OPR_RegistroGeneracionMaxisacos> RegistroGeneracionMaxisacos { get; set; }
        public OPR_RegistroTurno                     RegistroTurno               { get; set; }
        public OPR_Turno                             Turno                       { get; set; }
        public OPR_GetTurnoAnteriorSiguienteResult   TurnoAnteriorSiguiente      { get; set; }

        #endregion
    }
}