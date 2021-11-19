using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public partial class API_User
    {
        public bool HasPermiso(int idPermiso)
        {
            return this.API_PermisoUsuario.SingleOrDefault(X => X.IdPermiso == idPermiso) != null;
        }
    }
}