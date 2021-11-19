using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class EspesoresController : ApiController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public List<SelectEspesor> GetEspesorProducto()
        {
            List<SelectEspesor> list = new List<SelectEspesor>();
            List<CAL_EspesorProducto> espesoresList = dc.CAL_EspesorProducto.Where(X => X.Habilitado == true).OrderBy(X => X.Min).ThenBy(X => X.Max).ToList();

            foreach (var espesor in espesoresList)
            {
                list.Add(new SelectEspesor()
                {
                    IdEspesor = espesor.IdEspesorProducto,
                    Min = espesor.Min,
                    Max = espesor.Max,
                    Avg = espesor.Avg,
                    Observaciones = espesor.Observaciones,
                });
            }

            return list;
        }
    }
}
