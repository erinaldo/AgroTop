using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Temporada
    {
        public static Temporada TemporadaActiva()
        {
            var dc = new AgroFichasDBDataContext();
            return dc.Temporada.Where(t => t.Activa).FirstOrDefault();
        }

        public static Temporada TemporadaActivaFichas()
        {
            var dc = new AgroFichasDBDataContext();
            return dc.Temporada.Where(t => t.ActivaFichas).FirstOrDefault();
        }

    }
}