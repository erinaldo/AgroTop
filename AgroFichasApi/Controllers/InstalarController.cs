using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace AgroFichasApi.Controllers
{
    public class InstalarController : Controller
    {
        //
        // GET: /Instalar/

        public ActionResult Index()
        {
            var isIOS = Request.UserAgent.Contains("iPhone") ||
                        Request.UserAgent.Contains("iPad") ||
                        Request.UserAgent.Contains("iPod");
            
            if (isIOS)
            {
                var doc = new XmlDocument();
                doc.Load(Server.MapPath("~/content/apps/iosapp.xml"));

                var root = doc.DocumentElement;
                var version = root.SelectSingleNode("current").Attributes["version"].Value;
                var url = root.SelectSingleNode("current/plist").InnerText;

                var clientVer = "";
                if (Request.QueryString["v"] != null)
                    clientVer = Request.QueryString["v"];

                ViewData["clientVer"] = clientVer;
                ViewData["version"] = version;
                ViewData["url"] = url;

                return View("iOS");
            }
            else
            {
                return View("Android");
            }
            
        }

    }
}
