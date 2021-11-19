using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class TipoSiembra
    {
        public static IEnumerable<SelectListItem> SelectList(int? idTipoSiembra)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, idTipoSiembra);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int? idTipoSiembra)
        {
            return from s in dc.TipoSiembra
                   orderby s.Nombre
                   select new SelectListItem
                   {
                       Selected = (s.IdTipoSiembra == idTipoSiembra && idTipoSiembra != null),
                       Text = s.Nombre,
                       Value = s.IdTipoSiembra.ToString()
                   };
        }
    }
}