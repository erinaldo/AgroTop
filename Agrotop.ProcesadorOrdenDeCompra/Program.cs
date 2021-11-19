using NLog;
using System;
using System.IO;
using System.Configuration;
using System.Net;
using System.Xml;
using System.Linq;
using System.Net.Mail;
using Agrotop.PROCEA.Modelos;

namespace Agrotop.PROCEA
{
    class Program
    {
        private static AgrofichasDBDataContext dc = new AgrofichasDBDataContext();
        private static Logger logger = LogManager.GetLogger("fileLogger");

        static void Main(string[] args)
        {
            Console.WriteLine("PROCEA 1.6");
            Console.WriteLine("--------------------------------------------------------------------");
            logger.Info("PROCEA 1.6");
            logger.Info("--------------------------------------------------------------------");

            try
            {
                string[] filePaths = Directory.GetFiles(ConfigurationManager.AppSettings["OcsPendientes"], "*.xml");

                foreach (var filePath in filePaths)
                {
                    var ODC = Parser.Filename(filePath);
                    var endpoint = AppHelper.GetEndpointOrdenCompra(ODC, dc);
                    if (string.IsNullOrEmpty(endpoint))
                    {
                        logger.Error("Endpoint del API ODC no encontrado");
                        return;
                    }

                    Console.WriteLine("Procesando Liquidación #{0}...", ODC.IdLiquidacion);
                    logger.Info(string.Format("Procesando Liquidación #{0}...", ODC.IdLiquidacion));

                    try
                    {
                        using (StreamReader streamReader = new StreamReader(filePath))
                        {
                            String line = streamReader.ReadToEnd();

                            var responseString = PostXMLData(endpoint, line);
                            var filename = string.Format(@"{0}\response_{1}_{2}.xml", Properties.Settings.Default.XmlFilesFolder, ODC.IdLiquidacion, Guid.NewGuid().ToString());
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
                            {
                                file.WriteLine(responseString);
                            }

                            var apiResponse = Parser.XmlOrdenCompra(responseString);
                            if (apiResponse != null)
                            {
                                if (apiResponse.Msg == "success")
                                {
                                    Console.WriteLine("ODC Creada #{0}", ODC.IdLiquidacion);
                                    logger.Info(string.Format("ODC Creada #{0}", ODC.IdLiquidacion));

                                    endpoint = AppHelper.GetEndpointSelectTxt(ODC, dc);
                                    if (string.IsNullOrEmpty(endpoint))
                                    {
                                        logger.Error("Endpoint del API SELECTTXT no encontrado");
                                        return;
                                    }

                                    filename = string.Format(@"{0}\request_select_txt_{1}_{2}.xml", Properties.Settings.Default.XmlFilesFolder, ODC.IdLiquidacion, Guid.NewGuid().ToString());
                                    var requestXML = AppHelper.GetSelectTxtTemplate(apiResponse.Code);
                                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
                                    {
                                        file.WriteLine(requestXML);
                                    }

                                    responseString = PostXMLData(endpoint, requestXML);
                                    filename = string.Format(@"{0}\response_select_txt_{1}_{2}.xml", Properties.Settings.Default.XmlFilesFolder, ODC.IdLiquidacion, Guid.NewGuid().ToString());
                                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
                                    {
                                        file.WriteLine(responseString);
                                    }

                                    apiResponse = Parser.XmlSelectTxt(responseString);

                                    var resultSet = apiResponse.SQLResult.ResultSet.FirstOrDefault();
                                    if (resultSet == null)
                                    {
                                        logger.Error("Error (2da Llamada)");
                                        return;
                                    }

                                    Console.WriteLine("ODC Nro. #{0}", resultSet.DocNum);
                                    logger.Info(string.Format("ODC Nro. #{0}", resultSet.DocNum));

                                    DatabaseHelper.ActualizarODC(ODC, resultSet.DocNum, dc, logger);

                                    streamReader.Close();

                                    logger.Info(AppHelper.MoverArchivoXml(ODC));

                                    MailHelper.SendNotificacionOK(ODC, resultSet.DocNum, dc, logger);
                                }

                                if (apiResponse.Msg == "failure")
                                {
                                    Console.WriteLine("Error");
                                    Console.WriteLine(apiResponse.Code);
                                    logger.Error(apiResponse.Code);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                        Console.WriteLine("ERROR: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                Console.WriteLine("ERROR: " + ex.Message);
            }

            Console.WriteLine("--------------------------------------------------------------------");
        }

        static string PostXMLData(string destinationUrl, string requestXml)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(requestXml);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                return responseStr;
            }
            return null;
        }
    }
}
