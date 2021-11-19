using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class ImportanciaSeguimiento
    {
        public static IEnumerable<SelectListItem> SelectList(int? idImportanciaSeguimiento)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, idImportanciaSeguimiento);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int? idImportanciaSeguimiento)
        {
            return from s in dc.ImportanciaSeguimiento
                   orderby s.IdImportanciaSeguimiento
                   select new SelectListItem
                   {
                       Selected = (s.IdImportanciaSeguimiento == idImportanciaSeguimiento && idImportanciaSeguimiento != null),
                       Text = s.Nombre,
                       Value = s.IdImportanciaSeguimiento.ToString()
                   };
        }
    }
}