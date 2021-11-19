using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class FotoFichaPreSiembra
    {
        public string FotoPath
        {
            get
            {
                var baseFolder = ConfigurationManager.AppSettings["FotosFolder"];
                return Path.Combine(baseFolder, this.FileName.Substring(0, 1), this.FileName.Substring(0, 2), this.FileName);
            }
        }

        public string FotoUrl
        {
            get
            {
                return FotoFicha.FotoUrlForFileName(this.FileName);
            }
        }
    }
}