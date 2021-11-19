using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class RecomendacionViewModel
    {
        public int Id { get; set; }
        public int IdQuimico { get; set; }
        public decimal Dosis { get; set; }
        public int IdUM { get; set; }
        public string FechaAplicacion { get; set; }

        public decimal FerN { get; set; }
        public decimal FerP2O5 { get; set; }
        public decimal FerKO2 { get; set; }
        public decimal FerMgO { get; set; }
        public decimal FerS { get; set; }
        public decimal FerB { get; set; }
        public decimal FerZn { get; set; }
        public decimal FerCaO { get; set; }
    }
}