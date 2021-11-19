using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class TemporadaSelectorViewModel
    {
        public Temporada Temporada { get; set; }
        public List<Temporada> Temporadas { get; set; }
        public string HrefMask { get; set; }
    }
}