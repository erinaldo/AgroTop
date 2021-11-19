using AgroFichasWeb.Controllers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFTControlVersionIndexController : BaseApplicationController
    {
        public CALFTControlVersionIndexController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFTControlVersionIndex
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(331);
            return View();
        }
    }
}