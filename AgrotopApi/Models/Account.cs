using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace AgrotopApi.Models
{
    public class Account
    {
        public static API_User CheckPrivateKey(string privateKey, out string errMsg)
        {
            errMsg = "";

            var dc = new AgroFichasDBDataContext();
            var user = dc.API_User.SingleOrDefault(X => X.PrivateKey == privateKey);

            if (user == null)
            {
                errMsg = "La clave privada no es válida";
                return null;
            }

            if (user.Disabled)
            {
                errMsg = "Acceso deshabilitado";
                return null;
            }

            return user;
        }
    }
}