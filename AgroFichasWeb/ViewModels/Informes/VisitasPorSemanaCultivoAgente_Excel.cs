using AgroFichasWeb.Controllers;
using AgroFichasWeb.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Informes
{
    public class VisitasPorSemanaCultivoAgente_Excel
    {
        public byte[] RunReport(Temporada temporada)
        {
            var dc = new AgroFichasDBDataContext();
            var data = dc.rpt_VisitaPorSemanaCultivoAgente_Data(temporada.IdTemporada).ToList();

            var semanas = SemanaAno.FromRange(data);
            var meses = MesAno.FromSemanas(semanas);

            using (ExcelPackage pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add(temporada.Nombre);

                int row = 1;

                foreach (int? idCultivo in data.Select(d => d.IdCultivo).Distinct())
                {
                    var cultivo = dc.Cultivo.SingleOrDefault(c => c.IdCultivo == idCultivo);
                    var nombreCultivo = cultivo != null ? cultivo.Nombre : "(Desconocido)";

                    var r = ws.Cells[row++, 1];
                    r.Value = nombreCultivo;
                    r.Style.Font.Bold = true;
                    r.Style.Font.Size = 20;

                    int firstRow = row;

                    row = RenderMeses(ws, meses, row);

                    var ase = ws.Cells[row, 1];
                    ase.Value = "Asesor";
                    ase.Style.Font.Bold = true;
                    AddBorder(ase);

                    row = RenderSemanas(ws, semanas, row);

                    int firstDataRow = row;
                    foreach (var item in data.Where(d => d.IdCultivo == idCultivo).Select(d => new { UserIns = d.UserIns, Agente = d.Agente }).Distinct().OrderBy(d => d.Agente))
                    {
                        var a = ws.Cells[row, 1];
                        a.Value = item.Agente;
                        a.Style.Border.Left.Style = ExcelBorderStyle.Thin;

                        int col = 2;
                        foreach (var s in semanas)
                        {
                            var d = data.SingleOrDefault(x => x.Semana == s.Semana && x.Ano == s.Ano && x.UserIns == item.UserIns && x.IdCultivo == idCultivo);
                            if (d != null)
                            {
                                ws.Cells[row, col].Value = d.Visitas;
                            }
                            ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            col++;
                        }

                        var t = ws.Cells[row, col];
                        t.FormulaR1C1 = String.Format("SUM(RC{0}:RC{1})", 2, col - 1);
                        BoldCenter(t);
                        AddBorder(t);

                        row++;
                    }

                    int lastDataRow = row - 1;
                    RenderTotalSemanas(ws, semanas, row, firstDataRow, lastDataRow);
                    RenderBorderMeses(ws, meses, firstRow, row);

                    row += 3;
                }

                ws.Column(1).AutoFit();
                return pkg.GetAsByteArray();
            }
        }

        private int RenderTotalSemanas(ExcelWorksheet ws, List<SemanaAno> semanas, int row, int startRow, int endRow)
        {
            //Total columnas
            var r = ws.Cells[row, 1];
            r.Value = "Total";
            r.Style.Font.Bold = true;
            AddBorder(r);

            int col = 2;
            foreach (var s in semanas)
            {
                r = ws.Cells[row, col++];
                r.FormulaR1C1 = String.Format("SUM(R{0}C:R{1}C)", startRow, endRow);
                BoldCenter(r);

                AddBorder(r);
            }

            //Gran total
            r = ws.Cells[row, col++];
            r.FormulaR1C1 = String.Format("SUM(R{0}C:R{1}C)", startRow, endRow);
            BoldCenter(r);

            AddBorder(r);

            return ++row;
        }

        private int RenderMeses(ExcelWorksheet ws, List<MesAno> meses, int row)
        {
            int col = 2;
            foreach (var m in meses)
            {
                ws.Cells[row, col].Value = new DateTime(m.Ano, m.Mes, 1).ToString("MMM yyyy");

                var r = ws.Cells[row, col, row, col + m.CuentaSemanas - 1];
                r.Merge = true;
                BoldCenter(r);
                AddBorder(r);

                col += m.CuentaSemanas;
            }

            var t = ws.Cells[row, col];
            t.Value = "Total";
            BoldCenter(t);
            AddBorder(t);

            return ++row;
        }

        private void RenderBorderMeses(ExcelWorksheet ws, List<MesAno> meses, int firstRow, int lastRow)
        {
            int startCol = 2;
            foreach (var mes in meses)
            {
                int endCol = startCol + mes.CuentaSemanas - 1;
                AddBorder(ws.Cells[firstRow, startCol, lastRow, endCol]);

                startCol = endCol + 1;
            }
        }

        private int RenderSemanas(ExcelWorksheet ws, List<SemanaAno> semanas, int row)
        {
            var lastMoth = -1;
            var iSemana = 0;
            int col = 2;


            foreach (var s in semanas)
            {
                if (s.Mes != lastMoth)
                {
                    iSemana = 0;
                }
                lastMoth = s.Mes;

                var r = ws.Cells[row, col];
                r.Value = $"S-{++iSemana}";
                r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                BoldCenter(r);
                col++;
            }

            return ++row;
        }

        private void BoldCenter(ExcelRange range)
        {
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Font.Bold = true;
        }
             

        private void AddBorder(ExcelRange range)
        {
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }
    }
}