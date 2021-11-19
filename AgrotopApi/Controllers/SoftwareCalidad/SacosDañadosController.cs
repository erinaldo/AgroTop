using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class SacosDañadosController : ApiController
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public List<SelectSacosDañados> GetSacosDañados(int IdProducto, int IdOrdenProduccion)
        {
            List<SelectSacosDañados> selectList = (from X in dcSoftwareCalidad.CAL_DespachoPale
                                                   join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                                                   join Z in dcSoftwareCalidad.CAL_DetalleOrdenProduccion on Y.IdDetalleOrdenProduccion equals Z.IdDetalleOrdenProduccion
                                                   join A in dcSoftwareCalidad.CAL_Producto on Z.IdProducto equals A.IdProducto
                                                   join B in dcSoftwareCalidad.CAL_OrdenProduccion on Z.IdOrdenProduccion equals B.IdOrdenProduccion
                                                   where X.Habilitado == true
                                                   && B.Habilitado == true
                                                   && A.IdProducto == IdProducto
                                                   && B.IdOrdenProduccion == IdOrdenProduccion
                                                   && X.SacosDañados > 0
                                                   orderby X.IdPale
                                                   select new SelectSacosDañados
                                                   {
                                                       IdPale = X.IdPale,
                                                       SacosDañados = X.SacosDañados.Value
                                                   }).ToList();

            return selectList;
        }

    }
}
