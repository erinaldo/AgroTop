using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class LaboratorioViewModel
    {        
        public ProcesoIngreso ProcesoIngreso { get; set; }

        public int IdProcesoIngreso { get; set; }
        public string Pin { get; set; }
        private SYS_User PinUser = null;

        public LaboratorioViewModel()
        {

        }

        public LaboratorioViewModel(AgroFichasDBDataContext dc, int idProcesoIngreso)
        {
            this.IdProcesoIngreso = idProcesoIngreso;
            
            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.ProcesoIngreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == this.IdProcesoIngreso && pi.IdEstado == 2);
        }

        public void Validate(ModelStateDictionary modelState)
        {
            this.PinUser = SYS_User.UserFromPin(this.Pin, 27);
            if (PinUser == null)
                modelState.AddModelError("Pin", "El PIN no es válido");
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc,string ipAddress)
        {
            
            this.ProcesoIngreso.FechaHoraLaboratorio = DateTime.Now;
            this.ProcesoIngreso.UserLaboratorio = this.PinUser.UserName;
            this.ProcesoIngreso.IpLaboratorio = ipAddress;
            this.ProcesoIngreso.UserUpd = this.PinUser.UserName;
            this.ProcesoIngreso.FechaHoraUpd = DateTime.Now;
            this.ProcesoIngreso.IpUpd = ipAddress;

            dc.SubmitChanges();

            return this.ProcesoIngreso;
        }

    }
}