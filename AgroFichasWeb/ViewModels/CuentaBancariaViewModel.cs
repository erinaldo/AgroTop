using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class CuentaBancariaViewModel
    {
        public int IdCuentaBancaria { get; set; }
        public int IdTipoCuentaBancaria { get; set; }
        public int IdBanco { get; set; }
        public string NumeroCuenta { get; set; }
        public string Comentarios { get; set; }

        public string NombreTipoCuentaBancaria { get; set; }
        public string NombreBanco { get; set; }

        public static List<CuentaBancariaViewModel> FromDB(AgroFichasDBDataContext dc, Agricultor agricultor)
        {
            return (from c in agricultor.CuentaBancaria
                    select new CuentaBancariaViewModel()
                    {
                        IdCuentaBancaria =  c.IdCuentaBancaria,
                        IdTipoCuentaBancaria = c.IdTipoCuentaBancaria,
                        IdBanco = c.IdBanco,
                        NumeroCuenta = c.NumeroCuenta,
                        Comentarios = c.Comentarios,
                        NombreBanco = c.Banco.Nombre,
                        NombreTipoCuentaBancaria = c.TipoCuentaBancaria.Nombre
                    }).ToList();
        }

        public static int NextId(AgroFichasDBDataContext dc)
        {
            var last = dc.CuentaBancaria.Max(ic => (int?)ic.IdCuentaBancaria);
            return (last ?? 0) + 1000;
        }
    }
}