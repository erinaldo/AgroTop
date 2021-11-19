using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AgroFichasApi.Models;

namespace AgroFichasApi.Controllers
{
    public class LogViewerController : Controller
    {

        public ActionResult ShowDetails()
        {
            string fileName = Request.QueryString["File"];
            string lineNumber = Request.QueryString["Line"];
            var readingFile = APILog.ReadingFile(fileName, lineNumber);
            ViewData["APILog"] = readingFile;
            ViewData["FileName"] = fileName;
            ViewData["Line"] = lineNumber;
            return View();
        }

        public ActionResult ShowException()
        {
            string folderName = Request.QueryString["Folder"];
            string fileName = Request.QueryString["File"];
            var readingFile = APILog.ReadingException(folderName, fileName);
            ViewData["APILog"] = readingFile;
            ViewData["FileName"] = fileName;
            return View();
        }

        public ActionResult ShowFile()
        {
            string fileName = Request.QueryString["File"];
            var readingFile = APILog.ReadingFile(fileName);
            ViewData["APILog"] = readingFile;
            ViewData["FileName"] = fileName;
            return View();
        }

        public ActionResult Index()
        {
            var files = APILog.ListingDirectoryFiles();
            files.Sort();
            files.Reverse();
            ViewData["DirectoryFiles"] = files;
            return View();
        }

        public FileContentResult GetXmlRequest()
        {
            string fileName = Request.QueryString["File"];
            string folderName = Request.QueryString["Folder"];

            string fullPath = String.Format("{0}XmlRequests/{1}/{2}.xml", Properties.Settings.Default.APILogDirectory, folderName, fileName);

            return new FileContentResult(System.IO.File.ReadAllBytes(fullPath), "text/xml"); 
            
        }
    }
}
