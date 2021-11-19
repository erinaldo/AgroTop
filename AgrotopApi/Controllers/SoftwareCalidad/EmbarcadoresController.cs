using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class EmbarcadoresController : ApiController
    {
        // Dato Útil:
        // A lo que la gente de COMEX le llama Embarcador corresponde al Exportador
        // por lo tanto el Exportador puede ser AT o I.C.I. que están referenciadas
        // con Empresa

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public List<SelectEmbarcador> Get()
        {
            List<SelectEmbarcador> list = new List<SelectEmbarcador>();
            List<CAL_Exportador> embarcadorList = dc.CAL_Exportador.ToList();

            foreach (var embarcador in embarcadorList)
            {
                list.Add(new SelectEmbarcador()
                {
                    IdEmbarcador  = embarcador.IdExportador,
                    Nombre        = embarcador.Nombre
                });
            }

            return list;
        }
    }
}
