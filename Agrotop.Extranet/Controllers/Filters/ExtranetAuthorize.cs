using Agrotop.Extranet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Extranet.Controllers.Filters
{
    public class ExtranetAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return UserState.Current() != null;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            //Si tenemos sesión, verificamos si el usuario debe cambiar su contraseña antes de continuar
            if (!(filterContext.Result is HttpUnauthorizedResult))
            {
                //Revisamos a menos que ya estemos en la página de cambio de contraseña
                if (!(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToUpper() == "ACCOUNT" && filterContext.ActionDescriptor.ActionName.ToUpper() == "CHANGE"))
                {
                    var user = UserState.Current();
                    if (user.MustChangePassword)
                    {
                        filterContext.Result = new RedirectToRouteResult(
                            new System.Web.Routing.RouteValueDictionary
                            {
                                { "controller", "account" },
                                { "action", "change" }
                            });
                    }
                }
            }
        }
    }
}