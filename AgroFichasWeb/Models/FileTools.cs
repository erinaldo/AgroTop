using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace Utils.Common.Files
{
    public class FileTools
    {
        public FileTools()
        {
        }

        /// <summary>
        /// Convierte el arreglo de bytes buffer en un DataTable.
        /// </summary>
        /// <param name="buffer">Arreglo de bytes a ser convertido.</param>
        /// <param name="includeHeader">True si es que dentro del buffer se encuentra la fila de encabezado.</param>
        /// <returns>Datatable con los datos</returns>
        public static DataTable ByteBufferToTable(byte[] buffer, bool includeHeader)
        {
            DataTable result = new DataTable();

            // Se asume que el separador de decimales es punto "." y el de miles "," (aunque este ultimo no se usa) 
            CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Dictionary<string, int> indexs = new Dictionary<string, int>();
            DataTable dt = new DataTable();
            char[] delimiter = new char[] { ';' };
            char[] zero = new char[] { '0' };

            using (StreamReader sr = new StreamReader(new MemoryStream(buffer)))
            {
                try
                {
                    int rowsCompleted = 0;
                    int lastLength = 0;
                    bool readHeader = true;

                    while (sr.Peek() > -1)
                    {
                        bool addLine = true;
                        string line = sr.ReadLine();
                        string[] lineArray = line.Split(delimiter);

                        //Se chequea que tanto el orden como el nombre de las columnas correspondan según el orden dado
                        if (readHeader)
                        {
                            if (includeHeader)
                            {
                                int j = 0;
                                foreach (string column in lineArray)
                                {
                                    DataColumn c = new DataColumn(column);
                                    dt.Columns.Add(c);
                                    indexs.Add(column, j);
                                    j++;
                                }
                                //Se continua con la lectura del archivo
                                line = sr.ReadLine();
                                lineArray = line.Split(delimiter);
                            }
                            else
                            {
                                //Agrego columnas con nombre estandar, no se pasa a la siguiente linea del doc
                                for (int j = 0; j < lineArray.Length; j++)
                                {
                                    DataColumn c = new DataColumn("Column" + j);
                                    dt.Columns.Add(c);
                                    indexs.Add("Column" + j, j);
                                }
                            }
                            //Se cambia el estado de esta variable para no volver a chequear el header
                            readHeader = false;
                        }

                        DataRow nuevaFila = dt.NewRow();
                        if (lastLength > 0 && lastLength != lineArray.Length)
                        {
                            continue;
                        }
                        lastLength = lineArray.Length;
                        try
                        {
                            foreach (DataColumn column in dt.Columns)
                            {
                                int index = indexs[column.ColumnName];
                                string colName = column.ColumnName;
                                string value = lineArray[index];
                                nuevaFila[colName] = string.IsNullOrEmpty(value) ? DBNull.Value + "" : value;
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        if (addLine)
                        {
                            dt.Rows.Add(nuevaFila);
                            rowsCompleted++;
                        }
                    }
                    return dt;
                }
                finally
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                }
            }
        }
    }
}