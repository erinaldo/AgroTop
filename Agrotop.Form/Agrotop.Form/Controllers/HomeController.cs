using Agrotop.Form.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Form.Controllers
{
    public class HomeController : Controller
    {
        private AgrotopDBDataContext dc = new AgrotopDBDataContext();

        public ActionResult Index()
        {
            var incorporacion = new IncorporacionCamiongo();

            return View("Index", incorporacion);
        }


        [HttpPost]
        public ActionResult Index(IncorporacionCamiongo incorporacion, FormCollection formCollection)
        {
            var okMsg = "";
            var errMsg = "";


            if (ModelState.IsValid)
            {
                try
                {
                    incorporacion.RUT = formCollection["rut"];
                    incorporacion.Nombre = formCollection["name"];
                    incorporacion.Email = formCollection["email"];
                    incorporacion.FechaHoraIns = DateTime.Now;
                    incorporacion.IpIns = RemoteAddr();

                    dc.IncorporacionCamiongo.InsertOnSubmit(incorporacion);
                    dc.SubmitChanges();

                    okMsg = "Tu incorporación a la red se realizó con éxito, por favor espera a uno de nuestros ejecutivos que se contactará contigo.";

                    return RedirectToAction("Confirmar", new { okMsg = okMsg, errMsg = errMsg });
                }
                catch
                {
                    var ruleViolations = incorporacion.GetRuleViolations();
                    if (ruleViolations.Count() > 0)
                        ModelState.AddRuleViolations(ruleViolations);
                    else
                        throw;
                }
            }

            return View("Index", incorporacion);
        }

        public ActionResult Confirmar(int? id)
        {
            return View();
        }

        public string RemoteAddr()
        {
            return HttpContext.Request.ServerVariables["REMOTE_ADDR"];
        }
    }
}
