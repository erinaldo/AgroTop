using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Agrotop.PROCEA.Modelos
{
    class Parser
    {
        public static OrdenCompra Filename(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            var ordenDeCompra = new OrdenCompra();

            var pedazos = fileName.Replace(".xml", "").Split(new char[] { '_' });
            foreach (var pedazo in pedazos)
            {
                if (pedazo == "OC") { }

                if (pedazo.StartsWith("P"))
                {
                    ordenDeCompra.IdProyecto = int.Parse(pedazo.Replace("P", ""));
                }

                if (pedazo.StartsWith("L"))
                {
                    ordenDeCompra.IdLiquidacion = int.Parse(pedazo.Replace("L", ""));
                }

                if (pedazo.StartsWith("F"))
                {
                    ordenDeCompra.Firma = pedazo;
                }
            }

            ordenDeCompra.Filename = fileName;

            return ordenDeCompra;
        }

        public static Status XmlOrdenCompra(string responseXML)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseXML);

            Status status = new Status();

            XmlNode statusNode = doc.SelectSingleNode("/Status");
            status.Code = statusNode.SelectSingleNode("Code").InnerText;
            status.Msg = statusNode.SelectSingleNode("Msg").InnerText;

            return status;
        }

        public static Status XmlSelectTxt(string responseXML)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseXML);

            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("sap", "urn:com.sap.b1i.adapter:jdbcadapter");

            Status status = new Status();

            XmlNode statusNode = doc.SelectSingleNode("/Status");
            status.SQLTXT = statusNode.SelectSingleNode("SQLTXT").InnerText;

            SQLResult sQLResult = new SQLResult();
            List<Row> resultSet = new List<Row>();

            XmlNode sQLResultNode = statusNode.SelectSingleNode("SQLResult");
            XmlNode resultSetNode = sQLResultNode.FirstChild;

            foreach (XmlNode xmlNode in resultSetNode.ChildNodes)
            {
                string cardCode = "";
                string docEntry = "";
                string docNum = "";

                foreach (XmlNode innerXmlNode in xmlNode.ChildNodes)
                {
                    if (innerXmlNode.Name == "CardCode")
                        cardCode = innerXmlNode.InnerText;
                    if (innerXmlNode.Name == "DocEntry")
                        docEntry = innerXmlNode.InnerText;
                    if (innerXmlNode.Name == "DocNum")
                        docNum = innerXmlNode.InnerText;
                }

                resultSet.Add(new Row()
                {
                    CardCode = cardCode,
                    DocEntry = docEntry,
                    DocNum = docNum
                });
            }

            sQLResult.ResultSet = resultSet;
            status.SQLResult = sQLResult;

            return status;
        }
    }
}
