using AgroFichasWeb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class AutorizarPreciosViewModel
    {
        public ConvenioPrecioAutorizacion Autorizacion { get; set; }
        public ConvenioPrecioViewModel ConvenioPrecioPorAutorizar { get; set; }
        public List<ItemTablaPrecio> TablaPrecios { get; set; }

        public bool Autorizado { get; set; }
        public int IdConvenioPrecioAutorizacion { get; set; }

        public AutorizarPreciosViewModel()
        {

        }

        public AutorizarPreciosViewModel(AgroFichasDBDataContext dc, int idConvenioPrecioAutorizacion)
        {
            this.IdConvenioPrecioAutorizacion = idConvenioPrecioAutorizacion;

            LoadLookups(dc);
            
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Autorizacion = dc.ConvenioPrecioAutorizacion.Single(lq => lq.IdConvenioPrecioAutorizacion == this.IdConvenioPrecioAutorizacion);
            this.ConvenioPrecioPorAutorizar = JsonConvert.DeserializeObject<ConvenioPrecioViewModel>(this.Autorizacion.Data);
            this.TablaPrecios =  JsonConvert.DeserializeObject<List<ItemTablaPrecio>>(this.Autorizacion.TablaPrecios);

            this.ConvenioPrecioPorAutorizar.LoadLookups(dc);
        }

        public void Validate(ModelStateDictionary modelState)
        {
            
        }

        public ConvenioPrecioAutorizacion Persist(ControllerContext ctx, AgroFichasDBDataContext dc, string userName, string ipAddress, out bool requiereNotificacion)
        {
            requiereNotificacion = false;

            this.Autorizacion.Autorizada = this.Autorizado;
            this.Autorizacion.FechaHoraAut = DateTime.Now;
            this.Autorizacion.UserAut = userName;
            this.Autorizacion.IpAut = ipAddress;

            if (this.Autorizado)
            {
                var convenio = this.ConvenioPrecioPorAutorizar.Persist(ctx, dc, userName, ipAddress, out bool notificar);
                this.Autorizacion.IdConvenioPrecio = convenio.IdConvenioPrecio;

                requiereNotificacion = notificar;
            }

            dc.SubmitChanges();

            return this.Autorizacion;
        }
    }

    public class ItemTablaPrecio
    {
        public string NombreSucursal { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnidad { get; set;}

        public bool? Autorizado { get; set; }
        public string Autorizador { get; set; }

    }
}