using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectFichaTecnicaParametroAnalisis
    {
        public int IdParametroAnalisis { get; set; }

        public string Nombre { get; set; }

        public string Nombre_en { get; set; }

        public string UM { get; set; }

        public string UM_en { get; set; }

        public decimal MinValidValue { get; set; }

        public decimal MaxValidValue { get; set; }
    }
}