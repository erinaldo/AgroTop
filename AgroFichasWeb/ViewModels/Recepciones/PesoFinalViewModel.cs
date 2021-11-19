using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class PesoFinalViewModel
    {        
        public ProcesoIngreso ProcesoIngreso { get; set; }
        public List<Bodega> Bodegas { get; set; }

        public int IdProcesoIngreso { get; set; }
        public int PesoFinal { get; set; }
        public int IdBodega { get; set; }
        public bool Secador { get; set; }
        public string ObservacionesPesoFinal { get; set; }
        public int Manga { get; set; }

        public string Pin { get; set; }
        private SYS_User PinUser = null;

        public PesoFinalViewModel()
        {

        }

        public PesoFinalViewModel(AgroFichasDBDataContext dc, int idProcesoIngreso)
        {
            this.IdProcesoIngreso = idProcesoIngreso;
            LoadLookups(dc);

            this.IdBodega = this.ProcesoIngreso.IdBodega ?? 0;
            this.Secador = this.ProcesoIngreso.Secador ?? false;
            this.PesoFinal = this.ProcesoIngreso.PesoFinal ?? 0;
            this.ObservacionesPesoFinal = this.ProcesoIngreso.ObservacionesPesoFinal ?? "";
            this.Manga = this.ProcesoIngreso.Manga ?? 0;
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.ProcesoIngreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == this.IdProcesoIngreso);

            if (this.ProcesoIngreso.IdEstado != 6 && !this.ProcesoIngreso.EsPesoFinalEditable())
                throw new Exception("No es posible editar este peso final. Su estado no lo permite: " + this.ProcesoIngreso.EstadoProcesoIngreso.Nombre);

            this.Bodegas = this.Bodegas = this.ProcesoIngreso.Sucursal.Bodega.Where(b => b.IdSucursal == this.ProcesoIngreso.IdSucursal && (b.Habilitada || b.IdBodega == this.ProcesoIngreso.IdBodega)).ToList();
        }

        public void Validate(AgroFichasDBDataContext dc, ModelStateDictionary modelState)
        {
            if (this.PesoFinal <= 0 || this.PesoFinal > 100000)
                modelState.AddModelError("PesoFinal", "El Peso Final no es válido");

            if (this.IdBodega <= 0)
                modelState.AddModelError("IdBodega", "La Bodega es requerida");

            if (dc.Bodega.Single(b => b.IdBodega == this.IdBodega).EsManga && this.Manga <= 0)
                modelState.AddModelError("Manga", "El Número de la Manga es requerido");

            this.PinUser = SYS_User.UserFromPin(this.Pin, new int[] { 32, 61 });
            if (PinUser == null)
                modelState.AddModelError("Pin", "El PIN no es válido");
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string ipAddress)
        {
            this.ProcesoIngreso.PesoFinal = this.PesoFinal;
            this.ProcesoIngreso.IdBodega = this.IdBodega;
            this.ProcesoIngreso.Secador = this.Secador;
            this.ProcesoIngreso.ObservacionesPesoFinal  = this.ObservacionesPesoFinal ?? "";

            if (this.Bodegas.Single(b => b.IdBodega == this.IdBodega).EsManga)
                this.ProcesoIngreso.Manga = this.Manga;
            else
                this.ProcesoIngreso.Manga = null;
            
            //Siempre es necesario recalcular bruto y normal, en flujo normal o editando
            this.ProcesoIngreso.PesoBruto = Math.Abs(this.ProcesoIngreso.PesoInicial.Value - this.ProcesoIngreso.PesoFinal.Value);
            this.ProcesoIngreso.PesoNormal = this.ProcesoIngreso.CalcularPesoNormal(dc);

            //Fecha hora no es editable para mantener control de tiempos
            if (!this.ProcesoIngreso.FechaPesoFinal.HasValue)
                this.ProcesoIngreso.FechaPesoFinal = DateTime.Now;

            this.ProcesoIngreso.UserPesoFinal = this.PinUser.UserName;
            this.ProcesoIngreso.IpPesoFinal = ipAddress;
            this.ProcesoIngreso.UserUpd = this.PinUser.UserName;
            this.ProcesoIngreso.FechaHoraUpd = DateTime.Now;
            this.ProcesoIngreso.IpUpd = ipAddress;

            dc.SubmitChanges();

            return this.ProcesoIngreso;
        }

    }
}