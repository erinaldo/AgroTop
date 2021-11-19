using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Operaciones
{
    public class RegistroProductividadViewModel
    {
        public OPR_RegistroTurno                               RegistroTurno               { get; set; }
        public List<rpt_OPR_ProduccionTurnoResult>             ProduccionTurno             { get; set; }
        public List<rpt_OPR_ProduccionTurnoEstabilizadoResult> ProduccionTurnoEstabilizado { get; set; }
        public List<OPR_RegistroEnvasado>                      RegistroEnvasados           { get; set; }
        public List<OPR_RegistroGeneracionMaxisacos>           RegistroGeneracionMaxisacos { get; set; }
        public OPR_GetTurnoAnteriorSiguienteResult             TurnoAnteriorSiguiente      { get; set; }

        public string GetRendimientoCssClass(string proceso, decimal rendimiento)
        {
            switch (proceso)
            {
                case "Línea 1 y 2":
                    if (rendimiento < 70)
                    {
                        return "progress-bar-danger";
                    }
                    else
                    {
                        return "progress-bar-success";
                    }
                case "Estabilizado 1":
                    if (rendimiento < 98)
                    {
                        return "progress-bar-danger";
                    }
                    else
                    {
                        return "progress-bar-success";
                    }
                case "Estabilizado 2":
                    if (rendimiento < 95)
                    {
                        return "progress-bar-danger";
                    }
                    else
                    {
                        return "progress-bar-success";
                    }
                case "Cortado":
                    if (rendimiento < 95)
                    {
                        return "progress-bar-danger";
                    }
                    else
                    {
                        return "progress-bar-success";
                    }
                case "Laminado":
                    if (rendimiento < 97)
                    {
                        return "progress-bar-danger";
                    }
                    else
                    {
                        return "progress-bar-success";
                    }
            }

            return "";
        }
    }
}