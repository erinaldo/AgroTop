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
            var peso1000granos = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 8);

            var granosBajo21Mm    = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 13);
            var pesoHectolitro    = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 21);
            var granosDobles      = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 58);
            var granosPelados     = ingreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 59);

            var paGranosBajo21Mm  = parametros.Single(pa => pa.IdParametroAnalisis == 13);
            var paPesoHectolitro  = parametros.Single(pa => pa.IdParametroAnalisis == 21);
            var paGranosDobles    = parametros.Single(pa => pa.IdParametroAnalisis == 58);
            var paGranosPelados   = parametros.Single(pa => pa.IdParametroAnalisis == 59);

            var factorGranosBajo21Mm = 0M;
            var factorPesoHectolitro = 0M;
            var factorGranosDobles   = 0M;
            var factorGranosPelados  = 0M;
            if (granosBajo21Mm != null && granosBajo21Mm.Valor.HasValue)
            {
                factorGranosBajo21Mm = dc.RangoAnalisis.First(ra => ra.IdParametroAnalisis == 13 && granosBajo21Mm.Valor.Value >= ra.Desde && granosBajo21Mm.Valor.Value <= ra.Hasta).Bonificacion;
            }
            if (pesoHectolitro != null && pesoHectolitro.Valor.HasValue)
            {
                factorPesoHectolitro = dc.RangoAnalisis.First(ra => ra.IdParametroAnalisis == 21 && pesoHectolitro.Valor.Value >= ra.Desde && pesoHectolitro.Valor.Value <= ra.Hasta).Bonificacion;
            }
            if (granosDobles != null && granosDobles.Valor.HasValue)
            {
                factorGranosDobles = dc.RangoAnalisis.First(ra => ra.IdParametroAnalisis == 58 && granosDobles.Valor.Value >= ra.Desde && granosDobles.Valor.Value <= ra.Hasta).Bonificacion;
            }
            if (granosPelados != null && granosPelados.Valor.HasValue)
            {
                factorGranosPelados = dc.RangoAnalisis.First(ra => ra.IdParametroAnalisis == 59 && granosPelados.Valor.Value >= ra.Desde && granosPelados.Valor.Value <= ra.Hasta).Bonificacion;
            }

            var paPeso1000Granos = parametros.Single(pa => pa.IdParametroAnalisis == 8);
            var paHumedad = parametros.Single(pa => pa.IdParametroAnalisis == humedad.IdParametroAnalisis); //To allow not peristed Valor Analisis to work
            var paImpureza = parametros.Single(pa => pa.IdParametroAnalisis == impureza.IdParametroAnalisis);

            //Si no se encuentra valor de descuento para la impureza, se descuento 1:1
            var rangoImpureza = dc.RangoAnalisis.FirstOrDefault(ra => ra.IdParametroAnalisis == 5 && impureza.Valor.Value >= ra.Desde && impureza.Valor.Value <= ra.Hasta);
            var factorImpureza = 0M;
            if (rangoImpureza != null)
            {
                factorImpureza = rangoImpureza.Bonificacion;
            }
            else
            {
                factorImpureza = -impureza.Valor.Value;
                if (ingreso.IdTemporada >= 7)
                    factorImpureza += 1.5m;
            }

            //Humedad según tabla
            var factorHumedad = dc.RangoAnalisis.First(ra => ra.IdParametroAnalisis == 6 && humedad.Valor.Value >= ra.Desde && humedad.Valor.Value <= ra.Hasta).Bonificacion;
           

            //Peso de los mil granos es procedimental, pivotea alrededor de 38
            decimal factorPeso1000granos = (peso1000granos != null ? peso1000granos.Valor.Value - 38 : 0);

            //Si hay bonificación, solo es aplicable si la humedad es inferior a 13.5
            if (factorPeso1000granos > 0 && humedad.Valor.Value >= 13.5M)
                factorPeso1000granos = 0;

            //La bonificación máxima es 3;
            if (factorPeso1000granos > 3)
                factorPeso1000granos = 3;

            result.PesoNormal = (int)Math.Round(result.PesoBruto * (1.0M + (factorImpureza + factorHumedad + factorPeso1000granos + factorGranosBajo21Mm + factorPesoHectolitro + factorGranosDobles + factorGranosPelados) / 100.0M), 0);
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

            if (peso1000granos != null)
            {
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
            }

            if (granosBajo21Mm != null && granosBajo21Mm.Valor.HasValue)
            {
                result.Items.Add(new CalculatorItem()
                {
                    IdParametroAnalisis = granosBajo21Mm.IdParametroAnalisis,
                    NombreParametro = paGranosBajo21Mm.Nombre,
                    Valor = granosBajo21Mm.Valor.Value,
                    ValorString = granosBajo21Mm.ToString(paGranosBajo21Mm),
                    Factor = factorGranosBajo21Mm,
                    FactorString = factorGranosBajo21Mm.ToString("+#,##0.00;-#,##0.00") + " %",
                    BonoAprox = Math.Round(factorGranosBajo21Mm * result.PesoBruto / 100M, 2)
                });
            }

            if (pesoHectolitro != null && pesoHectolitro.Valor.HasValue)
            {
                result.Items.Add(new CalculatorItem()
                {
                    IdParametroAnalisis = pesoHectolitro.IdParametroAnalisis,
                    NombreParametro = paPesoHectolitro.Nombre,
                    Valor = pesoHectolitro.Valor.Value,
                    ValorString = pesoHectolitro.ToString(paPesoHectolitro),
                    Factor = factorPesoHectolitro,
                    FactorString = factorPesoHectolitro.ToString("+#,##0.00;-#,##0.00") + " %",
                    BonoAprox = Math.Round(factorPesoHectolitro * result.PesoBruto / 100M, 2)
                });
            }

            if (granosDobles != null && granosDobles.Valor.HasValue)
            {
                result.Items.Add(new CalculatorItem()
                {
                    IdParametroAnalisis = granosDobles.IdParametroAnalisis,
                    NombreParametro = paGranosDobles.Nombre,
                    Valor = granosDobles.Valor.Value,
                    ValorString = granosDobles.ToString(paGranosDobles),
                    Factor = factorGranosDobles,
                    FactorString = factorGranosDobles.ToString("+#,##0.00;-#,##0.00") + " %",
                    BonoAprox = Math.Round(factorGranosDobles * result.PesoBruto / 100M, 2)
                });
            }

            if (granosPelados != null && granosPelados.Valor.HasValue)
            {
                result.Items.Add(new CalculatorItem()
                {
                    IdParametroAnalisis = granosPelados.IdParametroAnalisis,
                    NombreParametro = paGranosPelados.Nombre,
                    Valor = granosPelados.Valor.Value,
                    ValorString = granosPelados.ToString(paGranosPelados),
                    Factor = factorGranosPelados,
                    FactorString = factorGranosPelados.ToString("+#,##0.00;-#,##0.00") + " %",
                    BonoAprox = Math.Round(factorGranosPelados * result.PesoBruto / 100M, 2)
                });
            }

            return result;
        }
    }
}