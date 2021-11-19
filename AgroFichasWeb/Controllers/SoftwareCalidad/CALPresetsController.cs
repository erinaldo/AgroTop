using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    public class CALPresetsController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALPresetsController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALPresets
        public ActionResult Index()
        {
            return View();
        }
    }
}