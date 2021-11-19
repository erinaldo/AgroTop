using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public partial class ValorAnalisis
    {
        public override string ToString()
        {
            if (this.Valor.HasValue)
                return this.Valor.Value.ToString(this.ParametroAnalisis.FormatString) + " " + this.ParametroAnalisis.UM;
            else
                return "-";
        }
    }
}