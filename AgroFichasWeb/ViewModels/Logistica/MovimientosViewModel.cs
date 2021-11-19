using AgroFichasWeb.Controllers;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels
{
    public class MovimientosViewModel
    {
        public MovimientosViewModel() { }

        public PaginatedList<LOG_Movimiento> Movimientos { get; set; }
        public PaginatedList<LOG_Movimiento> MovimientosPorBusqueda { get; set; }

        #region Filtro Requerimiento
        public int? IdRequerimiento { get; set; }
        public int? Key { get; set; }
        public IEnumerable<SelectListItem> SelectListRequerimientos { get; set; }
        #endregion

        #region VARS de Construcción de Vista
        public int Columnas { get; set; }
        public bool MostrarCompletar { get; set; }
        public bool MostrarEditar { get; set; }
        public bool MostrarEliminar { get; set; }
        #endregion

        #region VARS de Mensajería
        public string ErrorMessage { get; set; }
        public string OKMessage { get; set; }
        #endregion
    }
}