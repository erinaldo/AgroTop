using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{

    public class ItemContratoViewModel
    {
        public int IdItemContrato { get; set; }
        public int IdCultivoContrato { get; set; }
        public string NombreCultivoContrato { get; set; }
        public int Cantidad { get; set; }
        public int Superficie { get; set; }

        public static List<ItemContratoViewModel> FromDB(AgroFichasDBDataContext dc, Contrato contrato)
        {
            return (from item in contrato.ItemContrato
                    select new ItemContratoViewModel()
                    {
                        IdItemContrato = item.IdItemContrato,
                        IdCultivoContrato = item.IdCultivoContrato,
                        NombreCultivoContrato = item.CultivoContrato.Nombre,
                        Cantidad = item.Cantidad,
                        Superficie = item.Superficie
                    }).ToList();
        }

        public static int NextId(AgroFichasDBDataContext dc)
        {
            var last = dc.ItemContrato.Max(ic => (int?)ic.IdItemContrato);
            return (last ?? 0) + 1000;
        }
    }
}