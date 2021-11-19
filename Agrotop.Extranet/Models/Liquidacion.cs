using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public partial class Liquidacion
    {
        public static int LIQUIDACION_AUTORIZADA = 1;
        public static int LIQUIDACION_ENREVISION = 2;
        public static int LIQUIDACION_RECHAZADA = 0;

        public int EstadoAutorizacion
        {
            get
            {
                if (this.AutorizadaDescuentos.HasValue &&
                    this.AutorizadaIngresos.HasValue)
                {
                    if (this.AutorizadaIngresos.Value && this.AutorizadaDescuentos.Value)
                        return LIQUIDACION_AUTORIZADA;
                    else if (!this.AutorizadaIngresos.Value && !this.AutorizadaDescuentos.Value)
                        return LIQUIDACION_RECHAZADA;
                    else
                        return LIQUIDACION_ENREVISION;
                }
                else
                {
                    return LIQUIDACION_ENREVISION;
                }
            }
        }

        public string ColorAutorizacionIngresos
        {
            get
            {
                return ColorAutorizacion(this.AutorizadaIngresos);
            }
        }

        public string ColorAutorizacionDescuentos
        {
            get
            {
                return ColorAutorizacion(this.AutorizadaDescuentos);
            }
        }

        public string ColorAutorizacion(bool? value)
        {
            if (value.HasValue)
            {
                return value.Value ? "#83f03c" : "#ff8e73";
            }
            else
            {
                return "#FFE640";
            }
        }

        public string TextoAutorizacionIngresos
        {
            get
            {
                return TextoAutorizacion(this.AutorizadaIngresos);
            }
        }

        public string TextoAutorizacionDescuentos
        {
            get
            {
                return TextoAutorizacion(this.AutorizadaDescuentos);
            }
        }

        public string TextoAutorizacion(bool? value)
        {
            if (value.HasValue)
            {
                return value.Value ? "Autorizado" : "Rechazado";
            }
            else
            {
                return "Pendiente";
            }
        }

        public bool EsAnulable()
        {
            int[] estados = { 1, 2, 3, 4 };
            return estados.Contains(this.IdEstado);
        }

        public bool EsIngresosEditable()
        {
            int[] estados = { 1, 2, 4 };
            return estados.Contains(this.IdEstado) && !(this.AutorizadaIngresos.HasValue && this.AutorizadaIngresos.Value == true);
        }

        public bool EsDescuentosAnulable()
        {
            int[] estados = { 2, 3, 4 };
            return estados.Contains(this.IdEstado);
        }

        public decimal? Saldo
        {
            get
            {
                if (this.TotalDescuentos.HasValue)
                    return this.TotalPagar - this.TotalDescuentos.Value;

                return null;
            }
        }

        public List<Precio> TotalesMonedaPago
        {
            get
            {
                var dc = new AgrotopDBDataContext();
                var totales = new List<Precio>();

                foreach (var precio in this.PrecioIngreso)
                {
                    var totalMoneda = totales.SingleOrDefault(t => t.IdMoneda == precio.IdMonedaPago());
                    if (totalMoneda == null)
                    {
                        var moneda = dc.Moneda.Single(m => m.IdMoneda == precio.IdMonedaPago());
                        totalMoneda = new Precio()
                        {
                            IdMoneda = moneda.IdMoneda,
                            FormatoMoneda = moneda.Formato,
                            NombreMoneda = moneda.Nombre,
                            SimboloMoneda = moneda.Simbolo,
                            Valor = 0
                        };
                        totales.Add(totalMoneda);
                    }
                    totalMoneda.Valor += precio.TotalMonedaPago();

                }

                return totales;
            }
        }

        public int PesoBrutoIngresos()
        {
            var result = 0;
            var idsIngresos = new List<int>();
            foreach (var precio in this.PrecioIngreso)
            {
                if (!idsIngresos.Contains(precio.IdProcesoIngreso))
                {
                    result += precio.ProcesoIngreso.PesoBruto ?? 0;
                    idsIngresos.Add(precio.IdProcesoIngreso);
                }
            }
            return result;
        }

        public PropuestaFacturacion PropuestaFacturacion()
        {
            var propuesta = new PropuestaFacturacion();

            propuesta.Items = new List<ItemPropuestaFacturacion>();

            //Ingresos
            var ingresos = new ItemPropuestaFacturacion()
            {
                Nombre = "Ingresos",
                NumeroDocumento = 0,
                Peso = 0,
                PrecioUnidad = 0,
                Neto = 0
            };

            foreach (var precio in this.PrecioIngreso)
            {
                ingresos.Peso += precio.Cantidad;
                ingresos.Neto += precio.TotalNeto ?? 0;
            }

            if (ingresos.Peso != 0)
                ingresos.PrecioUnidad = Math.Round(ingresos.Neto / ((decimal)ingresos.Peso), 4);

            propuesta.Items.Add(ingresos);

            //Descuentos
            propuesta.DescuentosAsignados = false; // this.TotalDescuentos.HasValue;
            //foreach (var descuento in this.DescuentoLiquidacion.Where(d => d.Descuento.IdTipoDescuento == 3 || d.Descuento.IdTipoDescuento == 4))
            //{
            //    var item = new ItemPropuestaFacturacion()
            //    {
            //        Nombre = descuento.Descuento.TipoDescuento.Nombre,
            //        NumeroDocumento = descuento.Descuento.NumeroDocumento ?? 0,
            //        Peso = 0,
            //        PrecioUnidad = 0,
            //        Neto = descuento.Monto
            //    };

            //}

            propuesta.Calcular();
            return propuesta;
        }
    }

    public class PropuestaFacturacion
    {
        public List<ItemPropuestaFacturacion> Items { get; set; }
        public int PesoTotal { get; private set; }
        public decimal PrecioUnidadTotal { get; private set; }
        public decimal NetoTotal { get; private set; }
        public decimal NetoFacturado { get; private set; }
        public decimal AjusteTotal { get; private set; }
        public bool DescuentosAsignados { get; set; }

        public void Calcular()
        {
            this.PesoTotal = this.Items.Select(i => i.Peso).Sum();
            this.NetoTotal = this.Items.Select(i => i.Neto).Sum();

            if (this.PesoTotal != 0)
                this.PrecioUnidadTotal = Math.Round(this.NetoTotal / ((decimal)this.PesoTotal), 2);

            this.NetoFacturado = Math.Round(this.PrecioUnidadTotal * this.PesoTotal, 0);

            this.AjusteTotal = this.NetoTotal - this.NetoFacturado;
        }
    }

    public class ItemPropuestaFacturacion
    {
        public string Nombre { get; set; }
        public int NumeroDocumento { get; set; }
        public int Peso { get; set; }
        public decimal PrecioUnidad { get; set; }
        public decimal Neto { get; set; }

    }
}