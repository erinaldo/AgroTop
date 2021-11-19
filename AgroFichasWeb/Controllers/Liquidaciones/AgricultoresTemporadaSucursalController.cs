using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System.IO;
using System.Drawing;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class AgricultoresTemporadaSucursalController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public AgricultoresTemporadaSucursalController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "soloACubrir", Request.QueryString["soloACubrir"] ?? "" },
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Index(int? pageIndex, int? idTemporada, int? idEmpresa, bool? soloACubrir, string key = "")
        {
            CheckPermisoAndRedirect(1044);

            //Temporadas
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out Temporada temporada);

            //Items
            var items = dc.AgricultoresTemporadaSucursal(temporada.IdTemporada, idEmpresa ?? 0, key, soloACubrir ?? false).ToList();

            //var model = new PaginatedList<AgricultoresTemporadaResult>(items, pageIndex, this.DefaultPageSize);
            var model = new PaginatedList<AgricultoresTemporadaSucursalResult>(items, -1, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "soloACubrir", Request.QueryString["soloACubrir"] ?? "" },
                { "idSucursal", Request.QueryString["idSucursal"] ?? "" }
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["soloACubrir"] = soloACubrir ?? false;
            ViewData["intencionesSiembra"] = dc.CRM_Objetivos.Where(X => X.IdTemporada == temporada.IdTemporada).ToList();
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult IndexExport(int idTemporada, int? idEmpresa, bool? soloACubrir, string key = "")
        {
            CheckPermisoAndRedirect(1044);

            var items = dc.AgricultoresTemporadaSucursal(idTemporada, idEmpresa ?? 0, key, soloACubrir ?? false).ToList();

            return new CsvActionResult<AgricultoresTemporadaSucursalResult>(items.ToList(), "AgricultoresTemporada.csv", 1, ';', null);
        }

        public void exportarExcel(int idTemporada, int? idEmpresa, int? totalIntRaps, int? totalIntTrigo, int? totalIntAvena, int? totalIntLupino, int? totalIntMaiz, List<CRM_Objetivos> intSiembra, bool? soloACubrir, string key = "")
        {
            var items = dc.AgricultoresTemporadaSucursal(idTemporada, idEmpresa ?? 0, key, soloACubrir ?? false).ToList();
             
            var excelPackage = new ExcelPackage();

            //var intencionesSiembra = dc.CRM_Objetivos.Where(X => X.IdTemporada == temporada.IdTemporada).ToList();

            var model = new PaginatedList<AgricultoresTemporadaSucursalResult>(items, -1, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "soloACubrir", Request.QueryString["soloACubrir"] ?? "" },
                { "idSucursal", Request.QueryString["idSucursal"] ?? "" }
            };

            var rapIntTotal = 0;
            var rapContTotal = 0;
            var rapConvTotal = 0;
            var rapIngTotal = 0;
            var rapsACubrirTotal = 0;

            var trigoIntTotal = 0;
            var trigoContTotal = 0;
            var trigoConvTotal = 0;
            var trigoIngTotal = 0;
            var trigoACubrirTotal = 0;

            var avenaIntTotal = 0;
            int avenaContTotal = 0;
            var avenaConvTotal = 0;
            var avenaIngTotal = 0;
            var avenaACubrirTotal = 0;

            var lupinoIntTotal = 0;
            var lupinoContTotal = 0;
            var lupinoConvTotal = 0;
            var lupinoIngTotal = 0;
            var lupinoACubrirTotal = 0;

            var maizIntTotal = 0;
            var maizContTotal = 0;
            var maizConvTotal = 0;
            var maizIngTotal = 0;
            var maizACubrirTotal = 0;

            var rut1 = "";
            var rut2 = "";
            var nombre = "";


            var totalContRaps = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ContratadoRaps).Sum().Value / 1000M));
            var totalConvRaps = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ConvenioRaps).Sum().Value / 1000M));
            var totalIngRaps = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.IngresosRaps).Sum().Value / 1000M));
            var totalACubrirRaps = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ACubrirRaps).Sum().Value / 1000M));

            var totalContTrigo = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ContratadoTrigo).Sum().Value / 1000M));
            var totalConvTrigo = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ConvenioTrigo).Sum().Value / 1000M));
            var totalIngTrigo = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.IngresosTrigo).Sum().Value / 1000M));
            var totalACubrirTrigo = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ACubrirTrigo).Sum().Value / 1000M));

            var totalContAvena = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ContratadoAvena).Sum().Value / 1000M));
            var totalConvAvena = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ConvenioAvena).Sum().Value / 1000M));
            var totalIngAvena = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.IngresosAvena).Sum().Value / 1000M));
            var totalACubrirAvena = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ACubrirAvena).Sum().Value / 1000M));

            var totalContLupino = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ContratadoLupino).Sum().Value / 1000M));
            var totalConvLupino = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ConvenioLupino).Sum().Value / 1000M));
            var totalIngLupino = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.IngresosLupino).Sum().Value / 1000M));
            var totalACubrirLupino = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ACubrirLupino).Sum().Value / 1000M));

            var totalContMaiz = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ContratadoMaiz).Sum().Value / 1000M));
            var totalConvMaiz = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ConvenioMaiz).Sum().Value / 1000M));
            var totalIngMaiz = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.IngresosMaiz).Sum().Value / 1000M));
            var totalACubrirMaiz = Math.Round(Convert.ToDecimal(model.Source.Select(i => i.ACubrirMaiz).Sum().Value / 1000M));

            //Propiedades del archivo
            excelPackage.Workbook.Properties.Author = "Empresas Agrotop";
            excelPackage.Workbook.Properties.Title = " Exportación";
            //Propiedades Hoja de excel
            var sheet = excelPackage.Workbook.Worksheets.Add("AgricultoresTemporada");
            sheet.Name = "AgricultoresTemporada";
            //Empezamos a escribir sobre ella.
            //var rowindex = 1;
            //Hago un Merge de primeras 4 columnas para poner el titulo.
            sheet.Cells[1, 1].Value = "Agricultores Temporada";
            sheet.Cells[1, 1, 1, 4].Merge = true;

            #region Encabezados Informe
            //Encabezados
            sheet.Cells[3, 5].Value = "Ton Raps";
            sheet.Cells[3, 5, 3, 9].Merge = true;
            sheet.Cells[3, 10].Value = "Ton Trigo";
            sheet.Cells[3, 10, 3, 14].Merge = true;
            sheet.Cells[3, 15].Value = "Ton Avena";
            sheet.Cells[3, 15, 3, 19].Merge = true;
            sheet.Cells[3, 20].Value = "Ton Lupino";
            sheet.Cells[3, 20, 3, 24].Merge = true;

            Color colFromHex = ColorTranslator.FromHtml("#B9B9B9");
            Color colorSubTotal = ColorTranslator.FromHtml("#DEDEDE");
            //sheet.Cells["D3:W3"].Style.Font.Size = 14;
            sheet.Cells["E3:AC3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["E3:AC3"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            sheet.Cells["E3:AC3"].Style.Font.Bold = true;
            sheet.Cells["E3:AC3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
            sheet.Cells["E3:AC3"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            sheet.Cells["E3:AC3"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            sheet.Cells["E3:AC3"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            sheet.Cells["E3:AC3"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            sheet.Cells[4, 2].Value = "RUT";
            sheet.Cells[4, 3].Value = "Nombre";
            sheet.Cells[4, 4].Value = "Sucursal";

            sheet.Cells[4, 5].Value = "Int";
            sheet.Cells[4, 6].Value = "Cont";
            sheet.Cells[4, 7].Value = "Conv";
            sheet.Cells[4, 8].Value = "Ing";
            sheet.Cells[4, 9].Value = "Cubrir";

            sheet.Cells[4, 10].Value = "Int";
            sheet.Cells[4, 11].Value = "Cont";
            sheet.Cells[4, 12].Value = "Conv";
            sheet.Cells[4, 13].Value = "Ing";
            sheet.Cells[4, 14].Value = "Cubrir";

            sheet.Cells[4, 15].Value = "Int";
            sheet.Cells[4, 16].Value = "Cont";
            sheet.Cells[4, 17].Value = "Conv";
            sheet.Cells[4, 18].Value = "Ing";
            sheet.Cells[4, 19].Value = "Cubrir";

            sheet.Cells[4, 20].Value = "Int";
            sheet.Cells[4, 21].Value = "Cont";
            sheet.Cells[4, 22].Value = "Conv";
            sheet.Cells[4, 23].Value = "Ing";
            sheet.Cells[4, 24].Value = "Cubrir";

            sheet.Cells[4, 25].Value = "Int";
            sheet.Cells[4, 26].Value = "Cont";
            sheet.Cells[4, 27].Value = "Conv";
            sheet.Cells[4, 28].Value = "Ing";
            sheet.Cells[4, 29].Value = "Cubrir";

            sheet.Cells["B4:AC4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["B4:AC4"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            sheet.Cells["B4:AC4"].Style.Font.Bold = true;
            sheet.Cells["B4:AC4"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B4:AC4"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B4:AC4"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B4:AC4"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            sheet.Cells[5, 5].Value = totalIntRaps;
            sheet.Cells[5, 6].Value = totalContRaps;
            sheet.Cells[5, 7].Value = totalConvRaps;
            sheet.Cells[5, 8].Value = totalIngRaps;
            sheet.Cells[5, 9].Value = totalACubrirRaps;

            sheet.Cells[5, 10].Value = totalIntTrigo;
            sheet.Cells[5, 11].Value = totalContTrigo;
            sheet.Cells[5, 12].Value = totalConvTrigo;
            sheet.Cells[5, 13].Value = totalIngTrigo;
            sheet.Cells[5, 14].Value = totalACubrirTrigo;

            sheet.Cells[5, 15].Value = totalIntAvena;
            sheet.Cells[5, 16].Value = totalContAvena;
            sheet.Cells[5, 17].Value = totalConvAvena;
            sheet.Cells[5, 18].Value = totalIngAvena;
            sheet.Cells[5, 19].Value = totalACubrirAvena;

            sheet.Cells[5, 20].Value = totalIntLupino;
            sheet.Cells[5, 21].Value = totalContLupino;
            sheet.Cells[5, 22].Value = totalConvLupino;
            sheet.Cells[5, 23].Value = totalIngLupino;
            sheet.Cells[5, 24].Value = totalACubrirLupino;

            sheet.Cells[5, 25].Value = totalIntMaiz;
            sheet.Cells[5, 26].Value = totalContMaiz;
            sheet.Cells[5, 27].Value = totalConvMaiz;
            sheet.Cells[5, 28].Value = totalIngMaiz;
            sheet.Cells[5, 29].Value = totalACubrirMaiz;

            sheet.Cells["B5:AC5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["B5:AC5"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            sheet.Cells["B5:AC5"].Style.Font.Bold = true;
            sheet.Cells["B5:AC5"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B5:AC5"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B5:AC5"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B5:AC5"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            //Se puede poner un comentario en una celda
            //sheet.Cells[1, 1].AddComment("Agricultores Temporada", "Empresas Agrotop");
            //rowindex = 3;
            ////Pongo los encabezados del excel

            //var col = 1;
            //sheet.Cells[rowindex, col++].Value = "Mes";
            //sheet.Cells[rowindex, col++].Value = "Año";
            //sheet.Cells[rowindex, col++].Value = "Nº Recibo";
            //sheet.Cells[rowindex, col++].Value = "Total";
            #endregion

            var fila = 6;

            
            
            foreach(var item in items)
            {
                var cubrirRaps = (item.ConvenioRaps ?? 0) - (item.IngresosRaps ?? 0);
                cubrirRaps = cubrirRaps < 0 ? -cubrirRaps : 0;

                var cubrirTrigo = (item.ConvenioTrigo ?? 0) - (item.IngresosTrigo ?? 0);
                cubrirTrigo = cubrirTrigo < 0 ? -cubrirTrigo : 0;

                var cubrirAvena = (item.ConvenioAvena ?? 0) - (item.IngresosAvena ?? 0);
                cubrirAvena = cubrirAvena < 0 ? -cubrirAvena : 0;

                var cubrirLupino = (item.ConvenioLupino ?? 0) - (item.IngresosLupino ?? 0);
                cubrirLupino = cubrirLupino < 0 ? -cubrirLupino : 0;

                var cubrirLinaza = (item.ConvenioLinaza ?? 0) - (item.ConvenioLinaza ?? 0);
                cubrirLinaza = cubrirLinaza < 0 ? -cubrirLinaza : 0;

                var cubrirMaiz = (item.ConvenioMaiz ?? 0) - (item.ConvenioMaiz ?? 0);
                cubrirMaiz = cubrirMaiz < 0 ? -cubrirMaiz : 0;

                var intRaps = 0;
                var intAvena = 0;
                var intTrigo = 0;
                var intLupino = 0;
                var intMaiz = 0;
                var intencion = intSiembra.FirstOrDefault(X => item.IdForceManager.HasValue && X.ID == item.IdForceManager.Value);
                if (intencion != null)
                {
                    intRaps = intencion.HectareasRaps;
                    intTrigo = intencion.HectareasTrigo;
                    intAvena = intencion.HectareasAvena;
                    intLupino = intencion.HectareasLupino;
                    intMaiz = intencion.HectareasMaiz;
                }

                var col = 2;
                rut1 = item.Rut;
                if (!(rut2.Equals("")) && !(rut2.Equals(rut1)))
                {
                    sheet.Cells[fila, col++].Value = rut2;
                    sheet.Cells[fila, col++].Value = nombre;
                    sheet.Cells[fila, col++].Value = "SubTotal";

                    sheet.Cells[fila, col++].Value = rapIntTotal;
                    sheet.Cells[fila, col++].Value = rapContTotal;
                    sheet.Cells[fila, col++].Value = rapConvTotal;
                    sheet.Cells[fila, col++].Value = rapIngTotal;
                    sheet.Cells[fila, col++].Value = rapsACubrirTotal;

                    sheet.Cells[fila, col++].Value = trigoIntTotal;
                    sheet.Cells[fila, col++].Value = trigoContTotal;
                    sheet.Cells[fila, col++].Value = trigoConvTotal;
                    sheet.Cells[fila, col++].Value = trigoIngTotal;
                    sheet.Cells[fila, col++].Value = trigoACubrirTotal;

                    sheet.Cells[fila, col++].Value = avenaIntTotal;
                    sheet.Cells[fila, col++].Value = avenaContTotal;
                    sheet.Cells[fila, col++].Value = avenaConvTotal;
                    sheet.Cells[fila, col++].Value = avenaIngTotal;
                    sheet.Cells[fila, col++].Value = avenaACubrirTotal;

                    sheet.Cells[fila, col++].Value = lupinoIntTotal;
                    sheet.Cells[fila, col++].Value = lupinoContTotal;
                    sheet.Cells[fila, col++].Value = lupinoConvTotal;
                    sheet.Cells[fila, col++].Value = lupinoIngTotal;
                    sheet.Cells[fila, col++].Value = lupinoACubrirTotal;

                    sheet.Cells[fila, col++].Value = maizIntTotal;
                    sheet.Cells[fila, col++].Value = maizContTotal;
                    sheet.Cells[fila, col++].Value = maizConvTotal;
                    sheet.Cells[fila, col++].Value = maizIngTotal;
                    sheet.Cells[fila, col++].Value = maizACubrirTotal;
                    col = 2;

                    sheet.Cells["B" + fila + ":AC" + fila].Style.Font.Bold = true;
                    fila++;

                    rapIntTotal = 0;
                    rapContTotal = 0;
                    rapConvTotal = 0;
                    rapIngTotal = 0;
                    rapsACubrirTotal = 0;

                    trigoIntTotal = 0;
                    trigoContTotal = 0;
                    trigoConvTotal = 0;
                    trigoIngTotal = 0;
                    trigoACubrirTotal = 0;

                    avenaIntTotal = 0;
                    avenaContTotal = 0;
                    avenaConvTotal = 0;
                    avenaIngTotal = 0;
                    avenaACubrirTotal = 0;

                    lupinoIntTotal = 0;
                    lupinoContTotal = 0;
                    lupinoConvTotal = 0;
                    lupinoIngTotal = 0;
                    lupinoACubrirTotal = 0;

                    maizIntTotal = 0;
                    maizContTotal = 0;
                    maizConvTotal = 0;
                    maizIngTotal = 0;
                    maizACubrirTotal = 0;
                }
                rut2 = item.Rut;
                nombre = item.Nombre;

                rapIntTotal += intRaps == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(intRaps)));
                rapContTotal += item.ContratadoRaps == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ContratadoRaps / 1000M)));
                rapConvTotal += item.ConvenioRaps == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ConvenioRaps / 1000M)));
                rapIngTotal += item.IngresosRaps == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.IngresosRaps / 1000M)));
                rapsACubrirTotal += Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirRaps / 1000M)));

                trigoIntTotal += intTrigo == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(intTrigo)));
                trigoContTotal += item.ContratadoTrigo == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ContratadoTrigo / 1000M)));
                trigoConvTotal += item.ConvenioTrigo == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ConvenioTrigo / 1000M)));
                trigoIngTotal += item.IngresosTrigo == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.IngresosTrigo / 1000M)));
                trigoACubrirTotal += Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirTrigo / 1000M)));

                avenaIntTotal += intAvena == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(intAvena)));
                avenaContTotal += item.ContratadoAvena == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ContratadoAvena / 1000M)));
                avenaConvTotal += item.ConvenioAvena == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ConvenioAvena / 1000M)));
                avenaIngTotal += item.IngresosAvena == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.IngresosAvena / 1000M)));
                avenaACubrirTotal += Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirAvena / 1000M)));

                lupinoIntTotal += intLupino == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(intLupino)));
                lupinoContTotal += item.ContratadoLupino == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ContratadoLupino / 1000M)));
                lupinoConvTotal += item.ConvenioLupino == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ConvenioLupino / 1000M)));
                lupinoIngTotal += item.IngresosLupino == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.IngresosLupino / 1000M)));
                lupinoACubrirTotal += Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirLupino / 1000M)));

                maizIntTotal += intMaiz == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(intMaiz)));
                maizContTotal += item.ContratadoMaiz == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ContratadoMaiz / 1000M)));
                maizConvTotal += item.ConvenioMaiz == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.ConvenioMaiz / 1000M)));
                maizIngTotal += item.IngresosMaiz == 0 ? 0 : Convert.ToInt32(Math.Round(Convert.ToDecimal(item.IngresosMaiz / 1000M)));
                maizACubrirTotal += Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirMaiz / 1000M)));

                sheet.Cells[fila, col++].Value = item.Rut;
                sheet.Cells[fila, col++].Value = item.Nombre;
                sheet.Cells[fila, col++].Value = item.Sucursal;

                sheet.Cells[fila, col++].Value = intRaps;
                sheet.Cells[fila, col++].Value = item.ContratadoRaps.HasValue ? Math.Round(Convert.ToDecimal(item.ContratadoRaps / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.ConvenioRaps.HasValue ? Math.Round(Convert.ToDecimal(item.ConvenioRaps / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.IngresosRaps.HasValue ? Math.Round(Convert.ToDecimal(item.IngresosRaps / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirRaps / 1000M)));

                sheet.Cells[fila, col++].Value = intTrigo;
                sheet.Cells[fila, col++].Value = item.ContratadoTrigo.HasValue ? Math.Round(Convert.ToDecimal(item.ContratadoTrigo / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.ConvenioTrigo.HasValue ? Math.Round(Convert.ToDecimal(item.ConvenioTrigo / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.IngresosTrigo.HasValue ? Math.Round(Convert.ToDecimal(item.IngresosTrigo / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirTrigo / 1000M)));

                sheet.Cells[fila, col++].Value = intAvena;
                sheet.Cells[fila, col++].Value = item.ContratadoAvena.HasValue ? Math.Round(Convert.ToDecimal(item.ContratadoAvena / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.ConvenioAvena.HasValue ? Math.Round(Convert.ToDecimal(item.ConvenioAvena / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.IngresosAvena.HasValue ? Math.Round(Convert.ToDecimal(item.IngresosAvena / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirAvena / 1000M)));

                sheet.Cells[fila, col++].Value = intLupino;
                sheet.Cells[fila, col++].Value = item.ContratadoLupino.HasValue ? Math.Round(Convert.ToDecimal(item.ContratadoLupino / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.ConvenioLupino.HasValue ? Math.Round(Convert.ToDecimal(item.ConvenioLupino / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.IngresosLupino.HasValue ? Math.Round(Convert.ToDecimal(item.IngresosLupino / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirLupino / 1000M)));

                sheet.Cells[fila, col++].Value = intMaiz;
                sheet.Cells[fila, col++].Value = item.ContratadoMaiz.HasValue ? Math.Round(Convert.ToDecimal(item.ContratadoMaiz / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.ConvenioMaiz.HasValue ? Math.Round(Convert.ToDecimal(item.ConvenioMaiz / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = item.IngresosMaiz.HasValue ? Math.Round(Convert.ToDecimal(item.IngresosMaiz / 1000M)) : 0;
                sheet.Cells[fila, col++].Value = Convert.ToInt32(Math.Round(Convert.ToDecimal(cubrirMaiz / 1000M)));

                fila++;
            }

            
           
            sheet.Cells["B" + 6 + ":AC" + fila].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B" + 6 + ":AC" + fila].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B" + 6 + ":AC" + fila].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            sheet.Cells["B" + 6 + ":AC" + fila].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            //rowindex = 4;

            ////Recorro los recibos y los ponemos en el Excel
            //foreach (var r in items)
            //{
            //    col = 1;
            //    sheet.Cells[rowindex, col++].Value = r.ContratadoRaps;
            //    sheet.Cells[rowindex, col++].Value = r.ContratadoTrigo;
            //    sheet.Cells[rowindex, col++].Value = r.ContratadoAvena;
            //    sheet.Cells[rowindex, col++].Value = r.ContratadoLupino;
            //    rowindex++;



            // Ancho de celdas
            sheet.Cells.AutoFitColumns();

            //Establezco diseño al excel utilizando un diseño predefinido
            //var range = sheet.Cells[3, 1, rowindex, 4];
            //var table = sheet.Tables.Add(range, "tabla");
            //table.TableStyle = TableStyles.Dark9;

            //Ya lo tengo ahora lo devuelvo utilizo el Response porque es Web, sino puedes guardarlo directamente
            Response.ClearContent();
            Response.BinaryWrite(excelPackage.GetAsByteArray());
            Response.AddHeader("content-disposition", "attachment;filename=AgricultoresTemporada.xlsx");
            Response.ContentType = "application/excel";
            Response.Flush();
                Response.End();
            
        }
    } 
}
