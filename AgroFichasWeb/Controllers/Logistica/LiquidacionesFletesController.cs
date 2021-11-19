using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using AgroFichasWeb.ViewModels.Logistica;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class LiquidacionesFletesController : BaseApplicationController
    {
        //
        // GET: /LiquidacionesFletes/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public LiquidacionesFletesController()
        {
            SetCurrentModulo(6);
        }

        [HttpGet]
        public ActionResult CrearLiquidacion(int id)
        {
            CheckPermisoAndRedirect(123);
            var liquidacion = dc.LOG_Liquidacion.SingleOrDefault(f => f.IdLiquidacion == id);
            if (liquidacion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la propuesta de liquidación"); }

            string s = "";
            var pedidos = dc.LOG_LiquidacionLog.Where(x => x.IdLiquidacion == liquidacion.IdLiquidacion);
            foreach (var pedido in pedidos)
            {
                s += pedido.IdPedido + ",";
            }

            ViewData["IdRequerimiento"] = liquidacion.IdRequerimiento;
            ViewData["IdTransportista"] = pedidos.First().IdTransportista;
            ViewData["pedidos"] = s;
            return View(liquidacion);
        }

        [HttpPost]
        public ActionResult CrearLiquidacion(LOG_Liquidacion liquidacion, FormCollection formValues)
        {
            CheckPermisoAndRedirect(123);

            //string msgerr = "";
            //string msgok = "";

            var IdRequerimiento = int.Parse(formValues["IdRequerimiento"]);
            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == IdRequerimiento && x.IdEstado != 3 && x.IdEstado != 99);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            var IdTransportista = int.Parse(formValues["IdTransportista"]);
            var transportista = dc.LOG_Transportista.SingleOrDefault(x => x.IdTransportista == IdTransportista);
            if (transportista == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el transportista"); }

            if (liquidacion.NumeroFactura == 0)
            {
                return RedirectToAction("crearliquidacion", new { id = liquidacion.IdLiquidacion, msgerr = "El número de factura es requerido" });
            }

            string[] idPedidos = formValues["pedidos"].ToString().Split(',');
            var pedidos = (from a in dc.LOG_Pedido
                           join b in dc.LOG_AsignacionPedido on a.IdPedido equals b.IdPedido
                           join c in dc.LOG_Requerimiento on b.IdRequerimiento equals c.IdRequerimiento
                           where a.Habilitado == true
                           && idPedidos.Contains(a.IdPedido.ToString())
                           select a).ToList();
            decimal totalNeto = 0;
            decimal iva = 0;
            decimal totalBruto = 0;
            decimal totalMerma = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var pedido in pedidos)
            {
                var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                var movimiento = dc.LOG_Movimiento.Single(x => x.IdPedido == pedido.IdPedido && x.Habilitado == true);
                //Creación fila/columna para PDF
                var s = "<tr>";
                s += "    <td>" + asignacionpedido.Cultivo + "</td>";
                s += "    <td>" + movimiento.NumeroGuia + "</td>";
                s += "    <td>" + asignacionpedido.Origen + "</td>";
                s += "    <td>" + asignacionpedido.Destino + "</td>";
                s += "    <td>" + movimiento.PesajeSalidaKg.ToString("N0") + "</td>";
                s += "    <td>" + movimiento.PesajeLlegadaKg.Value.ToString("N0") + "</td>";
                s += "    <td>" + movimiento.Tolerancia.ToString("N0") + "</td>";
                s += "    <td>" + movimiento.DiferenciaPesajesKg.Value.ToString("N0") + "</td>";
                s += "    <td>" + movimiento.Merma.Value.ToString("N0") + "</td>";
                s += "    <td>" + movimiento.ValorFletePorKgTransportado.ToString("C2") + "</td>";
                s += "    <td align=\"right\">" + movimiento.TotalNeto.Value.ToString("C0") + "</td>";
                s += "</tr>";
                sb.Append(s);

                // Cálculo de los totales
                totalNeto += movimiento.TotalNeto.Value;
                iva += movimiento.IVA.Value;
                totalBruto += movimiento.TotalBruto.Value;

                //Cálculo de la merma
                var valorMerma = dc.LOG_Merma.SingleOrDefault(x => x.IdCultivo == pedido.IdCultivo);
                if (valorMerma == null)
                {
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, string.Format("No se ha encontrado el valor de la merma para el cultivo {0}", pedido.Cultivo.Nombre));
                }

                totalMerma += (movimiento.Merma.Value * valorMerma.Precio);
            }

            //Marcar pedidos como liquidados
            MarcarPedidosComoLiquidados(pedidos);

            var desglose = dc.LOG_LiquidacionLog.Where(x => x.IdLiquidacion == liquidacion.IdLiquidacion);
            var liquidacionBD = dc.LOG_Liquidacion.Single(x => x.IdLiquidacion == liquidacion.IdLiquidacion);

            liquidacionBD.NumeroFactura = liquidacion.NumeroFactura;
            liquidacionBD.UserUpd = User.Identity.Name;
            liquidacionBD.FechaHoraUpd = DateTime.Now;
            liquidacionBD.IpUpd = RemoteAddr();

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/liquidacion_template.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "NUMERODELIQUIDACION",   liquidacionBD.IdLiquidacion.ToString());
            RepTemp(ref htmlContent, "RAZONSOCIAL",           transportista.Nombre);
            RepTemp(ref htmlContent, "RUT",                   Rut.FormatearRut(transportista.RUT));
            RepTemp(ref htmlContent, "NUMERODEFACTURA",       liquidacionBD.NumeroFactura.ToString());
            RepTemp(ref htmlContent, "FECHAEMISION",          liquidacionBD.FechaHoraIns.ToString("dd/MM/yyyy"));
            RepTemp(ref htmlContent, "EMPRESA",               requerimiento.Empresa.Nombre);
            RepTemp(ref htmlContent, "PEDIDOS",               sb.ToString());
            RepTemp(ref htmlContent, "NETO",                  liquidacionBD.TotalNeto.Value.ToString("C0"));
            RepTemp(ref htmlContent, "DSCTOPP",               (liquidacionBD.PP_TotalNeto.Value * -1).ToString("C0"));
            RepTemp(ref htmlContent, "DSCTODP",               (liquidacionBD.DP_TotalNeto.Value * -1).ToString("C0"));
            RepTemp(ref htmlContent, "IVA",                   liquidacionBD.IVA.Value.ToString("C0"));
            RepTemp(ref htmlContent, "TOTAL",                 liquidacionBD.TotalBruto.Value.ToString("C0"));
            RepTemp(ref htmlContent, "PRONTOPAGO",            (liquidacionBD.ProntoPago ? "Sí" : "No"));
            RepTemp(ref htmlContent, "PRONTOPAGONETO",        liquidacionBD.PP_TotalNeto.Value.ToString("C0"));
            RepTemp(ref htmlContent, "PRONTOPAGOIVA",         liquidacionBD.PP_IVA.Value.ToString("C0"));
            RepTemp(ref htmlContent, "PRONTOPAGOTOTAL",       liquidacionBD.PP_TotalBruto.Value.ToString("C0"));
            RepTemp(ref htmlContent, "DIFERENCIAPESAJE",      (liquidacionBD.DiferenciaPesaje ? "Sí" : "No"));
            RepTemp(ref htmlContent, "DIFERENCIAPESAJENETO",  liquidacionBD.DP_TotalNeto.Value.ToString("C0"));
            RepTemp(ref htmlContent, "DIFERENCIAPESAJEIVA",   liquidacionBD.DP_IVA.Value.ToString("C0"));
            RepTemp(ref htmlContent, "DIFERENCIAPESAJETOTAL", liquidacionBD.DP_TotalBruto.Value.ToString("C0"));
            RepTemp(ref htmlContent, "TOTALPESAJESALIDA",     desglose.Sum(x => x.MovPesajeSalidaKg).ToString("N0"));
            RepTemp(ref htmlContent, "TOTALPESAJELLEGADA",    desglose.Sum(x => x.MovPesajeLlegadaKg).ToString("N0"));
            RepTemp(ref htmlContent, "TOTALMERMA",            desglose.Sum(x => x.MovMerma).ToString("N0"));
            RepTemp(ref htmlContent, "OBSERVACION",           liquidacionBD.Observacion);
            RepTemp(ref htmlContent, "OC", "");

            if (liquidacion.OC.HasValue || liquidacionBD.OC.HasValue)
            {
                if (liquidacion.OC.HasValue)
                    RepTemp(ref htmlContent, "OC", liquidacion.OC.Value.ToString());
                if (liquidacionBD.OC.HasValue)
                    RepTemp(ref htmlContent, "OC", liquidacionBD.OC.Value.ToString());
            }

            liquidacionBD.Liquidacion = htmlContent;
            dc.SubmitChanges();

            try
            {
                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/logistica/liquidaciones"), string.Format("{0}.pdf", liquidacionBD.GUID));
                var pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf((string)htmlContent);
                System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO
            }
            catch (Exception ex)
            {
                //this code segment write data to file.
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/logistica/liquidaciones"), string.Format("{0}.txt", liquidacionBD.GUID)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine("No se pudo generar el archivo PDF");
                writer.WriteLine(ex.ToString());
                writer.Close();
                throw new Exception("No se pudo generar el archivo PDF");
            }

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult CrearPropuestaPaso1(PropuestaLiquidacionViewModel propuestaLiquidacion, int[] pedidos)
        {
            CheckPermisoAndRedirect(123);
            if (propuestaLiquidacion.ValidateFirstLoadCrearPropuestaPaso1(pedidos, dc))
            {
                propuestaLiquidacion.SetMaterialesSelectList(null, dc);
                propuestaLiquidacion.SetPedidosSeleccionadosALiquidarParaHacerLookup(pedidos);
                propuestaLiquidacion.SetTransportistaLiquidar(pedidos, dc);
                propuestaLiquidacion.SetValorFlete(pedidos, dc);
                return View(propuestaLiquidacion);
            }
            else
            {
                propuestaLiquidacion.LoadLookups(dc);
                return View("liquidarindex", propuestaLiquidacion);
            }
        }

        [HttpPost]
        public ActionResult CrearPropuestaPaso2(PropuestaLiquidacionViewModel propuestaLiquidacion, FormCollection formValues)
        {
            CheckPermisoAndRedirect(123);
            if (propuestaLiquidacion.ValidateFirstLoadCrearPropuestaPaso2(propuestaLiquidacion, dc))
            {
                propuestaLiquidacion.SetPedidosSeleccionadosALiquidar(dc);
                propuestaLiquidacion.CrearPropuestaLiquidacion(User.Identity.Name, RemoteAddr(), dc);
                propuestaLiquidacion.CambiaEstadoAPropuestaDeLiquidacionCreada(dc);
                return RedirectToAction("index");
            }
            else
            {
                propuestaLiquidacion.LoadLookups(dc);
                return View("liquidarindex", propuestaLiquidacion);
            }
        }

        [HttpGet]
        public FileResult DescargarLiquidacion(int id)
        {
            CheckPermisoAndRedirect(122);
            var liquidacion = dc.LOG_Liquidacion.SingleOrDefault(f => f.IdLiquidacion == id);
            if (liquidacion == null)
            {
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la liquidación");
            }

            var pdf = Server.MapPath(string.Format("~/App_Data/pdfs/logistica/liquidaciones/{0}.pdf", liquidacion.GUID));

            return File(pdf, "application/pdf", String.Format("Liquidación-{0}.pdf", liquidacion.IdLiquidacion));
        }

        [HttpGet]
        public FileResult DescargarPropuestaLiquidacion(int id)
        {
            CheckPermisoAndRedirect(122);
            var propuestaLiquidacion = dc.LOG_Liquidacion.SingleOrDefault(f => f.IdLiquidacion == id);
            if (propuestaLiquidacion == null)
            {
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la propuesta de liquidación");
            }

            var pdf = Server.MapPath(string.Format("~/App_Data/pdfs/logistica/propuestaliquidacion/{0}.pdf", propuestaLiquidacion.GUID));

            return File(pdf, "application/pdf", String.Format("Propuesta de Liquidación-{0}.pdf", propuestaLiquidacion.IdLiquidacion));
        }

        [HttpGet]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(124);
            var liquidacion = dc.LOG_Liquidacion.SingleOrDefault(x => x.IdLiquidacion == id);
            if (liquidacion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la liquidación"); }

            var desgloses = dc.LOG_LiquidacionLog.Where(x => x.IdLiquidacion == liquidacion.IdLiquidacion).ToList();
            if (desgloses.Count > 0)
            {
                // Comprobando pedidos ya liquidados y duplicados
                var ok = true;
                foreach (var desglose in desgloses)
                {
                    var pedido = dc.LOG_LiquidacionLog.SingleOrDefault(x => x.IdPedido == desglose.IdPedido && x.IdLiquidacion != liquidacion.IdLiquidacion && x.LOG_Liquidacion.Habilitado == true);
                    if (pedido != null)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    // Si está ok significa que no hay duplicados y que se pueden reliquidar estos pedidos
                    foreach (var desglose in desgloses)
                    {
                        var pedido = dc.LOG_Pedido.Single(x => x.IdPedido == desglose.IdPedido);
                        pedido.IdEstado = 5; //Listo para liquidar
                        pedido.Observacion = "Pedido listo para reliquidar";

                        var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == desglose.IdPedido);
                        asignacionpedido.Estado = "Listo para liquidar";

                        dc.SubmitChanges();
                    }
                }
            }

            liquidacion.Habilitado = false;
            dc.SubmitChanges();

            string msgerr = "";
            string msgok = string.Format("La liquidación núm. {0} ha sido eliminada", liquidacion.IdLiquidacion);
            return RedirectToAction("index", new { id = 0, msgerr = msgerr, msgok = msgok });
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(122);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            string key = Request.QueryString["key"] ?? "";

            IQueryable<LOG_Liquidacion> items = (from x in dc.LOG_Liquidacion
                                                 where (
                                                    key == string.Empty ||
                                                    x.IdLiquidacion.ToString().Contains(key) ||
                                                    x.NumeroFactura.ToString().Contains(key))
                                                 && x.Habilitado == true
                                                 orderby x.IdLiquidacion descending
                                                 select x);

            var pagina = new PaginatedList<LOG_Liquidacion>(items, pageIndex, pageSize);

            ViewData["key"] = key;
            return View(pagina);
        }

        [HttpGet]
        public ActionResult LiquidarIndex(int id)
        {
            CheckPermisoAndRedirect(123);
            var model = new PropuestaLiquidacionViewModel();
            if (model.ValidateFirstLoadIndex(id, dc))
            {
                model.IdRequerimiento = id;
                model.LoadLookups(dc);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult LiquidarPropuestasIndex(int? id)
        {
            CheckPermisoAndRedirect(122);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            string key = Request.QueryString["key"] ?? "";

            IQueryable<LOG_Liquidacion> items = from x in dc.LOG_Liquidacion
                                                where (key == string.Empty ||
                                                x.IdLiquidacion.ToString().Contains(key))
                                                && x.Habilitado == true
                                                && x.NumeroFactura == 0
                                                orderby x.IdLiquidacion descending
                                                select x;

            var pagina = new PaginatedList<LOG_Liquidacion>(items, pageIndex, pageSize);

            ViewData["key"] = key;
            return View(pagina);
        }

        private void MarcarPedidosComoLiquidados(List<LOG_Pedido> pedidos)
        {
            foreach (var pedido in pedidos)
            {
                var pedidoBD = dc.LOG_Pedido.Single(x => x.IdPedido == pedido.IdPedido);
                pedidoBD.IdEstado = 7;

                dc.SubmitChanges();

                var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                asignacionpedido.Estado = pedido.LOG_EstadoPedido.Descripcion;

                dc.SubmitChanges();
            }
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }
    }
}
