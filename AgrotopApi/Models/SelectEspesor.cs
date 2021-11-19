using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectEspesor
    {
        public int IdEspesor { get; set; }

        public decimal Min { get; set; }

        public decimal Max { get; set; }

        public decimal Avg { get; set; }

        public string Observaciones { get; set; }
    }
}