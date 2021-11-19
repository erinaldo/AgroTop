using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public partial class PRO_EvaluacionSuelo
    {
        private AgrotopDBDataContext dataContext = new AgrotopDBDataContext();

        public PRO_ResultadoEvaluacionSuelo GetResultadoEvaluacionSuelo(int IdParametroAnalisis)
        {
            return dataContext.PRO_ResultadoEvaluacionSuelo.SingleOrDefault(X => X.IdEvaluacionSuelo == this.IdEvaluacionSuelo && X.IdParametroAnalisis == IdParametroAnalisis);
        }
    }
}