using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.Logistica.Utilidades
{
    public class CssClasses
    {
        public static string GetEstadoPedido(int IdPedido)
        {
            var pedido = LogisticaHelper.GetPedidoHabilitadoExistente(IdPedido);
            switch (pedido.IdEstado)
            {
                case 1:
                    return "enEsperaDeCamion";
                case 2:
                    return "enTransitoAPlantaDeOrigen";
                case 3:
                    return "enTransitoAPlantaDeDestino";
                case 4:
                    return "enRevisionDeDestino";
                case 5:
                    return "listoParaLiquidar";
                case 6:
                    return "propuestaDeLiquidacionCreada";
                case 7:
                    return "liquidado";
            }

            throw new Exception("Estado no definido");
        }

        public static string GetEstadoRequerimiento(int IdEstado)
        {
            switch (IdEstado)
            {
                case 1:
                    return "creado";
                case 2:
                    return "listoParaLiquidar";
                case 3:
                    return "liquidado";
                case 99:
                    return "anulado";
            }

            throw new Exception("Estado no definido");
        }
    }

    public class LogisticaHelper
    {
        static AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public enum Proviene
        {
            Origen,
            Destino
        }

        public static string GetBodega(int IdPedido, Proviene Proviene)
        {
            var pedido = GetPedidoHabilitadoExistente(IdPedido);
            if (pedido.Origen.HasValue && Proviene == Proviene.Origen)
            {
                return pedido.Bodega.Nombre;
            }

            if (pedido.Destino.HasValue && Proviene == Proviene.Destino)
            {
                return pedido.Bodega1.Nombre;
            }

            return (Proviene == Proviene.Origen ? (pedido.OtroOrigen ?? "") : (pedido.OtroDestino ?? ""));
        }

        public static LOG_AsignacionCamion GetCamionAsignado(int IdPedido)
        {
            return dc.LOG_AsignacionCamion.SingleOrDefault(x => x.IdPedido == IdPedido);
        }

        public static LOG_AsignacionCamion GetCamionAsignadoExistente(int IdPedido)
        {
            return dc.LOG_AsignacionCamion.Single(x => x.IdPedido == IdPedido);
        }

        public static LOG_Camion GetCamionPorTransportistaYHabilitado(int IdCamion, int IdTransportista)
        {
            return dc.LOG_Camion.SingleOrDefault(x => x.IdCamion == IdCamion && x.IdTransportista == IdTransportista && x.Habilitado == true);
        }

        public static LOG_Chofer GetChoferHabilitado(int IdChofer)
        {
            return dc.LOG_Chofer.SingleOrDefault(x => x.IdChofer == IdChofer && x.Habilitado == true);
        }

        public static LOG_Chofer GetChoferPorTransportistaYHabilitado(int IdChofer, int IdTransportista)
        {
            return dc.LOG_Chofer.SingleOrDefault(x => x.IdChofer == IdChofer && x.IdTransportista == IdTransportista && x.Habilitado == true);
        }

        public static List<LOG_ControlCamionPlanta> GetControles(int IdPedido)
        {
            return dc.LOG_ControlCamionPlanta.Where(x => x.IdPedido == IdPedido).ToList();
        }

        public static LOG_ControlCamionPlanta GetEstadoControlCamionPlantaPorControlesYaHechos(List<LOG_ControlCamionPlanta> Controles, int IdEstadoControlCamionPlanta)
        {
            return Controles.SingleOrDefault(x => x.IdEstadoControlCamionPlanta == IdEstadoControlCamionPlanta);
        }

        public static LOG_Movimiento GetMovimientoPorPedidoYHabilitado(int IdPedido)
        {
            return dc.LOG_Movimiento.FirstOrDefault(x => x.IdPedido == IdPedido && x.Habilitado == true);
        }

        public static LOG_AsignacionPedido GetPedidoAsignadoExistente(int IdPedido)
        {
            return dc.LOG_AsignacionPedido.Single(x => x.IdPedido == IdPedido);
        }

        public static LOG_AsignacionPedido GetPedidoAsignadoExistenteParaEdicion(int IdPedido, AgroFichasDBDataContext dc)
        {
            return dc.LOG_AsignacionPedido.Single(x => x.IdPedido == IdPedido);
        }

        public static LOG_Pedido GetPedidoHabilitado(int IdPedido)
        {
            return dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == IdPedido && x.Habilitado == true);
        }

        public static LOG_Pedido GetPedidoHabilitadoExistente(int IdPedido)
        {
            return dc.LOG_Pedido.Single(x => x.IdPedido == IdPedido && x.Habilitado == true);
        }

        public static LOG_Pedido GetPedidoHabilitadoExistenteParaEdicion(int IdPedido, AgroFichasDBDataContext dc)
        {
            return dc.LOG_Pedido.Single(x => x.IdPedido == IdPedido && x.Habilitado == true);
        }

        public static List<GetPedidosAsignadosPorRequerimientoResult> GetPedidosAsignadosPorRequerimientoYHabilitados(int IdRequerimiento)
        {
            return dc.GetPedidosAsignadosPorRequerimiento(IdRequerimiento).OrderByDescending(x => x.FechaHoraIns).ToList();
        }

        public static List<GetPedidosAsignadosPorRequerimientoYKeywordResult> GetPedidosAsignadosPorRequerimientoYPorKeywordYHabilitados(int IdRequerimiento, string keyword)
        {
            return dc.GetPedidosAsignadosPorRequerimientoYKeyword(IdRequerimiento, keyword).OrderByDescending(x => x.FechaHoraIns).ToList();
        }

        public static LOG_Requerimiento GetRequerimiento(int IdRequerimiento)
        {
            return dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == IdRequerimiento);
        }

        public static LOG_Requerimiento GetRequerimientoCreado(int IdRequerimiento)
        {
            return dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == IdRequerimiento && x.IdEstado == 1);
        }

        public static LOG_Requerimiento GetRequerimientoExistente(int IdRequerimiento)
        {
            return dc.LOG_Requerimiento.Single(x => x.IdRequerimiento == IdRequerimiento);
        }

        public static string GetSucursal(LOG_Pedido pedido, Proviene Proviene)
        {
            if (pedido.Origen.HasValue && Proviene == Proviene.Origen)
            {
                return pedido.Bodega.Sucursal.Nombre;
            }

            if (pedido.Destino.HasValue && Proviene == Proviene.Destino)
            {
                return pedido.Bodega1.Sucursal.Nombre;
            }

            return (Proviene == Proviene.Origen ? (pedido.OtroOrigen ?? "") : (pedido.OtroDestino ?? ""));
        }

        public static string GetSucursal(int IdPedido, Proviene Proviene)
        {
            var pedido = GetPedidoHabilitadoExistente(IdPedido);
            if (pedido.Origen.HasValue && Proviene == Proviene.Origen)
            {
                return pedido.Bodega.Sucursal.Nombre;
            }

            if (pedido.Destino.HasValue && Proviene == Proviene.Destino)
            {
                return pedido.Bodega1.Sucursal.Nombre;
            }

            return (Proviene == Proviene.Origen ? (pedido.OtroOrigen ?? "") : (pedido.OtroDestino ?? ""));
        }

        public static LOG_Transportista GetTransportistaExistente(int IdTransportista)
        {
            return dc.LOG_Transportista.Single(x => x.IdTransportista == IdTransportista);
        }

        public static LOG_Transportista GetTransportistaHabilitado(int IdTransportista)
        {
            return dc.LOG_Transportista.SingleOrDefault(x => x.IdTransportista == IdTransportista && x.Habilitado == true);
        }
    }
}