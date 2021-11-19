using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class SelectorSucursalViewModel
    {
        public int IdSucursal { get; set; }
        public string NombreSucursal { get; set; }
        public bool Seleccionado { get; set; }

        public static List<SelectorSucursalViewModel> ForConvenioPrecio(AgroFichasDBDataContext dc, int idConvenioPrecio)
        {
            var result = (from r in dc.SelectSucursalesConvenioPrecio(idConvenioPrecio)
                          select new SelectorSucursalViewModel()
                          {
                             IdSucursal = r.IdSucursal,
                             NombreSucursal = r.Nombre,
                             Seleccionado = r.Selected.Value
                          }).ToList();
            
            return result;
        }

        public static List<SelectorSucursalViewModel> ForConvenioPrecioAjuste(AgroFichasDBDataContext dc, int idConvenioPrecioAjuste)
        {
            var result = (from r in dc.SelectSucursalesConvenioPrecioAjuste(idConvenioPrecioAjuste)
                          select new SelectorSucursalViewModel()
                          {
                              IdSucursal = r.IdSucursal,
                              NombreSucursal = r.Nombre,
                              Seleccionado = r.Selected.Value
                          }).ToList();

            return result;
        }
    }
}