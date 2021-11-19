using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.AppLayer
{
    public class ExcelActionResult<T> : FileResult
    {
        private readonly IList<T> _list;
        private bool _headerLines;
        private readonly List<string> _fields;
        private bool _autoFit;

        public ExcelActionResult(IList<T> list,
            string fileDownloadName,
            bool headerLines = true,
            bool autoFit = true,
            List<string> fields = null)
            : base("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            _list = list;
            FileDownloadName = fileDownloadName;
            _headerLines = headerLines;
            _fields = fields;
            _autoFit = autoFit;
        }

        protected override void WriteFile(HttpResponseBase response)
        {
            var outputStream = response.OutputStream;
            using (var memoryStream = new MemoryStream())
            {
                using (ExcelPackage pkg = new ExcelPackage(memoryStream))
                {
                    var ws = pkg.Workbook.Worksheets.Add("Datos");
                    WriteList(ws);
                    pkg.Save();
                }
                outputStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }

        private void WriteList(ExcelWorksheet ws)
        {
            int row = 1;
            if (_headerLines)
            {
                WriteHeaderLine(ws, row);
                row++;
            }

            WriteDataLines(ws, row);

            if (_autoFit)
            {
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
            }
        }

        private void WriteHeaderLine(ExcelWorksheet ws, int row)
        {
            var count = typeof(T).GetProperties().Count();
            int col = 1;
            foreach (MemberInfo member in typeof(T).GetProperties())
            {
                if (_fields == null || _fields.Contains(member.Name))
                {
                    var name = member.Name;

                    if (name == "PesoBruto")
                        name = "PesoNeto";
                    if (name == "PesoNormal")
                        name = "PesoStandard";

                    WriteValue(ws, row, col, name);
                    col++;
                }
            }
        }

        private void WriteDataLines(ExcelWorksheet ws, int row)
        {
            var count = typeof(T).GetProperties().Count();
            foreach (T line in _list)
            {
                int col = 1;
                foreach (MemberInfo member in typeof(T).GetProperties())
                {
                    if (_fields == null || _fields.Contains(member.Name))
                    {
                        WriteValue(ws, row, col, GetPropertyValue(line, member.Name));
                        col++;
                    }
                }
                row++;
            }
        }

        private void WriteValue(ExcelWorksheet ws, int row, int col, Object value)
        {
            ws.Cells[row, col].Value = value;
            if (Type.GetTypeCode(value.GetType()) == TypeCode.DateTime)
            {
                ws.Cells[row, col].Style.Numberformat.Format = "dd/MM/yyyy";
            }
        }

        public static object GetPropertyValue(object src, string propName)
        {
            object obj = src.GetType().GetProperty(propName).GetValue(src, null);
            object value = obj ?? "";

            return value;
        }
    }
}