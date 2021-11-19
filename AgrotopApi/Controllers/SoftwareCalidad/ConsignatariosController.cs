using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class ConsignatariosController : ApiController
    {
        // Dato Útil (copiado de EmbarcadoresController):
        // A lo que la gente de COMEX le llama Embarcador corresponde al Exportador
        // por lo tanto el Exportador puede ser AT o I.C.I. que están referenciadas
        // con Empresa

        // Dato Útil:
        // A lo que la gente de COMEX le llama Consignatario eso es equivalente al Cliente
        // este ApiController está creado con los labels que se usarán en el formulario de
        // la creación de la Orden de Producción para evitar aún más confusiones.

        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public List<SelectConsignatario> Get(int id)
        {
            List<SelectConsignatario> list = new List<SelectConsignatario>();
            CAL_Exportador embarcador = dcSoftwareCalidad.CAL_Exportador.SingleOrDefault(X => X.IdExportador == id);
            List<Cliente> consignatarioList = (from X in dcAgroFichas.Cliente
                                               join Y in dcAgroFichas.ClienteEmpresa on X.IdCliente equals Y.IdCliente
                                               where Y.IdEmpresa == embarcador.IdEmpresa
                                               orderby X.RazonSocial
                                               select X).ToList();

            foreach (var consignatario in consignatarioList)
            {
                list.Add(new SelectConsignatario()
                {
                    IdConsignatario = consignatario.IdCliente,
                    Nombre          = consignatario.RazonSocial
                });
            }

            return list;
        }
    }
}
