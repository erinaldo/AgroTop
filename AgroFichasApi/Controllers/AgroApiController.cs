using AgroFichasApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Controllers
{
    public enum SyncType
    {
        Differential,
        Full,
        FullFile
    }

    public class AgroApiController : Controller
    {
        [HttpPost]
        public JsonResult Login(FormCollection formValues)
        {
            bool ok = true;
            string msg = "";
            string token = "";
            int userId = 0;
            string userName = "";
            string fullName = "";
            string email = "";
            Exception logEx = null;
            string logCode = "200";

            try
            {
                //Validate input
                userName = (formValues["username"] ?? "").ToString().Trim();
                var password = (formValues["pwd"] ?? "").ToString().Trim();
                var deviceId = (formValues["did"] ?? "").ToString().Trim();
                var apiKey = (formValues["apiKey"] ?? "").ToString().Trim();

                if (ok && userName == "")
                {
                    ok = false;
                    msg = "Username required";
                    logCode = "501";
                }

                if (ok && password == "")
                {
                    ok = false;
                    msg = "Password required";
                    logCode = "501";
                }

                if (ok && deviceId == "")
                {
                    ok = false;
                    msg = "DeviceId required";
                    logCode = "501";
                }

                if (ok && apiKey == "")
                {
                    ok = false;
                    msg = "Invalid apiKey";
                    logCode = "501";
                }

                //Check Login
                if (ok)
                {
                    var dc = new AgroFichasDBDataContext();
                    var r = dc.CheckLoginApi(userName, Utils.EncryptPassword(password), "AgroApi.Login", Utils.RemoteAddr());

                    var user = r.FirstOrDefault();
                    if (user != null)
                    {
                        ok = true;
                        msg = "Logged in succesfully";
                        userId = user.UserID;
                        userName = user.UserName;
                        fullName = user.FullName;
                        email = user.Email;
                        token = Account.GetToken(userId, deviceId, apiKey);
                    }
                    else
                    {
                        ok = false;
                        msg = "Wrong username or password";
                    }
                }
            }
            catch (Exception ex)
            {
                logEx = ex;
                logCode = "502";
                ok = false;
                msg = "There was an error trying to log you in. Please try again";
            }

            var result = Json(new { ok = ok, msg = msg, code = logCode, token = token, uid = userId, name = fullName, username = userName, email = email });
            APILog.Instance.WriteToLog("AgroApi.Login", Request, userId, logCode, result, logEx);

            return result;
        }

        [HttpPost]
        public ActionResult DiffSync(FormCollection formValues)
        {
            return DoSync(formValues, SyncType.Differential);
        }

        [HttpPost]
        public ActionResult FullSync(FormCollection formValues)
        {
            return DoSync(formValues, SyncType.Full);
        }

        [HttpPost]
        public ActionResult FullFileSync(FormCollection formValues)
        {
            return DoSync(formValues, SyncType.FullFile);
        }

        private ActionResult DoSync(FormCollection formValues, SyncType syncType)
        {
            var opResult = new OperationResult(true, "", "200", null);

            //Auth
            int userId = 0;
            string tokenMsg = "";
            string token = "";
            string deviceId = "";
            string apiKey = "";

            string xmlString = "";

            try
            {
                //Get Request
                var ms = new MemoryStream();
                Request.InputStream.CopyTo(ms);

                xmlString = Encoding.UTF8.GetString(ms.ToArray());

                var xml = new XmlDocument();
                xml.LoadXml(xmlString);

                var root = xml.DocumentElement;

                var user = Account.CheckTokenFromRequest(root, out tokenMsg, out token, out deviceId, out apiKey);
                if (user == null)
                    throw new Exception("Unathorized");
                else
                    userId = user.UserID;

                //Apply changes
                var dc = new AgroFichasDBDataContext();

                //Agricultores
                var result = new List<XElement>();
                result.AddRange(Repository.SyncTable<Agricultor>(dc, root, Agricultor.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<Predio>(dc, root, Predio.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<Potrero>(dc, root, Potrero.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<Siembra>(dc, root, Siembra.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<Ficha>(dc, root, Ficha.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<Recomendacion>(dc, root, Recomendacion.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<FotoFicha>(dc, root, FotoFicha.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<FichaPreSiembra>(dc, root, FichaPreSiembra.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<RecomendacionPreSiembra>(dc, root, RecomendacionPreSiembra.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<FotoFichaPreSiembra>(dc, root, FotoFichaPreSiembra.XmlTag, user.UserName));
                result.AddRange(Repository.SyncTable<IntencionSiembra>(dc, root, IntencionSiembra.XmlTag, user.UserName));

                //Response
                XDocument response = null;
                string file = "";

                if (syncType == SyncType.Full)
                {
                    var fullSync = new FullSync();
                    fullSync.FillResponse(dc);
                    response = fullSync.Response;
                }
                else if (syncType == SyncType.Differential)
                {
                    var diffSync = new DifferentialSync();
                    diffSync.FillResponse(result);
                    response = diffSync.Response;
                }
                else if (syncType == SyncType.FullFile)
                {
                    var fileSyc = new FullFileSync();
                    fileSyc.FillResponse(dc, user);
                    file = fileSyc.FileName;  //System.IO.Path.Combine(Server.MapPath("~/App_Data/sqlite/uploads"), "20130518_070131_donwoc_AgroFichasDB.db3");
                    opResult.Msg = file;
                }
                else
                {
                    throw new Exception("Unsopported Sync Type " + syncType.ToString());
                }

                APILog.Instance.WriteToLog("AgroApi.DoSync " + Enum.GetName(typeof(SyncType), syncType), Request, userId, opResult.LogCode, ToJsonResult(opResult), null, token, deviceId, apiKey, xmlString);

                if (response != null)
                {
                    return new XmlActionResult(response);
                }
                else
                {
                    return new FilePathResult(file, "application/octet-stream")
                    {
                        FileDownloadName = "AgroFichasDB.db3"
                    };
                }
            }
            catch (Exception ex)
            {
                opResult = new OperationResult(false, "There was an error trying to sync", "502", ex);
                APILog.Instance.WriteToLog("AgroApi.DoSync " + Enum.GetName(typeof(SyncType), syncType), Request, userId, opResult.LogCode, ToJsonResult(opResult), ex, token, deviceId, apiKey, xmlString);

                throw;
            }
        }

        [HttpPost]
        public void UploadFile(FormCollection formValues)
        {
            var opResult = new OperationResult(true, "", "200", null);

            //Auth
            int userId = 0;
            string tokenMsg = "";
            string token = "";
            string deviceId = "";
            string apiKey = "";

            try
            {
                var user = Account.CheckTokenFromRequest(Request, out tokenMsg, out token, out deviceId, out apiKey);
                if (user == null)
                    throw new Exception("Unathorized");
                else
                    userId = user.UserID;

                if (Request.Files.Count == 0)
                    throw new Exception("No files in request");

                var filename = Path.Combine(Server.MapPath("~/App_Data/sqlite/uploads"), DateTime.Now.ToString("yyyyMMdd_hhmmss_") + user.UserName + "_" + Request.Files[0].FileName);
                Request.Files[0].SaveAs(filename);

                opResult.Msg = filename;

                APILog.Instance.WriteToLog("AgroApi.UploadFile", Request, userId, opResult.LogCode, ToJsonResult(opResult), null, token, deviceId, apiKey, "");

            }
            catch (Exception ex)
            {
                opResult = new OperationResult(false, "There was an error trying to upload the file", "502", ex);
                APILog.Instance.WriteToLog("AgroApi.UploadFile", Request, userId, opResult.LogCode, ToJsonResult(opResult), ex, token, deviceId, apiKey, "");

                throw;
            }
        }

        public JsonResult ToJsonResult(OperationResult result)
        {
            return Json(new { ok = result.OK, msg = result.Msg, code = result.LogCode });
        }

        public ActionResult SendFicha(int id, string h)
        {
            OperationResult result;
            try
            {
                var dc = new AgroFichasDBDataContext();

                var ficha = dc.Ficha.SingleOrDefault(f => f.IdFicha == id);
                if (ficha == null || ficha.Hash2 != h)
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Ficha Not Found");

                result = ficha.Notify(dc);
            }
            catch (Exception ex)
            {
                result = new OperationResult(false, ex.Message, "501", ex);
            }

            APILog.Instance.WriteToLog("AgroApi.SendFicha " + id.ToString(), Request, 0, result);

            ViewData["result"] = result.OK;
            ViewData["msg"] = result.Msg;

            return View();
        }

        public ActionResult SendFichaPreSiembra(int id, string h)
        {
            OperationResult result;
            try
            {
                var dc = new AgroFichasDBDataContext();

                var ficha = dc.FichaPreSiembra.SingleOrDefault(f => f.IdFichaPreSiembra == id);
                if (ficha == null || ficha.Hash2 != h)
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "FichaPreSiembra Not Found");

                result = ficha.Notify();
            }
            catch (Exception ex)
            {
                result = new OperationResult(false, ex.Message, "501", ex);
            }

            APILog.Instance.WriteToLog("AgroApi.SendFicha " + id.ToString(), Request, 0, result);

            ViewData["result"] = result.OK;
            ViewData["msg"] = result.Msg;

            return View();
        }

        /*
         * TESTS 
         * **********************************************************/
        public void Notify(int id)
        {
            var dc = new AgroFichasDBDataContext();
            var ficha = dc.Ficha.SingleOrDefault(f => f.IdFicha == id);
            if (ficha == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Ficha Not Found");

            try
            {
                ficha.NotifyCreator(dc);
            }
            catch (Exception ex)
            {
                var s = "";
                s += "DateTime: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "\r\n";
                s += "IdFicha: " + ficha.IdFicha + "\r\n";
                s += ex.ToString();
                System.IO.File.WriteAllText(Server.MapPath("~/App_Data/notifications/errors/") + System.Guid.NewGuid() + ".txt", s);
            }
        }

        public ActionResult TestFullFileSync()
        {
            var dc = new AgroFichasDBDataContext();
            var user = dc.SYS_User.Single(u => u.UserName == "donwoc");

            var fileSyc = new FullFileSync();
            fileSyc.FillResponse(dc, user);
            var file = fileSyc.FileName;

            return new FilePathResult(file, "application/octet-stream")
            {
                FileDownloadName = "AgroFichasDB.db3"
            };


        }
    }
}
