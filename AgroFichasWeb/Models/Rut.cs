using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgroFichasWeb.Models
{
    public class Rut
    {
        /// <summary>
        /// Metodo de validación de rut con digito verificador
        /// dentro de la cadena
        /// </summary>
        /// <param name="rut">string</param>
        /// <returns>booleano</returns>
        public static bool ValidaRut(string rut)
        {
            if (String.IsNullOrEmpty(rut))
                return false;
            rut = rut.Replace(".", "").ToUpper();
            Regex expresion = new Regex("^([0-9]+-[0-9K])$");
            string dv = rut.Substring(rut.Length - 1, 1);
            if (!expresion.IsMatch(rut))
            {
                return false;
            }
            char[] charCorte = { '-' };
            string[] rutTemp = rut.Split(charCorte);
            if (dv != Digito(int.Parse(rutTemp[0])))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Método que valida el rut con el digito verificador
        /// por separado
        /// </summary>
        /// <param name="rut">integer</param>
        /// <param name="dv">char</param>
        /// <returns>booleano</returns>
        public static bool ValidaRut(string rut, string dv)
        {
            return ValidaRut(rut + "-" + dv);
        }

        public static string FormatearRut(string rut)
        {
            int cont = 0;
            string format;
            if (rut.Length == 0)
            {
                return "";
            }
            else
            {
                rut = rut.Replace(".", "");
                rut = rut.Replace("-", "");
                format = "-" + rut.Substring(rut.Length - 1);
                for (int i = rut.Length - 2; i >= 0; i--)
                {
                    format = rut.Substring(i, 1) + format;
                    cont++;
                    if (cont == 3 && i != 0)
                    {
                        format = "." + format;
                        cont = 0;
                    }
                }
                return format;
            }
        }

        public static string NomarlizarRut(string rut)
        {
            return rut.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();
        }

        /// <summary>
        /// método que calcula el digito verificador a partir
        /// de la mantisa del rut
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        private static string Digito(int rut)
        {
            int suma = 0;
            int multiplicador = 1;
            while (rut != 0)
            {
                multiplicador++;
                if (multiplicador == 8)
                    multiplicador = 2;
                suma += (rut % 10) * multiplicador;
                rut = rut / 10;
            }
            suma = 11 - (suma % 11);
            if (suma == 11)
            {
                return "0";
            }
            else if (suma == 10)
            {
                return "K";
            }
            else
            {
                return suma.ToString();
            }
        }
    }
}