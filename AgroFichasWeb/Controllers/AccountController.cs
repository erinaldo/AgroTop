using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace AgroFichasWeb.Controllers
{
    public class AccountController : Controller
    {
        UsuarioRepository db = new UsuarioRepository();

        public ActionResult Logon()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Logon(string usuario, string password, string returnUrl)
        {
            var usuarioAdmin = db.GetByUserNameAndPassword(usuario.Trim(), password.Trim());
            if (usuarioAdmin != null && usuarioAdmin.SYS_PermisoUsuario.Count > 0)
            {
                usuarioAdmin.LastLogin = DateTime.Now;
                db.Save();

                FormsAuthentication.SetAuthCookie(usuario, false);

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    foreach (var modulo in usuarioAdmin.Modulos)
                    {
                        var firstMenu = usuarioAdmin.ItemsMenu(modulo.ID).FirstOrDefault();
                        if (firstMenu != null)
                            Response.Redirect(firstMenu.Url, true);
                    }
                    throw new Exception("Usuario sin módulos");
                }
            }
            else
            {
                ModelState.AddModelError("_FORM", "El nombre de usuario o la contraseña son incorrectos.");
                return View("Logon");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Logon");
        }
    }
}