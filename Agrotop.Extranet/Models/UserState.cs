using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;

namespace Agrotop.Extranet.Models
{
    [Serializable]
    public class UserState
    {
        private int idAgricultor;
        private string userName;
        private string fullName;

        public int IdAgricultor
        {
            get { return this.idAgricultor; }
        }

        public string UserName
        {
            get { return this.userName; }
        }

        public string FullName
        {
            get { return this.fullName; }
        }

        public bool MustChangePassword
        {
            get;
            private set;
        }

        public UserState()
        {
        }

        public bool Create(string userName, string password, string source, string ipAddress)
        {
            var dc = new AgrotopDBDataContext();

            var check = dc.CheckLoginAgricultor(userName, password, source, ipAddress).FirstOrDefault();
            if (check == null || !check.AccessGranted.HasValue || !check.AccessGranted.Value)
                return false;

            return CreateFromIdAgricultor(check.IdAgricultor.Value);
        }

        private bool CreateFromIdAgricultor(int idAgricultor)
        {
            var dc = new AgrotopDBDataContext();
            var agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == idAgricultor);
            if (agricultor == null)
                return false;

            this.idAgricultor = agricultor.IdAgricultor;
            this.userName = agricultor.Rut;
            this.fullName = agricultor.Nombre;
            this.MustChangePassword = agricultor.MustChangePassword ?? false;
            return true;
        }


        public static UserState Current()
        {
            IPrincipal user = HttpContext.Current.User;
            if (user == null || !user.Identity.IsAuthenticated)
                return null;
            try
            {
                var userState = new UserState();
                if (!userState.CreateFromIdAgricultor(int.Parse(user.Identity.Name)))
                    return null;

                return userState;
            }
            catch
            {
                return null;
            }
        }
    }
}