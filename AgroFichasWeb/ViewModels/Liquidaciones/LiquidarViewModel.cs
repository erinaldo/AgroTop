using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class LiquidarViewModel
    {
        public Agricultor Agricultor { get; set; }
        public Temporada Temporada { get; set; }
        public Empresa Empresa { get; set; }
        public Cultivo Cultivo { get; set; }
        public List<ParametroAnalisis> ParametrosAnalisis { get; set; }
        public List<ValorAnalisis> ValoresAnalisis { get; set; }
        public List<Moneda> Monedas { get; set; }

        public int IdLiquidacion { get; set; }
        public int IdAgricultor { get; set; }
        public int IdEmpresaLiq { get; set; }
        public int IdTemporadaLiq { get; set; }
        public int IdCultivo { get; set; }
        public string Observaciones { get; set; }

        public List<IngresoParaLiquidacion> Ingresos { get; set; }

        public LiquidarViewModel()
        {

        }

        public LiquidarViewModel(AgroFichasDBDataContext dc, int idLiquidacion)
        {
            var liquidacion = dc.Liquidacion.Single(l => l.IdLiquidacion == idLiquidacion);

            this.IdLiquidacion = liquidacion.IdLiquidacion;
            this.IdAgricultor = liquidacion.IdAgricultor;
            this.IdEmpresaLiq = liquidacion.IdEmpresa;
            this.IdTemporadaLiq = liquidacion.IdTemporada;
            this.IdCultivo = liquidacion.PrecioIngreso.First().ProcesoIngreso.CultivoContrato.IdCultivo;
            this.Observaciones = liquidacion.ObservacionesCreacion ?? "";

            LoadLookups(dc);
        }

        public LiquidarViewModel(AgroFichasDBDataContext dc, int idAgricultor, int idEmpresa, int idTemporada, int idCultivo)
        {
            this.IdLiquidacion = 0;
            this.IdAgricultor = idAgricultor;
            this.IdEmpresaLiq = idEmpresa;
            this.IdTemporadaLiq = idTemporada;
            this.IdCultivo = idCultivo;
            this.Observaciones = "";

            this.Ingresos = (from pi in dc.ProcesoIngreso
                             where pi.IdAgricultor == this.IdAgricultor
                                && pi.IdTemporada == this.IdTemporadaLiq
                                && pi.IdEmpresa == this.IdEmpresaLiq
                                && pi.CultivoContrato.IdCultivo == this.IdCultivo
                                && (pi.IdEstado == 8 || pi.IdEstado == 11) //Cerrado o Liquidado Parcial
                             select IngresoParaLiquidacion.FromProcesoIngreso(dc, pi)).ToList();

            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Agricultor = dc.Agricultor.Single(a => a.IdAgricultor == this.IdAgricultor && a.Habilitado);
            this.Empresa = dc.Empresa.Single(e => e.IdEmpresa == this.IdEmpresaLiq);
            this.Temporada = dc.Temporada.Single(t => t.IdTemporada == this.IdTemporadaLiq);
            this.Cultivo = dc.Cultivo.Single(c => c.IdCultivo == this.IdCultivo);
            this.Monedas = dc.Moneda.ToList();

            ParametrosAnalisis = ProcesoIngreso.GetParametrosAnalisisLiquidacion(dc, this.IdCultivo);

            var idsIngresos = this.Ingresos.Select(a => a.IdProcesoIngreso).ToArray();
            var idsParams = this.ParametrosAnalisis.Select(p => p.IdParametroAnalisis).ToArray();

            this.ValoresAnalisis = dc.ValorAnalisis.Where(va => idsIngresos.Contains(va.IdProcesoIngreso) && idsParams.Contains(va.IdParametroAnalisis)).ToList();
        }

        public void Validate(ModelStateDictionary modelState, AgroFichasDBDataContext dc)
        {
            var totalNeto = 0M;
            if (this.Ingresos != null)
            {
                foreach (var ingreso in this.Ingresos.Where(i => i.Usar))
                {
                    var totalAsignado = 0;
                    var keyBaseI = String.Format("Ingresos[{0}].", ingreso.IdProcesoIngreso);

                    if (ingreso.Precios != null)
                    {
                        foreach (var precio in ingreso.Precios)
                        {
                            totalAsignado += precio.CantidadAsignada;

                            var keyBase = String.Format("{0}Precios[{1}].", keyBaseI, precio.IdPrecioIngreso);

                            if (precio.CantidadAsignada < 0)
                                modelState.AddModelError(keyBase + "CantidadAsignada", "La asignación debe ser mayor o igual cero en ingreso " + ingreso.IdProcesoIngreso);

                            if (precio.MaxCantidadAsignada.HasValue && precio.CantidadAsignada > precio.MaxCantidadAsignada)
                                modelState.AddModelError(keyBase + "CantidadAsignada", "La asignación supera a la cantidad disponible del convenio en ingreso " + ingreso.IdProcesoIngreso);

                            //cuando es null es porque ya existe en la bd y es un precio sin convenio
                            //cuandos es < 0 es un nuevo precio sin convenio
                            var moneda = this.Monedas.Single(m => m.IdMoneda == precio.IdMoneda);
                            if ((precio.IdConvenioPrecio == null || precio.IdConvenioPrecio.Value < 0) && precio.CantidadAsignada > 0)
                            {
                                if (precio.PrecioUnidadBase <= 0)
                                    modelState.AddModelError(keyBase + "PrecioUnidadBase", "El precio no es válido en ingreso " + ingreso.IdProcesoIngreso);
                                else if (!moneda.ValidarDecimalesPrecio(precio.PrecioUnidadBase))
                                    modelState.AddModelError(keyBase + "PrecioUnidadBase", String.Format("El precio en {1} debe tener a lo más {0} decimal(es) en ingreso " + ingreso.IdProcesoIngreso, moneda.DecimalesPrecio, moneda.Simbolo));
                            }

                            //La tasa de cambio debe ser 1 para CLP
                            if (precio.IdMoneda == 1 && precio.TasaCambio != 1)
                                modelState.AddModelError(keyBase + "TasaCambio", "La tasa de cambio debe ser 1 para la moneda CLP en ingreso " + ingreso.IdProcesoIngreso);

                            if (precio.TasaCambio <= 0)
                                modelState.AddModelError(keyBase + "TasaCambio", "La tasa de cambio no es válida en ingreso " + ingreso.IdProcesoIngreso);

                            if (!moneda.ValidarDecimales(precio.TasaCambio))
                                modelState.AddModelError(keyBase + "TasaCambio", String.Format("La tasa de cambio {1} debe tener a lo más {0} decimal(es) en ingreso " + ingreso.IdProcesoIngreso, moneda.Decimales, moneda.Simbolo));

                            var totalLinea = precio.TotalLinea(dc);
                            if (totalLinea < 0 || (precio.CantidadAsignada > 0 && totalLinea <= 0))
                                modelState.AddModelError(keyBase + "TotalLineaPrecio", "El total de la línea no es válido en ingreso " + ingreso.IdProcesoIngreso);

                            totalNeto += totalLinea;
                        }

                    }

                    if (totalAsignado < ingreso.PesoNormal)
                        modelState.AddModelError(keyBaseI + "TotalAsignado", "Se debe asignar por completo el ingreso " + ingreso.IdProcesoIngreso);
                    else if (totalAsignado > ingreso.PesoNormal)
                        modelState.AddModelError(keyBaseI + "TotalAsignado", "El ingreso " + ingreso.IdProcesoIngreso + " está sobre asignado");
                }
            }

            if (totalNeto <= 0)
                modelState.AddModelError("TotalNeto", "El total neto no es válido. Debe ser mayor a cero");
        }

        public Liquidacion Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            Liquidacion liquidacion;

            if (this.IdLiquidacion == 0)
            {
                liquidacion = new Liquidacion()
                {
                    IdEmpresa = this.IdEmpresaLiq,
                    IdAgricultor = this.IdAgricultor,
                    IdTemporada = this.IdTemporadaLiq,
                    IdMoneda = 1,
                    FactorIva = Parametros.FactorIva,
                    FactorIvaRetenido = 0M,
                    Nulo = false,
                    Retenida = false,
                    FechaHoraCreacion = DateTime.Now,
                    UserCreacion = userName,
                    IpCreacion = ipAddress,
                    FechaHoraIns = DateTime.Now,
                    UserIns = userName,
                    IpIns = ipAddress
                };
            }
            else
            {
                liquidacion = dc.Liquidacion.Single(l => l.IdLiquidacion == this.IdLiquidacion);
                liquidacion.FechaHoraUpd = DateTime.Now;
                liquidacion.UserUpd = userName;
                liquidacion.IpUpd = ipAddress;

                foreach (var ingresoVM in this.Ingresos.Where(i => i.Usar == false))
                {
                    //var ingresoDB = dc.ProcesoIngreso.SingleOrDefault(ing => ing.IdProcesoIngreso == ingresoVM.IdProcesoIngreso && ing.IdLiquidacion == this.IdLiquidacion);
                    //if (ingresoDB != null)
                    //{
                    //    ingresoDB.IdLiquidacion = null;
                    //}
                }
            }

            liquidacion.ObservacionesCreacion = this.Observaciones ?? "";
            liquidacion.TotalNeto = 0;

            List<Descuento> descuentoList = new List<Descuento>();

            foreach (var tipoServicio in dc.TipoServicio.Where(ts => ts.Habilitado == true))
            {
                decimal total = 0;
                Descuento dto = new Descuento();
                dto.IdAgricultor = this.IdAgricultor;
                dto.IdTipoDescuento = 5;
                dto.IdMoneda = 1;
                dto.Institucion = "2020-2021 GRANOTOP";
                dto.Fecha = DateTime.Now;
                dto.FechaVencimiento = DateTime.Now;
                dto.Comentarios = "";

                dto.UserIns = userName;
                dto.FechaHoraIns = DateTime.Now;
                dto.IpIns = ipAddress;

                foreach (var ingresoVM in this.Ingresos.Where(i => i.Usar))
                {
                    var ingresoDB = dc.ProcesoIngreso.Single(ing => ing.IdProcesoIngreso == ingresoVM.IdProcesoIngreso);

                    dto.IdTemporada = ingresoDB.IdTemporada;

                    foreach (var ds in ingresoDB.DescuentoServicio)
                    {

                        if (ds.IdTipoServicio == tipoServicio.IdTipoServicio)
                        {
                            dto.Comentarios += "Servicio " + tipoServicio.Nombre.ToString() + ": $" + Math.Round(ds.ValorUnitario, 2).ToString();
                            total += ds.TotalDescuento;
                        }
                    }
                }
                dto.Monto = total;
                if (dto.Monto > 0)
                {
                    descuentoList.Add(dto);
                }
            }

            if (descuentoList.Count != 0)
            {
                Decimal totalDescuento = 0;
                Descuento totalDesc = new Descuento();
                totalDesc.IdAgricultor = this.IdAgricultor;
                totalDesc.IdTipoDescuento = 5;
                totalDesc.IdMoneda = 1;
                totalDesc.Institucion = "2020-2021 GRANOTOP";
                totalDesc.Fecha = DateTime.Now;
                totalDesc.FechaVencimiento = DateTime.Now;
                totalDesc.Comentarios = "GRANOTOP FACTURA";
                totalDesc.UserIns = userName;
                totalDesc.FechaHoraIns = DateTime.Now;
                totalDesc.IpIns = ipAddress;
                totalDesc.IdTemporada = this.IdTemporadaLiq;
                totalDesc.Monto = 0;

                foreach (var desc in descuentoList)
                {
                    //totalDesc.Comentarios += desc.Comentarios + ", "; 
                    totalDescuento += desc.Monto;
                }

                totalDesc.Monto = Math.Round(totalDescuento * 1.19M, 0);

                if (descuentoList.Count > 0)
                {
                    dc.Descuento.InsertOnSubmit(totalDesc);
                    dc.SubmitChanges();
                }
            }

            foreach (var ingresoVM in this.Ingresos.Where(i => i.Usar))
            {
                var ingresoDB = dc.ProcesoIngreso.Single(ing => ing.IdProcesoIngreso == ingresoVM.IdProcesoIngreso);



                foreach (var precioVM in ingresoVM.Precios)
                {
                    var precioDB = ingresoDB.PrecioIngreso.Single(pi => pi.IdPrecioIngreso == precioVM.IdPrecioIngreso);
                    if (precioVM.CantidadAsignada > 0)
                    {
                        precioVM.BonoCantidad = 0;

                        if (precioVM.BonoUnidad != 0)
                            precioVM.BonoCantidad = (int)Math.Round((decimal)precioVM.CantidadAsignada / ingresoDB.PesoNormal * ingresoDB.PesoBruto ?? 0, 0);


                        precioDB.Cantidad = precioVM.CantidadAsignada;
                        precioDB.BonoCantidad = precioVM.BonoCantidad;
                        precioDB.IdMoneda = precioVM.IdMoneda;
                        precioDB.PrecioUnidad = precioVM.PrecioUnidadBase;
                        precioDB.BonoUnidad = precioVM.BonoUnidad;
                        precioDB.TasaCambio = precioVM.TasaCambio;
                        precioDB.SobrePrecioPor = precioVM.SobrePrecioPor / 100M;
                        precioDB.SobrePrecioTotal = precioVM.SobrePrecioTotal(dc);
                        precioDB.DescuentoPor = precioVM.DescuentoPor / 100M;
                        precioDB.DescuentoTotal = precioVM.DescuentoTotal(dc);
                        precioDB.BonoTotal = precioVM.BonoTotal(dc);
                        precioDB.TotalNeto = precioVM.TotalLinea(dc);
                        precioDB.UserUpd = userName;
                        precioDB.FechaHoraUpd = DateTime.Now;
                        precioDB.IpUpd = ipAddress;
                    }
                    else
                    {
                        dc.PrecioIngreso.DeleteOnSubmit(precioDB);
                    }

                    if (precioVM.Usar)
                    {
                        liquidacion.PrecioIngreso.Add(precioDB);
                        liquidacion.TotalNeto += precioVM.TotalLinea(dc);
                    }

                }
            }

            liquidacion.TotalIva = Math.Round(liquidacion.TotalNeto * liquidacion.FactorIva);
            liquidacion.TotalLiquidacion = liquidacion.TotalNeto + liquidacion.TotalIva;

            liquidacion.TotalIvaRetenido = Math.Round(liquidacion.TotalNeto * liquidacion.FactorIvaRetenido);
            liquidacion.TotalIvaNoRetenido = liquidacion.TotalIva - liquidacion.TotalIvaRetenido;

            liquidacion.TotalPagar = liquidacion.TotalLiquidacion - liquidacion.TotalIvaRetenido;

            dc.SubmitChanges();

            //Actualizamos el total liquidado de cada ingreso que tocamos
            foreach (var ingresoVM in this.Ingresos.Where(i => i.Usar))
            {
                var ingresoDB = dc.ProcesoIngreso.Single(ing => ing.IdProcesoIngreso == ingresoVM.IdProcesoIngreso);
                ingresoDB.UpdatePesoYNetoLiquidado();
                dc.SubmitChanges();
            }
            
            return liquidacion;
        }

        public class IngresoParaLiquidacion
        {
            public bool Usar { get; set; }
            public int IdProcesoIngreso { get; set; }
            public DateTime Fecha { get; set; }
            public int NumeroGuia { get; set; }
            public string CultivoContrato { get; set; }
            public int PesoBruto { get; set; }
            public int PesoNormal { get; set; }
            public int TotalLinea { get; set; }

            public List<PrecioIngresoLiquidacion> Precios { get; set; }

            public string NombreSucursal { get; set; }

            public static IngresoParaLiquidacion FromProcesoIngreso(AgroFichasDBDataContext dc, ProcesoIngreso ingreso)
            {
                var item = new IngresoParaLiquidacion()
                {
                    Usar = true,
                    IdProcesoIngreso = ingreso.IdProcesoIngreso,
                    Fecha = ingreso.FechaHoraLlegada.Value.Date,
                    NumeroGuia = ingreso.NumeroGuia,
                    CultivoContrato = ingreso.CultivoContrato.Nombre,
                    PesoBruto = ingreso.PesoBruto.Value,
                    PesoNormal = ingreso.PesoNormal.Value,
                    TotalLinea = 0,
                    NombreSucursal = ingreso.Sucursal.Nombre
                };

                item.Precios = (from p in dc.PreciosParaLiquidacion(item.IdProcesoIngreso)
                                select new PrecioIngresoLiquidacion()
                                {
                                    Seleccionable = !p.IdLiquidacion.HasValue,
                                    Usar = !p.IdLiquidacion.HasValue,
                                    IdPrecioIngreso = p.IdPrecioIngreso,
                                    IdConvenioPrecio = p.IdConvenioPrecio,
                                    CantidadAsignada = p.Cantidad,
                                    MaxCantidadAsignada = p.IdConvenioPrecio.HasValue ? (int?)(p.Cantidad + p.CantidadDisponible.Value) : null,
                                    PrecioUnidadBase = p.PrecioUnidad,
                                    BonoUnidad = p.BonoUnidad,
                                    BonoCantidad = p.BonoCantidad,
                                    IdMoneda = p.IdMoneda,
                                    SimboloMoneda = p.SimboloMoneda,
                                    FormatoMoneda = p.FormatoMoneda,
                                    Rut = p.Rut,
                                    Nombre = p.Nombre,
                                    IdContrato = p.IdContrato,
                                    NroContrato = p.NumeroContrato,
                                    ComentariosContrato = p.ComentariosContrato ?? "",
                                    TieneConvenioMoneda = p.TieneConvenioMoneda.Value,
                                    EsPiso = p.EsPiso,
                                    TasaCambio = p.TasaCambio ?? 0,
                                    TasaCambioDolar = p.IdMoneda == 2 ? p.TasaCambio ?? 0 : TasaCambio.GetTasaCambio(dc, ingreso.FechaHoraLlegada.Value.Date, 2).Valor,
                                    SobrePrecioPor = p.SobrePrecioPor * 100,
                                    DescuentoPor = p.DescuentoPor * 100,
                                    PesoBruto = p.PesoBruto ?? 0,
                                    PesoNormal = p.PesoNormal ?? 0
                                }).ToList();

                return item;
            }
        }

        public class PrecioIngresoLiquidacion
        {
            public bool Seleccionable { get; set; }
            public bool Usar { get; set; }
            public int IdPrecioIngreso { get; set; }
            public int IdItemContrato { get; set; }
            public int? IdConvenioPrecio { get; set; }

            public int CantidadAsignada { get; set; }
            public int BonoCantidad { get; set; }
            public int? MaxCantidadAsignada { get; set; }

            public decimal PrecioUnidadBase { get; set; }
            public decimal BonoUnidad { get; set; }
            public int IdMoneda { get; set; }
            public string SimboloMoneda { get; set; }
            public string FormatoMoneda { get; set; }
            public bool EsPiso { get; set; }

            public string Nombre { get; set; }
            public string Rut { get; set; }
            public string NroContrato { get; set; }
            public int IdContrato { get; set; }
            public string ComentariosContrato { get; set; }
            public bool TieneConvenioMoneda { get; set; }
            public decimal TasaCambio { get; set; }
            public decimal TasaCambioDolar { get; set; }
            public decimal SobrePrecioPor { get; set; }
            public decimal DescuentoPor { get; set; }

            public int PesoBruto { get; set; }
            public int PesoNormal { get; set; }

            public int TotalLinea(AgroFichasDBDataContext dc)
            {
                return (int)Math.Round(this.PrecioUnidadBase * this.TasaCambio * this.CantidadAsignada + this.BonoTotal(dc) * this.TasaCambio + (this.SobrePrecioTotal(dc) - this.DescuentoTotal(dc)) * this.TasaCambio);
            }

            public decimal SobrePrecioTotal(AgroFichasDBDataContext dc)
            {
                var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
                return Math.Round(this.PrecioUnidadBase * this.CantidadAsignada * (this.SobrePrecioPor / 100M), moneda.Decimales);
            }

            public decimal DescuentoTotal(AgroFichasDBDataContext dc)
            {
                var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
                return Math.Round(this.PrecioUnidadBase * this.CantidadAsignada * (this.DescuentoPor / 100M), moneda.Decimales);
            }

            public decimal BonoTotal(AgroFichasDBDataContext dc)
            {
                var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
                return Math.Round(this.BonoUnidad * this.BonoCantidad, moneda.Decimales);
            }
        }
    }
}