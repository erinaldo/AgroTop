using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models.Logistica.Utilidades
{
    public class LogisticaSelectList
    {
        private static AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public static IEnumerable<SelectListItem> SetCamiones(int IdTransportista, int? IdCamion = null)
        {
            IEnumerable<SelectListItem> selectList =
                from x in dc.LOG_Camion
                where x.IdTransportista == IdTransportista
                && x.Habilitado == true
                orderby x.Patente
                select new SelectListItem
                {
                    Selected = (x.IdCamion == IdCamion && IdCamion != null),
                    Text = x.Patente,
                    Value = x.IdCamion.ToString()
                };
            return selectList;
        }

        public static IEnumerable<SelectListItem> SetChoferes(int IdTransportista, int? IdChofer = null)
        {
            IEnumerable<SelectListItem> selectList =
                from x in dc.LOG_Chofer
                where x.IdTransportista == IdTransportista
                && x.Habilitado == true
                orderby x.Nombre
                select new SelectListItem
                {
                    Selected = (x.IdChofer == IdChofer && IdChofer != null),
                    Text = x.Nombre,
                    Value = x.IdChofer.ToString()
                };
            
            return selectList;
        }

        public static IEnumerable<SelectListItem> SetMarcas(int? IdMarca)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_Marca
                where s.Habilitado == true
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdMarca == IdMarca && IdMarca != null),
                    Text = s.Nombre,
                    Value = s.IdMarca.ToString()
                };
            return selectList;
        }

        public static IEnumerable<SelectListItem> SetRequerimientos(int? IdRequerimiento)
        {
            IEnumerable<SelectListItem> selectList =
                from x in dc.LOG_Requerimiento
                where x.IdEstado == 1
                orderby x.IdRequerimiento
                select new SelectListItem
                {
                    Selected = (x.IdRequerimiento == IdRequerimiento && IdRequerimiento != null),
                    Text = x.Glosa,
                    Value = x.IdRequerimiento.ToString()
                };
            return selectList;
        }

        public static IEnumerable<SelectListItem> SetTipoCamiones(int? IdTipoCamion)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_TipoCamion
                orderby s.Descripcion
                select new SelectListItem
                {
                    Selected = (s.IdTipoCamion == IdTipoCamion && IdTipoCamion != null),
                    Text = s.Descripcion,
                    Value = s.IdTipoCamion.ToString()
                };
            return selectList;
        }

        public static IEnumerable<SelectListItem> SetTipoDescargas(int? IdTipoDescarga)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_TipoDescarga
                orderby s.Descripcion
                select new SelectListItem
                {
                    Selected = (s.IdTipoDescarga == IdTipoDescarga && IdTipoDescarga != null),
                    Text = s.Descripcion,
                    Value = s.IdTipoDescarga.ToString()
                };
            return selectList;
        }

        public static IEnumerable<SelectListItem> SetTipoDoctoContrato(int? IdTipoDoctoContrato)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.TipoDoctoContrato
                orderby s.IdTipoDoctoContrato
                select new SelectListItem
                {
                    Selected = (s.IdTipoDoctoContrato == IdTipoDoctoContrato && IdTipoDoctoContrato != null),
                    Text = s.Descripcion,
                    Value = s.IdTipoDoctoContrato.ToString()
                };
            return selectList;
        }

        public static IEnumerable<SelectListItem> SetTransportistas(int? IdTransportista)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_Transportista
                where s.Habilitado == true
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdTransportista == IdTransportista && IdTransportista != null),
                    Text = s.Nombre,
                    Value = s.IdTransportista.ToString()
                };
            return selectList;
        }
    }
}