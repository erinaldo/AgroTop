using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class MuestraViewModel
    {
        public ProcesoIngreso ProcesoIngreso { get; set; }

        public int IdProcesoIngreso { get; set; }

        public string Pin { get; set; }
        private SYS_User PinUser = null;

        public MuestraViewModel()
        {

        }

        public MuestraViewModel(AgroFichasDBDataContext dc, int idProcesoIngreso)
        {
            this.IdProcesoIngreso = idProcesoIngreso;
            
            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.ProcesoIngreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == this.IdProcesoIngreso && pi.IdEstado == 1);
        }

        public void Validate(ModelStateDictionary modelState)
        {
            this.PinUser = SYS_User.UserFromPin(this.Pin, 26);
            if (PinUser == null)
                modelState.AddModelError("Pin", "El PIN no es válido");
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string ipAddress)
        {
            
            this.ProcesoIngreso.FechaHoraTomaMuestra = DateTime.Now;
            this.ProcesoIngreso.UserTomaMuestra = this.PinUser.UserName;
            this.ProcesoIngreso.IpTomaMuestra = ipAddress;
            this.ProcesoIngreso.UserUpd = this.PinUser.UserName;
            this.ProcesoIngreso.FechaHoraUpd = DateTime.Now;
            this.ProcesoIngreso.IpUpd = ipAddress;

            dc.SubmitChanges();

            return this.ProcesoIngreso;
        }

    }
}