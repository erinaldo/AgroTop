using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agrotop.PROCEA.Modelos
{
    class AppHelper
    {
        public static string GetEndpointOrdenCompra(OrdenCompra ODC, AgrofichasDBDataContext dc)
        {
            string endpoint = "";
            var ODCDB = dc.OC_OrdenCompra.Where(X => X.IdProyecto == ODC.IdProyecto && X.IdLiquidacion == ODC.IdLiquidacion).ToList();
            if (ODCDB.Count > 0)
            {
                var PrimeraGlosa = ODCDB.First();
                switch (PrimeraGlosa.IdEmpresa)
                {
                    case 1://Oleotop
                        endpoint = "http://192.168.11.92:8080/B1iXcellerator/exec/ipo/vP.001sap0000.in_HCSX/com.sap.b1i.vplatform.runtime/INB_HT_CALL_SYNC_XPT/INB_HT_CALL_SYNC_XPT.ipo/proc?wsaction=OrdenCompra10";
                        break;
                    case 2://Avenatop
                        endpoint = "http://192.168.11.92:8080/B1iXcellerator/exec/ipo/vP.001sap0000.in_HCSX/com.sap.b1i.vplatform.runtime/INB_HT_CALL_SYNC_XPT/INB_HT_CALL_SYNC_XPT.ipo/proc?wsaction=OrdenCompra05";
                        break;
                    case 3://Granotop
                        endpoint = "http://192.168.11.92:8080/B1iXcellerator/exec/ipo/vP.001sap0000.in_HCSX/com.sap.b1i.vplatform.runtime/INB_HT_CALL_SYNC_XPT/INB_HT_CALL_SYNC_XPT.ipo/proc?wsaction=OrdenCompra06";
                        break;
                    case 4://Saprosem
                        endpoint = "http://192.168.11.92:8080/B1iXcellerator/exec/ipo/vP.001sap0000.in_HCSX/com.sap.b1i.vplatform.runtime/INB_HT_CALL_SYNC_XPT/INB_HT_CALL_SYNC_XPT.ipo/proc?wsaction=OrdenCompra11";
                        break;
                    default:
                        break;
                }
            }

            return endpoint;
        }

        public static string GetEndpointSelectTxt(OrdenCompra ODC, AgrofichasDBDataContext dc)
        {
            string endpoint = "";
            var ODCDB = dc.OC_OrdenCompra.Where(X => X.IdProyecto == ODC.IdProyecto && X.IdLiquidacion == ODC.IdLiquidacion).ToList();
            if (ODCDB.Count > 0)
            {
                var PrimeraGlosa = ODCDB.First();
                switch (PrimeraGlosa.IdEmpresa)
                {
                    case 1://Oleotop
                        endpoint = "http://192.168.11.92:8080/B1iXcellerator/exec/ipo/vP.001sap0000.in_HCSX/com.sap.b1i.vplatform.runtime/INB_HT_CALL_SYNC_XPT/INB_HT_CALL_SYNC_XPT.ipo/proc?wsaction=SelectTXT10";
                        break;
                    case 2://Avenatop
                        endpoint = "http://192.168.11.92:8080/B1iXcellerator/exec/ipo/vP.001sap0000.in_HCSX/com.sap.b1i.vplatform.runtime/INB_HT_CALL_SYNC_XPT/INB_HT_CALL_SYNC_XPT.ipo/proc?wsaction=SelectTXT05";
                        break;
                    case 3://Granotop
                        endpoint = "http://192.168.11.92:8080/B1iXcellerator/exec/ipo/vP.001sap0000.in_HCSX/com.sap.b1i.vplatform.runtime/INB_HT_CALL_SYNC_XPT/INB_HT_CALL_SYNC_XPT.ipo/proc?wsaction=SelectTXT06";
                        break;
                    case 4://Saprosem
                        endpoint = "http://192.168.11.92:8080/B1iXcellerator/exec/ipo/vP.001sap0000.in_HCSX/com.sap.b1i.vplatform.runtime/INB_HT_CALL_SYNC_XPT/INB_HT_CALL_SYNC_XPT.ipo/proc?wsaction=SelectTXT11";
                        break;
                    default:
                        break;
                }
            }

            return endpoint;
        }

        public static string GetSelectTxtTemplate(string docEntry)
        {
            string requestXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<BOM>\r\n  <Select> DocNum </Select>\r\n  <From> OPOR </From>\r\n  <Filtros> WHERE DocEntry = {0} </Filtros>\r\n</BOM>";
            return string.Format(requestXml, docEntry);
        }

        public static string MoverArchivoXml(OrdenCompra ODC)
        {
            string path = string.Format(@"{0}\{1}", ConfigurationManager.AppSettings["OcsPendientes"], ODC.Filename);
            string path2 = string.Format(@"{0}\{1}", ConfigurationManager.AppSettings["OcsProcesadas"], ODC.Filename);

            try
            {
                File.Move(path, path2);
                return "XML OK";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}