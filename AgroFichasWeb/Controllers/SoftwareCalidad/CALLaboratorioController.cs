using AgroFichasWeb.Controllers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALLaboratorioController : BaseApplicationController
    {
        public CALLaboratorioController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALLaboratorio
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(396);
            return View();
        }
    }
}