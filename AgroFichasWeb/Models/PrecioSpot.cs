using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class PrecioSpot
    {
        public static vwPrecioSpot GetPrecioSpot(AgroFichasDBDataContext dc, DateTime fecha, int idCultivo, int idSucursal)
        {
            decimal? valorCLP = null;
            decimal? valorUSD = null;

            var pre = dc.vwPrecioSpot.Where(t => t.IdCultivo == idCultivo && t.Fecha <= fecha && t.IdSucursal == idSucursal).OrderByDescending(t => t.Fecha); //.FirstOrDefault();

            valorCLP = pre.FirstOrDefault(t => t.ValorCLP.HasValue)?.ValorCLP;
            valorUSD = pre.FirstOrDefault(t => t.ValorUSD.HasValue)?.ValorUSD;

            if (valorCLP == null || valorUSD == null)
            {
                var post = dc.vwPrecioSpot.Where(t => t.IdCultivo == idCultivo && t.Fecha > fecha && t.IdSucursal == idSucursal).OrderBy(t => t.Fecha);

                if (valorCLP == null)
                    valorCLP = post.FirstOrDefault(t => t.ValorCLP.HasValue)?.ValorCLP;

                if (valorUSD == null)
                    valorUSD = post.FirstOrDefault(t => t.ValorUSD.HasValue)?.ValorUSD;
            }

            return new vwPrecioSpot()
            {
                Fecha = fecha,
                IdCultivo = idCultivo,
                IdSucursal = idSucursal,
                Sucursal = dc.Sucursal.Single(s => s.IdSucursal == idSucursal)?.Nombre,
                ValorCLP = valorCLP,
                ValorUSD = valorUSD
            };
        }

        public static decimal GetPrecioSpotCLP(AgroFichasDBDataContext dc, DateTime fecha, int idCultivo, int idSucursal)
        {
            //Precio spot en pesos hacia atrás
            var pre = dc.vwPrecioSpot.Where(t => t.IdCultivo == idCultivo && t.IdSucursal == idSucursal && t.ValorCLP.HasValue).OrderByDescending(t => t.Fecha).FirstOrDefault();
            if (pre != null)
                return pre.ValorCLP.Value;

            //Precio spot en dólares * tasa cambio hacia atrás
            var preUSD = dc.vwPrecioSpot.Where(t => t.IdCultivo == idCultivo && t.IdSucursal == idSucursal && t.Fecha <= fecha && t.ValorUSD.HasValue).OrderByDescending(t => t.Fecha).FirstOrDefault();
            if (preUSD != null)
            {
                var tc = TasaCambio.GetValorTasaCambio(dc, fecha, 2);
                if (tc != 0)
                    return preUSD.ValorUSD.Value * tc;
            }

            //Precio spot en pesis hacia adelante
            var post = dc.vwPrecioSpot.Where(t => t.IdCultivo == idCultivo && t.IdSucursal == idSucursal && t.Fecha > fecha && t.ValorCLP.HasValue).OrderBy(t => t.Fecha).FirstOrDefault();
            if (post != null)
                return post.ValorCLP.Value;

            //Precio spot en dólares * tasa cambio hacia atrás
            var postUSD = dc.vwPrecioSpot.Where(t => t.IdCultivo == idCultivo && t.IdSucursal == idSucursal && t.Fecha > fecha && t.ValorUSD.HasValue).OrderBy(t => t.Fecha).FirstOrDefault();
            if (postUSD != null)
            {
                var tc = TasaCambio.GetValorTasaCambio(dc, fecha, 2);
                if (tc != 0)
                    return postUSD.ValorUSD.Value * tc;
            }


            return 0;
        }
    }
}