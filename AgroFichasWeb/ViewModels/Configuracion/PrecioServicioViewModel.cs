using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Configuracion
{
    public class PrecioServicioViewModel
    {
        public Cultivo Cultivo { get; set; }
        public bool IsNew { get; set; }

        public int IdCultivo { get; set; }
        public DateTime Fecha { get; set; }

        public List<PrecioServicioSucursalViewModel> Precios { get; set; }

        public PrecioServicioViewModel()
        {

        }


        public PrecioServicioViewModel(AgroFichasDBDataContext dc, int idCultivo, DateTime fecha, bool isNew)
        {
            this.IdCultivo = idCultivo;
            this.Fecha = fecha;
            this.IsNew = isNew;

            int index = 0;
            if (isNew)
            {
                this.Precios = (from s in dc.Sucursal.ToList() //ToList forces the query to run this part of the query to run in the db. The rest runs in the client
                               where s.Habilitada
                               orderby s.Nombre
                               select new PrecioServicioSucursalViewModel
                               {
                                   Index = ++index,
                                   IdSucursal = s.IdSucursal,
                                   Nombre = s.Nombre,
                                   ValorCLP = null,
                                   //ValorUSD = null
                               }).ToList();
            }
            else
            {
                //Run the query in the db first (so the index++ works)
                var items = (from s in dc.PrecioServicio
                             where s.IdCultivo == this.IdCultivo
                                && s.Fecha == this.Fecha
                             select s).ToList();

                this.Precios =  (from s in items
                                select new PrecioServicioSucursalViewModel()
                                {
                                    Index = ++index,
                                    IdSucursal = s.IdSucursal,
                                    //Nombre = s.Sucursal,
                                    //ValorCLP = s.ValorCLP,
                                    //ValorUSD = s.ValorUSD
                                }).ToList();
            }

            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Cultivo = dc.Cultivo.Single(c => c.IdCultivo == this.IdCultivo);
        }

        public void Validate(AgroFichasDBDataContext dc, ModelStateDictionary modelState)
        {
            if (this.IsNew)
            {
                //var existente = dc.vwPrecioServicio.SingleOrDefault(ps => ps.IdCultivo == this.IdCultivo && ps.Fecha == this.Fecha);
                //if (existente != null)
                //    modelState.AddModelError("Fecha", "Ya existe un precio spot para esta fecha y cultivo");
            }

            foreach (var sucursal in this.Precios)
            {

                if (sucursal.ValorCLP.HasValue && sucursal.ValorCLP < 0)
                    modelState.AddModelError("ValorCLP", $"El Valor en CLP de la sucursal {sucursal.Nombre} no es válido");

                //if (sucursal.ValorUSD.HasValue && sucursal.ValorUSD < 0)
                //    modelState.AddModelError("ValorUSD", $"El Valor en USD de la sucursal {sucursal.Nombre} no es válido");

                if (sucursal.ValorCLP.HasValue)
                {
                    var moneda = dc.Moneda.Single(m => m.IdMoneda == 1);
                    if (!moneda.ValidarDecimalesPrecio(sucursal.ValorCLP))
                        modelState.AddModelError("ValorCLP", $"El valor en CLP de la sucursal {sucursal.Nombre} debe tener a lo más {moneda.DecimalesPrecio} decimal(es)");
                }

                //if (sucursal.ValorUSD.HasValue)
                //{
                //    var moneda = dc.Moneda.Single(m => m.IdMoneda == 2);
                //    if (!moneda.ValidarDecimalesPrecio(sucursal.ValorUSD))
                //        modelState.AddModelError("ValorUSD", $"El valor en USD de la sucursal {sucursal.Nombre} debe tener a lo más {moneda.DecimalesPrecio} decimal(es)");
                //}
            }
        }

        public void Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            foreach (var precio in this.Precios)
            {
                var precioCLP = dc.PrecioServicio.SingleOrDefault(ps => ps.IdCultivo == this.IdCultivo/* && ps.IdMoneda == 1*/ && ps.Fecha == this.Fecha && ps.IdSucursal == precio.IdSucursal);
                if (precioCLP == null)
                {
                    precioCLP = new PrecioServicio()
                    {
                        IdSucursal = precio.IdSucursal,
                        IdCultivo = this.IdCultivo,
                        UserIns = userName,
                        FechaHoraIns = DateTime.Now,
                        IpIns = ipAddress
                    };
                    dc.PrecioServicio.InsertOnSubmit(precioCLP);
                }
                else
                {
                    precioCLP.UserUpd = userName;
                    precioCLP.FechaHoraUpd = DateTime.Now;
                    precioCLP.IpUpd = ipAddress;
                }
                precioCLP.Fecha = this.Fecha;
                precioCLP.Valor = precio.ValorCLP.HasValue ? precio.ValorCLP.Value : 0M;


                //var precioUSD = dc.PrecioSpot.SingleOrDefault(ps => ps.IdCultivo == this.IdCultivo && ps.IdMoneda == 2 && ps.Fecha == this.Fecha && ps.IdSucursal == precio.IdSucursal);
                //if (precioUSD == null)
                //{
                //    precioUSD = new PrecioSpot()
                //    {
                //        IdSucursal = precio.IdSucursal,
                //        IdCultivo = this.IdCultivo,
                //        IdMoneda = 2,
                //        UserIns = userName,
                //        FechaHoraIns = DateTime.Now,
                //        IpIns = ipAddress
                //    };
                //    dc.PrecioSpot.InsertOnSubmit(precioUSD);
                //}
                //else
                //{
                //    precioUSD.UserUpd = userName;
                //    precioUSD.FechaHoraUpd = DateTime.Now;
                //    precioUSD.IpUpd = ipAddress;
                //}

                //precioUSD.Fecha = this.Fecha;
                //precioUSD.Valor = precio.ValorUSD;
            }

            dc.SubmitChanges();
        }
    }

    public class PrecioServicioSucursalViewModel
    { 
        public int Index { get; set; }
        public int IdSucursal { get; set; }
        public string Nombre { get; set; }
        public decimal? ValorCLP { get; set; }
    }
}