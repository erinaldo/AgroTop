using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public enum AgriculorSearchType
    {
        ParaRelacionado = 0
    }

    public class AgricultorSelectorViewModel
    {
        public int IdAgricultor { get; set; }
        public string Nombre { get; set; }
        public string Titulo { get; set; }
    }

    public class AgriculorFinderViewModel
    {
        public AgriculorSearchType SearchType { get; set; }
        public int IdAgricultorArgument { get; set; }
    }

}