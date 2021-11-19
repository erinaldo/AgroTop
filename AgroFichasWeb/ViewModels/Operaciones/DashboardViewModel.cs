using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Operaciones
{
    public class DashboardViewModel
    {
        public DashboardViewModel() { }

        public List<rpt_OPR_OperadoresDelDiaResult> OperadoresDelDia { get; set; }

        public rpt_OPR_ResumenDelDiaResult ResumenDelDia { get; set; }

        public List<rpt_OPR_TurnosDelDiaResult> TurnosDelDia { get; set; }

        public List<rpt_OPR_TotalPlantaDelDiaResult> TotalPlantaDelDia { get; set; }

        public List<rpt_OPR_TotalDespachoDelDiaResult> TotalDespachoDelDia { get; set; }

        public bool PuedeVerProductividad { get; set; }
    }
}