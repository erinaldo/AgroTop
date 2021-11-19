using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class AsignarDescuentosViewModel
    {
        public Liquidacion Liquidacion { get; set; }
        public List<Moneda> Monedas { get; set; }


        public int IdLiquidacion { get; set; }
        public List<DescuentoParaLiquidacion> Descuentos { get; set; }
        public string Observaciones { get; set; }
        
        public AsignarDescuentosViewModel()
        {

        }

        public AsignarDescuentosViewModel(AgroFichasDBDataContext dc, int idLiquidacion)
        {
            this.IdLiquidacion = idLiquidacion;

            int id = 0;
            this.Descuentos = (from d in dc.DescuentosParaLiquidacion(this.IdLiquidacion)
                               select DescuentoParaLiquidacion.FromDescuentoParaLiquidacionResult(d, ++id)).ToList();

            LoadLookups(dc);

            //Repartimos los descuentos
            var porrepartir = this.Liquidacion.TotalPagar;
            foreach (var d in this.Descuentos)
            {
                if (d.Saldo >= porrepartir)
                    d.MontoAsignado = porrepartir;
                else
                    d.MontoAsignado = d.Saldo;

                porrepartir -= d.MontoAsignado;

                this.Observaciones += string.Format("{0}{1}", d.Comentarios, Environment.NewLine);
            }
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Liquidacion = dc.Liquidacion.Single(lq => lq.IdLiquidacion == this.IdLiquidacion);
            this.Monedas = dc.Moneda.ToList();
        }

        public void Validate(ModelStateDictionary modelState)
        {
            var totalAsignado = 0M;

            if (this.Descuentos != null)
            {
                foreach (var d in this.Descuentos)
                {
                    totalAsignado += d.MontoAsignado;

                    var moneda = this.Monedas.Single(m => m.IdMoneda == d.IdMoneda);

                    var baseKey = "Descuentos[" + d.ID + "].";
                    var key = baseKey + "MontoAsignado";

                    if (d.MontoAsignado < 0)
                        modelState.AddModelError(key, "La asignación debe ser mayor o igual a cero");

                    if (d.MontoAsignado > d.Saldo)
                        modelState.AddModelError(key, "La asignación supera a la cantidad disponible del descuento");

                    if (!moneda.ValidarDecimales(d.MontoAsignado))
                        modelState.AddModelError(key, String.Format("El monto asignado en {1} debe tener a lo más {0} decimal(es)", moneda.Decimales, moneda.Simbolo));
                }
            }

            if (totalAsignado > this.Liquidacion.TotalPagar)
                modelState.AddModelError("txtTotalDescuentos", "El descuento total es mayor que el total de los ingresos");
        }

        public Liquidacion Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            var totalAsignado = 0M;
            if (this.Descuentos != null)
            {
                foreach (var d in this.Descuentos)
                {
                    if (d.MontoAsignado > 0)
                    {
                        if (d.IdDescuento > 0)
                        {
                            var descuento = new DescuentoLiquidacion()
                            {
                                IdDescuento = d.IdDescuento,
                                Monto = d.MontoAsignado,
                                FechaHoraIns = DateTime.Now,
                                UserIns = userName,
                                IpIns = ipAddress
                            };
                            this.Liquidacion.DescuentoLiquidacion.Add(descuento);
                        }
                        else if (d.IdSaldoCtaCte > 0)
                        {
                            var descuento = new SaldoCtaCteLiquidacion()
                            {
                                IdSaldoCtaCte = d.IdSaldoCtaCte,
                                Monto = d.MontoAsignado,
                                FechaHoraIns = DateTime.Now,
                                UserIns = userName,
                                IpIns = ipAddress
                            };
                            this.Liquidacion.SaldoCtaCteLiquidacion.Add(descuento);
                        }
                        totalAsignado += d.MontoAsignado;
                    }
                }
            }

            this.Liquidacion.TotalDescuentos = (int)totalAsignado;
            this.Liquidacion.ObservacionesDescuentos = this.Observaciones ?? "";
            this.Liquidacion.FechaHoraDescuentos = DateTime.Now;
            this.Liquidacion.UserDescuentos = userName;
            this.Liquidacion.IpDescuentos = ipAddress;

            dc.SubmitChanges();

            return this.Liquidacion;
        }

        public class DescuentoParaLiquidacion
        {
            public int ID { get; set; }
            public int IdDescuento { get; set; }
            public int IdSaldoCtaCte { get; set; }
            public int IdMoneda { get; set; }
            public string Rut { get; set; }
            public string Nombre { get; set; }
            public string TipoDescuento { get; set; }
            public int NumeroDocumento { get; set; }
            public string Instituccion { get; set; }
            public decimal Monto { get; set; }
            public decimal Saldo { get; set; }
            public decimal MontoAsignado { get; set; }
            public string Comentarios { get; set; }

            public static DescuentoParaLiquidacion FromDescuentoParaLiquidacionResult(DescuentosParaLiquidacionResult descuento, int id)
            {
                return new DescuentoParaLiquidacion()
                {
                    ID = id,
                    IdSaldoCtaCte = descuento.IdSaldoCtaCte,
                    IdDescuento = descuento.IdDescuento,
                    Rut = descuento.Rut,
                    Nombre = descuento.Nombre,
                    TipoDescuento = descuento.TipoDescuento,
                    NumeroDocumento = descuento.NumeroDocumento ?? 0,
                    Instituccion = descuento.Institucion,
                    Monto = descuento.Monto ?? 0,
                    Saldo = descuento.Saldo ?? 0,
                    IdMoneda = 1,
                    Comentarios = descuento.Comentarios
                };
            }
        }
    }
}