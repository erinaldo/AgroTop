using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApiSap.Models
{
    public class Recepcion
    {
        public int CardCode { get; set; }
        public string Comments { get; set; }
        public string DocCurrency { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime TaxDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public string FolioPrefixString { get; set; }
        public int FolioNumber { get; set; }
        public string NumAtCard { get; set; }
        public int U_liquidacion { get; set; }
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string TaxCode { get; set; }

    }
}