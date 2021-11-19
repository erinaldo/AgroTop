using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.SoftwareCalidad
{
    public class CALTurnoPale
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades
        public DateTime Date { get; set; }
        public TimeSpan Desde { get; set; }
        public DateTime FechaHoraIns { get; set; }
        public TimeSpan Hasta { get; set; }
        public int Horas { get; set; }
        public int IdDetalleOrdenProduccion { get; set; }
        public int IdPale { get; set; }
        public int IdSubproducto { get; set; }
        public TimeSpan Time { get; set; }
        public string Turno { get; set; }
        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones

        #endregion
    }
}