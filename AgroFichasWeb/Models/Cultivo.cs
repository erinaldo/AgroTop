using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class Cultivo
    {

        public static IEnumerable<Cultivo> CultivosParaIntencionSiembra(AgroFichasDBDataContext dc)
        {
            return (from cc in dc.CultivoContrato
                    join cu in dc.Cultivo on cc.IdCultivo equals cu.IdCultivo
                    select cu).Distinct();
        }

        public static IEnumerable<SelectListItem> SelectListParaIntencionSiembra(int? idCultivo)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectListParaIntencionSiembra(dc, idCultivo);
        }

        public static IEnumerable<SelectListItem> SelectListParaIntencionSiembra(AgroFichasDBDataContext dc, int? idCultivo)
        {
            return from s in CultivosParaIntencionSiembra(dc)
                   orderby s.Nombre
                   select new SelectListItem
                   {
                       Selected = (s.IdCultivo == idCultivo && idCultivo != null),
                       Text = s.Nombre,
                       Value = s.IdCultivo.ToString()
                   };
        }

        public static IEnumerable<SelectListItem> SelectList(int? idCultivo)
        {
            var dc = new AgroFichasDBDataContext();
            return SelectList(dc, idCultivo);
        }

        public static IEnumerable<SelectListItem> SelectList(AgroFichasDBDataContext dc, int? idCultivo)
        {
            return from s in dc.Cultivo
                   orderby s.Nombre
                   select new SelectListItem
                   {
                       Selected = (s.IdCultivo == idCultivo && idCultivo != null),
                       Text = s.Nombre,
                       Value = s.IdCultivo.ToString()
                   };
        }


    }
}