using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    [WebsiteAuthorize]
    public class InformesOperacionesController : BaseApplicationController
    {
        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        //
        // GET: /InformeOperaciones/

        public InformesOperacionesController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(236);
            return View();
        }

        public ActionResult InformeDiario(DateTime? dateTime)
        {
            CheckPermisoAndRedirect(236);

            if (dateTime == null)
                dateTime = DateTime.Now;

            dc.CommandTimeout = 3 * 60;

            InformeDiarioViewModel model = new InformeDiarioViewModel();
            model.ProduccionEnPlanta = dc.rpt_OPR_InformeDiario_ProduccionEnPlanta(dateTime).ToList();
            model.Detenciones = dc.rpt_OPR_InformeDiario_Detenciones(dateTime).ToList();
            model.EvolucionProduccionesDiarias = dc.rpt_OPR_InformeDiario_EvolucionProduccionesDiarias(dateTime).ToList();
            model.ConsumosDiariosMateriaPrima = dc.rpt_OPR_InformeDiario_ConsumosDiariosMateriaPrima(dateTime).ToList();
            model.ConsumoAcumuladoMateriaPrima = dc.rpt_OPR_InformeDiario_ConsumoAcumuladoMateriaPrima(dateTime).ToList();

            ViewData["dateTime"] = dateTime;
            return View(model);
        }
    }
}
