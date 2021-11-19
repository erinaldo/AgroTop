using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class CsvExportBatchRecepcionesEncabezado
    {
        public int DocNum { get; set; }
        public string DocType { get; set; }
        public string CardCode { get; set; }
        public string Comments { get; set; }
        public string DocCurrency { get; set; }
        public string DocDate { get; set; }
        public string TaxDate { get; set; }
        public string DocDueDate { get; set; }
        public string FolioPrefixString { get; set; }
        public int FolioNumber { get; set; }
        public string NumAtCard { get; set; }
        public int DocTotalFC { get; set; }
        public int U_liquidacion { get; set; }

        public static List<CsvExportBatchRecepcionesEncabezado> FromExportBatch(AgroFichasDBDataContext dc, ExportBatch batch)
        {
            var result = new List<CsvExportBatchRecepcionesEncabezado>();

            foreach (var pi in batch.ProcesoIngreso)
            {
                string fecha = pi.FechaPesoFinal.Value.Year >= 2014 ? pi.FechaPesoFinal.Value.ToString("yyyyMMdd") : "20140101";

                bool usingSpot = false;
                decimal precioCLP = 0;
                decimal total = pi.Valorizar(dc, out precioCLP, out usingSpot).Value;
                var precio = Math.Round(total / (decimal)pi.PesoBruto * 1000, 1);

                var item = new CsvExportBatchRecepcionesEncabezado()
                {
                    DocNum = pi.IdProcesoIngreso,
                    DocType = "dDocument_Items",
                    CardCode = pi.Agricultor.SAPID(pi.IdEmpresa),
                    Comments = "Ingreso " + pi.IdProcesoIngreso + ". " + pi.CultivoContrato.Nombre + " " + pi.Variedad.Nombre,
                    DocCurrency = "$",
                    DocDate = fecha,
                    TaxDate = fecha,
                    DocDueDate = fecha,
                    FolioPrefixString = "GD",
                    FolioNumber = pi.NumeroGuia,
                    NumAtCard = "",
                    DocTotalFC = (int)pi.ValorizarConIva(dc).Value,
                    U_liquidacion = pi.IdPrimeraLiquidacion() ?? 0
                };

                result.Add(item);
            }

            return result;
        }
    }

    public class CsvExportBatchRecepcionesDetalle
    {
        public int DocNum { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string Currency { get; set; }
        public decimal UnitPrice { get; set; }
        public string TaxCode { get; set; }
        public string WarehouseCode { get; set; }
        public decimal RowTotalFC { get; set; }

        public static List<CsvExportBatchRecepcionesDetalle> FromExportBatch(AgroFichasDBDataContext dc, ExportBatch batch)
        {
            var result = new List<CsvExportBatchRecepcionesDetalle>();

            foreach (var pi in batch.ProcesoIngreso)
            {
                bool usingSpot = false;
                decimal precioCLP = 0;
                decimal total = pi.Valorizar(dc, out precioCLP, out usingSpot).Value;
                var precio = Math.Round(total / (decimal)pi.PesoBruto, 2);

                var item = new CsvExportBatchRecepcionesDetalle()
                {
                    DocNum = pi.IdProcesoIngreso,
                    LineNum = 1,
                    ItemCode = pi.CultivoContrato.SAPID,
                    Quantity = pi.PesoBruto.Value,
                    Currency = "CLP",
                    UnitPrice = precio,
                    TaxCode = "IVA",
                    WarehouseCode = pi.Bodega.SAPID(pi.IdEmpresa),
                    RowTotalFC = total
                };
                result.Add(item);
            }

            return result;
        }
    }

    public class CsvExportBatchRecepcionesEncabezadoEspecial
    {
        public int DocNum { get; set; }
        public string DocType { get; set; }
        public string CardCode { get; set; }
        public string Comments { get; set; }
        public string DocCurrency { get; set; }
        public string DocDate { get; set; }
        public string TaxDate { get; set; }
        public string DocDueDate { get; set; }
        public string FolioPrefixString { get; set; }
        public int FolioNumber { get; set; }
        public string NumAtCard { get; set; }
        public int DocTotal { get; set; }
        public int U_liquidacion { get; set; }

        public static List<CsvExportBatchRecepcionesEncabezadoEspecial> FromExportBatch(AgroFichasDBDataContext dc, ExportBatch batch)
        {
            var result = new List<CsvExportBatchRecepcionesEncabezadoEspecial>();

            foreach (var pi in batch.ProcesoIngreso)
            {
                string fecha = pi.FechaPesoInicial.Value.Year >= 2014 ? pi.FechaPesoFinal.Value.ToString("yyyyMMdd") : "20140101";

                bool usingSpot = false;
                decimal precioCLP = 0;
                decimal total = pi.Valorizar(dc, out precioCLP, out usingSpot).Value;
                var precio = Math.Round(total / (decimal)pi.PesoBruto * 1000, 1);

                var item = new CsvExportBatchRecepcionesEncabezadoEspecial()
                {
                    DocNum = pi.IdProcesoIngreso,
                    DocType = "dDocument_Items",
                    CardCode = pi.Agricultor.SAPID(pi.IdEmpresa),
                    Comments = "Ingreso " + pi.IdProcesoIngreso + ". " + pi.CultivoContrato.Nombre + " " + pi.Variedad.Nombre,
                    DocCurrency = "$",
                    DocDate = fecha,
                    TaxDate = fecha,
                    DocDueDate = fecha,
                    FolioPrefixString = "GD",
                    FolioNumber = pi.NumeroGuia,
                    NumAtCard = "",
                    DocTotal = (int)pi.ValorizarConIva(dc).Value,
                    U_liquidacion = pi.IdPrimeraLiquidacion() ?? 0
                };

                result.Add(item);
            }

            return result;
        }
    }

    public class CsvExportBatchRecepcionesDetalleEspecial
    {
        public int DocNum { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string Currency { get; set; }
        public decimal UnitPrice { get; set; }
        public string TaxCode { get; set; }
        public string WarehouseCode { get; set; }
        public decimal LineTotal { get; set; }

        public static List<CsvExportBatchRecepcionesDetalleEspecial> FromExportBatch(AgroFichasDBDataContext dc, ExportBatch batch)
        {
            var result = new List<CsvExportBatchRecepcionesDetalleEspecial>();

            foreach (var pi in batch.ProcesoIngreso)
            {
                bool usingSpot = false;
                decimal precioCLP = 0;
                decimal total = pi.Valorizar(dc, out precioCLP, out usingSpot).Value;
                var precio = Math.Round(total / (decimal)pi.PesoBruto, 2);

                var item = new CsvExportBatchRecepcionesDetalleEspecial()
                {
                    DocNum = pi.IdProcesoIngreso,
                    LineNum = 1,
                    ItemCode = pi.CultivoContrato.SAPID,
                    Quantity = pi.PesoBruto.Value,
                    Currency = "$",
                    UnitPrice = precio,
                    TaxCode = "IVA",
                    WarehouseCode = pi.Bodega.SAPID(pi.IdEmpresa),
                    LineTotal = total
                };
                result.Add(item);
            }

            return result;
        }
    }

    public class ExportBatchRecepcionesSAP
    {
        public string CardCode { get; set; }
        public string Comments { get; set; }
        public string DocCurrency { get; set; }
        public string DocDate { get; set; }
        public string TaxDate { get; set; }
        public string DocDueDate { get; set; }
        public string FolioPrefixString { get; set; }
        public int FolioNumber { get; set; }
        public string NumAtCard { get; set; }
        public int U_liquidacion { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string TaxCode { get; set; }


        public static List<ExportBatchRecepcionesSAP> FromExportBatchSAP(AgroFichasDBDataContext dc, ExportBatch batch)
        {
            var result = new List<ExportBatchRecepcionesSAP>();

            foreach (var pi in batch.ProcesoIngreso)
            {
                string fecha = pi.FechaPesoFinal.Value.Year >= 2014 ? pi.FechaPesoFinal.Value.ToString("yyyyMMdd") : "20140101";

                bool usingSpot = false;
                decimal precioCLP = 0;
                decimal total = pi.Valorizar(dc, out precioCLP, out usingSpot).Value;
                var precio = Math.Round(total / (decimal)pi.PesoBruto * 1000, 1);

                var item = new ExportBatchRecepcionesSAP()
                {
                    CardCode = pi.Agricultor.SAPID(pi.IdEmpresa),
                    Comments = "Ingreso " + pi.IdProcesoIngreso + ". " + pi.CultivoContrato.Nombre + " " + pi.Variedad.Nombre,
                    DocCurrency = "$",
                    DocDate = fecha,
                    TaxDate = fecha,
                    DocDueDate = fecha,
                    FolioPrefixString = "GD",
                    FolioNumber = pi.NumeroGuia,
                    NumAtCard = "",
                    U_liquidacion = pi.IdPrimeraLiquidacion() ?? 0,
                    ItemCode = pi.CultivoContrato.SAPID,
                    Quantity = pi.PesoBruto.Value,
                    Price = precio,
                    TaxCode = "IVA",
                };

                result.Add(item);
            }

            return result;
        }
    }
}