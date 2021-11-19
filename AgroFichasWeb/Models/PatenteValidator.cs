using AgroFichasWeb.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

public class PatenteValidator
{
    public static string formatearPatente(string patente)
    {
        if (String.IsNullOrEmpty(patente))
        {
            throw new Exception("La patente viene vacía");
        }

        if (patente.Length < 6)
        {
            throw new Exception("La patente viene mal formada");
        }

        patente = patente.Replace("-", "");

        var par1 = patente.Substring(0, 2);
        var par2 = patente.Substring(2, 2);
        var par3 = patente.Substring(4, 2);

        return string.Format("{0}-{1}-{2}", par1, par2, par3);
    }

}