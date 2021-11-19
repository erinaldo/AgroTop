using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public interface ISemanaAnoSource
    {
        int? Ano { get; set; }
        int? Semana { get; set; }
    }
}