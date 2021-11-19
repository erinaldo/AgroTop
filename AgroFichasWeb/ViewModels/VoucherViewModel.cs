using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class VoucherViewModel
    {
        public string Fecha { get; set; }

        public string Origen { get; set; }

        public string Destino { get; set; }

        public string Transportista { get; set; }

        public string Camion { get; set; }

        public string ImgSrc { get; set; }

        public string FirmaDigital { get; set; }

        public string ReturnUrl { get; set; }
    }
}