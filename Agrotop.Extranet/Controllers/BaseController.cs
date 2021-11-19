using Agrotop.Extranet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Extranet.Controllers
{
    public class BaseController : Controller
    {
        protected AgrotopDBDataContext dc = new AgrotopDBDataContext();
        protected UserState user = UserState.Current();

        public string RemoteAddr()
        {
            return HttpContext.Request.ServerVariables["REMOTE_ADDR"];
        }

    }
}
