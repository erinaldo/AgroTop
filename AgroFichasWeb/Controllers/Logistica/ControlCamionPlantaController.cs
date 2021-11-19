using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Zen.Barcode;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class ControlCamionPlantaController : BaseApplicationController
    {
        //
        // GET: /ControlCamionPlanta/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ControlCamionPlantaController()
        {
            SetCurrentModulo(6);//Logística y Corretaje
        }

        public ActionResult Index(int? id, int? idPedido)
        {
            CheckPermisoAndRedirect(141);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var idPedidoSelect = idPedido ?? 0;

            IQueryable<LOG_ControlCamionPlanta> items = from x in dc.LOG_ControlCamionPlanta
                                                        where (idPedidoSelect == 0 || x.IdPedido == idPedidoSelect)
                                                        select x;

            var pagina = new PaginatedList<LOG_ControlCamionPlanta>(items, pageIndex, pageSize);

            ViewData["idPedido"] = idPedido;
            return View(pagina);
        }

        public ActionResult CrearVoucherLlegadaOrigen()
        {
            CheckPermisoAndRedirect(141);
            var controlCamionPlanta = new LOG_ControlCamionPlanta();
            return View("crearvoucherllegadaorigen", controlCamionPlanta);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult CrearVoucherLlegadaOrigen(LOG_ControlCamionPlanta controlCamionPlanta)
        {
            CheckPermisoAndRedirect(141);
            if (ModelState.IsValid)
            {
                try
                {
                    var pedido = dc.LOG_Pedido.Single(x => x.IdPedido == controlCamionPlanta.IdPedido);
                    var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                    var asignacioncamion = dc.LOG_AsignacionCamion.Single(x => x.IdPedido == pedido.IdPedido);
                    var camion = dc.LOG_Camion.Single(x => x.IdCamion == asignacioncamion.IdCamion);
                    var transportista = dc.LOG_Transportista.Single(x => x.IdTransportista == camion.IdTransportista);

                    controlCamionPlanta.IdCamion = camion.IdCamion;
                    controlCamionPlanta.IdPedido = pedido.IdPedido;
                    controlCamionPlanta.IdEstadoControlCamionPlanta = 1;//Llegada a portería de origen
                    controlCamionPlanta.Firma = UniqueId.GetUniqueKey();
                    controlCamionPlanta.UserIns = User.Identity.Name;
                    controlCamionPlanta.FechaHoraIns = DateTime.Now;
                    controlCamionPlanta.IpIns = RemoteAddr();
                    controlCamionPlanta.PasoActual = 1;

                    Code128BarcodeDraw bdf = BarcodeDrawFactory.Code128WithChecksum;

                    string base64String = "";

                    using (MemoryStream s = new MemoryStream(imageToByteArray(bdf.Draw(controlCamionPlanta.Firma, 80, 3)) as byte[]))
                    {
                        byte[] imageBytes = s.ToArray();
                        base64String = Convert.ToBase64String(imageBytes);
                    }

                    controlCamionPlanta.Barcode = base64String;

                    dc.LOG_ControlCamionPlanta.InsertOnSubmit(controlCamionPlanta);
                    dc.SubmitChanges();

                    string Template = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/voucherorigen_template.html"), Encoding.UTF8);
                    RepTemp(ref Template, "FECHA", controlCamionPlanta.FechaHoraIns.ToString("dd/MM/yyyy hh:mm:ss"));
                    RepTemp(ref Template, "ORIGEN", asignacionpedido.Origen.ToUpper());
                    RepTemp(ref Template, "TRANSPORTISTA", transportista.Nombre.ToUpper());
                    RepTemp(ref Template, "CAMION", camion.Patente.ToUpper());
                    RepTemp(ref Template, "IMGSRC", string.Format("data:image/gif;base64,{0}", base64String));
                    RepTemp(ref Template, "FIRMADIGITAL", controlCamionPlanta.Firma);

                    try
                    {
                        string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/logistica/vouchers"), string.Format("{0}.pdf", controlCamionPlanta.Firma));
                        var htmlContent = Template;
                        var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
                        System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO
                    }
                    catch
                    {
                        throw new Exception("No se pudo generar el archivo PDF");
                    }

                    var voucher = new VoucherViewModel();
                    voucher.Camion = camion.Patente.ToUpper();
                    voucher.Fecha = controlCamionPlanta.FechaHoraIns.ToString("dd/MM/yyyy hh:mm:ss");
                    voucher.FirmaDigital = controlCamionPlanta.Firma;
                    voucher.ImgSrc = string.Format("data:image/gif;base64,{0}", base64String);
                    voucher.Origen = asignacionpedido.Origen.ToUpper();
                    voucher.Transportista = transportista.Nombre.ToUpper();
                    voucher.ReturnUrl = Comunes.HttpBasePath() + "controlcamionplanta";

                    return View("VoucherOrigen", voucher);
                }
                catch
                {
                    var rv = controlCamionPlanta.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crearvoucherllegadaorigen", controlCamionPlanta);
        }

        public ActionResult RegistrarPasosPlantaOrigenPaso1()
        {
            CheckPermisoAndRedirect(141);
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult RegistrarPasosPlantaOrigenPaso1(FormCollection formValues)
        {
            CheckPermisoAndRedirect(141);
            string msgerr = "";
            string msgok = "";
            string firma = Request["Firma"];
            var items = dc.LOG_ControlCamionPlanta.Where(x => x.Firma == firma && x.PasoActual <= 4);
            if (items.Count() == 0)
            {
                //Ingreso directo por URL o el codigo de barra no es valido
                msgerr = "El código de barra no es válido";
            }
            else if (items.Count() > 3)
            {
                //Completaron los pasos ingreso por porteria, inicio carga, termino carga, salida porteria
                msgerr = "Ya se registraron todos los pasos en planta de origen";
            }
            else
            {
                return RedirectToAction("registrarpasosplantaorigenpaso2", new { firma = firma });
            }
            ViewData["msgerr"] = msgerr;
            ViewData["msgok"] = msgok;
            return View("registrarpasosplantaorigenpaso1");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RegistrarPasosPlantaOrigenPaso2(string firma)
        {
            CheckPermisoAndRedirect(141);
            string msgerr = "";
            string msgok = "";
            string pasoActual = "";
            string proximoPaso = "";
            var items = dc.LOG_ControlCamionPlanta.Where(x => x.Firma == firma && x.PasoActual <= 4);
            if (items.Count() == 0)
            {
                //Ingreso directo por URL o el codigo de barra no es valido
                msgerr = "El código de barra no es válido";
            }
            else if (items.Count() > 3)
            {
                //Completaron los pasos ingreso por porteria, inicio carga, termino carga, salida porteria
                msgerr = "Ya se registraron todos los pasos en planta de origen";
            }
            else
            {
                var controlCamionPlanta = dc.LOG_ControlCamionPlanta.Where(x => x.Firma == firma).OrderByDescending(x => x.PasoActual).First();
                pasoActual = dc.LOG_EstadoControlCamionPlanta.Single(x => x.IdEstadoControlCamionPlanta == controlCamionPlanta.PasoActual).Descripcion;
                proximoPaso = dc.LOG_EstadoControlCamionPlanta.Single(x => x.IdEstadoControlCamionPlanta == (controlCamionPlanta.PasoActual + 1)).Descripcion;

                ViewData["firma"] = firma;
                ViewData["pasoActual"] = pasoActual;
                ViewData["proximoPaso"] = proximoPaso;
                return View("registrarpasosplantaorigenpaso2");
            }

            ViewData["msgerr"] = msgerr;
            ViewData["msgok"] = msgok;
            return View("registrarpasosplantaorigenpaso1");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult RegistrarPasosPlantaOrigenPaso2(FormCollection formValues)
        {
            CheckPermisoAndRedirect(141);
            string firma = Request["Firma"];
            var ultimoControlCamionPlanta = dc.LOG_ControlCamionPlanta.Where(x => x.Firma == firma).OrderByDescending(x => x.PasoActual).First();
            var pasoActual = dc.LOG_EstadoControlCamionPlanta.Single(x => x.IdEstadoControlCamionPlanta == ultimoControlCamionPlanta.PasoActual);
            var proximoPaso = dc.LOG_EstadoControlCamionPlanta.Single(x => x.IdEstadoControlCamionPlanta == (ultimoControlCamionPlanta.PasoActual + 1));

            var controlCamionPlanta = new LOG_ControlCamionPlanta();
            controlCamionPlanta.Barcode = "";
            controlCamionPlanta.IdCamion = ultimoControlCamionPlanta.IdCamion;
            controlCamionPlanta.IdPedido = ultimoControlCamionPlanta.IdPedido;
            controlCamionPlanta.IdEstadoControlCamionPlanta = proximoPaso.IdEstadoControlCamionPlanta;
            controlCamionPlanta.Firma = firma;
            controlCamionPlanta.UserIns = User.Identity.Name;
            controlCamionPlanta.FechaHoraIns = DateTime.Now;
            controlCamionPlanta.IpIns = RemoteAddr();
            controlCamionPlanta.PasoActual = (ultimoControlCamionPlanta.PasoActual + 1);
            dc.LOG_ControlCamionPlanta.InsertOnSubmit(controlCamionPlanta);
            dc.SubmitChanges();

            return RedirectToAction("index", new { idPedido = ultimoControlCamionPlanta.IdPedido });
        }

        public ActionResult CrearVoucherLlegadaDestino()
        {
            CheckPermisoAndRedirect(141);
            var controlCamionPlanta = new LOG_ControlCamionPlanta();
            return View("crearvoucherllegadadestino", controlCamionPlanta);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult CrearVoucherLlegadaDestino(LOG_ControlCamionPlanta controlCamionPlanta)
        {
            CheckPermisoAndRedirect(141);
            if (ModelState.IsValid)
            {
                try
                {
                    var pedido = dc.LOG_Pedido.Single(x => x.IdPedido == controlCamionPlanta.IdPedido);
                    var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                    var asignacioncamion = dc.LOG_AsignacionCamion.Single(x => x.IdPedido == pedido.IdPedido);
                    var camion = dc.LOG_Camion.Single(x => x.IdCamion == asignacioncamion.IdCamion);
                    var transportista = dc.LOG_Transportista.Single(x => x.IdTransportista == camion.IdTransportista);

                    controlCamionPlanta.IdCamion = camion.IdCamion;
                    controlCamionPlanta.IdPedido = pedido.IdPedido;
                    controlCamionPlanta.IdEstadoControlCamionPlanta = 5;//Llegada a portería de destino
                    controlCamionPlanta.Firma = UniqueId.GetUniqueKey();
                    controlCamionPlanta.UserIns = User.Identity.Name;
                    controlCamionPlanta.FechaHoraIns = DateTime.Now;
                    controlCamionPlanta.IpIns = RemoteAddr();
                    controlCamionPlanta.PasoActual = 5;

                    Code128BarcodeDraw bdf = BarcodeDrawFactory.Code128WithChecksum;

                    string base64String = "";

                    using (MemoryStream s = new MemoryStream(imageToByteArray(bdf.Draw(controlCamionPlanta.Firma, 80, 3)) as byte[]))
                    {
                        byte[] imageBytes = s.ToArray();
                        base64String = Convert.ToBase64String(imageBytes);
                    }

                    controlCamionPlanta.Barcode = base64String;

                    dc.LOG_ControlCamionPlanta.InsertOnSubmit(controlCamionPlanta);
                    dc.SubmitChanges();

                    string Template = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/voucherdestino_template.html"), Encoding.UTF8);
                    RepTemp(ref Template, "FECHA", controlCamionPlanta.FechaHoraIns.ToString("dd/MM/yyyy hh:mm:ss"));
                    RepTemp(ref Template, "DESTINO", asignacionpedido.Origen.ToUpper());
                    RepTemp(ref Template, "TRANSPORTISTA", transportista.Nombre.ToUpper());
                    RepTemp(ref Template, "CAMION", camion.Patente.ToUpper());
                    RepTemp(ref Template, "IMGSRC", string.Format("data:image/gif;base64,{0}", base64String));
                    RepTemp(ref Template, "FIRMADIGITAL", controlCamionPlanta.Firma);

                    try
                    {
                        string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/logistica/vouchers"), string.Format("{0}.pdf", controlCamionPlanta.Firma));
                        var htmlContent = Template;
                        var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
                        System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO
                    }
                    catch
                    {
                        throw new Exception("No se pudo generar el archivo PDF");
                    }

                    var voucher = new VoucherViewModel();
                    voucher.Camion = camion.Patente.ToUpper();
                    voucher.Fecha = controlCamionPlanta.FechaHoraIns.ToString("dd/MM/yyyy hh:mm:ss");
                    voucher.FirmaDigital = controlCamionPlanta.Firma;
                    voucher.ImgSrc = string.Format("data:image/gif;base64,{0}", base64String);
                    voucher.Destino = asignacionpedido.Destino.ToUpper();
                    voucher.Transportista = transportista.Nombre.ToUpper();
                    voucher.ReturnUrl = Comunes.HttpBasePath() + "controlcamionplanta";

                    return View("VoucherDestino", voucher);
                }
                catch
                {
                    var rv = controlCamionPlanta.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crearvoucherllegadadestino", controlCamionPlanta);
        }

        public ActionResult RegistrarPasosPlantaDestinoPaso1()
        {
            CheckPermisoAndRedirect(141);
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult RegistrarPasosPlantaDestinoPaso1(FormCollection formValues)
        {
            CheckPermisoAndRedirect(141);
            string msgerr = "";
            string msgok = "";
            string firma = Request["Firma"];
            var items = dc.LOG_ControlCamionPlanta.Where(x => x.Firma == firma && x.PasoActual > 4 && x.PasoActual <= 8);
            if (items.Count() == 0)
            {
                //Ingreso directo por URL o el codigo de barra no es valido
                msgerr = "El código de barra no es válido";
            }
            else if (items.Count() > 3)
            {
                //Completaron los pasos ingreso por porteria, inicio carga, termino carga, salida porteria
                msgerr = "Ya se registraron todos los pasos en planta de destino";
            }
            else
            {
                return RedirectToAction("registrarpasosplantadestinopaso2", new { firma = firma });
            }
            ViewData["msgerr"] = msgerr;
            ViewData["msgok"] = msgok;
            return View("registrarpasosplantadestinopaso1");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RegistrarPasosPlantaDestinoPaso2(string firma)
        {
            CheckPermisoAndRedirect(141);
            string msgerr = "";
            string msgok = "";
            string pasoActual = "";
            string proximoPaso = "";
            var items = dc.LOG_ControlCamionPlanta.Where(x => x.Firma == firma && x.PasoActual > 4 && x.PasoActual <= 8);
            if (items.Count() == 0)
            {
                //Ingreso directo por URL o el codigo de barra no es valido
                msgerr = "El código de barra no es válido";
            }
            else if (items.Count() > 3)
            {
                //Completaron los pasos ingreso por porteria, inicio carga, termino carga, salida porteria
                msgerr = "Ya se registraron todos los pasos en planta de destino";
            }
            else
            {
                var controlCamionPlanta = dc.LOG_ControlCamionPlanta.Where(x => x.Firma == firma).OrderByDescending(x => x.PasoActual).First();
                pasoActual = dc.LOG_EstadoControlCamionPlanta.Single(x => x.IdEstadoControlCamionPlanta == controlCamionPlanta.PasoActual).Descripcion;
                proximoPaso = dc.LOG_EstadoControlCamionPlanta.Single(x => x.IdEstadoControlCamionPlanta == (controlCamionPlanta.PasoActual + 1)).Descripcion;

                ViewData["firma"] = firma;
                ViewData["pasoActual"] = pasoActual;
                ViewData["proximoPaso"] = proximoPaso;
                return View("registrarpasosplantadestinopaso2");
            }

            ViewData["msgerr"] = msgerr;
            ViewData["msgok"] = msgok;
            return View("registrarpasosplantadestinopaso1");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult RegistrarPasosPlantaDestinoPaso2(FormCollection formValues)
        {
            CheckPermisoAndRedirect(141);
            string firma = Request["Firma"];
            var ultimoControlCamionPlanta = dc.LOG_ControlCamionPlanta.Where(x => x.Firma == firma).OrderByDescending(x => x.PasoActual).First();
            var pasoActual = dc.LOG_EstadoControlCamionPlanta.Single(x => x.IdEstadoControlCamionPlanta == ultimoControlCamionPlanta.PasoActual);
            var proximoPaso = dc.LOG_EstadoControlCamionPlanta.Single(x => x.IdEstadoControlCamionPlanta == (ultimoControlCamionPlanta.PasoActual + 1));

            var controlCamionPlanta = new LOG_ControlCamionPlanta();
            controlCamionPlanta.Barcode = "";
            controlCamionPlanta.IdCamion = ultimoControlCamionPlanta.IdCamion;
            controlCamionPlanta.IdPedido = ultimoControlCamionPlanta.IdPedido;
            controlCamionPlanta.IdEstadoControlCamionPlanta = proximoPaso.IdEstadoControlCamionPlanta;
            controlCamionPlanta.Firma = firma;
            controlCamionPlanta.UserIns = User.Identity.Name;
            controlCamionPlanta.FechaHoraIns = DateTime.Now;
            controlCamionPlanta.IpIns = RemoteAddr();
            controlCamionPlanta.PasoActual = (ultimoControlCamionPlanta.PasoActual + 1);
            dc.LOG_ControlCamionPlanta.InsertOnSubmit(controlCamionPlanta);
            dc.SubmitChanges();

            return RedirectToAction("index", new { idPedido = ultimoControlCamionPlanta.IdPedido });
        }

        private byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }
    }
}
