using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.AppLayer.Standarizadores
{
    public class LinazaCalculator
    {
        public static CalculatorResult Normalizar(AgroFichasDBDataContext dc, ProcesoIngreso ingreso)
        {
            var result = new CalculatorResult();

            result.PesoBruto = ingreso.PesoBruto.Value;

            var parametros = dc.ParametroAnalisis.ToList();

            var humedad = ingreso.ValorAnalisis.Single(va => va.IdParametroAnalisis == 40);
            var impureza = ingreso.ValorAnalisis.Single(va => va.IdParametroAnalisis == 41);

            var paHumedad = parametros.Single(pa => pa.IdParametroAnalisis == humedad.IdParametroAnalisis); //To allow not peristed Valor Analisis to work
            var paImpureza = parametros.Single(pa => pa.IdParametroAnalisis == impureza.IdParametroAnalisis);

            var rangoHumedad = dc.RangoAnalisis.FirstOrDefault(ra => ra.IdParametroAnalisis == 40 && humedad.Valor.Value >= ra.Desde && humedad.Valor.Value <= ra.Hasta);
            var factorHumedad = (rangoHumedad != null ? rangoHumedad.Bonificacion : -humedad.Valor.Value);

            var rangoImpureza = dc.RangoAnalisis.FirstOrDefault(ra => ra.IdParametroAnalisis == 41 && impureza.Valor.Value >= ra.Desde && impureza.Valor.Value <= ra.Hasta);
            var factorImpureza = (rangoImpureza != null ? rangoImpureza.Bonificacion : -impureza.Valor.Value);

            decimal descuentoHumedadKg = 0;
            decimal descuentoImpurezaKg = 0;
            if (humedad.Valor.Value > 9)
                descuentoHumedadKg = (result.PesoBruto * ((humedad.Valor.Value - 9) / 100));
            if (impureza.Valor.Value > 3)
                descuentoImpurezaKg = (result.PesoBruto * ((impureza.Valor.Value - 3) / 100));

            result.PesoNormal = (int)Math.Round(result.PesoBruto - (descuentoHumedadKg + descuentoImpurezaKg));
            result.Items = new List<CalculatorItem>();

            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = humedad.IdParametroAnalisis,
                NombreParametro = paHumedad.Nombre,
                Valor = humedad.Valor.Value,
                ValorString = humedad.ToString(paHumedad),
                Factor = ((factorHumedad + humedad.Valor.Value) * -1),
                FactorString = ((factorHumedad + humedad.Valor.Value) * -1).ToString("+#,##0.00;-#,##0.00") + " %",
                BonoAprox = descuentoHumedadKg * -1
            });

            result.Items.Add(new CalculatorItem()
            {
                IdParametroAnalisis = impureza.IdParametroAnalisis,
                NombreParametro = paImpureza.Nombre,
                Valor = impureza.Valor.Value,
                ValorString = impureza.ToString(paImpureza),
                Factor = ((impureza.Valor.Value + factorImpureza) * -1),
                FactorString = ((impureza.Valor.Value + factorImpureza) * -1).ToString("+#,##0.00;-#,##0.00") + " %",
                BonoAprox = descuentoImpurezaKg * -1
            });

            return result;
        }
    }
}