using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

//Based on http://codeclimber.net.nz/archive/2012/11/22/How-to-return-a-CSV-file-with-ASP-NET-MVC.aspx

namespace AgroFichasWeb.AppLayer
{
    public class CsvActionResult<T> : FileResult
    {
        private readonly IList<T> _list;
        private readonly char _separator;
        private int _headerLines = 1;
        private readonly List<string> _fields;

        public CsvActionResult(IList<T> list,
            string fileDownloadName,
            int headerLines = 1,
            char separator = ';',
            List<string>fields = null)
            : base("text/csv")
        {
            _list = list;
            FileDownloadName = fileDownloadName;
            _separator = separator;
            _headerLines = headerLines;
            _fields = fields;
        }

        protected override void WriteFile(HttpResponseBase response)
        {
            var outputStream = response.OutputStream;
            using (var memoryStream = new MemoryStream())
            {
                WriteList(memoryStream);
                outputStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }

        private void WriteList(Stream stream)
        {
            var streamWriter = new StreamWriter(stream, Encoding.Default);

            for (int i = 0; i < _headerLines; i++) 
            {
                WriteHeaderLine(streamWriter);
                streamWriter.WriteLine();
            }

            WriteDataLines(streamWriter);
            streamWriter.Flush();
        }

        private void WriteHeaderLine(StreamWriter streamWriter)
        {
            var count = typeof(T).GetProperties().Count();
            int i = 0; 
            foreach (MemberInfo member in typeof(T).GetProperties())
            {
                if (_fields == null || _fields.Contains(member.Name))
                {
                    i++;
                    var name = member.Name;

                    if (name == "PesoBruto")
                        name = "PesoNeto";
                    if (name == "PesoNormal")
                        name = "PesoStandard";

                    WriteValue(streamWriter, name, i != count);
                }
            }
        }

        private void WriteDataLines(StreamWriter streamWriter)
        {
            var count = typeof(T).GetProperties().Count();
            foreach (T line in _list)
            {
                int i = 0; 
                foreach (MemberInfo member in typeof(T).GetProperties())
                {
                    if (_fields == null || _fields.Contains(member.Name))
                    {
                        i++;
                        WriteValue(streamWriter, GetPropertyValue(line, member.Name), i != count);
                    }
                }
                streamWriter.WriteLine();
            }
        }

        private void WriteValue(StreamWriter writer, String value, bool separate = true)
        {
            //writer.Write("\"");
            writer.Write(value.Replace("\"", ""));
            if (separate)
                writer.Write(_separator);
            //writer.Write("\"" + _separator);
        }

        public static string GetPropertyValue(object src, string propName)
        {
            //src.GetType().GetProperty(propName).GetType();
            object obj = src.GetType().GetProperty(propName).GetValue(src, null);

            string value = (obj != null) ? obj.ToString() : "";

            if (value != "" && IsNumericType(obj))
            {
                value = value.Replace(".", ",");
            }

            return value;
        }

        public static bool IsNumericType(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}