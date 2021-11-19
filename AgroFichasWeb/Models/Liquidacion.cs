using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Liquidacion
    {
        AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

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

        public string TextoRetenida
        {
            get
            {
                return this.Retenida ? "Retenida" : "No Retenida";
            }

        }

        public string ColorRetenida
        {
            get
            {
                return this.ColorAutorizacion(!this.Retenida);
            }
        }

        public bool EsAnulable()
        {
            int[] estados = { 1, 2, 3, 4, 5, 6 };
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

        public bool PuedeRecibirFacturas()
        {
            return (this.IdEstado == 3 || this.IdEstado == 5) && !this.Retenida;
        }
        public bool PuedeRecibirFacturasReliquidadas()
        {
            return (this.IdEstado == 6) && !this.Retenida;
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

        public decimal SaldoPorFacturar
        {
            get
            {
                return this.TotalPagar - this.DoctoLiquidacion.Sum(i => i.TotalPagar);
            }
        }

        public List<Precio> TotalesMonedaPago
        {
            get
            {
                var dc = new AgroFichasDBDataContext();
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

        public List<Precio> TotalesMonedaPagoNulo
        {
            get
            {
                var dc = new AgroFichasDBDataContext();
                var totales = new List<Precio>();

                foreach (var precio in this.PrecioIngresoNulo)
                {
                    var totalMoneda = totales.SingleOrDefault(t => t.IdMoneda == precio.IdMonedaPago);
                    if (totalMoneda == null)
                    {
                        var moneda = dc.Moneda.Single(m => m.IdMoneda == precio.IdMonedaPago);
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
                    totalMoneda.Valor += precio.TotalMonedaPago;

                }

                return totales;
            }
        }

        public bool Seleccionado { get; set; }

        public EstadoLiquidacion EstadoLiquidacion
        {
            get
            {
                return dataContext.EstadoLiquidacion.Single(X => X.IdEstado == this.IdEstado);
            }
        }

        public bool Facturada
        {
            get
            {
                return this.SaldoPorFacturar == 0;
            }
        }

        public DateTime FechaHoraFacturada
        {
            get
            {
                var Doctos = this.Doctos();
                if (Doctos.Count == 1)
                    return Doctos.First().FechaHoraIns;
                else if (Doctos.Count > 1)
                    return Doctos.OrderByDescending(X => X.FechaHoraIns).First().FechaHoraIns;
                else
                    return DateTime.Now;
            }
        }

        public string UserFacturada
        {
            get
            {
                var Doctos = this.Doctos();
                if (Doctos.Count == 1)
                    return Doctos.First().UserIns;
                else if (Doctos.Count > 1)
                    return Doctos.OrderByDescending(X => X.FechaHoraIns).First().UserIns;
                else
                    return "";
            }
        }

        public int TotalKgStd
        {
            get
            {
                var totalKg = 0;
                foreach (var precio in this.PrecioIngreso.OrderBy(pi => pi.FechaHoraIns).ThenBy(X => X.IdProcesoIngreso))
                {
                    totalKg += precio.Cantidad;
                }

                foreach (var precio in this.PrecioIngresoNulo.OrderBy(pi => pi.FechaHoraLlegada).ThenBy(X => X.IdProcesoIngreso))
                {
                    totalKg += precio.Cantidad;
                }

                return totalKg;
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
            propuesta.DescuentosAsignados = this.TotalDescuentos.HasValue && this.AutorizadaDescuentos.HasValue && this.AutorizadaDescuentos.Value == true;
            foreach (var descuento in this.DescuentoLiquidacion.Where(d => d.Descuento.IdTipoDescuento == 3 || d.Descuento.IdTipoDescuento == 4))
            {
                var peso = descuento.Descuento.Cantidad.Value;
                var neto = descuento.Descuento.TotalNeto.Value;
                if (descuento.Monto != descuento.Descuento.Monto)
                {
                    peso = (int)Math.Round(peso * descuento.Monto / descuento.Descuento.Monto);
                    neto = (int)Math.Round(neto * descuento.Monto / descuento.Descuento.Monto, descuento.Descuento.Moneda.Decimales);
                }
                var item = new ItemPropuestaFacturacion()
                {
                    Nombre = descuento.Descuento.TipoDescuento.Nombre,
                    NumeroDocumento = descuento.Descuento.NumeroDocumento ?? 0,
                    Peso = -peso,
                    PrecioUnidad = -descuento.Descuento.PrecioUnidad.Value,
                    Neto = -neto
                };
                propuesta.Items.Add(item);
            }

            propuesta.Calcular();
            return propuesta;
        }

        public SaldoDoctos SaldoPendienteDoctos(int? idDoctoLiquidacion)
        {
            var saldo = new SaldoDoctos();
            var doctos = this.DoctoLiquidacion.Where(d => d.IdDoctoLiquidacion != (idDoctoLiquidacion ?? 0));

            saldo.TotalNeto = this.TotalNeto - doctos.Sum(d => d.TotalNeto * d.TipoDoctoLiquidacion.FactorLibro);
            saldo.TotalIva = this.TotalIva - doctos.Sum(d => d.TotalIva * d.TipoDoctoLiquidacion.FactorLibro);
            saldo.TotalIvaRetenido = this.TotalIvaRetenido - doctos.Sum(d => d.TotalIvaRetenido * d.TipoDoctoLiquidacion.FactorLibro);
            saldo.TotalPagar = this.TotalPagar - doctos.Sum(d => d.TotalPagar * d.TipoDoctoLiquidacion.FactorLibro);

            return saldo;
        }

        public List<DoctoLiquidacion> Doctos()
        {
            return dataContext.DoctoLiquidacion.Where(X => X.IdLiquidacion == this.IdLiquidacion && X.IdTipoDoctoLiquidacion == 1).ToList();
        }

        public List<DoctoLiquidacion> DoctosReliquidacion()
        {
            return dataContext.DoctoLiquidacion.Where(X => X.IdLiquidacion == this.IdLiquidacion && X.IdTipoDoctoLiquidacion == 2).ToList();
        }
        public List<DoctoLiquidacion> CuentasBancarias()
        {
            return (from O1 in dataContext.DoctoLiquidacion
                    join O2 in dataContext.CuentaBancaria on O1.IdCuentaBancaria equals O2.IdCuentaBancaria
                    join O3 in dataContext.Banco on O2.IdBanco equals O3.IdBanco
                    join O4 in dataContext.TipoCuentaBancaria on O2.IdTipoCuentaBancaria equals O4.IdTipoCuentaBancaria
                    where O1.IdLiquidacion == this.IdLiquidacion
                    && O1.IdTipoDoctoLiquidacion == 1
                    select O1).ToList();
        }
        #region Notifica Liquidación Anulada

        public void NotificaLiquidacionAnulada()
        {
            try
            {
                string destinatarios = "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionLiquidacionAnulada().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificaliquidacionanulada_template.html"), Encoding.UTF8);
                RepTemp(ref baseTemplate, "LIQUIDACION", this.IdLiquidacion.ToString());
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

                FileStream fileStream = new FileStream(HttpContext.Current.Server.MapPath(string.Format("~/App_Data/pdfs/liquidaciones/liquidaciones/anulacion/maillog_{0}.html", System.Guid.NewGuid().ToString())), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(baseTemplate); writer.Close();

                string subject = String.Format("La liquidación #{0} ha sido anulada", this.IdLiquidacion);

                SendMailFichas(destinatarios, baseTemplate, subject);
            }
            catch (Exception ex)
            {
                FileStream fileStream = new FileStream(HttpContext.Current.Server.MapPath(string.Format("~/App_Data/pdfs/liquidaciones/liquidaciones/anulacion/errorlog/errorlog_{0}.TXT", System.Guid.NewGuid().ToString())), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(ex.ToString()); writer.Close();
            }
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        private void SendMailFichas(string destinatarios, string body, string subject)
        {
            MailMessage objMM = new MailMessage();

            objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
            objMM.To.Add(destinatarios);
            objMM.Subject = subject;
            objMM.IsBodyHtml = true;

            objMM.Body = body;

            var objSmtp = new SmtpClient();
            objSmtp.Send(objMM);
        }

        #endregion
    }

    public class SaldoDoctos
    {
        public decimal TotalNeto { get; set; }
        public decimal TotalIva { get; set; }
        public decimal TotalIvaRetenido { get; set; }
        public decimal TotalPagar { get; set; }
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
            this.PesoTotal = this.Items.Select(x => x.Peso).Sum();
            this.NetoTotal = this.Items.Select(x => x.Neto).Sum();

            if (this.PesoTotal != 0)
                this.PrecioUnidadTotal = Math.Round(this.NetoTotal / ((decimal)this.PesoTotal), 2);

            this.NetoFacturado = Math.Round(this.PrecioUnidadTotal * this.PesoTotal, 0);

            int i = 0;
            while (this.NetoFacturado > this.NetoTotal && i <= 1000)
            {
                this.PrecioUnidadTotal -= 0.01M;
                this.NetoFacturado = Math.Round(this.PrecioUnidadTotal * this.PesoTotal, 0);
                i++;
            }

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