using AgrotopApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class PosicionContenedorController : ApiController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public List<SelectBandera> GetBandera(int IdContenedor)
        {
            List<SelectBandera> list = new List<SelectBandera>();

            List<CAL_PosicionContenedorYalero> posicionList = (from S1 in dc.CAL_PosicionContenedor
                                                               join T1 in dc.CAL_PosicionContenedorYalero on S1.IdPosicion equals T1.IdPosicion
                                                               where T1.IdContenedor == IdContenedor
                                                               orderby T1.IdContenedor
                                                               select T1).ToList();
            foreach (var posicion in posicionList)
            {
                list.Add(new SelectBandera()
                {
                    IdPosicion = posicion.IdPosicion,
                    IdContenedor = posicion.IdContenedor,
                    Descripcion = posicion.CAL_PosicionContenedor.Descripcion
                });
            }

            return list;
        }
    }
}
