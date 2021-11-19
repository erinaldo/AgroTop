using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class TipoRecomendacion
    {
        public static IEnumerable<SelectListItem> SelectList(int? idTipoRecomendacion)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, idTipoRecomendacion);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int? idTipoRecomendacion)
        {
            return from t in dc.TipoRecomendacion
                   where t.IdTipoRecomendacion != 3
                   orderby t.Nombre
                   select new SelectListItem()
                   {
                       Selected = (t.IdTipoRecomendacion == idTipoRecomendacion && idTipoRecomendacion != null),
                       Text = t.Nombre,
                       Value = t.IdTipoRecomendacion.ToString()
                   };
        }
    }
}