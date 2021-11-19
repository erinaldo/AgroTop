using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer.Standarizadores
{
    public class RapsCalculator
    {
        public static CalculatorResult Normalizar(AgroFichasDBDataContext dc, ProcesoIngreso ingreso)
        {
            var result = new CalculatorResult();

            result.PesoBruto = ingreso.PesoBruto.Value;
            
            var parametros = dc.ParametroAnalisis.ToList();
            var humedade = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 1);
            var matgrasa = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 2);
            var impureza = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 3);

            var factorHumedade = !humedade.Valor.HasValue ? 1.0M : Math.Round((1.0M - humedade.Valor.Value / 100.0M) / (1.0M - 0.09M), 4);
            var factorMatgrasa = !matgrasa.Valor.HasValue ? 1.0M : Math.Round(matgrasa.Valor.Value / 48.0M, 4);
            var factorImpureza = !impureza.Valor.HasValue ? 1.0M : Math.Round((1.0M - impureza.Valor.Value / 100.0M) / (1.0M - 0.03M), 4);

            result.PesoNormal = (int)Math.Round(result.PesoBruto * factorHumedade * factorMatgrasa * factorImpureza, 0);
            result.Items = new List<CalculatorItem>();

            var paHumedad = parametros.Single(pa => pa.IdParametroAnalisis == humedade.IdParametroAnalisis); //To allow not peristed Valor Analisis to work
            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = humedade.IdParametroAnalisis,
                NombreParametro = paHumedad.Nombre, 
                Valor = humedade.Valor,
                ValorString = humedade.ToString(paHumedad),
                Factor = factorHumedade,
                FactorString = factorHumedade.ToString("#,##0.0000"),
                BonoAprox = null
            });

            var paMatGrasa = parametros.Single(pa => pa.IdParametroAnalisis == matgrasa.IdParametroAnalisis); 
            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = matgrasa.IdParametroAnalisis,
                NombreParametro = paMatGrasa.Nombre,
                Valor = matgrasa.Valor,
                ValorString = matgrasa.ToString(paMatGrasa),
                Factor = factorMatgrasa,
                FactorString = factorMatgrasa.ToString("#,##0.0000"),
                BonoAprox = null
            });

            var paImpureza = parametros.Single(pa => pa.IdParametroAnalisis == impureza.IdParametroAnalisis);
            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = impureza.IdParametroAnalisis,
                NombreParametro = paImpureza.Nombre,
                Valor = impureza.Valor,
                ValorString = impureza.ToString(paImpureza),
                Factor = factorImpureza,
                FactorString = factorImpureza.ToString("#,##0.0000"),
                BonoAprox = null
            });

            return result;
        }
    }


}