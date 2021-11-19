using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class PagarLiquidacionesViewModel
    {
        public Temporada Temporada { get; set; }
        public Empresa Empresa { get; set; }
        public int IdTemporada { get; set; }
        public int IdEmpresa { get; set; }
        public List<PagarLiquidacionesItem> Items { get; set; }

        public PagarLiquidacionesViewModel()
        {
        }

        public PagarLiquidacionesViewModel(AgroFichasDBDataContext dc, int idTemporada, int idEmpresa, string key = "")
        {
            this.IdTemporada = idTemporada;
            this.IdEmpresa = idEmpresa;

            var estados = new int[] { 10 }; //Cerrado o Liquidado

            this.Items = (from liq in dc.Liquidacion
                          where liq.IdTemporada == idTemporada
                             && (IdEmpresa == 0 || liq.IdEmpresa == IdEmpresa)
                             && (liq.IdEstado == 5)
                             && (key == "" || liq.Agricultor.Nombre.Contains(key) || liq.Empresa.Nombre.Contains(key))
                          orderby liq.IdLiquidacion descending
                          select new PagarLiquidacionesItem()
                          {
                              Seleccionado = false,
                              IdLiquidacion = liq.IdLiquidacion,
                              Liquidacion = liq
                          }).ToList();

            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Temporada = dc.Temporada.Single(t => t.IdTemporada == this.IdTemporada);
            this.Empresa = dc.Empresa.SingleOrDefault(e => e.IdEmpresa == this.IdEmpresa);
        }

        public void Validate(AgroFichasDBDataContext dc, ModelStateDictionary modelState)
        {
            if (this.Items == null || this.Items.Where(i => i.Seleccionado).Count() == 0)
            {
                modelState.AddModelError("", "Seleccione al menos 1 liquidación a pagar");
            }
        }

        public void Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            foreach (var item in this.Items)
            {
                if (item.Seleccionado)
                {
                    var liq = dc.Liquidacion.Single(p => p.IdLiquidacion == item.IdLiquidacion);
                    liq.Pagada = true;
                    liq.FechaHoraPagada = DateTime.Now;
                    liq.UserPagada = userName;
                    liq.IpIns = ipAddress;
                }
            }

            dc.SubmitChanges();
        }
    }

    public class PagarLiquidacionesItem
    {
        public bool Seleccionado { get; set; }
        public int IdLiquidacion { get; set; }
        public Liquidacion Liquidacion { get; set; }
    }
}