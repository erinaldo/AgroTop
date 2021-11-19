using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public enum ContratoSearchType
    {
        ParaConvenioPrecio = 0
    }

    public class ContratoSelectorViewModel
    {
        public int IdContrato { get; set; }
        public Temporada Temporada { get; set; }
        public string NumeroContrato { get; set; }
        public string Nombre { get; set; }
        public string NombreCultivo { get; set; }
        public string NombreEmpresa { get; set; }
    }

    public class ContratoFinderViewModel
    {
        public ContratoSearchType SearchType { get; set; }
    }
}