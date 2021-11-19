using AgrotopApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using MoreLinq;

namespace AgrotopApi.Controllers.Fichas
{
    //[Authorize]
    public class AgricultoresController : ApiController
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        #endregion

        #region 3. Funciones


        public List<SelectRegion> GetRegion()
        {
            List<SelectRegion> list = new List<SelectRegion>();

            List<Region> regionList = (from X in dcAgroFichas.Region
                                       orderby X.IdRegion
                                       select X).ToList();

            foreach (var region in regionList)
            {
                list.Add(new SelectRegion()
                {
                    IdRegion = region.IdRegion,
                    Nombre = region.Nombre
                });
            }

            return list;
        }

        public List<SelectProvincia> GetProvincia(int id)
        {
            List<SelectProvincia> list = new List<SelectProvincia>();

            List<Provincia> provinciaList = (from X in dcAgroFichas.Provincia
                                             where X.IdRegion == id
                                             orderby X.IdRegion
                                             select X).ToList();

            foreach (var provincia in provinciaList)
            {
                list.Add(new SelectProvincia()
                {
                    IdProvincia = provincia.IdProvincia,
                    Nombre = provincia.Nombre
                });
            }

            return list;
        }

        public List<SelectComuna> GetComuna(int id)
        {
            List<SelectComuna> list = new List<SelectComuna>();

            List<Comuna> comunaList = (from X in dcAgroFichas.Comuna
                                             where X.IdProvincia == id
                                             orderby X.IdProvincia
                                             select X).ToList();

            foreach (var comuna in comunaList)
            {
                list.Add(new SelectComuna()
                {
                    IdComuna = comuna.IdComuna,
                    Nombre = comuna.Nombre
                });
            }

            return list;
        }
        #endregion

    }
}
