using AgrotopApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AgrotopApi.Controllers
{
    [Authorize]
    public class PlanificacionSemanalController : ApiController
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion

        #region 3. Funciones

        #endregion

        public List<SelectPlanificacionSemanal> GetPlanificacion(int ano , int semana)
        {
            List<SelectPlanificacionSemanal> psList = new List<SelectPlanificacionSemanal>();
            List<CTR_PlanificacionSemanal> pss = (from X in dcAgroFichas.CTR_PlanificacionSemanal
                                                  where X.Año == ano 
                                                  && X.Semana == semana
                                                  && X.Habilitado == true
                                                  orderby X.Semana
                                                  select X).ToList();
            foreach (var ps in pss)
            {
                psList.Add(new SelectPlanificacionSemanal()
                {
                    IdPlanificacionSemanal            = ps.IdPlanificacionSemanal,
                    Año                               = ps.Año,
                    Semana                            = ps.Semana,
                    IdEmpresa                         = ps.IdEmpresa,
                    Empresa                           = ps.Empresa.Nombre,
                    IdCliente                         = ps.IdCliente,
                    IdClienteSAPAT                    = ps.Cliente.IDAvenatop,
                    IdClienteSAPOT                    = ps.Cliente.IDOleotop,
                    IdClienteSAPICI                   = ps.Cliente.IDICI,
                    Cliente                           = ps.Cliente.RazonSocial,
                    IdProducto                        = ps.IdProducto,
                    Producto                          = ps.CTR_Producto.Nombre,
                    Destino                           = ps.Destino,
                    OC                                = ps.OC,
                    LC                                = ps.LC,
                    Pais                              = ps.Pais.PaisNombre,
                    Lote                              = ps.Lote,
                    DUS                               = ps.DUS,
                    Reserva                           = ps.Reserva,
                    Lunes                             = ps.Lunes,
                    FechaLunes                        = string.Format("{0}", ps.FechaLunes),
                    Martes                            = ps.Martes,
                    FechaMartes                       = string.Format("{0}", ps.FechaMartes),
                    Miercoles                         = ps.Miercoles,
                    FechaMiercoles                    = string.Format("{0}",ps.FechaMiercoles),
                    Jueves                            = ps.Jueves,
                    FechaJueves                       = string.Format("{0}", ps.FechaJueves),
                    Viernes                           = ps.Viernes,
                    FechaViernes                      = string.Format("{0}", ps.FechaViernes),
                    Sabado                            = ps.Sabado,
                    FechaSabado                       = string.Format("{0}", ps.FechaSabado),
                    Domingo                           = ps.Domingo,
                    FechaDomingo                      = string.Format("{0}", ps.FechaDomingo)
                });
            }
            return psList;
        }
    }
}