using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agrotop.ReporteControlPlanta
{
    class Utility
    {
        public static void RepTemp(ref String Template, String Key, String Value)
        {
            Template = Template.Replace("***" + Key + "***", Value);
        }
    }
}
