using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer.Standarizadores
{
    public class CalculatorResult
    {
        public int PesoBruto { get; set; }
        public int PesoNormal { get; set; }
        public int Bonificacion
        {
            get
            {
                return this.PesoNormal - this.PesoBruto;
            }
        }
        public List<CalculatorItem> Items { get; set; }
    }

    public class CalculatorItem
    {
        public int IdParametroAnalisis { get; set; }
        public string NombreParametro { get; set; }
        public decimal? Valor { get; set; }
        public string ValorString { get; set; }
        public decimal Factor { get; set; }
        public string FactorString { get; set; }
        public decimal? BonoAprox { get; set; }
    }
}