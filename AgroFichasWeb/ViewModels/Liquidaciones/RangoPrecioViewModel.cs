using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class RangoPrecioViewModel
    {
        public int IdNivelRangoPrecio { get; set; }
        public NivelRangoPrecio NivelRangoPrecio;
        public List<ItemRangoPrecio> Items { get; set; }

        public List<Cultivo> Cultivos { get; set; }
        
        public Moneda CLP { get; set; }
        public Moneda USD { get; set; }

        public static RangoPrecioViewModel Create(AgroFichasDBDataContext dc, int idNivelRangoPrecio)
        {
            int index = 0;
            var model = new RangoPrecioViewModel()
            {
                IdNivelRangoPrecio = idNivelRangoPrecio,
                Items = (from r in dc.GetRangosPrecio(idNivelRangoPrecio)
                         select new ItemRangoPrecio
                         {
                             IdCultivo = r.IdCultivo,
                             IdSucursal = r.IdSucursal,
                             Index = ++index,
                             PrecioMinCLP = r.PrecioMinCLP,
                             PrecioMaxCLP = r.PrecioMaxCLP,
                             PrecioMinUSD = r.PrecioMinUSD,
                             PrecioMaxUSD = r.PrecioMaxUSD,
                             NombreSucursal = r.NombreSucursal,
                             NombreCultivo = r.NombreCultivo,
                             PisoCLP = r.PisoCLP,
                             TechoCLP = r.TechoCLP,
                             PisoUSD = r.PisoUSD,
                             TechoUSD = r.TechoUSD
                         }).ToList()
            };

            model.LoadLookups(dc);

            return model;
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.NivelRangoPrecio = dc.NivelRangoPrecio.Single(r => r.IdNivelRangoPrecio == this.IdNivelRangoPrecio);

            var idCultivos = this.Items.Select(i => i.IdCultivo).Distinct();
            this.Cultivos = dc.Cultivo.Where(c => idCultivos.Contains(c.IdCultivo)).ToList();

            this.CLP = dc.Moneda.Single(m => m.IdMoneda == 1);
            this.USD = dc.Moneda.Single(m => m.IdMoneda == 2);
        }

        public void SetDefaults()
        { 
        }

        public void Validate(ModelStateDictionary modelState)
        {
            foreach (var item in this.Items)
            {
                var itemDescription = $"{item.NombreCultivo} > {item.NombreSucursal}";

                if (item.PrecioMinCLP.HasValue && item.PrecioMinCLP < 0) 
                    modelState.AddModelError($"Items[{item.Index}].PrecioMinCLP", $"{itemDescription} > CLP Mínimo no puede ser negativo.");

                if (item.PrecioMaxCLP.HasValue && item.PrecioMaxCLP < 0)
                    modelState.AddModelError($"Items[{item.Index}].PrecioMaxCLP", $"{itemDescription} > CLP Máximo no puede ser negativo.");

                if (item.PrecioMinCLP.HasValue && item.PrecioMaxCLP.HasValue && item.PrecioMinCLP > item.PrecioMaxCLP)
                    modelState.AddModelError($"Items[{item.Index}].PrecioMaxCLP", $"{itemDescription} > CLP Máximo no puede menor que CLP Mínimo.");

                if (item.PrecioMinUSD.HasValue && item.PrecioMinUSD < 0)
                    modelState.AddModelError($"Items[{item.Index}].PrecioMinUSD", $"{itemDescription} > USD Mínimo no puede ser negativo.");

                if (item.PrecioMaxUSD.HasValue && item.PrecioMaxUSD < 0)
                    modelState.AddModelError($"Items[{item.Index}].PrecioMaxUSD", $"{itemDescription} > USD Máximo no puede ser negativo.");

                if (item.PrecioMinUSD.HasValue && item.PrecioMaxUSD.HasValue && item.PrecioMinUSD > item.PrecioMaxUSD)
                    modelState.AddModelError($"Items[{item.Index}].PrecioMaxUSD", $"{itemDescription} > USD Máximo no puede menor que USD Mínimo.");

                
                if (!IsInRange(item.PrecioMinCLP, item.PisoCLP, item.TechoCLP))
                    modelState.AddModelError($"Items[{item.Index}].PrecioMinCLP", $"{itemDescription} > CLP Mínimo está fuera de rango.");

                if (!IsInRange(item.PrecioMaxCLP, item.PisoCLP, item.TechoCLP))
                    modelState.AddModelError($"Items[{item.Index}].PrecioMaxCLP", $"{itemDescription} > CLP Máximo está fuera de rango.");

                if (!IsInRange(item.PrecioMinUSD, item.PisoUSD, item.TechoUSD))
                    modelState.AddModelError($"Items[{item.Index}].PrecioMinUSD", $"{itemDescription} > USD Mínimo está fuera de rango.");

                if (!IsInRange(item.PrecioMaxUSD, item.PisoUSD, item.TechoUSD))
                    modelState.AddModelError($"Items[{item.Index}].PrecioMaxUSD", $"{itemDescription} > USD Máximo está fuera de rango.");
            }
        }

        private bool IsInRange(decimal? value, decimal? piso, decimal? techo)
        {
            if (value.HasValue)
            {
                if (piso.HasValue && value < piso)
                    return false;

                if (techo.HasValue && value > techo)
                    return false;
            }
            return true;
        }

        public void Persist(ControllerContext ctx, AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            foreach (var item in this.Items)
            {
                var rangoCLP = dc.RangoPrecio.SingleOrDefault(r => r.IdMoneda == 1 && r.IdCultivo == item.IdCultivo && r.IdNivelRangoPrecio == this.IdNivelRangoPrecio && r.IdSucursal == item.IdSucursal);
                if (rangoCLP == null)
                {
                    rangoCLP = new RangoPrecio()
                    {
                        FechaHoraIns = DateTime.Now,
                        IdCultivo = item.IdCultivo,
                        IdMoneda = 1,
                        IdNivelRangoPrecio = this.IdNivelRangoPrecio,
                        IdSucursal = item.IdSucursal,
                        IpIns = ipAddress,
                        UserIns = userName,
                        PrecioMin = item.PrecioMinCLP,
                        PrecioMax = item.PrecioMaxCLP
                    };
                    dc.RangoPrecio.InsertOnSubmit(rangoCLP);
                }
                else
                {
                    if (rangoCLP.PrecioMin != item.PrecioMinCLP || rangoCLP.PrecioMax != item.PrecioMaxCLP)
                    {
                        rangoCLP.PrecioMin = item.PrecioMinCLP;
                        rangoCLP.PrecioMax = item.PrecioMaxCLP;
                        rangoCLP.UserUpd = userName;
                        rangoCLP.IpUpd = ipAddress;
                        rangoCLP.FechaHoraUpd = DateTime.Now;
                    }
                }

                var rangoUSD = dc.RangoPrecio.SingleOrDefault(r => r.IdMoneda == 2 && r.IdCultivo == item.IdCultivo && r.IdNivelRangoPrecio == this.IdNivelRangoPrecio && r.IdSucursal == item.IdSucursal);
                if (rangoUSD == null)
                {
                    rangoUSD = new RangoPrecio()
                    {
                        FechaHoraIns = DateTime.Now,
                        IdCultivo = item.IdCultivo,
                        IdMoneda = 2,
                        IdNivelRangoPrecio = this.IdNivelRangoPrecio,
                        IdSucursal = item.IdSucursal,
                        IpIns = ipAddress,
                        UserIns = userName,
                        PrecioMin = item.PrecioMinUSD,
                        PrecioMax = item.PrecioMaxUSD
                    };
                    dc.RangoPrecio.InsertOnSubmit(rangoUSD);
                }
                else
                {
                    if (rangoUSD.PrecioMin != item.PrecioMinUSD || rangoUSD.PrecioMax != item.PrecioMaxUSD)
                    {
                        rangoUSD.PrecioMin = item.PrecioMinUSD;
                        rangoUSD.PrecioMax = item.PrecioMaxUSD;
                        rangoUSD.UserUpd = userName;
                        rangoUSD.IpUpd = ipAddress;
                        rangoUSD.FechaHoraUpd = DateTime.Now;
                    }
                }
            }

            dc.SubmitChanges();
        }
    }

    public class ItemRangoPrecio
    {
        public int Index { get; set; }
        public int IdCultivo { get; set; }
        public int IdSucursal { get; set; }
        public string NombreSucursal { get; set; }
        public string NombreCultivo { get; set; }
        public decimal? PrecioMinCLP { get; set; }
        public decimal? PrecioMaxCLP { get; set; }
        public decimal? PrecioMinUSD { get; set; }
        public decimal? PrecioMaxUSD { get; set; }
        public decimal? PisoCLP { get; set; }
        public decimal? TechoCLP { get; set; }
        public decimal? PisoUSD { get; set; }
        public decimal? TechoUSD { get; set; }
    }
}