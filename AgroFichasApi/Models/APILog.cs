using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace AgroFichasApi.Models
{
    public class APILog
    {
        private static APILog instance;
        private static Queue<APILogItem> logQueue;
        private static int maxLogAge = Properties.Settings.Default.MaxLogAge;
        private static int queueSize = Properties.Settings.Default.LogQueueSize;
        private static DateTime lastFlushed = DateTime.Now;
        public static string logDirectory = Properties.Settings.Default.APILogDirectory;

        private APILog() { }

        public static APILog Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new APILog();
                    logQueue = new Queue<APILogItem>();
                }
                return instance;
            }
        }

        public void WriteToLog(string method, HttpRequestBase request, int userID, JsonResult respuestaJson, OperationResult opResult)
        {
            this.WriteToLog(method, request, userID, opResult.LogCode, respuestaJson, opResult.LogException);
        }

        public void WriteToLog(string method, HttpRequestBase request, int userID, string status, JsonResult respuestaJson, Exception ex = null)
        {
            string token = (request.Form["token"] ?? "").ToString().Trim();
            string deviceID = (request.Form["did"] ?? "").ToString().Trim();
            string apiKey = (request.Form["apiKey"] ?? "").ToString().Trim();

            WriteToLog(method, request, userID, status, respuestaJson, ex, token, deviceID, apiKey, "");
        }

        public void WriteToLog(string method, HttpRequestBase request, int userID, string status, JsonResult respuestaJson, Exception ex, string token, string deviceID, string apiKey, string xmlRequest)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            string ip = getUserIP(request);
            string jsonResponse = "";
            string nombreArchivoException = "";
            string nombreArchivoXml = "";

            List<string> listaValores = new List<string>();

            if (respuestaJson != null)
                jsonResponse = serializer.Serialize(respuestaJson.Data);

            foreach (var f in request.Form.AllKeys)
                listaValores.Add(String.Format("{0}={1}", f, HttpUtility.UrlEncode(request.Form[f])));

            foreach (var r in request.QueryString.AllKeys)
                listaValores.Add(String.Format("{0}={1}", r, HttpUtility.UrlEncode(request.QueryString[r])));

            string formValues = string.Join("&", listaValores.ToArray());

            if (xmlRequest != "")
            {
                nombreArchivoXml = "XmlRequest-" + (Guid.NewGuid().ToString()) + ".xml";
                string carpeta = logDirectory + @"XmlRequests\" + DateTime.UtcNow.ToString("yyyy-MM") + @"\";

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                File.WriteAllText(carpeta + nombreArchivoXml, xmlRequest);
            }

            if (ex != null)
            {
                StringBuilder sb = new StringBuilder();
                nombreArchivoException = "APIException-" + (Guid.NewGuid().ToString()) + ".txt";
                string carpeta = logDirectory + @"Exceptions\" + DateTime.UtcNow.ToString("yyyy-MM") + @"\";

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                sb.AppendLine(DateTime.UtcNow.ToString());
                sb.AppendLine(method);
                sb.AppendLine(ex.ToString());

                File.WriteAllText(carpeta + nombreArchivoException, sb.ToString());
            }

            APILogItem logEntry = new APILogItem(method, token, deviceID, userID, ip, status, apiKey, formValues, jsonResponse, nombreArchivoException, nombreArchivoXml);

            lock (logQueue)
            {
                logQueue.Enqueue(logEntry);
                if (logQueue.Count >= queueSize || DoPeriodicFlush()) FlushLog();
            }
        }

        public void WriteToLog(string method, HttpRequestBase request, int userID, OperationResult opResult)
        {
            var respuestaJson = new JsonResult();
            respuestaJson.Data = new { ok = opResult.OK, msg = opResult.Msg };
            this.WriteToLog(method, request, userID, opResult.LogCode, respuestaJson, opResult.LogException);
        }

        private bool DoPeriodicFlush()
        {
            TimeSpan logAge = DateTime.Now - lastFlushed;
            if (logAge.TotalSeconds >= maxLogAge)
            {
                lastFlushed = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FlushLog()
        {
            while (logQueue.Count > 0)
            {
                APILogItem item = logQueue.Dequeue();
                string logPath = logDirectory + "APILog-" + item.logDate.ToString("yyyy-MM-dd") + ".txt";

                using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter log = new StreamWriter(fs))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(item.logDate.ToString("dd-MM-yyyy HH:mm:ss") + "\t");
                        sb.Append(item.method + "\t");
                        sb.Append(item.token + "\t");
                        sb.Append(item.deviceID + "\t");
                        sb.Append(item.userID + "\t");
                        sb.Append(item.ip + "\t");
                        sb.Append(item.status + "\t");
                        sb.Append(item.apiKey + "\t");
                        sb.Append(item.formValues + "\t");
                        sb.Append(item.jsonResponse + "\t");
                        sb.Append(item.errorFile + "\t");
                        sb.Append(item.xmlFile + "\t");

                        log.WriteLine(sb.ToString());
                    }
                }
            }
        }

        private string getUserIP(HttpRequestBase request)
        {
            string xForwardedFor = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if ((xForwardedFor ?? "").ToString().Trim() == "")
            {
                return request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                return xForwardedFor.Split(',').FirstOrDefault();
            }
        }

        // Listing files of Log folder
        public static List<String> ListingDirectoryFiles()
        {
            List<String> directoryFiles = new List<String>();
            if (Directory.Exists(Properties.Settings.Default.APILogDirectory))
            {
                string[] files = Directory.GetFiles(Properties.Settings.Default.APILogDirectory, "*.txt");
                foreach (var file in files)
                {
                    directoryFiles.Add(file.Replace(Properties.Settings.Default.APILogDirectory, "").Replace(".txt", ""));
                }
            }
            return directoryFiles;
        }

        // Reading the text file
        public static List<APILogItem> ReadingFile(string fileName, string lineNumber = null)
        {
            List<APILogItem> itemsFile = new List<APILogItem>();
            var dc = new AgroFichasDBDataContext();

            string file = String.Format("{0}{1}.txt", Properties.Settings.Default.APILogDirectory, fileName);
            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);
                string[] linesParsed = new string[1];

                if (lineNumber == null)
                    linesParsed = lines;
                else
                {
                    int thisLine;
                    int.TryParse(lineNumber, out thisLine);
                    linesParsed[0] = lines[thisLine];
                }

                foreach (string line in linesParsed)
                {
                    // Use a tab to indent each line of the file.
                    string[] items = line.Split('\t');
                    string formValues = items[8];
                    if (formValues == null)
                        formValues = "";

                    string jsonResponse = items[9].ToString();
                    if (jsonResponse == null)
                        jsonResponse = "";

                    // dd-MM-yyyy HH:mm:ss

                    var usCulture = new CultureInfo("en-us");

                    DateTime logDate;
                    if (items[0] == null)
                        logDate = new DateTime(2012, 01, 01);
                    else
                        if (!DateTime.TryParseExact(items[0].ToString(), "dd-MM-yyyy HH:mm:ss", usCulture, DateTimeStyles.None, out logDate))
                            logDate = new DateTime(2012, 01, 01);

                    int userID = 0;
                    string userName = "";
                    int.TryParse(items[4], out userID);
                    if (userID != 0)
                        try
                        {
                            userName = dc.SYS_User.FirstOrDefault(cs => cs.UserID == userID).FullName;
                        }
                        catch (Exception e)
                        {
                            userName = e.Message + ", " + items[4];
                        }

                    var apiLogItem = new APILogItem();
                    apiLogItem.method = (items[1].ToString() ?? "");
                    apiLogItem.token = (items[2].ToString() ?? "");
                    apiLogItem.deviceID = (items[3].ToString() ?? "");
                    apiLogItem.userID = userID;
                    apiLogItem.userName = userName;
                    apiLogItem.ip = (items[5].ToString() ?? "");
                    apiLogItem.status = (items[6].ToString() ?? "");
                    apiLogItem.apiKey = (items[7].ToString() ?? "");
                    apiLogItem.formValues = formValues;
                    apiLogItem.jsonResponse = jsonResponse;
                    apiLogItem.errorFile = (items[10].ToString() ?? "");
                    apiLogItem.logDate = logDate;
                    apiLogItem.xmlFile = (items[11].ToString() ?? "");
                    itemsFile.Add(apiLogItem);
                }
            }

            return itemsFile;
        }

        // Reading the exception file
        public static string ReadingException(string folderName, string fileName)
        {
            string exceptionFile = "";

            string file = String.Format("{0}Exceptions/{1}/{2}.txt", Properties.Settings.Default.APILogDirectory, folderName, fileName);

            if (File.Exists(file))
            {
                StreamReader sr = new System.IO.StreamReader(file);
                exceptionFile = sr.ReadToEnd();
                sr.Close();
            }

            return exceptionFile;
        }

        public static string ReadingXmlRequest(string folderName, string fileName)
        {
            string exceptionFile = "";

            string file = String.Format("{0}XmlRequests/{1}/{2}", Properties.Settings.Default.APILogDirectory, folderName, fileName);

            if (File.Exists(file))
            {
                StreamReader sr = new System.IO.StreamReader(file);
                exceptionFile = sr.ReadToEnd();
                sr.Close();
            }

            return exceptionFile;
        }
    }

    public class APILogItem
    {
        public string method;
        public string token;
        public string deviceID;
        public int userID;
        public string userName;
        public string ip;
        public string status;
        public string apiKey;
        public string formValues;
        public string jsonResponse;
        public string errorFile;
        public DateTime logDate;
        public string xmlFile;

        public APILogItem()
        {

        }

        public APILogItem(string method, string token, string deviceID, int customerID, string ip,
            string status, string apiKey, string formValues, string jsonResponse, string errorFile, string xmlFile)
        {
            this.method = method;
            this.token = token;
            this.deviceID = deviceID;
            this.userID = customerID;
            this.ip = ip;
            this.status = status;
            this.apiKey = apiKey;
            this.formValues = formValues;
            this.jsonResponse = jsonResponse;
            this.errorFile = errorFile;
            this.logDate = DateTime.UtcNow;
            this.xmlFile = xmlFile;
        }
    }
}