using AgroFichasApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasApi.Controllers
{
    public class DumpDataController : Controller
    {
        //
        // GET: /DumData/

        public ActionResult Index()
        {
            var dc = new AgroFichasDBDataContext();
            ViewData["agricultores"] = dc.Agricultor.OrderBy(a => a.Nombre);

            return View();
        }

    }
}
