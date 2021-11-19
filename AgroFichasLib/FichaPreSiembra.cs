using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroFichasLib
{
    public class FichaPreSiembra
    {
        public static string Hash(int idFichaPreSiembra, int idPredio, string userIns)
        {
            return Hasher.CreateHash("Js7sdjhS8(" + idFichaPreSiembra.ToString() + idPredio.ToString() + userIns);
        }

        public static string Hash2(int idFichaPreSiembra, int idPredio)
        {
            return Hasher.CreateHash("Js7sdjhS8(" + idFichaPreSiembra.ToString() + idPredio.ToString() + "movil");
        }
    }
}
