using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroFichasLib
{
    public class Ficha
    {
        public static string Hash(int idFicha, int idPredio, string userIns)
        {
            return Hasher.CreateHash("m$%Sh" + idFicha.ToString() + idPredio.ToString() + userIns);
        }

        public static string Hash2(int idFicha, int idPredio)
        {
            return Hasher.CreateHash("m$%Sh" + idFicha.ToString() + idPredio.ToString() + "movil");
        }
    }
}
