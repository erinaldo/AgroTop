using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class AgricultorRelacionadoViewModel
    {
        public int IdAgricultor { get; set; }
        public string Rut { get; set; }
        public string Nombre { get; set; }

        public static List<AgricultorRelacionadoViewModel> FromDB(AgroFichasDBDataContext dc, Agricultor agricultor)
        {
            return agricultor.RelacionadosHijos(dc, false);
        }
    }
}