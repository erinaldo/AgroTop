using AgroFichasWeb.Controllers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class ModulosController : BaseApplicationController
    {
        //
        // GET: /Modulos/

        public ActionResult Index(int id)
        {
            string url = "";
            var modulo = CurrentUser.Modulos.SingleOrDefault(m => m.ID == id);
            if (modulo != null)
            {
                var menu = CurrentUser.ItemsMenu(modulo.ID).FirstOrDefault();
                if (menu != null)
                {
                    url = menu.Url;
                }
            }
            if (url != "")
                return Redirect(url);
            else
                throw new HttpException((Int32)HttpStatusCode.NotFound, "Not Found");

        }

    }
}
