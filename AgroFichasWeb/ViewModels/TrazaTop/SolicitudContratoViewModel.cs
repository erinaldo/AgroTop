using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.TrazaTop
{
    public class SolicitudContratoViewModel
    {
        Models.AgroFichasDBDataContext context = new Models.AgroFichasDBDataContext();

        public List<SolicitudContrato> SolicitudContratos { get; set; }

        public List<Permiso> Permisos { get; set; }

        public Permiso Permiso { get; set; }

        public IEnumerable<SelectListItem> GetTemporadas(int? IdTemporada = null)
        {
            IEnumerable<SelectListItem> selectList = (from t in context.Temporada
                                                      orderby t.Nombre
                                                      select new SelectListItem
                                                      {
                                                          Selected = (t.IdTemporada == IdTemporada && IdTemporada != null),
                                                          Text = t.Nombre,
                                                          Value = t.IdTemporada.ToString()
                                                      });
            return selectList;
        }
    }
}