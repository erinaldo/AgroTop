using AgrotopApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class SacosController : ApiController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public List<SelectSaco> Get()
        {
            List<SelectSaco> list = new List<SelectSaco>();

            List<CAL_Saco> sacoList = (from X in dc.CAL_Saco
                                       where X.Habilitado == true
                                       orderby X.Nombre
                                       select X).ToList();

            foreach (var saco in sacoList)
            {
                list.Add(new SelectSaco()
                {
                    IdSaco = saco.IdSaco,
                    Nombre = saco.Nombre
                });
            }

            return list;
        }

        public List<SelectPesoSaco> GetPesoSaco(int IdSaco)
        {
            List<SelectPesoSaco> list = new List<SelectPesoSaco>();

            List<CAL_PesoSaco> pesoSacoList = (from S1 in dc.CAL_Saco
                                               join T1 in dc.CAL_TipoSaco on S1.IdTipoSaco equals T1.IdTipoSaco
                                               join P1 in dc.CAL_PesoTipoSaco on T1.IdTipoSaco equals P1.IdTipoSaco
                                               join P2 in dc.CAL_PesoSaco on P1.IdPesoSaco equals P2.IdPesoSaco
                                               where S1.IdSaco == IdSaco
                                               && S1.Habilitado == true
                                               && T1.Habilitado == true
                                               && P2.Habilitado == true
                                               orderby P2.Peso
                                               select P2).ToList();

            foreach (var pesoSaco in pesoSacoList)
            {
                list.Add(new SelectPesoSaco()
                {
                    IdPesoSaco = pesoSaco.IdPesoSaco,
                    Peso = pesoSaco.Peso
                });
            }

            return list;
        }

        public string GetTipoColorHiloSaco(int IdSaco)
        {
            CAL_TipoColorHiloSaco cAL_TipoColorHiloSaco = (from S1 in dc.CAL_Saco
                                                           join T1 in dc.CAL_TipoColorHiloSaco on S1.IdTipoColorHiloSaco equals T1.IdTipoColorHiloSaco
                                                           where S1.IdSaco == IdSaco
                                                           && S1.Habilitado == true
                                                           && T1.Habilitado == true
                                                           select T1).SingleOrDefault();
            if (cAL_TipoColorHiloSaco != null)
                return cAL_TipoColorHiloSaco.Nombre;
            else
                return string.Empty;
        }
    }
}
