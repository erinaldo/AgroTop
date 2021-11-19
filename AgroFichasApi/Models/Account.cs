using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace AgroFichasApi.Models
{
    public class Account
    {
        public static SYS_User CheckTokenFromRequest(XmlElement root, out string errMsg, out string token, out string deviceId, out string apiKey)
        {
            errMsg = "";
            token = "";
            deviceId = "";
            apiKey = "";
            try
            {
                var element = root.SelectSingleNode("auth");

                token = element.Attributes["token"].Value;
                deviceId = element.Attributes["did"].Value;
                apiKey = element.Attributes["apiKey"].Value;

                return CheckToken(token, deviceId, apiKey, out errMsg);
            }
            catch
            {
                return null;
            }
        }

        public static SYS_User CheckTokenFromRequest(HttpRequestBase request, out string errMsg, out string token, out string deviceId, out string apiKey)
        {
            errMsg = "";
            token = "";
            deviceId = "";
            apiKey = "";
            try
            {

                token = (request["token"] ?? "").ToString().Trim();
                deviceId = (request["did"] ?? "").ToString().Trim();
                apiKey = (request["apiKey"] ?? "").ToString().Trim();

                return CheckToken(token, deviceId, apiKey, out errMsg);
            }
            catch
            {
                return null;
            }
        }

        private static SYS_User CheckToken(string token, string deviceId, string appId, out string errMsg)
        {
            errMsg = "";

            var dc = new AgroFichasDBDataContext();
            var mobsession = dc.MobileSession.SingleOrDefault(ms => ms.LoginToken == token);

            if (mobsession == null)
            {
                errMsg = "Token not found";
                return null;
            }

            if (mobsession.Invalidated)
            {
                errMsg = "Token invalidated";
                return null;
            }

            if (mobsession.DeviceID != deviceId)
            {
                errMsg = "Token not for this device";
                return null;
            }

            //if (mobsession.AppID != appId)
            //{
            //    errMsg = "Token from wrong AppID";
            //    return null;
            //}

            return mobsession.SYS_User;
        }

        public static string GetToken(int userId, string deviceId, string appId)
        {
            var dc = new AgroFichasDBDataContext();
            var user = dc.SYS_User.Single(c => c.UserID == userId);
            var mobsession = dc.MobileSession.SingleOrDefault(ms => ms.UserID == user.UserID && ms.DeviceID == deviceId && ms.AppID == appId && ms.Invalidated == false);
            
            if (mobsession == null)
            {
                mobsession = new MobileSession()
                {
                    LoginToken = Guid.NewGuid().ToString().Replace("-", ""),
                    UserID = user.UserID,
                    DeviceID = deviceId,
                    AppID = appId,
                    LastAccessDate = DateTime.UtcNow,
                    Invalidated = false,
                    DateTimeIns = DateTime.UtcNow,
                    IpIns = Utils.RemoteAddr(),
                    UserIns = "Account.GetToken"
                };
                dc.MobileSession.InsertOnSubmit(mobsession);
            }
            mobsession.DateTimeUpd = DateTime.UtcNow;
            mobsession.IpUpd = Utils.RemoteAddr();
            mobsession.UserUpd = "Account.GetToken";
            dc.SubmitChanges();

            return mobsession.LoginToken;
        }
    }
}