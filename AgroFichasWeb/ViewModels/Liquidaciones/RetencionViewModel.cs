using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class RetencionViewModel
    {
        public Liquidacion Liquidacion { get; set; }

        public int IdLiquidacion { get; set; }
        public bool Retenida { get; set; }
        public string RetenidaMemo { get; set; }

        public RetencionViewModel()
        {

        }

        public RetencionViewModel(AgroFichasDBDataContext dc, int idLiquidacion)
        {
            this.IdLiquidacion = idLiquidacion;
            
            LoadLookups(dc);

            this.Retenida = this.Liquidacion.Retenida;
            this.RetenidaMemo = this.Liquidacion.RetenidaMemo ?? "";
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Liquidacion = dc.Liquidacion.Single(lq => lq.IdLiquidacion == this.IdLiquidacion);
        }

        public void Validate(ModelStateDictionary modelState)
        {
            if (this.Retenida && String.IsNullOrWhiteSpace(this.RetenidaMemo))
                modelState.AddModelError("RetenidaMemo", "Ingrese el motivo de la retención");
        }

        public Liquidacion Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {

            this.Liquidacion.Retenida = this.Retenida;
            this.Liquidacion.RetenidaMemo = this.RetenidaMemo;
            this.Liquidacion.FechaHoraRetenida= DateTime.Now;
            this.Liquidacion.UserRetenida = userName;
            this.Liquidacion.IpDetenida = ipAddress;

            this.Liquidacion.UserUpd = userName;
            this.Liquidacion.FechaHoraUpd = DateTime.Now;
            this.Liquidacion.IpUpd = ipAddress;

            dc.SubmitChanges();

            return this.Liquidacion;
        }
    }
}