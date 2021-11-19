using AgrotopApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class CertificadosCargaGranelController : ApiController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public List<SelectAnalisisPeriodico> GetAnalisisPeriodicos(int IdDetalleOrdenProduccion, int IdTipoAnalisis)
        {
            List<SelectAnalisisPeriodico> list = new List<SelectAnalisisPeriodico>();
            list = (from X in dcSoftwareCalidad.CAL_GetAnalisisPeriodicosPorDetalleOrdenProduccionYTipoAnalisis(IdDetalleOrdenProduccion, IdTipoAnalisis)
                    select new SelectAnalisisPeriodico
                    {
                        Descripcion = string.Format("{0:dd/MM/yy HH:mm} - Lote {1} - {2} ({3}) - Frec: {4} ({5} {6})", X.FechaAnalisis, X.LoteComercial, X.ProductoNombre, X.SubproductoNombre, X.Frecuencia, X.CntDias, (X.CntDias > 1 ? "días" : "día")),
                        IdAnalisisPeriodico = X.IdAnalisisPeriodico
                    }).ToList();

            return list;
        }
    }
}
