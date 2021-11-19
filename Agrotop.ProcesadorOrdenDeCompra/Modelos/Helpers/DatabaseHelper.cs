using Agrotop.PROCEA.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agrotop.PROCEA
{
    class DatabaseHelper
    {
        public static void ActualizarLiquidacion(OrdenCompra ODC, string code, AgrofichasDBDataContext dc, NLog.Logger logger)
        {
            switch (ODC.IdProyecto)
            {
                case 1:
                    {
                        var liquidacion = dc.Liquidacion.SingleOrDefault(X => X.IdLiquidacion == ODC.IdLiquidacion);
                        if (liquidacion != null)
                        {
                            liquidacion.OC = int.Parse(code);
                            liquidacion.FechaHoraUpd = DateTime.Now;
                            liquidacion.UserUpd = "ODC";
                            liquidacion.IpUpd = "192.168.11.92";
                            dc.SubmitChanges();
                        }
                    }

                    logger.Info("Liquidación actualizada");
                    break;
                case 2:
                    {
                        var liquidacion = dc.LOG_Liquidacion.SingleOrDefault(X => X.IdLiquidacion == ODC.IdLiquidacion);
                        if (liquidacion != null)
                        {
                            liquidacion.OC = int.Parse(code);
                            liquidacion.FechaHoraUpd = DateTime.Now;
                            liquidacion.UserUpd = "ODC";
                            liquidacion.IpUpd = "192.168.11.92";
                            dc.SubmitChanges();
                        }
                    }
                    break;
            }
        }

        public static void ActualizarODC(OrdenCompra ODC, string code, AgrofichasDBDataContext dc, NLog.Logger logger)
        {
            try
            {
                var ODCDB = dc.OC_OrdenCompra.Where(X => X.IdProyecto == ODC.IdProyecto && X.IdLiquidacion == ODC.IdLiquidacion).ToList();
                if (ODCDB.Count > 0)
                {
                    foreach (var glosa in ODCDB)
                    {
                        glosa.IdEstado = 2;
                        glosa.OC = int.Parse(code);
                        glosa.FechaHoraUpd = DateTime.Now;
                        glosa.UserUpd = "ODC";
                        glosa.IpUpd = "192.168.11.92";
                        dc.SubmitChanges();
                    }

                    ActualizarLiquidacion(ODC, code, dc, logger);

                    MailHelper.SendNotificacionOK(ODC, code, dc, logger);
                }
                else
                {
                    logger.Error("No existe la ODC");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR: " + ex.ToString());
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        public static Empresa GetEmpresa(int IdProyecto, int IdLiquidacion, AgrofichasDBDataContext dc)
        {
            if (IdProyecto == 1)
            {
                return null;
            }

            if (IdProyecto == 2)
            {
                return (from X in dc.LOG_Liquidacion
                        join Y in dc.LOG_Requerimiento on X.IdRequerimiento equals Y.IdRequerimiento
                        join Z in dc.Empresa on Y.IdEmpresa equals Z.IdEmpresa
                        where X.IdLiquidacion == IdLiquidacion
                        select Z).SingleOrDefault();
            }

            return null;
        }

        public static OC_Proyecto GetProyecto(int IdProyecto, AgrofichasDBDataContext dc)
        {
            return dc.OC_Proyecto.SingleOrDefault(X => X.IdProyecto == IdProyecto);
        }
    }
}
