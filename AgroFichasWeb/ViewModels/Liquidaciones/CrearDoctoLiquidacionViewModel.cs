using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class CrearDoctoLiquidacionViewModel
    {
        public Liquidacion Liquidacion { get; set; }
        public List<TipoDoctoLiquidacion> TiposDocto { get; set; }
        public List<TipoFechaPago> TiposFechaPago { get; set; }
        public List<FormaPago> FormasPago { get; set; }
        public List<Banco> Bancos { get; set; }
        public List<TipoCuentaBancaria> TiposCuentaBancaria { get; set; }
        public SaldoDoctos SaldoPendienteDoctos { get; set; }
        public int? IdDoctoLiquidacion { get; set; }
        public int IdLiquidacion { get; set; }
        public int IdTipoDoctoLiquidacion { get; set; }
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public decimal TotalNeto { get; set; }
        public decimal TotalIva { get; set; }
        public decimal TotalIvaRetenido { get; set; }
        public decimal FactorIva { get; set; }
        public decimal FactorIvaRetenido { get; set; }
        public string Observaciones { get; set; }
        public int IdTipoFechaPago { get; set; }
        public DateTime? FechaPagoEspecial { get; set; }
        public int IdFormaPago { get; set; }
        public int IdCuentaBancaria { get; set; }

        public CrearDoctoLiquidacionViewModel()
        {

        }

        public CrearDoctoLiquidacionViewModel(AgroFichasDBDataContext dc, int idLiquidacion)
        {
            this.IdDoctoLiquidacion = null;
            this.IdLiquidacion = idLiquidacion;
            this.Fecha = DateTime.Today;
            this.Observaciones = "";

            LoadLookups(dc);

            this.IdTipoDoctoLiquidacion = this.TiposDocto.First().IdTipoDoctoLiquidacion;
            this.IdTipoFechaPago = this.TiposFechaPago.First().IdTipoFechaPago;
            this.FechaPagoEspecial = DateTime.Today;
            this.IdFormaPago = this.FormasPago.First().IdFormaPago;
            
            this.Rut = this.Liquidacion.Agricultor.Rut;
            this.Nombre = this.Liquidacion.Agricultor.Nombre;

            this.TotalNeto = this.SaldoPendienteDoctos.TotalNeto;
            this.TotalIva = this.SaldoPendienteDoctos.TotalIva;
            this.TotalIvaRetenido = this.SaldoPendienteDoctos.TotalIvaRetenido;
            this.FactorIva = this.Liquidacion.FactorIva;
            this.FactorIvaRetenido = this.Liquidacion.FactorIvaRetenido;
        }

        public void Load(AgroFichasDBDataContext dc, int idDoctoLiquidacion)
        {
            var docto = dc.DoctoLiquidacion.Single(d => d.IdDoctoLiquidacion == idDoctoLiquidacion);

            this.IdDoctoLiquidacion = idDoctoLiquidacion;
            this.IdLiquidacion = docto.IdLiquidacion;
            this.Fecha = docto.Fecha;
            this.Observaciones = docto.Observaciones;
            this.Numero = docto.Numero;

            this.IdTipoDoctoLiquidacion = docto.IdTipoDoctoLiquidacion;
            this.IdTipoFechaPago = docto.IdTipoFechaPago;
            this.FechaPagoEspecial = docto.FechaPagoEspecial;
            this.IdFormaPago = docto.IdFormaPago;

            this.Rut = docto.Rut;
            this.Nombre = docto.Nombre;

            this.TotalNeto = docto.TotalNeto;
            this.TotalIva = docto.TotalIva;
            this.TotalIvaRetenido = docto.TotalIvaRetenido;
            this.FactorIva = docto.FactorIva;
            this.FactorIvaRetenido = docto.FactorIvaRetenido;

            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Liquidacion = dc.Liquidacion.Single(lq => lq.IdLiquidacion == this.IdLiquidacion);
            this.TiposDocto = dc.TipoDoctoLiquidacion.ToList();
            this.TiposFechaPago = dc.TipoFechaPago.ToList();
            this.FormasPago = dc.FormaPago.ToList();
            this.Bancos = dc.Banco.ToList();
            this.TiposCuentaBancaria = dc.TipoCuentaBancaria.ToList();

            this.SaldoPendienteDoctos = this.Liquidacion.SaldoPendienteDoctos(this.IdDoctoLiquidacion);
        }

        public void Validate(ModelStateDictionary modelState)
        {
            if (this.IdTipoDoctoLiquidacion <= 0)
                modelState.AddModelError("IdTipoDoctoLiquidacion", "El Tipo de documento es requerido");

            if (this.Numero <= 0)
                modelState.AddModelError("Numero", "El Número de documento es requerido");

            if (this.IdFormaPago <= 0)
                modelState.AddModelError("IdFormaPago", "La Forma de Pago es requerida");

            if (this.IdCuentaBancaria <= 0)
                modelState.AddModelError("IdCuentaBancaria", "La Cuenta Bancaria es requerida");

            if (this.TotalNeto > this.SaldoPendienteDoctos.TotalNeto)
                modelState.AddModelError("TotalNeto", "El Total Neto supera el máximo pendiente de documentación");

            if (this.TotalIva > this.SaldoPendienteDoctos.TotalIva)
                modelState.AddModelError("TotalIva", "El Total Iva supera el máximo pendiente de documentación");

            if (this.TotalIvaRetenido > this.SaldoPendienteDoctos.TotalIvaRetenido)
                modelState.AddModelError("TotalIvaRetenido", "El Total Iva Retenido supera el máximo pendiente de documentación");

            if (this.TotalIvaRetenido > this.TotalIva)
                modelState.AddModelError("TotalIvaRetenido", "El Total Iva Rentenido no puede ser mayor al Total Iva");

            if (this.TotalNeto + this.TotalIva - this.TotalIvaRetenido > this.SaldoPendienteDoctos.TotalPagar)
                modelState.AddModelError("TotalPagar", "El Total a Pagar supera el máximo pendiente de documentación");
            
            if (this.IdTipoFechaPago == 2 && this.FechaPagoEspecial == null)
                modelState.AddModelError("FechaPagoEspecial", "La Fecha de pago especial es requerida");
        }

        public DoctoLiquidacion Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            DoctoLiquidacion docto;

            if (this.IdDoctoLiquidacion == null)
            {
                docto = new DoctoLiquidacion()
                {
                    IdLiquidacion = this.IdLiquidacion,
                    Rut = this.Rut,
                    Nombre = this.Nombre,
                    FactorIva = this.FactorIva,
                    FactorIvaRetenido = this.FactorIvaRetenido,
                    FechaHoraIns = DateTime.Now,
                    UserIns = userName,
                    IpIns = ipAddress
                };
                dc.DoctoLiquidacion.InsertOnSubmit(docto);
            }
            else
            {
                docto = dc.DoctoLiquidacion.Single(d => d.IdDoctoLiquidacion == this.IdDoctoLiquidacion.Value);
                docto.FechaHoraUpd = DateTime.Now;
                docto.UserUpd = userName;
                docto.IpUpd = ipAddress;
            }

            docto.IdTipoDoctoLiquidacion = this.IdTipoDoctoLiquidacion;
            docto.Numero = this.Numero;
            docto.Fecha = this.Fecha;
            docto.Observaciones = this.Observaciones ?? "";
            docto.TotalNeto = this.TotalNeto;
            docto.TotalIva = this.TotalIva;
            docto.TotalIvaRetenido = this.TotalIvaRetenido;
            docto.TotalIvaNoRetenido = this.TotalIva - this.TotalIvaRetenido;
            docto.TotalNetoIva = this.TotalNeto + this.TotalIva;
            docto.TotalPagar = this.TotalNeto + this.TotalIva - this.TotalIvaRetenido;

            docto.IdTipoFechaPago = this.IdTipoFechaPago;
            docto.FechaPagoEspecial = this.IdTipoFechaPago == 1 ? null : this.FechaPagoEspecial;

            docto.IdFormaPago = this.IdFormaPago;
            docto.IdCuentaBancaria = this.IdCuentaBancaria;

            dc.SubmitChanges();
            return docto;
        }
    }
}