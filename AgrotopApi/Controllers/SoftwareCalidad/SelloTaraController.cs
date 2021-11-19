using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class SelloTaraController : ApiController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public string GetSello(string NContenedor)
        {
            CAL_RITSelloTara sellotara = (from X in dc.CAL_RITSelloTara
                                             where X.NContenedor == NContenedor
                                             select X).SingleOrDefault();




            if (sellotara != null)
                return sellotara.SelloLinea;
            else
                return String.Format(" ");
        }

        public decimal GetTara(string NContenedor)
        {
            CAL_RITSelloTara sellotara = (from X in dc.CAL_RITSelloTara
                                          where X.NContenedor == NContenedor
                                          select X).SingleOrDefault();




            if (sellotara != null)
                return sellotara.Tara;
            else
                return 0;
        }

        public string GetSelloPallets(string NContenedor)
        {
            CAL_RITSelloTara sellotara = (from X in dc.CAL_RITSelloTara
                                          where X.NContenedor == NContenedor
                                          select X).SingleOrDefault();




            if (sellotara != null)
                return sellotara.SelloLinea;
            else
                return String.Format(" ");
        }

        public decimal GetTaraPallets(string NContenedor)
        {
            CAL_RITSelloTara sellotara = (from X in dc.CAL_RITSelloTara
                                          where X.NContenedor == NContenedor
                                          select X).SingleOrDefault();




            if (sellotara != null)
                return sellotara.Tara;
            else
                return 0;
        }
    }
}
