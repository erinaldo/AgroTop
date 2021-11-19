using AgrotopApi.Models;
using System.Linq;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class PaleController : ApiController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public ResultTipoPale GetTipoPale(int id)
        {
            ResultTipoPale resultTipoPale = new ResultTipoPale();
            CAL_TipoPale cAL_TipoPale = dc.CAL_TipoPale.SingleOrDefault(X => X.IdTipoPale == id);
            if (cAL_TipoPale != null)
            {
                resultTipoPale.IdTipoPale = cAL_TipoPale.IdTipoPale;
                resultTipoPale.Descripcion = cAL_TipoPale.Descripcion;
                resultTipoPale.CntMax = cAL_TipoPale.CntMax;
            }

            return resultTipoPale;
        }
    }
}
