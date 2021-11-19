using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class FotoFichaViewModel
    {
        public int Id { get; set; }
        public string FotoUrl { get; set; }
        public string Observaciones { get; set; }
        public string FileName { get; set; }
    }
}