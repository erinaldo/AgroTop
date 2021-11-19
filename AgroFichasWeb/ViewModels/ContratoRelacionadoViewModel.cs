using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    //public class ContratoRelacionadoViewModel
    //{
    //    public int IdContrato { get; set; }
    //    public string Numero { get; set; }
    //    public string Nombre { get; set; }
    //    public string Empresa { get; set; }

    //    public static List<ContratoRelacionadoViewModel> FromDB(AgroFichasDBDataContext dc, ConvenioPrecio convenio)
    //    {
    //        return (from rel in convenio.ConvenioPrecioContrato
    //               select new ContratoRelacionadoViewModel()
    //               {
    //                   IdContrato = rel.IdContrato,
    //                   Numero = rel.Contrato.NumeroContrato,
    //                   Empresa = rel.Contrato.Empresa.Nombre,
    //                   Nombre = " - " + rel.Contrato.CultivoContrato.Nombre + " - " + rel.Contrato.Agricultor.Nombre
    //               }).ToList();
    //    }
    //}
}