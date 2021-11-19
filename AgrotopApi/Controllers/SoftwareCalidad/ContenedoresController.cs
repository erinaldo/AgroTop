using AgrotopApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class ContenedoresController : ApiController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public List<SelectContenedor> Get()
        {
            List<SelectContenedor> list = new List<SelectContenedor>();

            List<CAL_Contenedor> contenedorList = (from X in dc.CAL_Contenedor
                                                   where X.AptoParaAlimentos == true
                                                   select X).ToList();

            foreach (var contenedor in contenedorList)
            {
                list.Add(new SelectContenedor()
                {
                    IdContenedor = contenedor.IdContenedor,
                    Nombre = string.Format("{0} ft", contenedor.Tamaño)
                });
            }

            return list;
        }
    }
}
