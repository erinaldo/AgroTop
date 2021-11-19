
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class AnalisisViewModel
    {
        public ProcesoIngreso ProcesoIngreso { get; set; }
        public List<Bodega> Bodegas { get; set; }
        public List<Variedad> Variedades { get; set; }

        public int IdProcesoIngreso { get; set; }
        public int IdBodega { get; set; }
        public bool Secador { get; set; }
        public int IdVariedad { get; set; }

        public List<ValorAnalisisViewModel> ValoresAnalisis { get; set; }
        public string ObservacionesAnalisis { get; set; }

        public string Pin { get; set; }
        private SYS_User PinUser = null;

        public AnalisisViewModel()
        {

        }

        public AnalisisViewModel(AgroFichasDBDataContext dc, int idProcesoIngreso)
        {
            this.IdProcesoIngreso = idProcesoIngreso;
            
            LoadLookups(dc);

            this.IdBodega = this.ProcesoIngreso.IdBodega ?? 0;
            this.IdVariedad = this.ProcesoIngreso.IdVariedad ?? 0;
            this.ObservacionesAnalisis = this.ProcesoIngreso.ObservacionesAnalisis ?? "";
            this.Secador = this.ProcesoIngreso.Secador ?? false;
            this.ProcesoIngreso.SetUpValoresAnalisis(dc);
            this.ValoresAnalisis = (from va in this.ProcesoIngreso.ValorAnalisis
                                    orderby va.ParametroAnalisis.Orden
                                    select new ValorAnalisisViewModel() { 
                                        IdParametroAnalisis = va.IdParametroAnalisis, 
                                        Valor = va.Valor,
                                        Nombre = va.ParametroAnalisis.Nombre,
                                        UM = va.ParametroAnalisis.UM,
                                        Orden = va.ParametroAnalisis.Orden,
                                        Requerido = va.ParametroAnalisis.Requerido,
                                        IdSucursal = this.ProcesoIngreso.IdSucursal
                                    }).ToList();

        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.ProcesoIngreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == this.IdProcesoIngreso);
            
            if (this.ProcesoIngreso.IdEstado != 3 && !this.ProcesoIngreso.EsAnalisisEditable())
                throw new Exception("No es posible editar este análisis. Su estado no lo permite: " + this.ProcesoIngreso.EstadoProcesoIngreso.Nombre);

            this.Bodegas = this.ProcesoIngreso.Sucursal.Bodega.Where(b => b.IdSucursal == this.ProcesoIngreso.IdSucursal && (b.Habilitada || b.IdBodega == this.ProcesoIngreso.IdBodega)).ToList();
            this.Variedades = this.ProcesoIngreso.CultivoContrato.Cultivo.Variedad.OrderBy(v => v.Nombre).ToList();
        }

        public void Validate(AgroFichasDBDataContext dc, ModelStateDictionary modelState)
        {
            if (this.IdBodega <= 0)
                modelState.AddModelError("IdBodega", "La Bodega es requerida");
            if (this.IdVariedad <= 0)
                modelState.AddModelError("IdVariedad", "La Variedad es requerida");

            int i = 0;
            if (this.ValoresAnalisis != null)
            {
                foreach (var valor in this.ValoresAnalisis)
                {
                    valor.Validate(dc, modelState, String.Format("ValoresAnalisis[{0}].Valor", i));
                    i++;
                }

                //Avena: Si Peso 1000 granos < 35 => Granos bajo 2.1mm es requerido
                var peso100granos = this.ValoresAnalisis.SingleOrDefault(v => v.IdParametroAnalisis == 8);
                if (peso100granos != null)
                {
                    var parametro1000 = dc.ParametroAnalisis.Single(pa => pa.IdParametroAnalisis == peso100granos.IdParametroAnalisis);
                    if (peso100granos.Valor.HasValue && parametro1000.MinAutValue.HasValue && peso100granos.Valor.Value < parametro1000.MinAutValue.Value)
                    {
                        var granos21 = this.ValoresAnalisis.SingleOrDefault(v => v.IdParametroAnalisis == 13);
                        if (granos21 == null || granos21.Valor == null)
                        {
                            var parametro21 = dc.ParametroAnalisis.Single(pa => pa.IdParametroAnalisis == 13);
                            modelState.AddModelError(String.Format("ValoresAnalisis[{0}].Valor", i), String.Format("{0} es requerido si el valor de {1} es menor que {2}", parametro21.Nombre, parametro1000.Nombre, parametro1000.MinAutValue.Value));
                        }
                    }
                }
            }

            this.PinUser = SYS_User.UserFromPin(this.Pin, new int[] { 28, 60 });
           
            if (PinUser == null)
                modelState.AddModelError("Pin", "El PIN no es válido");
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string ipAddress, out bool notificarAlertas)
        {
            notificarAlertas = false;

            this.ProcesoIngreso.IdBodega = this.IdBodega;
            this.ProcesoIngreso.ObservacionesAnalisis = this.ObservacionesAnalisis ?? "";
            this.ProcesoIngreso.Secador = this.Secador;
            this.ProcesoIngreso.IdVariedad = this.IdVariedad;

            if (this.ValoresAnalisis != null)
            {
                foreach (var valor in this.ValoresAnalisis)
                {
                    var valorAnalisis = this.ProcesoIngreso.ValorAnalisis.Single(va => va.IdParametroAnalisis == valor.IdParametroAnalisis);

                    valorAnalisis.Valor = valor.Valor;
                    valorAnalisis.UserUpd = this.PinUser.UserName;
                    valorAnalisis.FechaHoraUpd = DateTime.Now;
                    valorAnalisis.IpUpd = ipAddress;
                }
            }

            //Autorización
            if (this.ProcesoIngreso.IdEstado == 3) //En Laboratorio, flujo normal
                this.ProcesoIngreso.CalcularAutorizacionPorAnalisis(out notificarAlertas);

            if (this.ProcesoIngreso.IdEstado == 4 ||
                this.ProcesoIngreso.IdEstado == 5 ||
                this.ProcesoIngreso.IdEstado == 9) //Aún no se ha pesado (Autorizado, En Revisión y Rechazado)
                this.ProcesoIngreso.CalcularAutorizacionPorAnalisis(out notificarAlertas);

            //Quedan los estados 6: Primer Pesaje, 7: Segundo Pesaje
            //El camión ya se descargó, no volvemos a calcular la autorización.

            //Peso Standard, en flujo normal no se calcula (no hemos pesado al camión aún)
            //Si tenemos peso bruto debemos volver a estandarizar, esto solo debería pasar en IdEstado = 7: Segundo Pesaje
            if (this.ProcesoIngreso.IdEstado == 7)
                this.ProcesoIngreso.PesoNormal = this.ProcesoIngreso.CalcularPesoNormal(dc);

            //Fecha hora no es editable para mantener control de tiempos
            if (!this.ProcesoIngreso.FechaHoraAnalisis.HasValue)
                this.ProcesoIngreso.FechaHoraAnalisis = DateTime.Now;

            this.ProcesoIngreso.UserAnalisis = this.PinUser.UserName;
            this.ProcesoIngreso.IpAnalisis = ipAddress;
            this.ProcesoIngreso.UserUpd = this.PinUser.UserName;
            this.ProcesoIngreso.FechaHoraUpd = DateTime.Now;
            this.ProcesoIngreso.IpUpd = ipAddress;
           
            dc.SubmitChanges();

            return this.ProcesoIngreso;
        }
    }

    public class ValorAnalisisViewModel
    {
        public int IdParametroAnalisis { get; set; }
        public int IdSucursal { get; set; }
        public decimal? Valor { get; set; }
        public string Nombre { get; set; }
        public string UM { get; set; }
        public int Orden { get; set; }
        public bool Requerido { get; set; }

        public bool CanBeDelayed()
        {
            return this.IdParametroAnalisis == 2 && this.IdSucursal == 2; //Materia Grasa para los tambores
        }

        public bool IsSelectList()
        {
            return ValorAnalisis.IsSelectList(this.IdParametroAnalisis);
        }

        public List<SelectListItem> GetSelectList()
        {
            //if (this.IdParametroAnalisis == 11)
            //{
                var selectedValue = Valor ?? 2;

                var list = new List<SelectListItem>();

                list.Add(new SelectListItem() { Value = "0", Text = "Rechazado", Selected = selectedValue == 0 });
                list.Add(new SelectListItem() { Value = "1", Text = "Aceptado", Selected = selectedValue == 1 });
                list.Add(new SelectListItem() { Value = "2", Text = "(No asignado)", Selected = selectedValue == 2 });

                return list;
            //}
        }

        public void Validate(AgroFichasDBDataContext dc, ModelStateDictionary modelState, string errorKey)
        {
            var parametro = dc.ParametroAnalisis.Single(pa => pa.IdParametroAnalisis == this.IdParametroAnalisis);
            if (parametro.Requerido && !this.CanBeDelayed() && !this.Valor.HasValue)
                modelState.AddModelError(errorKey, String.Format("{0} es requerido", parametro.Nombre));

            if (this.Valor.HasValue)
            {
                if (this.Valor.Value < parametro.MinValidValue)
                    modelState.AddModelError(errorKey, String.Format("El valor mínimo de {0} es {1} {2}", parametro.Nombre, parametro.MinValidValue.ToString(parametro.FormatString), parametro.UM));

                if (this.Valor.Value > parametro.MaxValidValue)
                    modelState.AddModelError(errorKey, String.Format("El valor máximo de {0} es {1} {2}", parametro.Nombre, parametro.MaxValidValue.ToString(parametro.FormatString), parametro.UM));

                if (this.Valor.Value != Math.Round(this.Valor.Value, parametro.Decimales))
                    modelState.AddModelError(errorKey, String.Format("{0} debe tener a lo más {1} decimal(es)", parametro.Nombre, parametro.Decimales));
            }
        }
    }
}