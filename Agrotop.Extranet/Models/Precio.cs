using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public class Precio
    {
        public int IdMoneda { get; set; }
        public string SimboloMoneda { get; set; }
        public string NombreMoneda { get; set; }
        public string FormatoMoneda { get; set; }
        public decimal Valor { get; set; }
        public string ValorFormateado 
        {
            get
            {
                return String.Format(this.FormatoMoneda, this.Valor);
            }
        }
    }
}