using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer.Standarizadores
{
    public class AvenaCalculator
    {
        public static CalculatorResult Normalizar(AgroFichasDBDataContext dc, ProcesoIngreso ingreso)
        {
            var result = new CalculatorResult();

            result.PesoBruto = ingreso.PesoBruto.Value;

            var parametros = dc.ParametroAnalisis.ToList();

            var impureza = ingreso.ValorAnalisis.Single(va => va.IdParametroAnalisis == 5);
            var humedad = ingreso.ValorAnalisis.Single(va => va.IdParametroAnalisis == 6);
            var peso1000granos = ingreso.ValorAnalisis.Single(va => va.IdParametroAnalisis == 8);

            var paPeso1000Granos = parametros.Single(pa => pa.IdParametroAnalisis == peso1000granos.IdParametroAnalisis);
            var paHumedad = parametros.Single(pa => pa.IdParametroAnalisis == humedad.IdParametroAnalisis); //To allow not peristed Valor Analisis to work
            var paImpureza = parametros.Single(pa => pa.IdParametroAnalisis == impureza.IdParametroAnalisis);

            //Si no se encuentra valor de descuento para la impureza, se descuento 1:1
            var rangoImpureza = dc.RangoAnalisis.FirstOrDefault(ra => ra.IdParametroAnalisis == 5 && impureza.Valor.Value >= ra.Desde && impureza.Valor.Value <= ra.Hasta);
            var factorImpureza = rangoImpureza != null ? rangoImpureza.Bonificacion : -impureza.Valor.Value;

            //Humedad según tabla
            var factorHumedad = dc.RangoAnalisis.First(ra => ra.IdParametroAnalisis == 6 && humedad.Valor.Value >= ra.Desde && humedad.Valor.Value <= ra.Hasta).Bonificacion;
           

            //Peso de los mil granos es procedimental, pivotea alrededor de 38
            decimal factorPeso1000granos = peso1000granos.Valor.Value - 38;

            //Si hay bonificación, solo es aplicable si la humedad es inferior a 13.5
            if (factorPeso1000granos > 0 && humedad.Valor.Value >= 13.5M)
                factorPeso1000granos = 0;

            //La bonificación máxima es 3;
            if (factorPeso1000granos > 3)
                factorPeso1000granos = 3;

            result.PesoNormal = (int)Math.Round(result.PesoBruto * (1.0M + (factorImpureza + factorHumedad + factorPeso1000granos) / 100.0M), 0);
            result.Items = new List<CalculatorItem>();

            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = humedad.IdParametroAnalisis,
                NombreParametro = paHumedad.Nombre,
                Valor = humedad.Valor.Value,
                ValorString = humedad.ToString(paHumedad),
                Factor = factorHumedad,
                FactorString = factorHumedad.ToString("+#,##0.00;-#,##0.00") + " %",
                BonoAprox = Math.Round(factorHumedad * result.PesoBruto / 100M, 2)
            });

            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = impureza.IdParametroAnalisis,
                NombreParametro = paImpureza.Nombre,
                Valor = impureza.Valor.Value,
                ValorString = impureza.ToString(paImpureza),
                Factor = factorImpureza,
                FactorString = factorImpureza.ToString("+#,##0.00;-#,##0.00") + " %",
                BonoAprox = Math.Round(factorImpureza * result.PesoBruto / 100M, 2)
            });

            
            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = peso1000granos.IdParametroAnalisis,
                NombreParametro = paPeso1000Granos.Nombre,
                Valor = peso1000granos.Valor.Value,
                ValorString = peso1000granos.ToString(paPeso1000Granos),
                Factor = factorPeso1000granos,
                FactorString = factorPeso1000granos.ToString("+#,##0.00;-#,##0.00") + " %",
                BonoAprox = Math.Round(factorPeso1000granos * result.PesoBruto / 100M, 2)
            });

            return result;
        }
    }
}