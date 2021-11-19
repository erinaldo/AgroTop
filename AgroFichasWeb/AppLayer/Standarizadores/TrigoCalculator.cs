using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer.Standarizadores
{
    public class TrigoCalculator
    {
        public static CalculatorResult Normalizar(ProcesoIngreso ingreso)
        {
            var result = new CalculatorResult();

            result.PesoBruto = ingreso.PesoBruto.Value;
            result.PesoNormal = result.PesoBruto;

            result.Items = new List<CalculatorItem>();

            return result;
        }
    }
}