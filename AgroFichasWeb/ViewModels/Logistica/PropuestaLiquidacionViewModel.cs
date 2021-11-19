using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace AgroFichasWeb.ViewModels.Logistica
{
    public class PropuestaLiquidacionViewModel
    {
        public PropuestaLiquidacionViewModel() { }

        #region Propiedades Globales

        public string ErrorMessage { get; set; }

        public int IdRequerimiento { get; set; }

        #endregion

        #region Liquidar Index

        #region Propiedades

        public List<LOG_GetPedidosALiquidarResult> PedidosALiquidar { get; set; }

        #endregion

        public bool ValidateFirstLoadIndex(int id, AgroFichasDBDataContext dc)
        {
            var IsValid = true;
            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == id && x.IdEstado != 3 && x.IdEstado != 99);
            if (requerimiento == null)
            {
                this.ErrorMessage = "No se ha encontrado el requerimiento";
                IsValid = false;
            }
            return IsValid;
        }

        #endregion

        #region Crear Propuesta de Liquidación Paso 1

        #region Propiedades

        public List<LOG_Pedido> PedidosSeleccionadosALiquidar { get; set; }

        public string PedidosSeleccionadosALiquidarParaHacerLookup { get; set; }

        public IEnumerable<SelectListItem> MaterialesSelectList { get; set; }

        public bool ProntoPago { get; set; }

        public string Observacion { get; set; }

        public int IdMaterial { get; set; }

        public string CondicionPago { get; set; }

        public int IdTransportista { get; set; }

        public decimal ValorFlete { get; set; }

        #endregion

        public bool ValidateFirstLoadCrearPropuestaPaso1(int[] pedidos, AgroFichasDBDataContext dc)
        {
            var isValid = true;
            if (pedidos == null || pedidos.Length < 1)
            {
                isValid = false;
                this.ErrorMessage = "Debe seleccionar al menos un pedido";
            }

            if (isValid && Convert.ToBoolean(dc.LOG_CheckPropuestaLiquidacion(ConvertIntArrayToStringArray(pedidos)).FirstOrDefault().Column1) == false)
            {
                isValid = false;
                var s = "Ha ocurrido un error, los casos posibles son:";
                s += "<ul>";
                s += "\t<li>Un/unos pedidos no existen</li>";
                s += "\t<li>Un/unos pedidos no están listos para liquidar</li>";
                s += "\t<li>Debe agrupar los pedidos por transportista</li>";
                s += "\t<li>Debe agrupar los pedidos por valor del flete</li>";
                s += "</ul>";
                this.ErrorMessage = s;
            }

            return isValid;
        }

        public void SetMaterialesSelectList(int? IdMaterial, AgroFichasDBDataContext dc)
        {
            string[] ignorados = { "FIPRPA01", "FIDEPE01" };
            this.MaterialesSelectList = (from X in dc.OC_Material
                                         join Y in dc.Empresa on X.IdEmpresa equals Y.IdEmpresa
                                         join Z in dc.LOG_Requerimiento on Y.IdEmpresa equals Z.IdEmpresa
                                         where X.IdEmpresa == Z.IdEmpresa
                                         && X.IdProyecto == 2
                                         && Z.IdRequerimiento == this.IdRequerimiento
                                         && ignorados.Contains(X.CodigoMaterial) == false
                                         orderby X.Descripcion
                                         select new SelectListItem
                                         {
                                             Selected = (X.IdMaterial == IdMaterial && IdMaterial != null),
                                             Text = X.Descripcion,
                                             Value = X.IdMaterial.ToString()
                                         });
        }

        public void SetPedidosSeleccionadosALiquidar(AgroFichasDBDataContext dc)
        {
            this.PedidosSeleccionadosALiquidar = new List<LOG_Pedido>();
            var pedidos = this.PedidosSeleccionadosALiquidarParaHacerLookup.Split(new char[] { ',' });
            foreach (var pedido in pedidos)
            {
                var pedidoDB = dc.LOG_Pedido.Single(X => X.IdPedido == Convert.ToInt32(pedido));
                this.PedidosSeleccionadosALiquidar.Add(pedidoDB);
            }
        }

        public void SetPedidosSeleccionadosALiquidarParaHacerLookup(int[] pedidos)
        {
            this.PedidosSeleccionadosALiquidarParaHacerLookup = ConvertIntArrayToStringArray(pedidos);
        }

        public void SetTransportistaLiquidar(int[] pedidos, AgroFichasDBDataContext dc)
        {
            this.IdTransportista = dc.LOG_GetTransportistas(ConvertIntArrayToStringArray(pedidos)).FirstOrDefault().IdTransportista;
        }

        public void SetValorFlete(int[] pedidos, AgroFichasDBDataContext dc)
        {
            this.ValorFlete = dc.LOG_GetValorFlete(ConvertIntArrayToStringArray(pedidos)).FirstOrDefault().ValorFletePorKgTransportado;
        }

        #endregion

        #region Crear Propuesta de Liquidación Paso 2

        #region Propiedades

        #endregion

        public bool ValidateFirstLoadCrearPropuestaPaso2(PropuestaLiquidacionViewModel propuestaLiquidacion, AgroFichasDBDataContext dc)
        {
            var isValid = true;
            if (propuestaLiquidacion.IdMaterial == 0)
            {
                isValid = false;
                this.ErrorMessage = "Debe seleccionar un material";
            }
            if (isValid && string.IsNullOrEmpty(propuestaLiquidacion.CondicionPago))
            {
                isValid = false;
                this.ErrorMessage = "Debe ingresar la condición de pago";
            }
            return isValid;
        }

        public void CrearPropuestaLiquidacion(string userIns, string ipIns, AgroFichasDBDataContext dc)
        {
            var liquidacion = new LOG_Liquidacion()
            {
                Liquidacion = "",
                PropuestaLiquidacion = "",
                NumeroFactura = 0,
                IdRequerimiento = this.IdRequerimiento,
                GUID = System.Guid.NewGuid().ToString().Replace("-", ""),
                Habilitado = true,
                UserIns = userIns,
                FechaHoraIns = DateTime.Now,
                IpIns = ipIns,
                TotalNeto = 0,
                IVA = 0,
                TotalBruto = 0,
                PP_TotalNeto = 0,
                PP_IVA = 0,
                PP_TotalBruto = 0,
                DP_TotalNeto = 0,
                DP_IVA = 0,
                DP_TotalBruto = 0,
                ProntoPago = this.ProntoPago,
                DiferenciaPesaje = false,
                Observacion = (this.Observacion ?? "")
            };

            var movimientos = dc.LOG_GetMovimientosParaPropuestaLiquidacion(ConvertListToStringArray(this.PedidosSeleccionadosALiquidar), this.IdRequerimiento, this.ProntoPago).ToList();
            liquidacion.TotalNeto = movimientos.Sum(X => X.TotalNeto);
            liquidacion.IVA = movimientos.Sum(X => X.IVA);
            liquidacion.TotalBruto = movimientos.Sum(X => X.TotalBruto);
            liquidacion.PP_TotalNeto = movimientos.Sum(X => X.PP_TotalNeto);
            liquidacion.PP_IVA = movimientos.Sum(X => X.PP_IVA);
            liquidacion.PP_TotalBruto = movimientos.Sum(X => X.PP_TotalBruto);
            liquidacion.DP_TotalNeto = movimientos.Sum(X => X.DP_TotalNeto);
            liquidacion.DP_IVA = movimientos.Sum(X => X.DP_IVA);
            liquidacion.DP_TotalBruto = movimientos.Sum(X => X.DP_TotalBruto);

            liquidacion.DiferenciaPesaje = liquidacion.DP_TotalNeto > 0;

            dc.LOG_Liquidacion.InsertOnSubmit(liquidacion);
            dc.SubmitChanges();

            CrearDesgloseDePropuestaDeLiquidacion(liquidacion, dc);
            CrearPropuestaLiquidacionPdf(liquidacion, dc);
            CrearOrdenDeCompra(liquidacion, userIns, ipIns, dc);
        }

        public void CrearDesgloseDePropuestaDeLiquidacion(LOG_Liquidacion liquidacion, AgroFichasDBDataContext dc)
        {
            var desgloses = dc.LOG_GetDesgloseDePropuestaLiquidacion(ConvertListToStringArray(this.PedidosSeleccionadosALiquidar), liquidacion.IdLiquidacion, this.IdRequerimiento, this.IdTransportista);
            foreach (var desglose in desgloses)
            {
                var liquidacionLog = new LOG_LiquidacionLog();
                liquidacionLog.IdLiquidacion = desglose.IdLiquidacion;
                liquidacionLog.IdRequerimiento = desglose.IdRequerimiento;
                liquidacionLog.IdPedido = desglose.IdPedido;
                liquidacionLog.IdCultivo = desglose.IdCultivo;
                liquidacionLog.IdTransportista = desglose.IdTransportista;
                liquidacionLog.MovPesajeSalidaKg = desglose.MovPesajeSalidaKg;
                liquidacionLog.MovPesajeLlegadaKg = desglose.MovPesajeLlegadaKg;
                liquidacionLog.MovFechaSalida = desglose.MovFechaSalida;
                liquidacionLog.MovFechaLlegada = desglose.MovFechaLlegada;
                liquidacionLog.MovValorFletePorKgTransportado = desglose.MovValorFletePorKgTransportado;
                liquidacionLog.MovTolerancia = desglose.MovTolerancia;
                liquidacionLog.MovMerma = desglose.MovMerma;
                liquidacionLog.MovTotalNeto = desglose.MovTotalNeto;
                liquidacionLog.MovIVA = desglose.MovIVA;
                liquidacionLog.MovTotalBruto = desglose.MovTotalBruto;
                liquidacionLog.LiqProntoPago = desglose.LiqProntoPago;
                liquidacionLog.LiqDiferenciaPesaje = desglose.LiqDiferenciaPesaje;
                liquidacionLog.LiqPP_TotalNeto = desglose.LiqPP_TotalNeto;
                liquidacionLog.LiqPP_IVA = desglose.LiqPP_IVA;
                liquidacionLog.LiqPP_TotalBruto = desglose.LiqPP_TotalBruto;
                liquidacionLog.LiqDP_TotalNeto = desglose.LiqDP_TotalNeto;
                liquidacionLog.LiqDP_IVA = desglose.LiqDP_IVA;
                liquidacionLog.LiqDP_TotalBruto = desglose.LiqDP_TotalBruto;
                liquidacionLog.LiqTotalNeto = desglose.LiqTotalNeto;
                liquidacionLog.LiqIVA = desglose.LiqIVA;
                liquidacionLog.LiqTotalBruto = desglose.LiqTotalBruto;
                liquidacionLog.IdTipoMovimiento = desglose.IdTipoMovimiento;
                dc.LOG_LiquidacionLog.InsertOnSubmit(liquidacionLog);
                dc.SubmitChanges();
            }
        }

        public void CrearPropuestaLiquidacionPdf(LOG_Liquidacion liquidacion, AgroFichasDBDataContext dc)
        {
            StringBuilder sb = new StringBuilder();
            var filas = dc.LOG_GetFilaColumnaParaPdfDePropuestaLiquidacion(ConvertListToStringArray(this.PedidosSeleccionadosALiquidar));
            foreach (var fila in filas)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine(string.Format("<td>{0}</td>", fila.Producto));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.NumeroGuia));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.Origen));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.Destino));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.PesajeSalidaKg.Value.ToString("N0")));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.PesajeLlegadaKg.Value.ToString("N0")));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.Tolerancia.Value.ToString("N0")));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.DiferenciaPesajesKg.Value.ToString("N0")));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.Merma.Value.ToString("N0")));
                sb.AppendLine(string.Format("<td>{0}</td>", fila.ValorFletePorKgTransportado.Value.ToString("C2")));
                sb.AppendLine(string.Format("<td align=\"right\">{0}</td>", fila.TotalNeto.Value.ToString("C0")));
                sb.AppendLine("</tr>");
            }

            var cuerpo = dc.LOG_GetCuerpoParaPdfDePropuestaLiquidacion(liquidacion.IdLiquidacion).FirstOrDefault();
            var template = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/liquidacion_propuesta_template.html"), System.Text.Encoding.UTF8);
            RepTemp(ref template, "NUMERODELIQUIDACION", cuerpo.NumeroDeLiquidacion.ToString());
            RepTemp(ref template, "RAZONSOCIAL", cuerpo.RazonSocial);
            RepTemp(ref template, "RUT", Rut.FormatearRut(cuerpo.Rut));
            RepTemp(ref template, "FECHAEMISION", cuerpo.FechaEmision.ToString("dd/MM/yyyy"));
            RepTemp(ref template, "EMPRESA", cuerpo.Empresa);
            RepTemp(ref template, "PEDIDOS", sb.ToString());
            RepTemp(ref template, "NETO", cuerpo.Neto.Value.ToString("C0"));
            RepTemp(ref template, "DSCTOPP", cuerpo.DsctoPp.Value.ToString("C0"));
            RepTemp(ref template, "DSCTODP", cuerpo.DsctoDp.Value.ToString("C0"));
            RepTemp(ref template, "IVA", cuerpo.Iva.Value.ToString("C0"));
            RepTemp(ref template, "TOTAL", cuerpo.Total.Value.ToString("C0"));
            RepTemp(ref template, "PRONTOPAGO", cuerpo.ProntoPago);
            RepTemp(ref template, "PRONTOPAGONETO", cuerpo.ProntoPagoNeto.Value.ToString("C0"));
            RepTemp(ref template, "PRONTOPAGOIVA", cuerpo.ProntoPagoIva.Value.ToString("C0"));
            RepTemp(ref template, "PRONTOPAGOTOTAL", cuerpo.ProntoPagoTotal.Value.ToString("C0"));
            RepTemp(ref template, "DIFERENCIAPESAJE", cuerpo.DiferenciaPesaje);
            RepTemp(ref template, "DIFERENCIAPESAJENETO", cuerpo.DiferenciaPesajeNeto.Value.ToString("C0"));
            RepTemp(ref template, "DIFERENCIAPESAJEIVA", cuerpo.DiferenciaPesajeIva.Value.ToString("C0"));
            RepTemp(ref template, "DIFERENCIAPESAJETOTAL", cuerpo.DiferenciaPesajeTotal.Value.ToString("C0"));
            RepTemp(ref template, "TOTALPESAJESALIDA", cuerpo.TotalPesajeSalida.Value.ToString("N0"));
            RepTemp(ref template, "TOTALPESAJELLEGADA", cuerpo.TotalPesajeLlegada.Value.ToString("N0"));
            RepTemp(ref template, "TOTALMERMA", cuerpo.TotalMerma.Value.ToString("N0"));
            RepTemp(ref template, "OBSERVACION", cuerpo.Observacion);

            liquidacion.PropuestaLiquidacion = template;
            dc.SubmitChanges();

            try
            {
                string path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/logistica/propuestaliquidacion"), string.Format("{0}.pdf", liquidacion.GUID));
                var pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(template);
                System.IO.File.WriteAllBytes(path, pdfBytes);
            }
            catch (Exception ex)
            {
                FileStream fileStream = new FileStream(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/logistica/propuestaliquidacion"), string.Format("{0}.txt", liquidacion.GUID)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine("No se pudo generar el archivo PDF en la liquidación #" + liquidacion.IdLiquidacion);
                writer.WriteLine(ex.ToString());
                writer.Close();

                // Enviar email con error a Fernando Rodriguez e IT
                // Pendiente
            }
        }

        public void CrearOrdenDeCompra(LOG_Liquidacion liquidacion, string userIns, string ipIns, AgroFichasDBDataContext dc)
        {
            var ordenDeCompra = dc.LOG_GetOrdenDeCompraDeLogisticaYCorretaje(liquidacion.IdLiquidacion, this.IdMaterial, this.CondicionPago, userIns, ipIns).ToList();
            foreach (var glosa in ordenDeCompra)
            {
                dc.OC_OrdenCompra.InsertOnSubmit(new OC_OrdenCompra()
                {
                    IdEmpresa = glosa.IdEmpresa,
                    IdProyecto = glosa.IdProyecto,
                    IdLiquidacion = glosa.IdLiquidacion,
                    IdEstado = glosa.IdEstado,
                    FechaDocumento = glosa.FechaDocumento,
                    IdProveedor = glosa.IdProveedor,
                    IdCentroCosto = glosa.IdCentroCosto,
                    IdMaterial = glosa.IdMaterial,
                    Moneda = glosa.Moneda,
                    Cantidad = glosa.Cantidad,
                    PrecioUnitario = glosa.PrecioUnitario,
                    PrecioTotal = glosa.PrecioTotal,
                    CondicionPago = glosa.CondicionPago,
                    UserIns = userIns,
                    FechaHoraIns = DateTime.Now,
                    IpIns = ipIns,
                    Firma = ""
                });
                dc.SubmitChanges();
            }

            CrearXmlOrdenDeCompra(ordenDeCompra, dc);
        }

        public void CrearXmlOrdenDeCompra(List<LOG_GetOrdenDeCompraDeLogisticaYCorretajeResult> ordenDeCompra, AgroFichasDBDataContext dc)
        {
            var PrimeraGlosa = ordenDeCompra.First();
            var Firma = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            var Filename = string.Format("OC_P2_L{0}_F{1}.xml", PrimeraGlosa.IdLiquidacion, Firma);
            var FullPath = string.Format(@"{0}\{1}", ConfigurationManager.AppSettings["OcsPendientes"], Filename);

            XmlTextWriter writer = new XmlTextWriter(FullPath, System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
            writer.WriteStartElement("BOM");
            writer.WriteStartElement("Documents");
            writer.WriteStartElement("row");
            writer.WriteStartElement("CardCode");
            writer.WriteString(PrimeraGlosa.IdProveedor);
            writer.WriteEndElement();///CardCode
            writer.WriteStartElement("DocDate");
            writer.WriteString(string.Format("{0:yyyyMMdd}", PrimeraGlosa.FechaDocumento));
            writer.WriteEndElement();///DocDate
            writer.WriteStartElement("DocDueDate");
            writer.WriteString(string.Format("{0:yyyyMMdd}", PrimeraGlosa.FechaDocumento));
            writer.WriteEndElement();///DocDueDate
            writer.WriteStartElement("TaxDate");
            writer.WriteString(string.Format("{0:yyyyMMdd}", PrimeraGlosa.FechaDocumento));
            writer.WriteEndElement();///TaxDate
            writer.WriteStartElement("Comments");
            writer.WriteString(string.Format(@"LIQ.{0}\N{1}", PrimeraGlosa.IdLiquidacion, PrimeraGlosa.CondicionPago));
            writer.WriteEndElement();///Comments
            writer.WriteEndElement();///row
            writer.WriteEndElement();///Documents
            writer.WriteStartElement("Document_Lines");

            foreach (var glosa in ordenDeCompra)
            {
                writer.WriteStartElement("row");
                writer.WriteStartElement("CostingCode");
                writer.WriteString(dc.GetCentroCosto(glosa.IdProyecto, glosa.IdEmpresa, glosa.IdCentroCosto).SingleOrDefault().Id);
                writer.WriteEndElement();
                writer.WriteStartElement("ItemCode");
                writer.WriteString(dc.GetMaterial(glosa.IdMaterial).SingleOrDefault().Id);
                writer.WriteEndElement();
                writer.WriteStartElement("Quantity");
                writer.WriteValue(glosa.Cantidad);
                writer.WriteEndElement();
                writer.WriteStartElement("Price");
                writer.WriteValue(glosa.PrecioUnitario);
                writer.WriteEndElement();
                writer.WriteStartElement("LineTotal");
                writer.WriteValue(glosa.PrecioTotal);
                writer.WriteEndElement();
                writer.WriteEndElement();///row
            }

            writer.WriteEndElement();///Document_Lines
            writer.WriteEndElement();///BOM
            writer.WriteEndDocument();
            writer.Close();

            // Firma
            var ordenDeCompraDB = dc.OC_OrdenCompra.Where(X => X.IdProyecto == PrimeraGlosa.IdProyecto && X.IdLiquidacion == PrimeraGlosa.IdLiquidacion);
            foreach (var glosa in ordenDeCompraDB)
            {
                glosa.Firma = Firma;
                dc.SubmitChanges();
            }
        }

        public void CambiaEstadoAPropuestaDeLiquidacionCreada(AgroFichasDBDataContext dc)
        {
            foreach (var pedido in this.PedidosSeleccionadosALiquidar)
            {
                var pedidoBD = dc.LOG_Pedido.Single(x => x.IdPedido == pedido.IdPedido);
                pedidoBD.IdEstado = 6;

                dc.SubmitChanges();

                var asignacionPedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                asignacionPedido.Estado = pedido.LOG_EstadoPedido.Descripcion;

                dc.SubmitChanges();
            }
        }

        #endregion

        #region Funciones

        private string ConvertIntArrayToStringArray(int[] pedidos)
        {
            var I = 0;
            var J = pedidos.Length;
            var strArray = "";
            foreach (var pedido in pedidos)
            {
                I++;
                if (I < J)
                    strArray += pedido + ",";
                else
                    strArray += pedido;
            }

            return strArray;
        }

        private string ConvertListToStringArray(List<LOG_Pedido> pedidos)
        {
            var I = 0;
            var J = pedidos.Count;
            var strArray = "";
            foreach (var pedido in pedidos)
            {
                I++;
                if (I < J)
                    strArray += pedido.IdPedido + ",";
                else
                    strArray += pedido.IdPedido;
            }

            return strArray;
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.PedidosALiquidar = dc.LOG_GetPedidosALiquidar(this.IdRequerimiento).ToList();
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}