using AgrotopApiSap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace AgrotopApiSap.Controllers
{
    public class CurrentVersionController : ApiController
    {
        Models.AgroFichasDBDataContext dataContext = new Models.AgroFichasDBDataContext();

        public JsonResult<string> GetCurrentVersion()
        {
            SYS_Version _Version = dataContext.SYS_Version.SingleOrDefault();
            if (_Version != null)
            {
                return Json(_Version.CurrentVersion.ToString());
            }

            return Json(string.Empty);
        }
    }
}
