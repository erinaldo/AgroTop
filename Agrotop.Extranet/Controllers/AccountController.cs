using Agrotop.Extranet.Controllers.Filters;
using Agrotop.Extranet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Agrotop.Extranet.Controllers
{
    public class AccountController : BaseController
    {
        public ActionResult Login()
        {
            return View("Logon");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(string usuario, string password, string returnUrl)
        {
            var userState = new UserState();
            var ok = userState.Create(Agricultor.NomarlizarRut(usuario), Agricultor.HashPassword(password), "AccountController.Logon", RemoteAddr());

            if (ok)
            {
                FormsAuthentication.SetAuthCookie(userState.IdAgricultor.ToString(), false);
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("index", "home");
            }
            else
            {
                ModelState.AddModelError("_FORM", "El rut o la contraseña son incorrectos.");
                return View("Logon");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("login");
        }


        public ActionResult InitRecovery()
        {
            return View();
        }

        [HttpPost]
        public ActionResult InitRecovery(string usuario)
        {
            var agricultor = dc.Agricultor.SingleOrDefault(ag => ag.Rut == Agricultor.NomarlizarRut(usuario));
            if (agricultor == null || !agricultor.Habilitado)
            {
                ModelState.AddModelError("_FORM", "El rut no se encuentra registrado en nuestro sistema.");
                return View("InitRecovery");
            }
            
            if (String.IsNullOrEmpty(agricultor.Email))
            {
                ModelState.AddModelError("_FORM", "Lo sentimos, pero no tenemos registrada su dirección de email. Contacte a nuestro servicio al cliente.");
                return View("InitRecovery");

            }
            var request = agricultor.CreatePasswordResetRequest(RemoteAddr());

            return View("InitRecoverySent", request);
        }

        public ActionResult Reset(string id)
        {
            var pRequest = dc.PasswordResetRequest.SingleOrDefault(rr => rr.ParamKey == id);

            if (pRequest == null || !pRequest.IsResetRequestValid())
                return View("ResetInvalid");

            return View(pRequest);
        }

        [HttpPost]
        public ActionResult Reset(string id, string password1, string password2)
        {
            var pRequest = dc.PasswordResetRequest.SingleOrDefault(rr => rr.ParamKey == id);

            if (pRequest == null || !pRequest.IsResetRequestValid())
                return View("ResetInvalid");

            if (String.IsNullOrEmpty(password1) || password1.Length < 4)
            {
                ModelState.AddModelError("_FORM", "Su nueva contraseña debe tener al menos 4 caracteres");
                return View(pRequest);
            }

            if (password1 != password2)
            {
                ModelState.AddModelError("_FORM", "Las contraseñas no coinciden");
                return View(pRequest);
            }

            dc.PasswordLog.InsertOnSubmit( new PasswordLog()
            {
                 DateTimeIns = DateTime.Now,
                 IpIns = RemoteAddr(),
                 NewPassword = Agricultor.HashPassword(password1),
                 OldPassword = pRequest.Agricultor.Password,
                 UserIns = "passwordreset",
                 UserName = pRequest.Agricultor.Rut
            });
           
            pRequest.Agricultor.Password = Agricultor.HashPassword(password1);
            pRequest.Agricultor.MustChangePassword = false;
            pRequest.Used = true;
            pRequest.DateTimeUsed = DateTime.Now;
            pRequest.IpUsed = RemoteAddr();

           

            dc.SubmitChanges();

            return View("ResetSuccess");
        }

        [ExtranetAuthorize]
        public ActionResult Change()
        {
            return View();
        }

        [ExtranetAuthorize]
        [HttpPost]
        public ActionResult Change(string password1, string password2)
        {
            if (String.IsNullOrEmpty(password1) || password1.Length < 4)
            {
                ModelState.AddModelError("_FORM", "Su nueva contraseña debe tener al menos 4 caracteres");
                return View();
            }

            if (password1 != password2)
            {
                ModelState.AddModelError("_FORM", "Las contraseñas no coinciden");
                return View();
            }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == UserState.Current().IdAgricultor);

            dc.PasswordLog.InsertOnSubmit(new PasswordLog()
            {
                DateTimeIns = DateTime.Now,
                IpIns = RemoteAddr(),
                NewPassword = Agricultor.HashPassword(password1),
                OldPassword = agricultor.Password,
                UserIns = "change:" + UserState.Current().UserName,
                UserName = agricultor.Rut
            });

            agricultor.Password = Agricultor.HashPassword(password1);
            agricultor.MustChangePassword = false;
            
            dc.SubmitChanges();

            return RedirectToAction("index", "home");
        }
    }
}
