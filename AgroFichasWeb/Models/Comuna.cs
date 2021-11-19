using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class Comuna
    {
        public static IEnumerable<SelectListItem> SelectList(int? IdComuna)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, IdComuna);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int? IdComuna)
        {
            return from s in dc.Comuna
                   orderby s.Nombre
                   select new SelectListItem
                   {
                       Selected = (s.IdComuna == IdComuna && IdComuna != null),
                       Text = s.Nombre,
                       Value = s.IdComuna.ToString()
                   };
        }

    }
}