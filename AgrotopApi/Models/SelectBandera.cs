using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectBandera
    {
        public int IdPosicion { get; set; }
        public int IdContenedor { get; set; }

        public string Descripcion { get; set; }
    }
}