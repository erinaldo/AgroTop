using AgroFichasWeb.Controllers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALCertificadosController : BaseApplicationController
    {
        public CALCertificadosController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALCertificados
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(377);
            return View();
        }
    }
}