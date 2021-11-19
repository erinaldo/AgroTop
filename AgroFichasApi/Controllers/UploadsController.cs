using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasApi.Controllers
{
    public class UploadsController : Controller
    {
        //
        // GET: /Uploads/

        public ActionResult Index()
        {
            var files = System.IO.Directory.GetFiles(Server.MapPath("~/App_Data/sqlite/uploads"));
            ViewData["files"] = files.OrderByDescending(f => f).ToList();
            return View();
        }

        public FileResult GetFile()
        {
            var name = Request.QueryString["f"];
            var file = System.IO.Path.Combine(Server.MapPath("~/App_Data/sqlite/uploads"), name);
            return new FilePathResult(file, "application/octet-stream")
            {
                FileDownloadName = name
            };
        }

    }
}
