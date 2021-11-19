using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.ControlTiempo
{
    public class InformeGeneralViewModel
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public List<rpt_CTR_ControlDeTiempoResult> InformeGeneral { get; set; }
        public int IdEstado { get; set; }
        public int IdIncremento { get; set; }

        public IEnumerable<SelectListItem> GetEstados(int? IdEstado)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CTR_Estado
                                                     orderby X.IdEstado descending
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdEstado == IdEstado && IdEstado != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdEstado.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetIncrementos(int? IdIntervalo)
        {
            List<Incremento> list = new List<Incremento>();
            list.Add(new Incremento() { IdIncremento = -1,  Descripcion = "Últimas 24 horas" });
            list.Add(new Incremento() { IdIncremento = -7,  Descripcion = "Últimos 7 días" });
            list.Add(new Incremento() { IdIncremento = -30, Descripcion = "Últimos 30 días" });
            list.Add(new Incremento() { IdIncremento = -60, Descripcion = "Últimos 60 días" });
            list.Add(new Incremento() { IdIncremento = -90, Descripcion = "Últimos 90 días" });


            IEnumerable<SelectListItem> selectList = from X in list
                                                     orderby X.IdIncremento
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdIncremento == IdIntervalo && IdIntervalo != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdIncremento.ToString()
                                                     };
            return selectList;
        }

        public string GetPromedio(double Promedio_Minuto)
        {
            TimeSpan t = TimeSpan.FromMinutes(Promedio_Minuto);
            return string.Format(@"{0:D2}d {1:D2}:{2:D2}:{3:D2}", t.Days, t.Hours, t.Minutes, t.Seconds);
        }
    }

    public class Incremento
    {
        public int IdIncremento { get; set; }

        public string Descripcion { get; set; }
    }
}