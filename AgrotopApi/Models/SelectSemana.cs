using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectSemana
    {
        public int NumeroDeLaSemana { get; set; }

        public DateTime FechaDeInicio { get; set; }

        public DateTime FechaFinal { get; set; }
    }
}