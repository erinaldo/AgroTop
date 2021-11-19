using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Recepciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Recepciones
{
    public class ReStandarizarController : Controller
    {
        //
        // GET: /ReStandarizar/

        public ActionResult Index(int dosave = 0)
        {
            var dc = new AgroFichasDBDataContext();

            var model = new ReStandarizarViewModel();
            model.Load(Server.MapPath("~/App_Data/restandarizar.txt"), dc);

            if (dosave == 1)
            {
                dc.SubmitChanges();
            }

            return View(model);
        }

    }

    
}
