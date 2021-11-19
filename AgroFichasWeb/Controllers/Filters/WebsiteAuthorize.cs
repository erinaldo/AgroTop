using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Filters
{
    public class WebsiteAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool result = SYS_User.Current() != null;
            return result;
        }


        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                        {
                                { "controller", "Account" },
                                { "action", "Logon" },
                                { "ReturnUrl", filterContext.HttpContext.Request.RawUrl }
                        });
            }
        }
    }
}