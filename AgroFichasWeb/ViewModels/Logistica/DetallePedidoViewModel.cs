using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Logistica
{
    public class DetallePedidoViewModel
    {
        public DetallePedidoViewModel() { }

        public LOG_AsignacionCamion CamionAsignado { get; set; }
        public LOG_Movimiento Movimiento { get; set; }
        public LOG_Pedido Pedido { get; set; }
        public LOG_AsignacionPedido PedidoAsignado { get; set; }
        public LOG_Requerimiento Requerimiento { get; set; }

        #region Timing
        public List<LOG_ControlCamionPlanta> Controles { get; set; }
        public LOG_ControlCamionPlanta LlegadaPorteriaOrigen { get; set; }
        public LOG_ControlCamionPlanta InicioCargaOrigen { get; set; }
        public LOG_ControlCamionPlanta TerminoCargaOrigen { get; set; }
        public LOG_ControlCamionPlanta SalidaPorteriaOrigen { get; set; }
        public LOG_ControlCamionPlanta LlegadaPorteriaDestino { get; set; }
        public LOG_ControlCamionPlanta InicioCargaDestino { get; set; }
        public LOG_ControlCamionPlanta TerminoCargaDestino { get; set; }
        public LOG_ControlCamionPlanta SalidaPorteriaDestino { get; set; }
        public TimeSpan TiempoTotalCamionCargaOrigen { get; set; }
        public TimeSpan TiempoTotalCamionCargaDestino { get; set; }
        public TimeSpan TiempoTotalCamionPlantaOrigen { get; set; }
        public TimeSpan TiempoTotalCamionPlantaDestino { get; set; }
        #endregion
    }
}