using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class EditarProcesoIngresoViewModel
    {
        private AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

        public ProcesoIngreso ProcesoIngreso { get; set; }

        public List<TipoVehiculo> TiposVehiculo { get; set; }
        public List<Bodega> Bodegas { get; set; }
        public List<Variedad> Variedades { get; set; }
        public List<Empresa> Empresas { get; set; }
        public List<Sucursal> Sucursales { get; set; }
        public List<CultivoContrato> CultivoContratos { get; set; }
        public List<TipoGuia> TipoGuias { get; set; }

        public Agricultor Agricultor { get; set; }

        public int IdProcesoIngreso { get; set; }
        public int IdVariedad { get; set; }
        public int IdTipoVehiculo { get; set; }
        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }
        public int IdCultivoContrato { get; set; }
        public string Patente { get; set; }
        public string Chofer { get; set; }
        public int NumeroGuia { get; set; }

        public bool UltimaEntrega { get; set; }
        public bool LiquidacionDolar { get; set; }

        public int IdBodega { get; set; }
        public int Manga { get; set; }
        public bool Secador { get; set; }

        public int? NroIngresoManual { get; set; }
        public int? NroGuiaPropia { get; set; }

        public int IdTipoGuia { get; set; }

        public int IdTipoServicio { get; set; }

        public string email { get; set; }
        public string fono1 { get; set; }
        public string fono2 { get; set; }

        public int IdAgricultor { get; set; }

        public EditarProcesoIngresoViewModel()
        {

        }

        public EditarProcesoIngresoViewModel(AgroFichasDBDataContext dc, int idProcesoIngreso, int idAgricultor)
        {
            this.IdAgricultor = idAgricultor;
            this.IdProcesoIngreso = idProcesoIngreso;

            Agricultor = dc.Agricultor.Single(a => a.IdAgricultor == this.IdAgricultor && a.Habilitado);
            var idsRelacionados = this.Agricultor.IdsRelacionadosHermanos(dc, true, true);

            LoadLookups(dc);

            this.IdVariedad = this.ProcesoIngreso.IdVariedad ?? 0;
            this.IdTipoVehiculo = this.ProcesoIngreso.IdTipoVehiculo;
            this.Patente = this.ProcesoIngreso.Patente;
            this.Chofer = this.ProcesoIngreso.Chofer;
            this.NumeroGuia = this.ProcesoIngreso.NumeroGuia;
            this.UltimaEntrega = this.ProcesoIngreso.UltimaEntrega;
            this.LiquidacionDolar = this.ProcesoIngreso.LiquidacionDolar;

            this.IdBodega = this.ProcesoIngreso.IdBodega ?? 0;
            this.Manga = this.ProcesoIngreso.Manga ?? 0;
            this.Secador = this.ProcesoIngreso.Secador ?? false;
            this.NroIngresoManual = this.ProcesoIngreso.NroIngresoManual;
            this.NroGuiaPropia = this.ProcesoIngreso.NroGuiaPropia;

            this.IdEmpresa = this.ProcesoIngreso.IdEmpresa;
            this.IdSucursal = this.ProcesoIngreso.IdSucursal;
            this.IdCultivoContrato = this.ProcesoIngreso.IdCultivoContrato;
            this.IdTipoGuia = this.ProcesoIngreso.IdTipoGuia;
            this.IdTipoServicio = this.ProcesoIngreso.IdTipoServicio ?? 0;
            this.email = this.Agricultor.Email;
            this.fono1 = this.Agricultor.Fono1;
            this.fono2 = this.Agricultor.Fono2;

        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.ProcesoIngreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == this.IdProcesoIngreso);

            this.Bodegas = dc.Bodega.Where(b => b.IdSucursal == this.ProcesoIngreso.IdSucursal && (b.Habilitada || b.IdBodega == (this.ProcesoIngreso.IdBodega ?? 0))).ToList();
            this.TiposVehiculo = dc.TipoVehiculo.OrderBy(tv => tv.IdTipoVehiculo).ToList();
            this.Variedades = dc.Variedad.OrderBy(v => v.Cultivo.Nombre).ThenBy(v => v.Nombre).ToList();

            this.Empresas = dc.Empresa.ToList();
            this.Sucursales = dc.Sucursal.ToList();
            this.CultivoContratos = dc.CultivoContrato.OrderBy(x => x.Nombre).ToList();
            this.TipoGuias = dc.TipoGuia.ToList();
        }

        public void Validate(AgroFichasDBDataContext dc, ModelStateDictionary modelState)
        {
            List<ProcesoIngreso> procesos1 = dataContext.ProcesoIngreso.Where(X => X.IdAgricultor == this.ProcesoIngreso.IdAgricultor && X.NumeroGuia == this.NumeroGuia && X.IdTipoGuia == this.IdTipoGuia && X.IdEstado != 99).ToList();

            if (this.IdTipoVehiculo <= 0)
                modelState.AddModelError("IdTipoVehiculo", "El Tipo de Vehículo es requerido");

            if (this.NumeroGuia <= 0)
                modelState.AddModelError("NumeroGuia", "El Número de la Guía no es válido");
            else if (procesos1.Count == 1)
            {
                ProcesoIngreso procesoIngreso = procesos1.SingleOrDefault(X => X.IdProcesoIngreso == this.ProcesoIngreso.IdProcesoIngreso && X.IdAgricultor == this.ProcesoIngreso.IdAgricultor && X.NumeroGuia == this.NumeroGuia && X.IdTipoGuia == this.IdTipoGuia && X.IdEstado != 99);
                if (procesoIngreso != null)
                {
                    //Pasa
                }
                else
                {
                    modelState.AddModelError("NumeroGuia", "El Número de la Guía está asignado a otra llegada");
                }
            }
            else if (procesos1.Count > 1)
            {
                modelState.AddModelError("NumeroGuia", "El Número de la Guía está asignado en múltiples llegadas");
            }
            else
            {
                //Pasa
            }

            if (String.IsNullOrWhiteSpace(this.Patente) || this.Patente.Trim().Length < 6)
                modelState.AddModelError("Patente", "La Patente no es válida");

            if (String.IsNullOrWhiteSpace(this.Chofer))
                modelState.AddModelError("Chofer", "El Chofer es requerido");

            if (this.IdVariedad <= 0)
                modelState.AddModelError("IdVariedad", "La Variedad es requerida");

            if (this.ProcesoIngreso.IdBodega.HasValue && this.IdBodega <= 0)
                modelState.AddModelError("IdBodega", "La Bodega es requerida");

            if (this.IdBodega > 0 && dc.Bodega.Single(b => b.IdBodega == this.IdBodega).EsManga && this.Manga <= 0)
                modelState.AddModelError("Manga", "El Número de la Manga es requerido");

            if (this.NroIngresoManual.HasValue && this.NroIngresoManual.Value <= 0)
                modelState.AddModelError("NroIngresoManual", "El Nro de Ingreso Manual no es válido. Si no existe, déjelo en blanco");

            if (this.NroGuiaPropia.HasValue && this.NroGuiaPropia.Value <= 0)
                modelState.AddModelError("NroGuiaPropia", "El Nro de Guía Propia no es válido. Si no existe, déjelo en blanco");

            if (this.IdEmpresa <= 0)
                modelState.AddModelError("IdTipoVehiculo", "La Empresa es requerida");

            if (this.IdSucursal <= 0)
                modelState.AddModelError("IdTipoVehiculo", "La Sucursal es requerida");

            if (this.IdCultivoContrato <= 0)
                modelState.AddModelError("IdTipoVehiculo", "El Cultivo Contrato es requerido");
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            var email = this.email == null ? "" : this.email;
            var fono1 = this.fono1 == null ? "" : this.fono1;
            var fono2 = this.fono2 == null ? "" : this.fono2;

           
            this.ProcesoIngreso.IdTipoVehiculo = this.IdTipoVehiculo;
            this.ProcesoIngreso.IdVariedad = this.IdVariedad;
            this.ProcesoIngreso.Patente = this.Patente;
            this.ProcesoIngreso.Chofer = this.Chofer;
            this.ProcesoIngreso.NumeroGuia = this.NumeroGuia;
            this.ProcesoIngreso.UltimaEntrega = this.UltimaEntrega;
            this.ProcesoIngreso.LiquidacionDolar = this.LiquidacionDolar;
            this.ProcesoIngreso.NroIngresoManual = this.NroIngresoManual;
            this.ProcesoIngreso.NroGuiaPropia = this.NroGuiaPropia;

            this.ProcesoIngreso.IdEmpresa = this.IdEmpresa;
            this.ProcesoIngreso.IdSucursal = this.IdSucursal;
            this.ProcesoIngreso.IdCultivoContrato = this.IdCultivoContrato;

            if (this.IdBodega > 0)
            {
                this.ProcesoIngreso.IdBodega = this.IdBodega;
                this.ProcesoIngreso.Secador = this.Secador;

                if (this.Bodegas.Single(b => b.IdBodega == this.IdBodega).EsManga)
                    this.ProcesoIngreso.Manga = this.Manga;
                else
                    this.ProcesoIngreso.Manga = null;
            }
            else
            {
                this.ProcesoIngreso.IdBodega = null;
                this.ProcesoIngreso.Secador = null;
                this.ProcesoIngreso.Manga = null;
            }

            this.ProcesoIngreso.UserUpd = userName;
            this.ProcesoIngreso.FechaHoraUpd = DateTime.Now;
            this.ProcesoIngreso.IpUpd = ipAddress;

            this.ProcesoIngreso.IdTipoGuia = this.IdTipoGuia;
            this.ProcesoIngreso.IdTipoServicio = this.IdTipoServicio;

            ProcesoIngreso.ActualizaContactoAgricultor(dc, email, fono1, fono2);
            dc.SubmitChanges();

            return this.ProcesoIngreso;
        }
    }
}