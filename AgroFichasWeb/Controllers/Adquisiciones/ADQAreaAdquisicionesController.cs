using AgroFichasWeb.Controllers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Adquisiciones
{
    [WebsiteAuthorize]
    public class ADQAreaAdquisicionesController : BaseApplicationController
    {
        public ADQAreaAdquisicionesController()
        {
            SetCurrentModulo(11);
        }

        // GET: ADQOrdenCompra
        public ActionResult Index()
        {
            return View();
        }
    }
}