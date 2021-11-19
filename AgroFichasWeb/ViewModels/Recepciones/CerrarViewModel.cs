using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class CerrarViewModel
    {
        public ProcesoIngreso ProcesoIngreso { get; set; }
        public List<ItemContratoParaCierre> ItemsContratosParaCierre { get; set; }
        public List<Moneda> Monedas { get; set; }
        public List<Comuna> Comunas { get; set; }

        public TasaCambio TasaCambioDolarSpot { get; set; }
        public vwPrecioSpot PrecioSpot { get; set; }
        public int IdProcesoIngreso { get; set; }
        public int IdComunaOrigen { get; set; }
        public bool LiquidarEnDolares { get; set; }
        public bool SolicitoLiquidarEnDolares { get; set; }

        private int idConvenioPrecioTemp = 0;

        public List<PrecioServicioList> PrecioServicios { get; set; }
        public class PrecioServicioList
        {
            public int IdTipoServicio { get; set; }
            public string TipoServicio { get; set; }
            public decimal Valor { get; set; }
            public DateTime Fecha { get; set; }
            public int? PesoBruto { get; set; }
            public decimal TotalDescuento { get; set; }
            public int IdSucursal { get; set; }
            public int IdCultivo { get; set; }
            public int? IdPrecioServicio { get; set; }
        }

        public CerrarViewModel()
        {
        }

        public CerrarViewModel(AgroFichasDBDataContext dc, int idProcesoIngreso)
        {

            this.IdProcesoIngreso = idProcesoIngreso;

            LoadLookups(dc);

            this.IdComunaOrigen = this.ProcesoIngreso.IdComunaOrigen;
            this.ItemsContratosParaCierre = (from result in dc.ItemsContratosParaCierreInicial(this.IdProcesoIngreso)
                                             select ItemContratoParaCierre.FromItemsContratosParaCierreInicial(dc, result, this.TasaCambioDolarSpot, this.PrecioSpot, this.ProcesoIngreso.IdSucursal, ref idConvenioPrecioTemp)).ToList();

            //Repartir convenios para cierre de acuerdo a prioridades
            var porrepartir = (int)this.ProcesoIngreso.PesoNormal.Value;
            foreach (var ic in this.ItemsContratosParaCierre)
            {
                foreach (var convenio in ic.Convenios)
                {
                    if (convenio.IdConvenioPrecio > 0)
                    {
                        if (convenio.CantidadDisponible >= porrepartir)
                            convenio.CantidadAsignadaIngreso = porrepartir;
                        else
                            convenio.CantidadAsignadaIngreso = convenio.CantidadDisponible;
                    }
                    else
                    {
                        convenio.CantidadAsignadaIngreso = 0; // porrepartir;
                    }

                    porrepartir -= convenio.CantidadAsignadaIngreso;
                }
            }

            List<TipoServicio> tipoServicio = new List<TipoServicio>();
            tipoServicio = dc.TipoServicio.ToList();
            var procesoIngreso = dc.ProcesoIngreso.FirstOrDefault(pi => pi.IdProcesoIngreso == idProcesoIngreso);

            this.PrecioServicios = new List<PrecioServicioList>();
            if (this.ProcesoIngreso.CultivoContrato.IdCultivo == 10)
            {
                if (this.ProcesoIngreso.DescuentoServicio.Count() == 0)
                {
                    procesoIngreso.IngresoDescuentoServicio(this.ProcesoIngreso, "", "");
                }
            }
            foreach (TipoServicio ts in tipoServicio)
            {
                PrecioServicio servicio = dc.PrecioServicio.Where(ps => ps.IdCultivo == procesoIngreso.CultivoContrato.IdCultivo && ps.IdSucursal == procesoIngreso.IdSucursal && ps.Fecha <= procesoIngreso.FechaHoraLlegada && ps.IdTipoServicio == ts.IdTipoServicio && ps.Habilitado == true).OrderByDescending(ps => ps.Fecha).FirstOrDefault();



                if (servicio != null)
                {
                    PrecioServicios.Add(new PrecioServicioList()
                    {
                        IdTipoServicio = ts.IdTipoServicio,
                        TipoServicio = ts.Nombre,
                        Valor = servicio.Valor > 0 ? servicio.Valor : 0,
                        Fecha = servicio.Fecha,
                        PesoBruto = this.ProcesoIngreso.PesoBruto,
                        TotalDescuento = ts.IdTipoServicio != 1 ? Math.Round((servicio.Valor > 0 ? servicio.Valor : 0) * Convert.ToDecimal(procesoIngreso.PesoBruto), 0)  : Math.Round(((procesoIngreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 62).Valor.Value - 14.5M) * (servicio.Valor > 0 ? servicio.Valor : 0)) * Convert.ToDecimal(procesoIngreso.PesoBruto), 0),
                        IdSucursal = servicio.IdSucursal,
                        IdPrecioServicio = servicio.IdPrecioServicio,
                        IdCultivo = this.ProcesoIngreso.IdCultivoContrato

                    });
                }
                else
                {
                    PrecioServicios.Add(new PrecioServicioList()
                    {
                        IdTipoServicio = ts.IdTipoServicio,
                        TipoServicio = ts.Nombre,
                        Valor = 0,
                        Fecha = DateTime.Now,
                        PesoBruto = this.ProcesoIngreso.PesoBruto,
                        TotalDescuento = 0,
                        IdSucursal = this.ProcesoIngreso.IdSucursal,
                        IdPrecioServicio = null,
                        IdCultivo = this.ProcesoIngreso.IdCultivoContrato
                    });
                }
            }


            this.LiquidarEnDolares = this.SolicitoLiquidarEnDolares;
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.ProcesoIngreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == this.IdProcesoIngreso && pi.IdEstado == 7);
            this.Monedas = dc.Moneda.ToList();
            this.Comunas = dc.Comuna.OrderBy(c => c.Nombre).ToList();

            this.TasaCambioDolarSpot = TasaCambio.GetTasaCambio(dc, this.ProcesoIngreso.FechaHoraLlegada.Value.Date, 2);
            this.PrecioSpot = Models.PrecioSpot.GetPrecioSpot(dc, this.ProcesoIngreso.FechaHoraLlegada.Value.Date, this.ProcesoIngreso.CultivoContrato.IdCultivo, this.ProcesoIngreso.IdSucursal);
            this.SolicitoLiquidarEnDolares = this.ProcesoIngreso.SolicitoLiquidacionEnDolares(dc);


        }

        public void Validate(ModelStateDictionary modelState)
        {
            var totalAsignado = 0;
            var hayDolares = false;

            if (this.ItemsContratosParaCierre != null)
            {
                foreach (var ic in this.ItemsContratosParaCierre)
                {
                    foreach (var convenio in ic.Convenios)
                    {
                        totalAsignado += convenio.CantidadAsignadaIngreso;
                        if (convenio.IdMoneda == 2)
                            hayDolares = true;

                        var moneda = this.Monedas.Single(m => m.IdMoneda == convenio.IdMoneda);
                        
                        var baseKey = "ItemsContratosParaCierre[" + ic.IdItemContrato + "].Convenios[" + convenio.IdConvenioPrecio + "].";
                        var key = baseKey + "CantidadAsignadaIngreso";

                        if (convenio.CantidadAsignadaIngreso < 0)
                            modelState.AddModelError(key, "La asignación debe ser mayor o igual a cero");

                        if (convenio.IdConvenioPrecio > 0 && convenio.CantidadAsignadaIngreso > convenio.CantidadDisponible)
                            modelState.AddModelError(key, "La asignación supera a la cantidad disponible del convenio");

                        if (convenio.IdConvenioPrecio < 0 && convenio.CantidadAsignadaIngreso > 0)
                        {
                            var keypu = baseKey + "PrecioUnidadBase";
                            if (convenio.PrecioUnidadBase <= 0)
                            {
                                modelState.AddModelError(keypu, "El precio no es válido");
                            }
                            else
                            {
                                if (!moneda.ValidarDecimalesPrecio(convenio.PrecioUnidadBase))
                                    modelState.AddModelError(keypu, String.Format("El precio en {1} debe tener a lo más {0} decimal(es)", moneda.DecimalesPrecio, moneda.Simbolo));
                            }

                            var keybono = baseKey + "BonoUnidad";

                            if (!moneda.ValidarDecimalesPrecio(convenio.BonoUnidad))
                                modelState.AddModelError(keypu, String.Format("El bono en {1} debe tener a lo más {0} decimal(es)", moneda.DecimalesPrecio, moneda.Simbolo));
                        }

                        if (convenio.IdMoneda == 1 && convenio.TasaCambio != 1)
                        {
                            modelState.AddModelError(baseKey + "TasaCambio", "La tasa de cambio debe ser 1");
                        }

                        if (convenio.IdMoneda != 1)
                        {
                            if (convenio.TasaCambio < 0)
                                modelState.AddModelError(baseKey + "TasaCambio", "La tasa de cambio no es válida");

                            if (!moneda.ValidarDecimales(convenio.TasaCambio))
                                modelState.AddModelError(baseKey + "TasaCambio", String.Format("La Tasa de cambio en {1} debe tener a lo más {0} decimal(es)", moneda.Decimales, moneda.Simbolo));
                        }
                    }
                }
            }

            if (this.LiquidarEnDolares && !hayDolares)
                modelState.AddModelError("LiquidarEnDolares", "Indicó Liquidar en Dólares pero el cierre no contiene precios en dólares");

            if (totalAsignado < this.ProcesoIngreso.PesoNormal)
                modelState.AddModelError("TotalAsignado", "Se debe asignar por completo el ingreso");
            else if (totalAsignado > this.ProcesoIngreso.PesoNormal)
                modelState.AddModelError("TotalAsignado", "El ingreso está sobre asignado");

            if (this.IdComunaOrigen == 999999)
                modelState.AddModelError("IdComunaOrigen", "La Comuna de origen es requerida");
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            if (this.ProcesoIngreso.PrecioIngreso.Count > 0)
                throw new Exception("El ingreso ya tiene un cierre anterior");

            int totalNeto = 0;
            foreach (var ic in this.ItemsContratosParaCierre)
            {
                foreach (var convenio in ic.Convenios)
                {
                    if (convenio.CantidadAsignadaIngreso > 0)
                    {
                        convenio.BonoCantidad = 0;

                        if (convenio.BonoUnidad != 0)
                            convenio.BonoCantidad = (int)Math.Round((decimal)convenio.CantidadAsignadaIngreso / this.ProcesoIngreso.PesoNormal * this.ProcesoIngreso.PesoBruto ?? 0, 0);
                        
                        this.ProcesoIngreso.PrecioIngreso.Add(new PrecioIngreso()
                        {
                            IdConvenioPrecio = convenio.IdConvenioPrecio > 0 ? (int?)convenio.IdConvenioPrecio : null,
                            IdItemContrato = ic.IdItemContrato,
                            Cantidad = convenio.CantidadAsignadaIngreso,
                            PrecioUnidad = convenio.PrecioUnidadBase,
                            BonoUnidad = convenio.BonoUnidad,
                            BonoCantidad =  convenio.BonoCantidad,
                            BonoTotal = convenio.BonoTotal(dc),
                            IdMoneda = convenio.IdMoneda,
                            TasaCambio = convenio.TasaCambio,
                            SobrePrecioPor = convenio.SobrePrecioPor / 100M,
                            SobrePrecioTotal = convenio.SobrePrecioTotal(dc),
                            DescuentoPor = convenio.DescuentoPor / 100M,
                            DescuentoTotal = convenio.DescuentoTotal(dc),
                            TotalNeto = convenio.TotalLinea(dc),
                            UserIns = userName,
                            FechaHoraIns = DateTime.Now,
                            IpIns = ipAddress
                        });
                        totalNeto += convenio.TotalLinea(dc);
                    }
                }
            }
            if (this.ProcesoIngreso.CultivoContrato.IdCultivo == 10)
            {
                if (this.PrecioServicios != null)
                {
                    foreach (var valor in this.PrecioServicios)
                    {
                        DescuentoServicio descuentoServicios = new DescuentoServicio();
                            
                        descuentoServicios = this.ProcesoIngreso.DescuentoServicio.SingleOrDefault(ds => ds.IdTipoServicio == valor.IdTipoServicio);

                        //if (descuentoServicios == null)
                        //{
                        //    ProcesoIngreso procesoIngreso = new ProcesoIngreso();



                        //    descuentoServicios = this.ProcesoIngreso.DescuentoServicio.Single(ds => ds.IdTipoServicio == valor.IdTipoServicio);
                        //}
                        descuentoServicios.ValorUnitario = valor.Valor;
                        descuentoServicios.TotalDescuento = valor.TotalDescuento;
                        descuentoServicios.Fecha = valor.Fecha;
                        descuentoServicios.IdProcesoIngreso = this.ProcesoIngreso.IdProcesoIngreso;
                        descuentoServicios.IdTipoServicio = valor.IdTipoServicio;
                        descuentoServicios.IdPrecioServicio = valor.IdPrecioServicio;
                        descuentoServicios.IdSucursal = valor.IdSucursal;
                        descuentoServicios.IdCultivo = this.ProcesoIngreso.CultivoContrato.IdCultivo;
                        descuentoServicios.UserIns = userName;
                        descuentoServicios.FechaHoraIns = DateTime.Now;
                        descuentoServicios.IpIns = ipAddress;
                        descuentoServicios.Habilitado = true;
                    }
                }
            }

            this.ProcesoIngreso.IdComunaOrigen = this.IdComunaOrigen;
            this.ProcesoIngreso.IdMonedaLiquidacion = this.LiquidarEnDolares ? 2 : 1;
            this.ProcesoIngreso.TotalNetoRecepcion = totalNeto;
            this.ProcesoIngreso.FechaCierre = DateTime.Now;
            this.ProcesoIngreso.UserCierre = userName;
            this.ProcesoIngreso.IpCierre = ipAddress;
            this.ProcesoIngreso.UserUpd = userName;
            this.ProcesoIngreso.FechaHoraUpd = DateTime.Now;
            this.ProcesoIngreso.IpUpd = ipAddress;

            dc.SubmitChanges();

            return this.ProcesoIngreso;
        }

        public class ItemContratoParaCierre
        {
            public int IdItemContrato { get; set; }
            
            public string Rut { get; set; }
            public string Nombre { get; set; }
            public string NumeroContrato { get; set; }
            public string CultivoContrato { get; set; }
            public string ComentariosContrato { get; set; }
            public bool TieneConvenioMoneda { get; set; }
            public int CantidadContrato { get; set; }
            public int CantidadDisponible { get; set; }
            public List<ConvenioParaCierre> Convenios { get; set; }

            public static ItemContratoParaCierre FromItemsContratosParaCierreInicial(AgroFichasDBDataContext dc, ItemsContratosParaCierreInicialResult result, TasaCambio tasaCambioSpot, vwPrecioSpot precioSpot, int idSucursal, ref int idConvenioPrecioTemp)
            {
                var item = new ItemContratoParaCierre()
                {
                    IdItemContrato = result.IdItemContrato,
                    Rut = result.Rut,
                    Nombre = result.Nombre,
                    NumeroContrato = result.NumeroContrato,
                    CultivoContrato = result.CultivoContrato,
                    CantidadContrato = result.CantidadContrato,
                    CantidadDisponible = result.CantidadDisponible ?? 0,
                    ComentariosContrato = result.ComentariosContrato ?? "",
                    TieneConvenioMoneda = result.TieneConvenioMoneda.Value
                };

                item.Convenios = (from r in dc.ConveniosParaCierreInicial(result.IdContrato, idSucursal)
                                  select ConvenioParaCierre.FromConveniosParaCierreResult(r, tasaCambioSpot, precioSpot)).ToList();

                var monedaDefault = Moneda.MonedaDefault(dc);
                for (int i = 1; i <= 2; i++)
                {
                    item.Convenios.Add(new ConvenioParaCierre()
                    {
                        IdConvenioPrecio = --idConvenioPrecioTemp,
                        IdConvenioPrecioItemTabla = -i,
                        IdMoneda = monedaDefault.IdMoneda,
                        SimboloMoneda = monedaDefault.Simbolo,
                        FormatoMoneda = monedaDefault.Formato,
                        //PrecioUnidad = monedaDefault.IdMoneda == 1 ? precioSpot.ValorCLP ?? 0 : precioSpot.ValorUSD ?? 0,
                        PrecioUnidadBase = monedaDefault.IdMoneda == 1 ? precioSpot.ValorCLP ?? 0 : precioSpot.ValorUSD ?? 0,
                        BonoUnidad = 0,
                        CantidadConvenio = 0,
                        CantidadDisponible = 0,
                        CantidadAsignadaIngreso = 0,
                        TasaCambio = monedaDefault.IdMoneda == 1 ? 1 : tasaCambioSpot.Valor
                    });
                }
                return item;
            }
        }

        public class ConvenioParaCierre
        {
            public int IdConvenioPrecio { get; set; }
            public int IdConvenioPrecioItemTabla { get; set; }
            public int IdMoneda { get; set; }
            public string SimboloMoneda { get; set; }
            public string FormatoMoneda { get; set; }
            //public decimal PrecioUnidad { get; set; }
            public decimal PrecioUnidadBase { get; set; }
            public decimal BonoUnidad { get; set; }
            public int BonoCantidad { get; set; }
            

            public bool EsPiso { get; set; }
            public int CantidadConvenio { get; set; }
            public int CantidadDisponible { get; set; }
            public int CantidadAsignadaIngreso { get; set; }

            public decimal TasaCambio { get; set; }
            public decimal SobrePrecioPor { get; set; }
            public decimal DescuentoPor { get; set; }

            
            public int TotalLinea(AgroFichasDBDataContext dc)
            {
                return (int) Math.Round(this.PrecioUnidadBase * this.TasaCambio * this.CantidadAsignadaIngreso + this.BonoTotal(dc) * this.TasaCambio +  (this.SobrePrecioTotal(dc) - this.DescuentoTotal(dc)) * this.TasaCambio);
            }

            public decimal SobrePrecioTotal(AgroFichasDBDataContext dc)
            {
                var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
                return Math.Round(this.PrecioUnidadBase * this.CantidadAsignadaIngreso * (this.SobrePrecioPor / 100M), moneda.Decimales);
            }

            public decimal DescuentoTotal(AgroFichasDBDataContext dc)
            {
                var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
                return Math.Round(this.PrecioUnidadBase * this.CantidadAsignadaIngreso * (this.DescuentoPor / 100M), moneda.Decimales);
            }

            public decimal BonoTotal(AgroFichasDBDataContext dc)
            {
                var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
                return Math.Round(this.BonoUnidad * this.BonoCantidad, moneda.Decimales);
            }
            public static ConvenioParaCierre FromConveniosParaCierreResult(ConveniosParaCierreInicialResult result, TasaCambio tasaCambioSpot, vwPrecioSpot precioSpot)
            {
                return new ConvenioParaCierre()
                {
                    IdConvenioPrecio = result.IdConvenioPrecio,
                    IdConvenioPrecioItemTabla = result.IdConvenioPrecioItemTabla,
                    IdMoneda = result.IdMoneda,
                    SimboloMoneda = result.SimboloMoneda,
                    FormatoMoneda = result.FormatoMoneda,
                    PrecioUnidadBase = result.PrecioUnidadBase,
                    BonoUnidad = result.BonoUnidad ?? 0,
                    CantidadConvenio = result.CantidadConvenio,
                    CantidadDisponible = result.CantidadDisponible.Value,
                    CantidadAsignadaIngreso = 0,
                    BonoCantidad = 0,
                    TasaCambio = result.IdMoneda == 1 ? 1 : tasaCambioSpot.Valor,
                    EsPiso = result.EsPiso,
                    SobrePrecioPor = 0,
                    DescuentoPor = 0
                };
            }
        }

    }
}