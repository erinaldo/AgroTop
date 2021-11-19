using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.TrazaTop
{
    public class PlanillaContratoViewModel
    {
        public List<Temporada> Temporadas { get; set; }

        public List<Cultivo> Cultivos { get; set; }

        public string UserIns { get; set; }

        public DateTime FechaHoraIns { get; set; }

        public string IpIns { get; set; }

        public string UserUpd { get; set; }

        public DateTime? FechaHoraUpd { get; set; }

        public string IpUpd { get; set; }

        public List<Permiso> Permisos { get; set; }

        public Permiso Permiso { get; set; }
    }
}