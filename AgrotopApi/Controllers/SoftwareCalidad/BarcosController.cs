using AgrotopApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class BarcosController : ApiController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public List<SelectBarco> Get(int id)
        {
            List<SelectBarco> list = new List<SelectBarco>();
            List<Barco> barcoList = dc.Barco.Where(X => X.IdCarrier == id && X.Habilitado == true).ToList();

            foreach (var barco in barcoList)
            {
                list.Add(new SelectBarco()
                {
                    IdBarco = barco.IdBarco,
                    Nombre  = barco.Nombre
                });
            }

            return list;
        }
    }
}
