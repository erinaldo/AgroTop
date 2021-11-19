using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Operaciones
{
    public class RegistroBalanzaViewModel
    {
        public RegistroBalanzaViewModel() { }

        #region PROPIEDADES

        #region MENSAJES
        public string                    MensajeError            { get; set; }
        public string                    MensajeExito            { get; set; }
        #endregion

        #region PERMISOS
        public bool PuedeCrear    { get; set; }
        public bool PuedeEditar   { get; set; }
        public bool PuedeEliminar { get; set; }
        #endregion

        public List<OPR_Balanza>                   Balanzas                 { get; set; }
        public string                              IdBalanzas               { get; set; }
        public List<OPR_RegistroBalanza>           RegistroBalanzas         { get; set; }
        public List<OPR_RegistroBalanza>           RegistroBalanzasEfectivo { get; set; }
        public OPR_RegistroTurno                   RegistroTurno            { get; set; }
        public OPR_RegistroTurno                   RegistroTurnoEfectivo    { get; set; }
        public OPR_Turno                           Turno                    { get; set; }
        public OPR_GetTurnoAnteriorSiguienteResult TurnoAnteriorSiguiente   { get; set; }

        #endregion
    }
}