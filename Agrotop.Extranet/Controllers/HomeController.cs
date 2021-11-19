using Agrotop.Extranet.Controllers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Extranet.Controllers
{
    [ExtranetAuthorize]
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return RedirectToAction("index", "recepciones");
        }

    }
}
