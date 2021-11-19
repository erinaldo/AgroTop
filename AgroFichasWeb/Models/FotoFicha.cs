using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class FotoFicha
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
                return FotoUrlForFileName(this.FileName);
            }
        }
        
        public static string FotoUrlForFileName(string filename)
        {
            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            return $"{baseUrl}/content/fotos/{filename.Substring(0, 1)}/{filename.Substring(0, 2)}/{filename}";

        }
    }
}