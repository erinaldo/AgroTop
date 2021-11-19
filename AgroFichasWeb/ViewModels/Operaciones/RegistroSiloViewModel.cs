using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Operaciones
{
    public class RegistroSiloViewModel
    {
        public RegistroSiloViewModel() { }

        #region PROPIEDADES

        #region MENSAJES
        public string MensajeError { get; set; }
        public string MensajeExito { get; set; }
        #endregion

        #region PERMISOS
        public bool PuedeCrear    { get; set; }
        public bool PuedeEditar   { get; set; }
        public bool PuedeEliminar { get; set; }
        #endregion

        public string                              IdSilos                  { get; set; }
        public string                              IdSilosExt               { get; set; }
        public List<OPR_RegistroSilo>              RegistroSilos            { get; set; }
        public List<OPR_RegistroSilo>              RegistroSilosEfectivo    { get; set; }
        public List<OPR_RegistroSilo>              RegistroSilosExt         { get; set; }
        public List<OPR_RegistroSilo>              RegistroSilosEfectivoExt { get; set; }
        public OPR_RegistroTurno                   RegistroTurno            { get; set; }
        public OPR_RegistroTurno                   RegistroTurnoEfectivo    { get; set; }
        public List<OPR_Silo>                      Silos                    { get; set; }
        public OPR_Turno                           Turno                    { get; set; }
        public OPR_GetTurnoAnteriorSiguienteResult TurnoAnteriorSiguiente   { get; set; }

        #endregion

        /// <summary>
        /// Calcula la cubicación de los silos
        /// </summary>
        /// <param name="registroSilo"></param>
        /// <returns></returns>
        public decimal CalcularCubicacion(OPR_RegistroSilo registroSilo, bool calcularMaximaCubicacion = false)
        {
            var densidad = registroSilo.OPR_Silo.OPR_Densidad.Valor;

            if (calcularMaximaCubicacion)
            {
                if (registroSilo.OPR_Silo.IdTipoSilo == 1)
                    return (densidad * ((2 * registroSilo.OPR_Silo.AlturaBase) * (8 - 0)) + (densidad * 2 * registroSilo.OPR_Silo.AlturaBase * registroSilo.OPR_Silo.AlturaCono / 3)) * 1000;

                if (registroSilo.OPR_Silo.IdTipoSilo == 2)
                    return (densidad * ((4.5m * 4.5m * 3.14m) * registroSilo.OPR_Silo.OPR_TipoSilo.UnidadesMaximas) + (densidad * (4.5m * 4.5m) * 3.14m * registroSilo.OPR_Silo.AlturaCono / 3)) * 1000;
            }

            if (registroSilo.OPR_Silo.IdTipoSilo == 1)
                if (registroSilo.Unidades == 9)
                    return 0;
                else
                    return (densidad * ((2 * registroSilo.OPR_Silo.AlturaBase) * (8 - registroSilo.Unidades)) + (densidad * 2 * registroSilo.OPR_Silo.AlturaBase * registroSilo.OPR_Silo.AlturaCono / 3)) * 1000;

            if (registroSilo.OPR_Silo.IdTipoSilo == 2)
                return (densidad * ((4.5m * 4.5m * 3.14m) * registroSilo.Unidades) + (densidad * (4.5m * 4.5m) * 3.14m * registroSilo.OPR_Silo.AlturaCono / 3)) * 1000;

            throw new Exception("No se puede calcular la cubicación del silo");
        }
    }
}