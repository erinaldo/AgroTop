using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class AutorizarDescuentosViewModel
    {
        public Liquidacion Liquidacion { get; set; }

        public bool Autorizado { get; set; }
        public int IdLiquidacion { get; set; }
        public string Observaciones { get; set; }
        
        public AutorizarDescuentosViewModel()
        {

        }

        public AutorizarDescuentosViewModel(AgroFichasDBDataContext dc, int idLiquidacion)
        {
            this.IdLiquidacion = idLiquidacion;
            LoadLookups(dc);
            this.Observaciones = this.Liquidacion.ObservacionesAutDes;
            if (string.IsNullOrEmpty(this.Observaciones))
                this.Observaciones = this.Liquidacion.ObservacionesDescuentos;
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Liquidacion = dc.Liquidacion.Single(lq => lq.IdLiquidacion == this.IdLiquidacion);
        }

        public void Validate(ModelStateDictionary modelState)
        {
            
        }

        public Liquidacion Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            this.Liquidacion.ObservacionesAutDes = this.Observaciones ?? "";
            this.Liquidacion.AutorizadaDescuentos = this.Autorizado;
            this.Liquidacion.FechaHoraAutDes = DateTime.Now;
            this.Liquidacion.UserAutDes = userName;
            this.Liquidacion.IpAutDes = ipAddress;

            this.Liquidacion.UserUpd = userName;
            this.Liquidacion.FechaHoraUpd = DateTime.Now;
            this.Liquidacion.IpUpd = ipAddress;

            dc.SubmitChanges();

            return this.Liquidacion;
        }
    }
}