using AgroFichasWeb.Controllers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALListasEmpaqueController : BaseApplicationController
    {
        public CALListasEmpaqueController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALListasEmpaque
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(387);
            return View();
        }
    }
}