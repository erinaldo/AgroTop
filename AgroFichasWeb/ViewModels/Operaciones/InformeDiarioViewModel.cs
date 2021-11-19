using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Operaciones
{
    public class InformeDiarioViewModel
    {
        public InformeDiarioViewModel() { }

        public List<rpt_OPR_InformeDiario_ProduccionEnPlantaResult> ProduccionEnPlanta { get; set; }

        public List<rpt_OPR_InformeDiario_DetencionesResult> Detenciones { get; set; }

        public List<rpt_OPR_InformeDiario_EvolucionProduccionesDiariasResult> EvolucionProduccionesDiarias { get; set; }

        public List<rpt_OPR_InformeDiario_ConsumosDiariosMateriaPrimaResult> ConsumosDiariosMateriaPrima { get; set; }

        public List<rpt_OPR_InformeDiario_ConsumoAcumuladoMateriaPrimaResult> ConsumoAcumuladoMateriaPrima { get; set; }

        public string ConvertMinutosAHoras(int minutes)
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(minutes);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }
    }
}