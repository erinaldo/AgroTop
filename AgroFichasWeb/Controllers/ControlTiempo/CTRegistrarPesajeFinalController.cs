using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.ControlTiempo
{
    [WebsiteAuthorize]
    public class CTRegistrarPesajeFinalController : BaseApplicationController
    {
        //
        // GET: /CTRegistrarPesajeFinal/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        #region index
        public CTRegistrarPesajeFinalController()
        {
            SetCurrentModulo(9);
        }

        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(234);

            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = from us in dc.PlantaUsuario
                                                  join pl in dc.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
                                                  where us.UserID == CurrentUser.UserID
                                                  && pl.Habilitado
                                                  select new SelectListItem
                                                  {
                                                      Value = pl.IdPlantaProduccion.ToString(),
                                                      Text = pl.Nombre,
                                                      Selected = IdPlantaProduccionSelect == pl.IdPlantaProduccion
                                                  };

            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            ViewData["PlantasProduccion"] = plantas;

            List<CTR_ControlTiempo> controlesTiempo = dc.CTR_ControlTiempo.Where(X => X.IdEstado == 2 && X.Habilitado == true && X.IdPlantaProduccion == IdPlantaProduccionSelect).ToList();
            return View(controlesTiempo);
        }
        #endregion 

        #region registrar
        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(234);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2 && X.Habilitado == true);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            ViewBag.Romana = (from c in dc.CTR_MantenedorRomana where (c.EsPlanta == true && c.Nombre == controlTiempo.PlantaProduccion.Nombre && c.Vigente == true) select c).SingleOrDefault();
            ViewBag.Lineas = (from c in dc.CTR_PlanificacionSemanal_Detalle where (c.IdPlanificacionSemanal == controlTiempo.IdPlanificacionSemanal && c.Saldo > 0) select c).ToList();
            ViewBag.LineasAsociadas = (from c in dc.CTR_ControlTiempo_Detalle where (c.IdControlTiempo == controlTiempo.IdControlTiempo && c.NumeroGuia == null) select c).ToList();

            return View(controlTiempo);
        }

        [HttpPost]
        public ActionResult Registrar(int id, FormCollection formValues, string documentos_asignados)
        {
            CheckPermisoAndRedirect(234);

            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2 && X.Habilitado == true);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            if (ModelState.IsValid)
            {
                try
                {

                    UpdateModel(controlTiempo, new string[] { "PesoFinal" });

                    //if (controlTiempo.ValidacionNumeroDeGuiaYPesoFinal(controlTiempo, ModelState))
                    // {
                    DetalleControlTiempo(controlTiempo, documentos_asignados);

                    controlTiempo.FechaPesajeFinal = DateTime.Now;
                    controlTiempo.FechaHoraUpd = DateTime.Now;
                    controlTiempo.IpUpd = RemoteAddr();
                    controlTiempo.UserUpd = User.Identity.Name;
                    dc.SubmitChanges();



                    //return Redirect("~/ctregistrarpesajefinal");


                    //}
                }
                catch
                {
                    var rv = controlTiempo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            ViewBag.Romana = (from c in dc.CTR_MantenedorRomana where (c.EsPlanta == true && c.Nombre == controlTiempo.PlantaProduccion.Nombre && c.Vigente == true) select c).SingleOrDefault();
            ViewBag.Lineas = (from c in dc.CTR_PlanificacionSemanal_Detalle where (c.IdPlanificacionSemanal == controlTiempo.IdPlanificacionSemanal && c.Saldo > 0) select c).ToList();
            ViewBag.LineasAsociadas = (from c in dc.CTR_ControlTiempo_Detalle where (c.IdControlTiempo == controlTiempo.IdControlTiempo && c.NumeroGuia == null) select c).ToList();
            return View("registrar", controlTiempo);
        }

        public ActionResult RegistrarSoloPeso(int id, decimal peso)
        {
            CheckPermisoAndRedirect(234);
            CTR_ControlTiempo controltiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2 && X.Habilitado == true);
            if (controltiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            try
            {
                controltiempo.PesoFinal = peso;
                controltiempo.FechaPesajeFinal = DateTime.Now;
                controltiempo.FechaHoraUpd = DateTime.Now;
                controltiempo.IpUpd = RemoteAddr();
                controltiempo.UserUpd = User.Identity.Name;
                dc.SubmitChanges();
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "Asignacion de peso final en control tiempo N° " + controltiempo.IdControlTiempo + " ha terminado con errores. No se ha actualizado", okMsg = "" });
            }

            return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha asignado un total de " + controltiempo.PesoFinal.Value.ToString("N0") + " kgs al control tiempo N° " + controltiempo.IdControlTiempo });
        }

        private bool DetalleControlTiempo(CTR_ControlTiempo obj, string lineas_asociadas)
        {
            bool resultado = false;
            try
            {
                string[] lineas = lineas_asociadas.Split('-');

                foreach (var linea in lineas)
                {
                    string[] contenidos = linea.Split(',');
                    CTR_ControlTiempo_Detalle obj_detalle = new CTR_ControlTiempo_Detalle()
                    {
                        IdControlTiempo = obj.IdControlTiempo,
                        IdPlanificacionDetalle = Convert.ToInt32(contenidos[0]),
                        PesoAsociado = Convert.ToDecimal(contenidos[2])
                    };
                    dc.CTR_ControlTiempo_Detalle.InsertOnSubmit(obj_detalle);
                    dc.SubmitChanges();

                    /*CTR_PlanificacionSemanal_Detalle obj_planificacion = (from c in dc.CTR_PlanificacionSemanal_Detalle where c.IdPlanificacionDetalle == obj_detalle.IdPlanificacionDetalle select c).FirstOrDefault();
                    obj_planificacion.Saldo -= obj_detalle.PesoAsociado;

                    UpdateModel(obj_planificacion, new string[] { "Saldo" });
                    dc.SubmitChanges();*/


                    List<CTR_PlanificacionSemanal_Detalle> listPlan = (from c in dc.CTR_PlanificacionSemanal_Detalle where c.NumeroDocumento == contenidos[1] select c).ToList();

                    foreach (var item in listPlan)
                    {
                        item.Saldo -= obj_detalle.PesoAsociado;
                        dc.SubmitChanges();
                    }


                }
                resultado = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return resultado;
        }
        [HttpPost]
        public ActionResult InsertarGuias(int id_control, string aux_guias, int NumeroGuia)
        { 

            string[] detalles = aux_guias.Split(',');
            foreach (var item in detalles)
            {
                CTR_ControlTiempo_Detalle obj = (from c in dc.CTR_ControlTiempo_Detalle where c.IdControlTiempoDetalle == Convert.ToInt32(item) select c).FirstOrDefault();
                obj.NumeroGuia = NumeroGuia;
                UpdateModel(obj, new string[] { "NumeroGuia" });
                dc.SubmitChanges();
            }

            return RedirectToAction("Registrar", new { id = id_control });
        }

        public ActionResult PasarASalida(int id)
        {
            CheckPermisoAndRedirect(234);

            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            var guias = (from c in dc.CTR_ControlTiempo_Detalle where (c.IdControlTiempo == controlTiempo.IdControlTiempo && c.NumeroGuia != null) select c).ToList();

            if (controlTiempo.PesoFinal == null) return Json(new Response { IsSuccess = false, Message = "No existe peso final en base de datos. El camión no puede pasar a salida." }, JsonRequestBehavior.AllowGet);

            if (guias.Count() == 0) return Json(new Response { IsSuccess = false, Message = "No existen guias asociadas. EL camión no puede pasar a salida." }, JsonRequestBehavior.AllowGet);


            controlTiempo.IdEstado = 3;
            UpdateModel(controlTiempo, new string[] { "IdEstado" });
            dc.SubmitChanges();

            return Json(new Response { IsSuccess = true}, JsonRequestBehavior.AllowGet);

        }
        #endregion


        public ActionResult GenerarTicket(int id)
        {
            CheckPermisoAndRedirect(233);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2 && X.Habilitado == true  );
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            List<CTR_ControlTiempo_Detalle> detalle = (from c in dc.CTR_ControlTiempo_Detalle where c.IdControlTiempo == id select c).ToList();
            string numeros = "";
            if (detalle.Count > 0)
            {
                foreach (var item in detalle)
                {
                    if (!numeros.Contains(item.NumeroGuia.ToString()))
                    {
                        if (numeros.Length == 0)
                        {
                            numeros += item.NumeroGuia.ToString();
                        }
                        else
                        {
                            numeros = numeros + " - " + item.NumeroGuia.ToString();
                        }
                    }
                }
            }
            else
            {
                numeros = "No existen números asignados";
            }


            TicketPdf ticketPdf = new TicketPdf
            {
                Imprimir = true,
                RazonSocial = controlTiempo.Empresa.RazonSocial,
                Comuna = controlTiempo.PlantaProduccion.Comuna.Nombre,
                Actividad = controlTiempo.Empresa.Actividad,
                Ciudad = controlTiempo.PlantaProduccion.Comuna.Provincia.Nombre,
                Rut = controlTiempo.Empresa.Rut,
                Fono = controlTiempo.TelefonoChofer,
                Direccion = controlTiempo.Empresa.Direccion,
                Email = controlTiempo.Empresa.Email,
                Patente = controlTiempo.Patente,
                TipoCamion = controlTiempo.TipoCamion,
                Empresa = controlTiempo.NombreTransportista,
                FechaEntrada = controlTiempo.FechaLlegada,
                FechaSalida = controlTiempo.FechaPesajeFinal,
                Chofer = controlTiempo.RutChofer,
                Transportista = controlTiempo.NombreChofer,
                TipoOperacion = "",
                NumeroGuia = numeros,
                Observacion = controlTiempo.Observaciones,
                PesoInicial = controlTiempo.PesoInicial.Value
            };
            if (controlTiempo.PesoFinal == null)
            {
                ticketPdf.PesoFinal = 0;
            }
            else
            {
                ticketPdf.PesoFinal = controlTiempo.PesoFinal.Value;
            }

            FileStreamResult fileStream = (FileStreamResult)ticketPdf.TicketMain();

            return fileStream;
        }

        public ActionResult ReasignarLineas(string jsonResult, int id_control)
        {

            DataTable dt = (DataTable)JsonConvert.DeserializeObject(jsonResult, (typeof(DataTable)));
            foreach (DataRow item in dt.Rows)
            {
                CTR_ControlTiempo_Detalle obj_control = (from c in dc.CTR_ControlTiempo_Detalle where c.IdControlTiempoDetalle == Convert.ToInt32(item["IdControlTiempoDetalle"].ToString()) select c).FirstOrDefault();

                List<CTR_PlanificacionSemanal_Detalle> listPlan = (from c in dc.CTR_PlanificacionSemanal_Detalle where c.NumeroDocumento == obj_control.CTR_PlanificacionSemanal_Detalle.NumeroDocumento select c).ToList();

                foreach (var plan in listPlan)
                {
                    plan.Saldo += obj_control.PesoAsociado;
                    dc.SubmitChanges();
                }

                dc.CTR_ControlTiempo_Detalle.DeleteOnSubmit(obj_control);
                dc.SubmitChanges();

            }
            return Json(new Response { IsSuccess = true, Id = id_control }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EnviarTicket(int id)
        {
            CheckPermisoAndRedirect(233);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            if (string.IsNullOrEmpty(controlTiempo.EmailChofer)) { return RedirectToAction("EmailResponse", new { errMsg = "El chofer no tiene un correo electrónico asociado", okMsg = "" }); }

            List<CTR_ControlTiempo_Detalle> detalle = (from c in dc.CTR_ControlTiempo_Detalle where c.IdControlTiempo == id select c).ToList();
            string numeros = "";
            if (detalle.Count > 0)
            {
                foreach (var item in detalle)
                {
                    if (!numeros.Contains(item.NumeroGuia.ToString()))
                    {
                        if (numeros.Length == 0)
                        {
                            numeros += item.NumeroGuia.ToString();
                        }
                        else
                        {
                            numeros = numeros + " - " + item.NumeroGuia.ToString();
                        }
                    }
                }
            }
            else
            {
                numeros = "No existen números asignados";
            }

            MailMessage mail = new MailMessage();
            TicketPdf ticketPdf = new TicketPdf
            {
                Imprimir = false,
                RazonSocial = controlTiempo.Empresa.RazonSocial,
                Comuna = controlTiempo.PlantaProduccion.Comuna.Nombre,
                Actividad = controlTiempo.Empresa.Actividad,
                Ciudad = controlTiempo.PlantaProduccion.Comuna.Provincia.Nombre,
                Rut = controlTiempo.Empresa.Rut,
                Fono = controlTiempo.TelefonoChofer,
                Direccion = controlTiempo.Empresa.Direccion,
                Email = controlTiempo.Empresa.Email,
                Patente = controlTiempo.Patente,
                TipoCamion = controlTiempo.TipoCamion,
                Empresa = controlTiempo.Empresa.Nombre,
                FechaEntrada = controlTiempo.FechaLlegada,
                FechaSalida = controlTiempo.FechaPesajeFinal,
                Chofer = controlTiempo.RutChofer,
                Transportista = controlTiempo.NombreTransportista,
                TipoOperacion = "",
                NumeroGuia = numeros,
                Observacion = controlTiempo.Observaciones,
                PesoFinal = controlTiempo.PesoFinal,
                PesoInicial = controlTiempo.PesoInicial
            };
            var file = ticketPdf.TicketMain();
            mail.From = new MailAddress("emailstests2021@gmail.com", "Notificación ticket");
            mail.To.Add(controlTiempo.EmailChofer);
            mail.Subject = "Este es un correo de prueba";
            mail.Body = "Este es un cuerpo de prueba";
            System.Net.Mail.Attachment attachment;
            Stream stream = (Stream)file;
            attachment = new System.Net.Mail.Attachment(stream, "Ticket.pdf", "application/pdf");
            mail.Attachments.Add(attachment);

            System.Net.Mail.SmtpClient objSmtp = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("emailstests2021@gmail.com", "FinalFantasy9")

            };
            objSmtp.Send(mail);

            return RedirectToAction("EmailResponse", new { errMsg = "", okMsg = "El correo electrónico se ha enviado correctamente" });
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(233);
            ViewData["UserID"] = CurrentUser.UserID;
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            return View(controlTiempo);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(233);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controlTiempo == null) { return RedirectToAction("editar", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(controlTiempo, new string[] { "IdEmpresa", "IdProducto", "IdCliente" });

                    controlTiempo.FechaHoraUpd = DateTime.Now;
                    controlTiempo.IpUpd = RemoteAddr();
                    controlTiempo.UserUpd = User.Identity.Name;
                    dc.SubmitChanges();
                    return RedirectToAction("registrar", new { id = controlTiempo.IdControlTiempo });
                }
                catch
                {
                    var rv = controlTiempo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("editar", new { id = controlTiempo.IdControlTiempo });
        }

        public ActionResult RegistrarTraslado(int id)
        {
            CheckPermisoAndRedirect(234);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            return View(controlTiempo);
        }
        [HttpPost]
        public ActionResult RegistrarTraslado(int id, FormCollection formValues, int NumeroGuia, string DesCodigo, int Cantidad)
        {
            CheckPermisoAndRedirect(234);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            CTR_PlanificacionSemanal_Detalle obj_detalle = dc.CTR_PlanificacionSemanal_Detalle.SingleOrDefault(X => X.IdPlanificacionSemanal == controlTiempo.IdPlanificacionSemanal);
            if (obj_detalle == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la planificacion asociada", okMsg = "" }); }

            UpdateModel(controlTiempo, new string[] { "PesoFinal" });

            controlTiempo.FechaPesajeFinal = DateTime.Now;
            controlTiempo.FechaHoraUpd = DateTime.Now;
            controlTiempo.IpUpd = RemoteAddr();
            controlTiempo.UserUpd = User.Identity.Name;
            controlTiempo.IdEstado = 3;
            dc.SubmitChanges();

            obj_detalle.NombreProducto = DesCodigo;

            dc.SubmitChanges();

            CTR_ControlTiempo_Detalle obj_detalle_control = new CTR_ControlTiempo_Detalle()
            {
                IdControlTiempo = controlTiempo.IdControlTiempo,
                IdPlanificacionDetalle = obj_detalle.IdPlanificacionDetalle,
                PesoAsociado = Cantidad,
                NumeroGuia = NumeroGuia
            };
            dc.CTR_ControlTiempo_Detalle.InsertOnSubmit(obj_detalle_control);
            dc.SubmitChanges();



            return Redirect("~/ctregistrarpesajefinal");
        }

        public ActionResult RegistrarSoloPesoTraslado(int id, decimal peso)
        {
            CheckPermisoAndRedirect(234);
            CTR_ControlTiempo controltiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controltiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            try
            {
                controltiempo.PesoFinal = peso;
                controltiempo.FechaPesajeFinal = DateTime.Now;
                controltiempo.FechaHoraUpd = DateTime.Now;
                controltiempo.IpUpd = RemoteAddr();
                controltiempo.UserUpd = User.Identity.Name;
                dc.SubmitChanges();

            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "Asignacion de peso final en control tiempo N° " + controltiempo.IdControlTiempo + " ha terminado con errores. No se ha actualizado", okMsg = "" });
            }

            return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha asignado un total de " + controltiempo.PesoFinal.Value.ToString("N0") + " kgs al control tiempo N° " + controltiempo.IdControlTiempo });
        }
        public ActionResult RegistrarSinPlanificacion(int id)
        {
            CheckPermisoAndRedirect(234);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            return View(controlTiempo);
        }

        [HttpPost]
        public ActionResult RegistrarSinPlanificacion(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(234);
            CTR_ControlTiempo controlTiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controlTiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(controlTiempo, new string[] { "NumeroGuia", "PesoFinal" });

                    if (controlTiempo.ValidacionNumeroDeGuiaYPesoFinal(controlTiempo, ModelState))
                    {
                        controlTiempo.IdEstado = 3;
                        controlTiempo.FechaPesajeFinal = DateTime.Now;
                        controlTiempo.FechaHoraUpd = DateTime.Now;
                        controlTiempo.IpUpd = RemoteAddr();
                        controlTiempo.UserUpd = User.Identity.Name;
                        dc.SubmitChanges();
                        return Redirect("~/ctregistrarpesajefinal");
                    }
                }
                catch
                {
                    var rv = controlTiempo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            return View("RegistrarSinPlanificacion", controlTiempo);
        }

        public ActionResult RegistrarSoloPesoSinPlanificacion(int id, decimal peso)
        {
            CheckPermisoAndRedirect(234);
            CTR_ControlTiempo controltiempo = dc.CTR_ControlTiempo.SingleOrDefault(X => X.IdControlTiempo == id && X.CTR_Estado.IdEstado == 2);
            if (controltiempo == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el control de tiempo", okMsg = "" }); }

            try
            {
                controltiempo.PesoFinal = peso;
                controltiempo.FechaPesajeFinal = DateTime.Now;
                controltiempo.FechaHoraUpd = DateTime.Now;
                controltiempo.IpUpd = RemoteAddr();
                controltiempo.UserUpd = User.Identity.Name;
                dc.SubmitChanges();

            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "Asignacion de peso final en control tiempo N° " + controltiempo.IdControlTiempo + " ha terminado con errores. No se ha actualizado", okMsg = "" });
            }

            return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha asignado un total de " + controltiempo.PesoFinal.Value.ToString("N0") + " kgs al control tiempo N° " + controltiempo.IdControlTiempo });
        }

        public ActionResult GetCodigo(int IdBodega1, int IdBodega2)
        {
            Bodega obj_1 = dc.Bodega.Where(c => c.IdBodega == IdBodega1).FirstOrDefault();
            Bodega obj_2 = dc.Bodega.Where(c => c.IdBodega == IdBodega2).FirstOrDefault();

            return Json(new[] { new { codigo_bodega_1 = obj_1.IDAvenatop, codigo_bodega_2 = obj_2.IDAvenatop } }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmailResponse()
        {
            return View();
        }


        #region GetTablas

        public ActionResult GetTablaDetallePlanificacionSemanal(int Id)
        {
            List<CTR_PlanificacionSemanal_Detalle> tabla = (from c in dc.CTR_PlanificacionSemanal_Detalle where (c.IdPlanificacionSemanal == Id && c.Saldo > 0) select c).ToList();
            return View(tabla);
        }

        public ActionResult GetTablaGuiasSinNumero(int Id)
        {
            List<CTR_ControlTiempo_Detalle> tabla = (from c in dc.CTR_ControlTiempo_Detalle where (c.IdControlTiempo == Id && c.NumeroGuia == null) select c).ToList();
            return View(tabla);
        }




        #endregion

    }
}