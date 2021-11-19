using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Operaciones
{
    public class TurnoViewModel
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public TurnoViewModel() { }

        #region PROPIEDADES

        #region MENSAJES
        public string MensajeError { get; set; }
        public string MensajeExito { get; set; }
        #endregion

        #region PERMISOS
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool PuedeVerBalanzas { get; set; }
        public bool PuedeVerDetenciones { get; set; }
        public bool PuedeVerEnvasado { get; set; }
        public bool PuedeVerGeneracionMaxisacos { get; set; }
        public bool PuedeVerProductividad { get; set; }
        public bool PuedeVerSilos { get; set; }
        #endregion

        public SYS_User Operador { get; set; }
        public OPR_RegistroTurno RegistroTurnoActual { get; set; }
        public List<vw_OPR_RegistroTurnos> RegistroTurnos { get; set; }
        public OPR_Turno Turno { get; set; }

        #endregion

        public string GetFechaHora(DateTime fechaHoraIns, TimeSpan hora)
        {
            return string.Format(@"{0:dd-MM-yyyy} {1:hh\:mm\:ss}", fechaHoraIns, hora);
        }

        public string GetOperador(int userID)
        {
            return dc.SYS_User.Single(X => X.UserID == userID).FullName;
        }
    }
}