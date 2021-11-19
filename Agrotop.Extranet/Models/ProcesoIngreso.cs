using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public partial class ProcesoIngreso
    {
        public static int ANALISIS_AUTORIZADO = 1;
        public static int ANALISIS_ENREVISION = 2;
        public static int ANALISIS_RECHAZADO = 0;


        public static List<ParametroAnalisis> GetParametrosAnalisisLiquidacion(int idCultivo)
        {
            var dc = new AgrotopDBDataContext();
            return GetParametrosAnalisisLiquidacion(dc, idCultivo);
        }

        public static List<ParametroAnalisis> GetParametrosAnalisisLiquidacion(AgrotopDBDataContext dc, int idCultivo)
        {
            return dc.ParametroAnalisis.Where(p => p.IdCultivo == idCultivo && p.MostrarEnLiquidacion == true).OrderBy(p => p.Orden).ToList();
        }


        public int[] IdsLiquidaciones()
        {
            return this.PrecioIngreso.Where(pi => pi.IdLiquidacion.HasValue).Select(pi => pi.IdLiquidacion.Value).Distinct().ToArray();
        }

        public List<Liquidacion> Liquidaciones()
        {
            var result = new List<Liquidacion>();
            foreach (var precio in this.PrecioIngreso.Where(pi => pi.IdLiquidacion.HasValue))
            {
                if (result.SingleOrDefault(r => r.IdLiquidacion == precio.IdLiquidacion) == null)
                    result.Add(precio.Liquidacion);
            }
            return result;
        }
    }
}