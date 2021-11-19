using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class EstadoSiembra
    {
        public static IEnumerable<SelectListItem> SelectList(int? idEstadoSiembra)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, idEstadoSiembra);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int? idEstadoSiembra)
        {
            return from s in dc.EstadoSiembra
                   orderby s.IdEstadoSiembra
                   select new SelectListItem
                   {
                       Selected = (s.IdEstadoSiembra == idEstadoSiembra && idEstadoSiembra != null),
                       Text = s.Nombre,
                       Value = s.IdEstadoSiembra.ToString()
                   };
        }
    }
}