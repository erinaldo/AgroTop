using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public partial class PasswordResetRequest
    {
        public bool IsResetRequestValid()
        {

            if (!this.Valid)
                return false;

            if (this.ExpirationDateTime < DateTime.Now)
                return false;

            if (!this.EmailSent)
                return false;

            if (this.Used)
                return false;

            if (!this.IdAgricultor.HasValue)
                return false;
            
            if (!this.Agricultor.Habilitado)
                return false;

            return true;
        }

    }
}