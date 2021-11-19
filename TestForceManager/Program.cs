using ForceManagerLib;
using ForceManagerLib.Models.Requests;
using ForceManagerLib.Models.TrazaTop;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;

namespace TestForceManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Proxy proxy = new Proxy();
            string token = proxy.Login();

            JsonResultCreateDocumentResource jsonResult = CreateDocumentToResourceStep1(token);
            if (jsonResult.locator != "")
            {
                if (CreateDocumentToResourceStep2(jsonResult.locator))
                {
                    System.Console.WriteLine("File Upload OK");
                }
                else
                {
                    System.Console.WriteLine("Error");
                }
            }

            System.Console.ReadLine();
        }

        public static JsonResultCreateDocumentResource CreateDocumentToResourceStep1(string token)
        {
            JsonResultCreateDocumentResource jsonResult = new JsonResultCreateDocumentResource();

            //Agricultor de Prueba
            string endpoint = "https://api.forcemanager.net/api/v4/accounts/1268/documents";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Session-Key", token);
            httpWebRequest.Method = "POST";

            JsonRequestCreateDocumentResource jsonRequest = new JsonRequestCreateDocumentResource()
            {
                folderId = -1,
                name = "Contrato.pdf",
                level = 2,
            };

            string postData = JsonConvert.SerializeObject(jsonRequest);
            byte[] bytes = Encoding.ASCII.GetBytes(postData);
            httpWebRequest.ContentLength = bytes.Length;
            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                jsonResult = JsonConvert.DeserializeObject<JsonResultCreateDocumentResource>(result);
            }

            httpWebResponse.Close();

            return jsonResult;
        }

        public static bool CreateDocumentToResourceStep2(string locator)
        {
            bool OK = true;

            string endpoint = locator;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.ContentType = "application/octet-stream";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Method = "PUT";

            FileStream fileStream = new FileStream("c:\\Lorem ipsum dolor sit amet.pdf", FileMode.Open);
            MemoryStream memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);

            byte[] bytes = memoryStream.ToArray();
            httpWebRequest.ContentLength = bytes.Length;
            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                OK = true;
            }
            else
            {
                OK = false;
            }

            httpWebResponse.Close();
            memoryStream.Close();
            fileStream.Close();

            return OK;
        }
    }
}