using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class RecepcionesExportModel
    {
        public int NumeroIngreso { get; set; }
        public string Estado { get; set; }
        public string Planta { get; set; }
        public DateTime Llegada { get; set; }
        public string Nombre { get; set; }
        public int Guia { get; set; }
        public string Cultivo { get; set; }
        public int? Neto { get; set; }
        public int? Standard { get; set; }

        public static RecepcionesExportModel FromProcesoIngreso(ProcesoIngreso pi)
        {
            return new RecepcionesExportModel()
            {
                NumeroIngreso = pi.IdProcesoIngreso,
                Estado = pi.EstadoProcesoIngreso.Nombre,
                Planta = pi.Sucursal.Nombre,
                Llegada = pi.FechaHoraLlegada.Value,
                Nombre = pi.Agricultor.Nombre,
                Guia = pi.NumeroGuia,
                Cultivo = pi.CultivoEmpresa,
                Neto = pi.PesoBruto,
                Standard = pi.PesoNormal
            };
        }
    }

    public class ContratosExportModel
    {
        public string Habilitado { get; set; }
        public string NumeroContrato { get; set; }
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public string Empresa { get; set; }
        public string Cultivo { get; set; }
        public int Kg { get; set; }
        public decimal PrecioUnidad { get; set; }
        public string Tipo { get; set; }

        public static ContratosExportModel FromContrato(Contrato contrato)
        {
            return new ContratosExportModel()
            {
                Habilitado = contrato.Habilitado ? "Sí" : "No",
                NumeroContrato = contrato.NumeroContrato,
                Rut = contrato.Agricultor.Rut,
                Nombre = contrato.Agricultor.Nombre,
                Empresa = contrato.Empresa.Nombre,
                Cultivo = contrato.DescripcionCultivos(" "),
                Kg = contrato.ItemContrato.Sum(ic => ic.Cantidad),
                PrecioUnidad = contrato.ConvenioPrecio.Sum(cp => cp.PrecioUnidad),
                Tipo = contrato.TipoContrato?.Descripcion
            };
        }
    }

    public class ConveniosPrecioExportModel
    {
        public string Habilitado { get; set; }
        public int ID { get; set; }
        public string NumeroContrato { get; set; }
        public string Empresa { get; set; }
        public string Cultivo { get; set; }
        public string Nombre { get; set; }
        public int Kg { get; set; }
        public string Moneda { get; set; }
        public decimal Precio { get; set; }
        public string Piso { get; set; }

        public static ConveniosPrecioExportModel FromConvenioPrecio(ConvenioPrecio cp)
        {
            return new ConveniosPrecioExportModel()
            {
                Habilitado = cp.Habilitado ? "Sí" : "No",
                ID = cp.IdConvenioPrecio,
                NumeroContrato = cp.Contrato.NumeroContrato,
                Empresa = cp.Contrato.Empresa.Nombre,
                Nombre = cp.Contrato.Agricultor.Nombre,
                Cultivo = cp.Contrato.DescripcionCultivos(" "),
                Kg = cp.Cantidad,
                Moneda = cp.Moneda.Simbolo,
                Precio = cp.PrecioUnidad,
                Piso = cp.EsPiso ? "Sí" : "No"
            };
        }

    }

    public class ConveniosMonedaExportModel
    {
        public string Habilitado { get; set; }
        public int ID { get; set; }
        public string NumeroContrato { get; set; }
        public string Empresa { get; set; }
        public string Cultivo { get; set; }
        public string Nombre { get; set; }
        public string Moneda { get; set; }
        public decimal Cantidad { get; set; }
        public decimal TasaCambio { get; set; }
        public string MonedaCambio { get; set; }

        public static ConveniosMonedaExportModel FromConvenioCambio(ConvenioCambioMoneda cm)
        {
            return new ConveniosMonedaExportModel()
            {
                Habilitado = cm.Habilitado ? "Sí" : "No",
                ID = cm.IdConvenioCambioMoneda,
                NumeroContrato = cm.Contrato.NumeroContrato,
                Empresa = cm.Contrato.Empresa.Nombre,
                Nombre = cm.Contrato.Agricultor.Nombre,
                Cultivo = cm.Contrato.DescripcionCultivos(" "),
                Moneda = cm.Moneda1.Simbolo,
                Cantidad = cm.Cantidad,
                TasaCambio = cm.PrecioUnidad,
                MonedaCambio = cm.Moneda.Simbolo + "/" + cm.Moneda1.Simbolo
            };
        }

    }

    public class DescuentosExportModel
    {
        public int Numero { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public string Moneda { get; set; }
        public decimal Cantidad { get; set; }
        public string Institucion { get; set; }
        public DateTime Vencimiento { get; set; }

        public static DescuentosExportModel FromDescuento(Descuento des)
        {
            return new DescuentosExportModel()
            {
                Numero = des.IdDescuento,
                Tipo = des.TipoDescuento.Nombre,
                Nombre = des.Agricultor.Nombre,
                Moneda = des.Moneda.Simbolo,
                Cantidad = des.Monto,
                Institucion = des.Institucion,
                Vencimiento = des.FechaVencimiento
            };
        }
    }

    public class SaldosCtaCteExportModel
    {
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public string Empresa { get; set; }
        public int CuentaCorriente { get; set; }
        public int Documentado { get; set; }
        public int Total { get; set; }

        public static SaldosCtaCteExportModel FromSaldoCtaCte(SaldoCtaCte saldo)
        {
            return new SaldosCtaCteExportModel()
            {
                Rut = saldo.Agricultor.Rut,
                Nombre = saldo.Agricultor.Nombre,
                Empresa = saldo.Empresa.Nombre,
                CuentaCorriente = saldo.MontoCtaCte,
                Documentado = saldo.MontoDocumentado,
                Total = saldo.MontoTotal.Value
            };
        }
    }

    public class LiquidacionesExportModel
    {
        public string Estado { get; set; }
        public string Retenida { get; set; }
        public string AutIngresos { get; set; }
        public string AutDescuentos { get; set; }
        public DateTime Fecha { get; set; }
        public string Nombre { get; set; }
        public int Numero { get; set; }
        public string Empresa { get; set; }
        public decimal Ingresos { get; set; }
        public decimal? Descuentos { get; set; }
        public decimal? Saldo { get; set; }
        public decimal? TotalKgStd { get; set; }
        public string Facturas { get; set; }
        public string FacturasReliquidacion { get; set; }
        public string CuentaBancaria { get; set; }
        
        public static LiquidacionesExportModel FromLiquidacion(Liquidacion liq)
        {
            string Facturas = "";
            foreach (var doctos in liq.Doctos())
            {
                Facturas += doctos.Numero.ToString() + ",";
            }

            if (!string.IsNullOrEmpty(Facturas))
                Facturas = Facturas.Remove(Facturas.Length - 1);

            string FacturasReliquidacion = "";
            foreach (var doctos in liq.DoctosReliquidacion())
            {
                FacturasReliquidacion += doctos.Numero.ToString() + ",";
            }

            if (!string.IsNullOrEmpty(FacturasReliquidacion))
                FacturasReliquidacion = FacturasReliquidacion.Remove(FacturasReliquidacion.Length - 1);

            string CuentaBancaria = "";
            foreach (var cuentasbancarias in liq.CuentasBancarias())
            {
                CuentaBancaria += String.Format("{0} {1} {2}", cuentasbancarias.CuentaBancaria.NumeroCuenta, cuentasbancarias.CuentaBancaria.TipoCuentaBancaria.Nombre, cuentasbancarias.CuentaBancaria.Banco.Nombre) + ",";
            }

            if (!string.IsNullOrEmpty(CuentaBancaria))
                CuentaBancaria = CuentaBancaria.Remove(CuentaBancaria.Length - 1);

            return new LiquidacionesExportModel()
            {
                Estado = liq.EstadoLiquidacion.Nombre,
                Retenida = liq.Retenida ? "Sí: " + liq.TextoRetenida : "No",
                AutIngresos = liq.AutorizadaIngresos != null ? (liq.AutorizadaIngresos.Value ? "Autorizado" : "Rechazado") : "",
                AutDescuentos = liq.AutorizadaDescuentos != null ? (liq.AutorizadaDescuentos.Value ? "Autorizado" : "Rechazado") : "",
                Fecha = liq.FechaHoraCreacion.Value,
                Numero = liq.IdLiquidacion,
                Nombre = liq.Agricultor.Nombre,
                Empresa = liq.Empresa.Nombre,
                Ingresos = liq.TotalPagar,
                Descuentos = liq.TotalDescuentos,
                Saldo = liq.Saldo,
                TotalKgStd = liq.TotalKgStd,
                Facturas = Facturas,
                FacturasReliquidacion = FacturasReliquidacion,
                CuentaBancaria = CuentaBancaria
            };
        }
   }

    public class NominasExportModel
    {
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public string FormaPago { get; set; }
        public int? CodigoBanco { get; set; }
        public string CuentaBancaria { get; set; }
        public int Factura { get; set; }
        public decimal? Saldo { get; set; }
        public string Tipo { get; set; }
        public string Glosa { get; set; }
        public string Email { get; set; }
        

        public static NominasExportModel FromDoctoLiquidacion(DoctoLiquidacion Doctoliq)
        {
            string Email = "";
            if (string.IsNullOrEmpty(Doctoliq.Liquidacion.Agricultor.Email))
                Email = "abastecimiento@empresasagrotop.cl";
            else
                Email = Doctoliq.Liquidacion.Agricultor.Email;

            return new NominasExportModel()
            {
                Rut = Doctoliq.Liquidacion.Agricultor.Rut,
                Nombre = Doctoliq.Liquidacion.Agricultor.Nombre,
                FormaPago = (Doctoliq.CuentaBancaria.Banco.IdBanco == 5 ?  "CCT":"OTC"),
                CodigoBanco = Doctoliq.CuentaBancaria.Banco.CodigoBCI,
                CuentaBancaria = Doctoliq.CuentaBancaria.NumeroCuenta,
                Factura = Doctoliq.Numero,
                Saldo = Doctoliq.Liquidacion.Saldo,
                Tipo = "FAC",
                Glosa = String.Format("F{0} L{1}", Doctoliq.Numero, Doctoliq.IdLiquidacion),
                Email = Email
            };
        }
    }

    public class MovimientosExportModel
    {
        public MovimientosExportModel() { }

        public int NumReq { get; set; }
        public string GlosaReq { get; set; }
        public int NumPed { get; set; }
        public int NumGuia { get; set; }
        public string Producto { get; set; }
        public string Transportista { get; set; }
        public string Estado { get; set; }
        public string Salida { get; set; }
        public string Destino { get; set; }
        public decimal KgsSalida { get; set; }
        public decimal KgsLlegada { get; set; }
        public decimal Tolerancia { get; set; }
        public decimal Diferencia { get; set; }
        public decimal Merma { get; set; }
        public decimal Precio { get; set; }
        public decimal Neto { get; set; }
        public string FechaSalida { get; set; }
        public string FechaLlegada { get; set; }
    }

    public class IntencionSiembraExportModel
    {
        public string Temporada { get; set; }
        public string Nombre { get; set; }
        public string Cultivo { get; set; }
        public string Comuna { get; set; }
        public string Provincia { get; set; }
        public string Region { get; set; }
        public string PuntoEntrega { get; set; }
        public int Hectareas { get; set; }
        public int Kilogramos { get; set; }

        public static IntencionSiembraExportModel FromIntencionSiembra(IntencionSiembra intencionSiembra)
        {
            return new IntencionSiembraExportModel()
            {
                Temporada = intencionSiembra.Temporada.Nombre,
                Nombre = intencionSiembra.Agricultor.Nombre,
                Cultivo = intencionSiembra.Cultivo.Nombre,
                Comuna = intencionSiembra.Comuna.Nombre,
                Provincia = intencionSiembra.Comuna.Provincia.Nombre,
                Region = intencionSiembra.Comuna.Provincia.Region.Nombre,
                PuntoEntrega = intencionSiembra.PuntoEntrega,
                Hectareas = intencionSiembra.Superficie,
                Kilogramos = intencionSiembra.Cantidad
            };
        }
    }
} 