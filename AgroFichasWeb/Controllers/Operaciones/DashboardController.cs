using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class DashboardController : BaseApplicationController
    {
        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        //
        // GET: /Dashboard/

        public ActionResult Index(DateTime? dateTime)
        {
            if (dateTime == null)
                dateTime = DateTime.Now;

            var model = new DashboardViewModel();
            model.OperadoresDelDia = dc.rpt_OPR_OperadoresDelDia(dateTime).ToList();
            model.ResumenDelDia = dc.rpt_OPR_ResumenDelDia(dateTime).SingleOrDefault();
            model.TurnosDelDia = dc.rpt_OPR_TurnosDelDia(dateTime).ToList();
            model.TotalPlantaDelDia = dc.rpt_OPR_TotalPlantaDelDia(dateTime).ToList();
            model.TotalDespachoDelDia = dc.rpt_OPR_TotalDespachoDelDia(dateTime).ToList();
            model.PuedeVerProductividad = CheckPermiso(213);
            ViewData["dateTime"] = dateTime;
            return View(model);
        }
    }
}