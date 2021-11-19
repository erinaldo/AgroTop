using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class TipoFicha
    {
        public static IEnumerable<SelectListItem> SelectList(int? idTipoFicha)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, idTipoFicha);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int? idTipoFicha)
        {
            return from s in dc.TipoFicha
                   orderby s.Nombre
                   select new SelectListItem
                   {
                       Selected = (s.IdTipoFicha == idTipoFicha && idTipoFicha != null),
                       Text = s.Nombre,
                       Value = s.IdTipoFicha.ToString()
                   };
        }
    }
}