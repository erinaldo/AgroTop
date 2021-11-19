using Agrotop.Extranet.Controllers.Filters;
using Agrotop.Extranet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Extranet.Controllers
{
    [ExtranetAuthorize]
    public class MenuRecepcionesController : BaseController
    {
        public ActionResult Index(string id)
        {
            string token = "";
            if (id == "oleotop")
            {
                ViewBag.Title = "Recepciones Oleotop";
                token = "area=oleotop";
            }
            else if (id == "granotop")
            {
                ViewBag.Title = "Recepciones Granotop";
                token = "area=granotop";
            }
            else if (id == "avenatop")
            {
                ViewBag.Title = "Recepciones Avenatop";
                token = "area=avenatop";
            }
            else 
            {
                 throw new HttpException((Int32)System.Net.HttpStatusCode.NotFound, "area not found");
            }

            var rut = UserState.Current().UserName; //"10853638-1"; //Oleotop
            token += String.Format("&action=login&rut={0}&ts={1:yyyy-MM-dd-hh-mm}", rut, DateTime.Now);

            var enc = new Encryptor();
            token = enc.EncryptString(token);

            string redirectUrl = string.Format("http://200.54.154.91:8080/bridge.php?h={0}", Url.Encode(token));

            ViewData["frameUrl"] = redirectUrl;

            return View();
        }

    }



}
