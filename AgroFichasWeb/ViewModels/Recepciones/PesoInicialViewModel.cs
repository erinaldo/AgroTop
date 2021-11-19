using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class PesoInicialViewModel
    {
        public ProcesoIngreso ProcesoIngreso { get; set; }
        public List<Bodega> Bodegas { get; set; }

        public int IdProcesoIngreso { get; set; }
        public int PesoInicial { get; set; }
        public int IdBodega { get; set; }
        public bool Secador { get; set; }
        public string ObservacionesPesoInicial { get; set; }

        public string Pin { get; set; }
        private SYS_User PinUser = null;

        public PesoInicialViewModel()
        {

        }

        public PesoInicialViewModel(AgroFichasDBDataContext dc, int idProcesoIngreso)
        {
            this.IdProcesoIngreso = idProcesoIngreso;
            LoadLookups(dc);

            this.IdBodega = this.ProcesoIngreso.IdBodega ?? 0;
            this.Secador = this.ProcesoIngreso.Secador ?? false;
            this.PesoInicial = this.ProcesoIngreso.PesoInicial ?? 0;
            this.ObservacionesPesoInicial = this.ProcesoIngreso.ObservacionesPesoInicial ?? "";
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.ProcesoIngreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == this.IdProcesoIngreso);

            if (this.ProcesoIngreso.IdEstado != 4 && !this.ProcesoIngreso.EsPesoInicialEditable())
                throw new Exception("No es posible editar este peso inicial. Su estado no lo permite: " + this.ProcesoIngreso.EstadoProcesoIngreso.Nombre);

            this.Bodegas = this.Bodegas = this.ProcesoIngreso.Sucursal.Bodega.Where(b => b.IdSucursal == this.ProcesoIngreso.IdSucursal && (b.Habilitada || b.IdBodega == this.ProcesoIngreso.IdBodega)).ToList();
        }

        public void Validate(ModelStateDictionary modelState)
        {
            if (this.PesoInicial <= 0 || this.PesoInicial > 100000)
                modelState.AddModelError("PesoInicial", "El Peso Inicial no es válido");
            if (this.IdBodega <= 0)
                modelState.AddModelError("IdBodega", "La Bodega es requerida");

            this.PinUser = SYS_User.UserFromPin(this.Pin, new int[] { 31, 61 });
            if (PinUser == null)
                modelState.AddModelError("Pin", "El PIN no es válido");
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string ipAddress)
        {
            this.ProcesoIngreso.PesoInicial = this.PesoInicial;
            this.ProcesoIngreso.IdBodega = this.IdBodega;
            this.ProcesoIngreso.Secador = this.Secador;
            this.ProcesoIngreso.ObservacionesPesoInicial = this.ObservacionesPesoInicial ?? "";
            
            //Peso Bruto y normal debe actualizarse si es necesario (fuera del flujo normal, es decir editando)
            //4:Autorizado Lab es flujo normal
            //6:Peso inicial estamos editando pero no tiene peso bruto aún
            if (this.ProcesoIngreso.IdEstado == 7) ///7: Segundo pesaje, es necesario recalcular 
            {
                this.ProcesoIngreso.PesoBruto = Math.Abs(this.ProcesoIngreso.PesoInicial.Value - this.ProcesoIngreso.PesoFinal.Value);
                this.ProcesoIngreso.PesoNormal = this.ProcesoIngreso.CalcularPesoNormal(dc);
            }

            //Fecha hora no es editable para mantener control de tiempos
            if (!this.ProcesoIngreso.FechaPesoInicial.HasValue)
                this.ProcesoIngreso.FechaPesoInicial = DateTime.Now;

            this.ProcesoIngreso.UserPesoInicial = this.PinUser.UserName;
            this.ProcesoIngreso.IpPesoInicial = ipAddress;
            this.ProcesoIngreso.UserUpd = this.PinUser.UserName;
            this.ProcesoIngreso.FechaHoraUpd = DateTime.Now;
            this.ProcesoIngreso.IpUpd = ipAddress;

            dc.SubmitChanges();

            return this.ProcesoIngreso;
        }
    }
}