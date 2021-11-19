using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer.Standarizadores
{
    public class MaizCalculator
    {
        public static CalculatorResult Normalizar(AgroFichasDBDataContext dc, ProcesoIngreso ingreso)
        {
            var result = new CalculatorResult();

            result.PesoBruto = ingreso.PesoBruto.Value;
            
            var parametros = dc.ParametroAnalisis.ToList();
            var humedade = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 62);
            var granoPartido = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 63);
            var impureza = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 64);

            var factorHumedade = 0.0M;
            if(humedade.Valor.Value > 14.5M)
                factorHumedade = !humedade.Valor.HasValue ? 1.0M : Math.Round((humedade.Valor.Value - 14.5M)/(100 - 14.5M), 4)/*(1.0M - humedade.Valor.Value / 100.0M) / (1.0M - 0.09M), 4*/;

            var factorGranoPartido = 0.0M;
            if(granoPartido.Valor.Value > 1.5M)
                factorGranoPartido = !granoPartido.Valor.HasValue ? 1.0M : Math.Round((granoPartido.Valor.Value - 1.5M)/(100-1.5M), 4)/*matgrasa.Valor.Value / 48.0M, 4*/;

            var factorImpureza = 0.0M;
            if(impureza.Valor.Value > 0.5M)
                factorImpureza = !impureza.Valor.HasValue ? 1.0M : Math.Round((impureza.Valor.Value - 0.5M) / (100 - 0.5M), 4/*(1.0M - impureza.Valor.Value / 100.0M) / (1.0M - 0.03M), 4*/);

            var descHumedad = Math.Round((result.PesoBruto * factorHumedade) * -1, 2);
            var descGranoPartido = Math.Round((result.PesoBruto * factorGranoPartido) * -1, 2);
            var descImpureza = Math.Round((result.PesoBruto * factorImpureza) * -1, 2);

            result.PesoNormal = (int)Math.Round(result.PesoBruto + (descHumedad + descGranoPartido + descImpureza), 0);
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
                BonoAprox = descHumedad
            });

            var paMatGrasa = parametros.Single(pa => pa.IdParametroAnalisis == granoPartido.IdParametroAnalisis); 
            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = granoPartido.IdParametroAnalisis,
                NombreParametro = paMatGrasa.Nombre,
                Valor = granoPartido.Valor,
                ValorString = granoPartido.ToString(paMatGrasa),
                Factor = factorGranoPartido,
                FactorString = factorGranoPartido.ToString("#,##0.0000"),
                BonoAprox = descGranoPartido
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
                BonoAprox = descImpureza
            });

            return result;
        }
    }


}