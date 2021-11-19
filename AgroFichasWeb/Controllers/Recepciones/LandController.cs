using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using AgroFichasWeb.ViewModels.Recepciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers
{
    public class LandController : Controller
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ActionResult Autorizar(string h)
        {
            AutorizarViewModel model = null;
            try
            {
                int userID;
                int idProcesoIngreso;

                AutorizarViewModel.DecodeToken(dc, h, out userID, out idProcesoIngreso);
                model = new AutorizarViewModel(dc, idProcesoIngreso);

                ViewData["msg"] = "";
                ViewData["token"] = h;
            }
            catch (Exception ex)
            {
                ViewData["msg"] = ex.Message;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Autorizar2(string h)
        {
            AutorizarViewModel model = null;
            try
            {
                int userID;
                int idProcesoIngreso;

                AutorizarViewModel.DecodeToken(dc, h, out userID, out idProcesoIngreso);

                model = new AutorizarViewModel(dc, idProcesoIngreso);
                UpdateModel(model, new string[] { "Autorizado"} );

                var userName = dc.SYS_User.Single(u => u.UserID == userID).UserName;
                var ingreso = model.Persist(dc, userName, RemoteAddr());

                var msgerr = "";
                var msgok = "";
                if (ingreso.Autorizado == ProcesoIngreso.ANALISIS_RECHAZADO)
                {
                    string msg = "";
                    if (ingreso.NotificarRechazoFinalLaboratorio(ControllerContext, out msg))
                        msgok = msg;
                    else
                        msgerr = msg;
                }

                return RedirectToAction("AutorizarFin", new { id = model.ProcesoIngreso.IdProcesoIngreso, msgerr = msgerr, msgok = msgok  });
            }
            catch (Exception ex)
            {
                ViewData["msg"] = ex.Message;
                return View("Autorizar");
            }
        }

        public ActionResult AutorizarFin(int id)
        {
            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);
            return View(ingreso);
        }

        public string RemoteAddr()
        {
            return HttpContext.Request.ServerVariables["REMOTE_ADDR"];
        }

    }
}
