using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using static AgroFichasWeb.ViewModels.Recepciones.CerrarViewModel;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class IntencionContratoPrecioIngresosViewModel
    {
        public List<rpt_IntencionContratoPrecioIngresosResult> Items;
        public List<IngresoValorizadoViewModel> IngresosValorizados { get; set; }
        private List<ContratoParaValorizacion> Contratos { get; set; }
        private List<ConvenioPrecioParaValorizacion> Convenios { get; set; }
        private string LogFile { get; set; }
        private int IdTemporada { get; set; }
        private int IdCultivo { get; set; }

        public IntencionContratoPrecioIngresosViewModel(AgroFichasDBDataContext dc, int idTemporada, int idCultivo, string userName)
        {
            this.IdTemporada = idTemporada;
            this.IdCultivo = idCultivo;

            var logDir = HttpContext.Current.Server.MapPath("~/App_Data/ICPC");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            this.LogFile = Path.Combine(logDir, DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + "_" + userName + ".txt");

            //
            this.Items = dc.rpt_IntencionContratoPrecioIngresos(idTemporada, idCultivo).ToList();


            //
            var ingresos = dc.ProcesoIngreso.Where(t =>
                t.IdTemporada == idTemporada && 
                t.CultivoContrato.IdCultivo == idCultivo &&
                t.Nulo == 0 &&
                t.PesoFinal.HasValue /*&&
                t.IdAgricultor == 3245*/);

            SetUpConvenios(dc);

            this.IngresosValorizados = new List<IngresoValorizadoViewModel>();
            foreach (var ingreso in ingresos.OrderBy(i => i.IdProcesoIngreso))
            {

                DoLogSep();
                DoLog($"Procesando Ingreso: {ingreso.IdProcesoIngreso} {ingreso.Agricultor.Nombre} ({ingreso.Agricultor.IdAgricultor}). {ingreso.Sucursal.Nombre} - {ingreso.Comuna.Provincia.Nombre} - {ingreso.Comuna.Nombre}");
                var neto = ingreso.ValorizarSinSpot(out decimal decimalPrecioCLP);
                IngresoValorizadoViewModel result;
                if (neto.HasValue)
                {
                    result = new IngresoValorizadoViewModel()
                    {
                        IdProcesoIngreso = ingreso.IdProcesoIngreso,
                        Cantidad = ingreso.PesoNormal ?? 0,
                        Neto = neto.Value,
                        OrigenValor = "Cierre de Ingreso"
                    };
                }
                else
                {
                    result = this.ValorizarIngreso(dc, ingreso);
                }

                var comunaOrigen = ingreso.ComunaOrigen(dc);
                result.SetFecha(ingreso.FechaHoraLlegada.Value);
                result.IdSucursal = ingreso.IdSucursal;
                result.IdAgricultor = ingreso.IdAgricultor;
                result.IdComuna = comunaOrigen.IdComuna;
                result.IdProvincia = comunaOrigen.IdProvincia;

                this.IngresosValorizados.Add(result);
                DoLog(result.ToString());
            }
        }

        private void SetUpConvenios(AgroFichasDBDataContext dc)
        {
            this.Contratos = (from c in dc.Contrato
                              join ic in dc.ItemContrato on c.IdContrato equals ic.IdContrato
                              join cc in dc.CultivoContrato on ic.IdCultivoContrato equals cc.IdCultivoContrato
                              where c.IdTemporada == this.IdTemporada
                                 && cc.IdCultivo == this.IdCultivo
                              select new ContratoParaValorizacion()
                              {
                                  IdContrato = c.IdContrato
                              }).ToList();

            this.Convenios = new List<ConvenioPrecioParaValorizacion>();
            foreach (var contrato in this.Contratos)
            {
                foreach (var cc in dc.ConveniosParaCierreInicial(contrato.IdContrato, null))
                {
                    if (this.Convenios.SingleOrDefault(x => x.IdConvenioPrecioItemTabla == cc.IdConvenioPrecioItemTabla) == null)
                    {
                        this.Convenios.Add(new ConvenioPrecioParaValorizacion()
                        {
                            CantidadConvenio = cc.CantidadConvenio,
                            CantidadDisponible = cc.CantidadDisponible ?? 0,
                            EsPiso = cc.EsPiso,
                            FormatoMoneda = cc.FormatoMoneda,
                            IdContrato = cc.IdContrato,
                            IdConvenioPrecio = cc.IdConvenioPrecio,
                            IdConvenioPrecioItemTabla = cc.IdConvenioPrecioItemTabla,
                            IdMoneda = cc.IdMoneda,
                            IdSucursal = cc.IdSucursal,
                            PrecioUnidad = cc.PrecioUnidad,
                            SimboloMoneda = cc.SimboloMoneda
                        });
                    }
                }
            }
        }

        private IngresoValorizadoViewModel ValorizarIngreso(AgroFichasDBDataContext dc, ProcesoIngreso ingreso)
        {
            var valorizacion = new IngresoValorizadoViewModel()
            {
                IdProcesoIngreso = ingreso.IdProcesoIngreso,
                OrigenValor = "",
                Cantidad = 0,
                Neto = 0
            };
            
            var tasaCambioDolarSpot = TasaCambio.GetTasaCambio(dc, ingreso.FechaHoraLlegada.Value.Date, 2);
            var precioSpot = PrecioSpot.GetPrecioSpot(dc, ingreso.FechaHoraLlegada.Value.Date, ingreso.CultivoContrato.IdCultivo, ingreso.IdSucursal);
            int idConvenioPrecioTemp = 0;

            var itemsContratosParaCierre = (from result in dc.ItemsContratosParaCierreInicial(ingreso.IdProcesoIngreso)
                                            select this.FromItemsContratosParaCierreInicial(dc, result, tasaCambioDolarSpot, precioSpot, ingreso.IdSucursal, ref idConvenioPrecioTemp)).ToList();

            var porrepartir = (int)ingreso.PesoNormal.Value;
            foreach (var ic in itemsContratosParaCierre)
            {
                foreach (var convenio in ic.Convenios)
                {
                    var asignadoIngreso = (convenio.CantidadDisponible >= porrepartir) ? porrepartir : convenio.CantidadDisponible;

                    //TODO: Agregar BonoUnidad a calculos
                    valorizacion.Cantidad += asignadoIngreso;
                    valorizacion.Neto += asignadoIngreso * convenio.PrecioUnidadBase * convenio.TasaCambio;
                    valorizacion.OrigenValor += $"Convenio {convenio.IdConvenioPrecio} => {asignadoIngreso} Kg x {convenio.PrecioUnidadBase} {convenio.SimboloMoneda} x {convenio.TasaCambio} = {asignadoIngreso * convenio.PrecioUnidadBase * convenio.TasaCambio}. QtyConvenio: {convenio.CantidadDisponible} => {convenio.CantidadDisponible - asignadoIngreso}.. ";

                    convenio.CantidadDisponible -= asignadoIngreso;
                    this.Convenios.Single(c => c.IdConvenioPrecioItemTabla == convenio.IdConvenioPrecioItemTabla).CantidadDisponible -= asignadoIngreso;

                    porrepartir -= asignadoIngreso;
                }
            }

            if (porrepartir > 0)
            {
                if (precioSpot != null && precioSpot.ValorCLP != null && precioSpot.ValorCLP.Value > 0)
                {
                    var asignadoIngreso = porrepartir;

                    valorizacion.Cantidad += asignadoIngreso;
                    valorizacion.Neto += asignadoIngreso * precioSpot.ValorCLP.Value;
                    valorizacion.OrigenValor += $"SPOT {precioSpot.Fecha.ToString("dd/MM/yy")} => {asignadoIngreso} Kg x {precioSpot.ValorCLP.Value} CLP = {asignadoIngreso * precioSpot.ValorCLP.Value}.. ";

                    porrepartir -= asignadoIngreso;
                }
                else
                {
                    valorizacion.OrigenValor += "No se encontró precio SPOT.. ";
                }
            }

            valorizacion.OrigenValor += $"Pendiente por Asignar: {porrepartir}.. ";

            return valorizacion;
        }


        private ItemContratoParaCierre FromItemsContratosParaCierreInicial(AgroFichasDBDataContext dc, ItemsContratosParaCierreInicialResult result, TasaCambio tasaCambioSpot, vwPrecioSpot precioSpot, int idSucursal, ref int idConvenioPrecioTemp)
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

            item.Convenios = (from c in this.Convenios
                              where c.IdContrato == result.IdContrato
                                 && c.IdSucursal == idSucursal
                              select new ConvenioParaCierre()
                              {
                                  IdConvenioPrecio = c.IdConvenioPrecio,
                                  IdConvenioPrecioItemTabla = c.IdConvenioPrecioItemTabla,
                                  IdMoneda = c.IdMoneda,
                                  SimboloMoneda = c.SimboloMoneda,
                                  FormatoMoneda = c.FormatoMoneda,
                                  PrecioUnidadBase = c.PrecioUnidad, //TODO:Check el uso de PrecioUnidad
                                  CantidadConvenio = c.CantidadConvenio,
                                  CantidadDisponible = c.CantidadDisponible,
                                  CantidadAsignadaIngreso = 0,
                                  TasaCambio = c.IdMoneda == 1 ? 1 : tasaCambioSpot.Valor,
                                  EsPiso = c.EsPiso,
                                  SobrePrecioPor = 0,
                                  DescuentoPor = 0
                              }).ToList();

            //item.Convenios = (from r in dc.ConveniosParaCierreInicial(result.IdContrato, idSucursal)
            //                  select ConvenioParaCierre.FromConveniosParaCierreResult(r, tasaCambioSpot, precioSpot)).ToList();

            //var monedaDefault = Moneda.MonedaDefault(dc);
            //for (int i = 1; i <= 2; i++)
            //{
            //    item.Convenios.Add(new ConvenioParaCierre()
            //    {
            //        IdConvenioPrecio = --idConvenioPrecioTemp,
            //        IdConvenioPrecioItemTabla = -i,
            //        IdMoneda = monedaDefault.IdMoneda,
            //        SimboloMoneda = monedaDefault.Simbolo,
            //        FormatoMoneda = monedaDefault.Formato,
            //        PrecioUnidad = monedaDefault.IdMoneda == 1 ? precioSpot.ValorCLP ?? 0 : precioSpot.ValorUSD ?? 0,
            //        CantidadConvenio = 0,
            //        CantidadDisponible = 0,
            //        CantidadAsignadaIngreso = 0,
            //        TasaCambio = monedaDefault.IdMoneda == 1 ? 1 : tasaCambioSpot.Valor
            //    });
            //}
            return item;
        }

        private void DoLog(string message)
        {
            File.AppendAllText(this.LogFile, DateTime.Now.ToString("hh:mm:ss ") +  message + "\n");
        }

        private void DoLogSep()
        {
            DoLog($"============================================================================================");
        }
    }

    public class IngresoValorizadoViewModel
    {
        public int IdProcesoIngreso { get; set; }
        public int IdSucursal { get; set; }
        public int IdComuna { get; set; }
        public int IdProvincia { get; set; }
        public int IdAgricultor { get; set; }
        public int Cantidad { get; set; }
        public decimal Neto { get; set; }

        private DateTime fecha;

        public DateTime GetFecha()
        {
            return fecha;
        }

        public void SetFecha(DateTime value)
        {
            fecha = value;
            this.Ano = fecha.Year;
            this.Mes = fecha.Month;

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            var cal = dfi.Calendar;
            this.Semana = cal.GetWeekOfYear(fecha, dfi.CalendarWeekRule, DayOfWeek.Monday);
        }

        public int Ano { get; private set; }
        public int Mes { get; private set; }
        public int Semana { get; private set; }

        public string OrigenValor { get; set; }

        public override string ToString()
        {
            return $"{IdProcesoIngreso} => {Cantidad} Kg, ${Neto} Neto.\nOrigen Valor:\n{OrigenValor.Replace(".. ", "\n")}";
        }
    }

    public class ContratoParaValorizacion
    {
        public int IdContrato { get; set; }
        public int Disponible { get; set; }
    }

    public class ConvenioPrecioParaValorizacion
    {
        public int IdConvenioPrecio { get; set; }
        public int IdContrato { get; set; }
        public int IdConvenioPrecioItemTabla { get; set; }
        public int IdSucursal { get; set; }
        public int CantidadConvenio { get; set; }
        public decimal PrecioUnidad { get; set; }
        public int CantidadDisponible { get; set; }
        public int IdMoneda { get; set; }
        public string SimboloMoneda { get; set; }
        public string FormatoMoneda { get; set; }
        public bool EsPiso { get; set; }
    }
}