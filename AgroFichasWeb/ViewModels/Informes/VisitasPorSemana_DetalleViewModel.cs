using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Informes
{
    public class VisitasPorSemana_DetalleViewModel
    {
        public int? Semana { get; set; }
        public Temporada Temporada { get; set; }
        public Agricultor Agricultor { get; set; }
        public List<Ficha> Fichas { get; set; }
        public SYS_User UsuarioIngreso { get; set; }
    }
}