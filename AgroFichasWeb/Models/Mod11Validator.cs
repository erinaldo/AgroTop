using System;

public class Mod11Validator
{
    public int Numero { get; set; }
    public string DigitoVerificador { get; set; }

    public Mod11Validator(int numero, string digitoVerificador)
    {
        this.Numero = numero;
        this.DigitoVerificador = digitoVerificador.ToUpper();
    }

    public bool ObtenerValidezMod11()
    {
        return this.DigitoVerificador == CalcularDigitoVerificador(this.Numero);
    }

    private string CalcularDigitoVerificador(int numero)
    {
        string cadenaNumero = numero.ToString();
        int calculador = 0;

        int[] factores = { 3, 2, 7, 6, 5, 4, 3, 2 };
        int indiceFactor = factores.Length - 1;

        for (int i = cadenaNumero.Length - 1; i >= 0; i--)
        {
            calculador = calculador + (factores[indiceFactor] * int.Parse(cadenaNumero.Substring(i, 1)));
            indiceFactor--;
        }

        string digitoVerificador;
        int resultado = 11 - (calculador % 11);

        if (resultado == 11)
        {
            digitoVerificador = "0";
        }
        else if (resultado == 10)
        {
            digitoVerificador = "K";
        }
        else
        {
            digitoVerificador = resultado.ToString();
        }

        return digitoVerificador;
    }
}