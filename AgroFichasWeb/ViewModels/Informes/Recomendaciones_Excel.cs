using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Informes
{
    public class Recomendaciones_Excel
    {
        

        //public byte[] RunReport(Temporada temporada)
        //{
        //    var dc = new AgroFichasDBDataContext();
        //    var data = dc.rpt_Recomendaciones(temporada.IdTemporada).ToList();

        //    var semanas = SemanaAno.FromRange(data);
        //    var meses = MesAno.FromSemanas(semanas);

        //    using (ExcelPackage pkg = new ExcelPackage())
        //    {
        //        var ws = pkg.Workbook.Worksheets.Add(temporada.Nombre);

        //        int row = 1;

        //        foreach (int? idCultivo in data.Select(d => d.IdCultivo).Distinct())
        //        {
        //            var cultivo = dc.Cultivo.SingleOrDefault(c => c.IdCultivo == idCultivo);
        //            var nombreCultivo = cultivo != null ? cultivo.Nombre : "(Desconocido)";

        //            var r = ws.Cells[row++, 1];
        //            r.Value = nombreCultivo;
        //            r.Style.Font.Bold = true;
        //            r.Style.Font.Size = 20;

        //            int firstRow = row;

        //            row = RenderMeses(ws, meses, row);

        //            var ase = ws.Cells[row, 1];
        //            ase.Value = "Asesor";
        //            ase.Style.Font.Bold = true;
        //            AddBorder(ase);

        //            row = RenderSemanas(ws, semanas, row);

        //            int firstDataRow = row;
        //            foreach (var item in data.Where(d => d.IdCultivo == idCultivo).Select(d => new { UserIns = d.UserIns, Agente = d.Agente }).Distinct().OrderBy(d => d.Agente))
        //            {
        //                var a = ws.Cells[row, 1];
        //                a.Value = item.Agente;
        //                a.Style.Border.Left.Style = ExcelBorderStyle.Thin;

        //                int col = 2;
        //                foreach (var s in semanas)
        //                {
        //                    var d = data.SingleOrDefault(x => x.Semana == s.Semana && x.Ano == s.Ano && x.UserIns == item.UserIns && x.IdCultivo == idCultivo);
        //                    if (d != null)
        //                    {
        //                        ws.Cells[row, col].Value = d.Visitas;
        //                    }
        //                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //                    col++;
        //                }

        //                var t = ws.Cells[row, col];
        //                t.FormulaR1C1 = String.Format("SUM(RC{0}:RC{1})", 2, col - 1);
        //                BoldCenter(t);
        //                AddBorder(t);

        //                row++;
        //            }

        //            int lastDataRow = row - 1;
        //            RenderTotalSemanas(ws, semanas, row, firstDataRow, lastDataRow);
        //            RenderBorderMeses(ws, meses, firstRow, row);

        //            row += 3;
        //        }

        //        ws.Column(1).AutoFit();
        //        return pkg.GetAsByteArray();
        //    }
        //}
    }
}