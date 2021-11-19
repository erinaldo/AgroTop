using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Logistica
{
    public class DetalleRequerimientoViewModel
    {
        public DetalleRequerimientoViewModel(int IdRequerimiento)
        {
            Init();
            this.PedidosAsignados = LogisticaHelper.GetPedidosAsignadosPorRequerimientoYHabilitados(IdRequerimiento);
        }

        public DetalleRequerimientoViewModel(int IdRequerimiento, string keyword)
        {
            Init();
            this.PedidosAsignadosPorBusqueda = LogisticaHelper.GetPedidosAsignadosPorRequerimientoYPorKeywordYHabilitados(IdRequerimiento, keyword);
        }

        public void Init()
        {
            this.PedidosAsignados = new List<GetPedidosAsignadosPorRequerimientoResult>();
            this.PedidosAsignadosPorBusqueda = new List<GetPedidosAsignadosPorRequerimientoYKeywordResult>();
        }

        public List<GetPedidosAsignadosPorRequerimientoResult> PedidosAsignados { get; set; }
        public List<GetPedidosAsignadosPorRequerimientoYKeywordResult> PedidosAsignadosPorBusqueda { get; set; }
        public LOG_Requerimiento Requerimiento { get; set; }

        #region VARS de Construcción de Vista
        public int Columnas { get; set; }
        public bool MostrarCrear { get; set; }
        public bool MostrarEditar { get; set; }
        public bool MostrarEliminar { get; set; }
        #endregion

        #region VARS de Mensajería
        public string ErrorMessage { get; set; }
        public string OKMessage { get; set; }
        #endregion
    }
}