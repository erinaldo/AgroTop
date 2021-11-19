using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALLECargaGranelController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALLECargaGranelController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALLECargaGranel
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(387);


            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = from us in dcAgroFichas.PlantaUsuario
                                                  join pl in dcAgroFichas.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
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
            List<CAL_LECargaGranel> list = new List<CAL_LECargaGranel>();

            if (IdPlantaProduccionSelect == 0)
            {

                list = dcSoftwareCalidad.CAL_LECargaGranel.Where(X => X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).OrderByDescending(X => X.IdLECargaGranel).ToList();

            }
            else
            {
                list = dcSoftwareCalidad.CAL_LECargaGranel.Where(X => X.Habilitado == true && X.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect).OrderByDescending(X => X.FechaHoraIns).OrderByDescending(X => X.IdLECargaGranel).ToList();
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(388),
                                                      CheckPermiso(387),
                                                      CheckPermiso(389),
                                                      CheckPermiso(390));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(388);
            CAL_LECargaGranel listaEmpaque = new CAL_LECargaGranel();
            return View(listaEmpaque);
        }

       

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(389);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            return View("Crear", listaEmpaque);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(389);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(listaEmpaque, new string[] { "IdOrdenProduccion", "IdCarrier", "IdBarco", "NReserva", "PuertoEmbarque", "PuertoDestino", "DUS", "NFactura" });

                    listaEmpaque.FechaHoraUpd = DateTime.Now;
                    listaEmpaque.IpUpd = RemoteAddr();
                    listaEmpaque.UserUpd = User.Identity.Name;
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha editado la lista de empaque" });
                }
                catch
                {
                    var rv = listaEmpaque.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", listaEmpaque);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(390);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                listaEmpaque.Habilitado = false;
                listaEmpaque.UserUpd = User.Identity.Name;
                listaEmpaque.FechaHoraUpd = DateTime.Now;
                listaEmpaque.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("La lista de empaque {0} ha sido eliminada", listaEmpaque.IdLECargaGranel);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Contenedores(int id)
        {
            CheckPermisoAndRedirect(387);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            List<CAL_LECargaGranelContenedor> list = dcSoftwareCalidad.CAL_LECargaGranelContenedor.Where(X => X.IdLECargaGranel == listaEmpaque.IdLECargaGranel).ToList();

            ViewData["listaEmpaque"] = listaEmpaque;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(388),
                                                      CheckPermiso(387),
                                                      CheckPermiso(389),
                                                      CheckPermiso(390));
            return View(list);
        }

        public ActionResult AgregarContenedor(int id)
        {
            CheckPermisoAndRedirect(388);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            CAL_LECargaGranelContenedor contenedor = new CAL_LECargaGranelContenedor();
            contenedor.IdLECargaGranel = listaEmpaque.IdLECargaGranel;

            ViewData["listaEmpaque"] = listaEmpaque;
            return View(contenedor);
        }

        [HttpPost]
        public ActionResult Crear(CAL_LECargaGranel listaEmpaque, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(388);
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar Luego
                    listaEmpaque.certkey = "";
                    listaEmpaque.Contenedores = "0 cnt";
                    listaEmpaque.PesoNetoTotal = 0;
                    listaEmpaque.PesoBrutoTotal = 0;
                    // Rescatados del Form
                    listaEmpaque.Habilitado = true;
                    listaEmpaque.FechaHoraIns = DateTime.Now;
                    listaEmpaque.IpIns = RemoteAddr();
                    listaEmpaque.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_LECargaGranel.InsertOnSubmit(listaEmpaque);
                    dcSoftwareCalidad.SubmitChanges();
                    listaEmpaque.NotificarCreacion();

                    var obj_contenedor = new CAL_LECargaGranelContenedor();

                    List<SelectContenedoresListaEmpaqueGranel> listaConetenedores = obj_contenedor.GetContenedoresList(0);

                    if (listaConetenedores.Count() > 0)
                    {
                        foreach (var item in listaConetenedores)
                        {
                            obj_contenedor = new CAL_LECargaGranelContenedor()
                            {
                                IdDespachoCargaGranel = Convert.ToInt32(item.Value),
                                IdLECargaGranel = listaEmpaque.IdLECargaGranel,
                                Tara = 0,
                                SelloLinea = "0",
                                NGuiaDespacho = 0,
                                NContenedor = "",
                                NProducto = "",
                                Fecha = DateTime.Now,
                                PesoNeto = 0,
                                PesoBruto = 0,
                                VGM = 0,
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = User.Identity.Name,
                            };
                            dcSoftwareCalidad.CAL_LECargaGranelContenedor.InsertOnSubmit(obj_contenedor);
                            dcSoftwareCalidad.SubmitChanges();

                            obj_contenedor.NContenedor = obj_contenedor.CAL_DespachoCargaGranel.CAL_RITContenedor.NContenedor;
                            obj_contenedor.NProducto = obj_contenedor.CAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion.CAL_Subproducto.Nombre.ToUpper();
                            obj_contenedor.Fecha = obj_contenedor.CAL_DespachoCargaGranel.FechaHoraIns;

                            listaEmpaque.Contenedores = "--";
                            listaEmpaque.PesoNetoTotal += obj_contenedor.PesoNeto;
                            listaEmpaque.PesoBrutoTotal += obj_contenedor.PesoBruto;
                            dcSoftwareCalidad.SubmitChanges();

                        }
                        string errMsg = "";
                        string okMsg = "Se ha creado la lista de empaque número " + listaEmpaque.IdLECargaGranel + ", junto con un total de " + listaConetenedores.Count() + " contenedores.";
                        return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
                    }
                    else
                    {
                        string errMsg = "";
                        string okMsg = "No se encontró ningun contenedor a la hora de crear la lista de empaque";
                        return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
                    }

                }
                catch
                {
                    var rv = listaEmpaque.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View(listaEmpaque);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AgregarContenedor(CAL_LECargaGranelContenedor contenedor, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(388);
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar Luego
                    contenedor.NContenedor = "";
                    contenedor.NProducto = "";
                    contenedor.Fecha = DateTime.Now;
                    // Rescatados del Form
                    contenedor.PesoBruto = (contenedor.PesoNeto);
                    contenedor.VGM = (contenedor.PesoBruto + contenedor.Tara);
                    contenedor.FechaHoraIns = DateTime.Now;
                    contenedor.IpIns = RemoteAddr();
                    contenedor.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_LECargaGranelContenedor.InsertOnSubmit(contenedor);
                    dcSoftwareCalidad.SubmitChanges();
                    // Actualizado
                    contenedor.NContenedor = contenedor.CAL_DespachoCargaGranel.CAL_RITContenedor.NContenedor;
                    contenedor.NProducto = contenedor.CAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion.CAL_Subproducto.Nombre.ToUpper();
                    contenedor.Fecha = contenedor.CAL_DespachoCargaGranel.FechaHoraIns;

                    // Actualizado - CAL_LECargaGranel
                    CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == contenedor.IdLECargaGranel && X.Habilitado == true);

                    //int J = 0;
                    //List<CAL_GetTamañoContenedoresParaListaEmpaque_CargaGranelResult> tamaños = dcSoftwareCalidad.CAL_GetTamañoContenedoresParaListaEmpaque_CargaGranel(contenedor.CAL_DespachoCargaGranel.IdOrdenProduccion).ToList();
                    //string contenedores = "";
                    //foreach (CAL_GetTamañoContenedoresParaListaEmpaque_CargaGranelResult tamaño in tamaños)
                    //{
                    //    contenedores += string.Format("{0} cnt {1} ft.", tamaño.CountTamaño, tamaño.Tamaño);
                    //    J++;
                    //    if (J != tamaños.Count)
                    //        contenedores += ", ";
                    //}

                    listaEmpaque.Contenedores = "--";
                    listaEmpaque.PesoNetoTotal += contenedor.PesoNeto;
                    listaEmpaque.PesoBrutoTotal += contenedor.PesoBruto;
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Contenedores", new { id = listaEmpaque.IdLECargaGranel });
                }
                catch
                {
                    var rv = contenedor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            ViewData["listaEmpaque"] = contenedor.CAL_LECargaGranel;
            return View(contenedor);
        }

        public ActionResult EditarContenedor(int id, int IdDespachoCargaGranel)
        {
            CheckPermisoAndRedirect(389);
            CAL_LECargaGranelContenedor contenedor = dcSoftwareCalidad.CAL_LECargaGranelContenedor.SingleOrDefault(X => X.IdLECargaGranel == id && X.IdDespachoCargaGranel == IdDespachoCargaGranel);
            if (contenedor == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
            }

            contenedor.EditPesoBruto = contenedor.PesoBruto;
            contenedor.EditPesoNeto = contenedor.PesoNeto;
            ViewData["listaEmpaque"] = contenedor.CAL_LECargaGranel;
            return View("EditarContenedor", contenedor);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditarContenedor(int id, int IdDespachoCargaGranel, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(389);
            CAL_LECargaGranelContenedor contenedor = dcSoftwareCalidad.CAL_LECargaGranelContenedor.SingleOrDefault(X => X.IdLECargaGranel == id && X.IdDespachoCargaGranel == IdDespachoCargaGranel);
            if (contenedor == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(contenedor, new string[] { "NGuiaDespacho", "SelloLinea", "PesoNeto", "Tara", "EditPesoNeto", "EditPesoBruto" });

                    contenedor.PesoBruto = (contenedor.PesoNeto);
                    contenedor.VGM = (contenedor.PesoBruto + contenedor.Tara);

                    CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == contenedor.IdLECargaGranel && X.Habilitado == true);
                    List<CAL_LECargaGranelContenedor> contenedores = dcSoftwareCalidad.CAL_LECargaGranelContenedor.Where(X => X.IdLECargaGranel == contenedor.IdLECargaGranel).ToList();
                    listaEmpaque.PesoNetoTotal -= contenedor.EditPesoNeto;
                    listaEmpaque.PesoBrutoTotal -= contenedor.EditPesoBruto;
                    listaEmpaque.PesoNetoTotal += contenedor.PesoNeto;
                    listaEmpaque.PesoBrutoTotal += contenedor.PesoBruto;

                    contenedor.FechaHoraUpd = DateTime.Now;
                    contenedor.IpUpd = RemoteAddr();
                    contenedor.UserUpd = User.Identity.Name;
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Contenedores", new { id = listaEmpaque.IdLECargaGranel, errMsg = "", okMsg = "Se ha editado el contenedor" });
                }
                catch
                {
                    var rv = contenedor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["listaEmpaque"] = contenedor.CAL_LECargaGranel;
            return View("EditarContenedor", contenedor);
        }

        public ActionResult EliminarContenedor(int id, int IdDespachoCargaGranel)
        {
            CheckPermisoAndRedirect(390);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            CAL_LECargaGranelContenedor contenedor = dcSoftwareCalidad.CAL_LECargaGranelContenedor.SingleOrDefault(X => X.IdLECargaGranel == id && X.IdDespachoCargaGranel == IdDespachoCargaGranel);
            if (contenedor == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                listaEmpaque.PesoNetoTotal -= contenedor.PesoNeto;
                listaEmpaque.PesoBrutoTotal -= contenedor.PesoBruto;
                dcSoftwareCalidad.CAL_LECargaGranelContenedor.DeleteOnSubmit(contenedor);
                dcSoftwareCalidad.SubmitChanges();

                //int J = 0;
                //List<CAL_GetTamañoContenedoresParaListaEmpaque_CargaGranelResult> tamaños = dcSoftwareCalidad.CAL_GetTamañoContenedoresParaListaEmpaque_CargaGranel(listaEmpaque.IdOrdenProduccion).ToList();
                //string contenedores = "";
                //foreach (CAL_GetTamañoContenedoresParaListaEmpaque_CargaGranelResult tamaño in tamaños)
                //{
                //    contenedores += string.Format("{0} cnt {1} ft.", tamaño.CountTamaño, tamaño.Tamaño);
                //    J++;
                //    if (J != tamaños.Count)
                //        contenedores += ", ";
                //}

                //if (tamaños.Count == 0)
                //    contenedores = "0 cnt";

                listaEmpaque.Contenedores = "--";
                dcSoftwareCalidad.SubmitChanges();

                okMsg = "El contenedor ha sido eliminado de la lista de empaque";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Contenedores", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult CrearPDF(int id)
        {
            CheckPermisoAndRedirect(387);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            string cnts = "";
            if (listaEmpaque.GetTamañoContenedores().Count == 0)
            {
                cnts = "0 cnt.";
            }
            else
            {
                foreach (var tc in listaEmpaque.GetTamañoContenedores())
                {
                    cnts += string.Format("{0} cnt. {1}' ft.<br>", tc.CountContenedor, tc.Tamaño);
                }
            }

            string Factura = "";
            if (string.IsNullOrEmpty(listaEmpaque.NFactura))
            {
                Factura = "";
            }
            else
            {
                Factura = listaEmpaque.NFactura.ToString();
            }

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/lista_de_empaque_carga_a_granel_template.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "AÑO", DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "LOTE", listaEmpaque.CAL_OrdenProduccion.LoteComercial);
            RepTemp(ref htmlContent, "EMB", listaEmpaque.CAL_OrdenProduccion.CAL_Exportador.Nombre);
            RepTemp(ref htmlContent, "CSG", listaEmpaque.CAL_OrdenProduccion.GetCliente(listaEmpaque.CAL_OrdenProduccion.IdCliente).RazonSocial);
            RepTemp(ref htmlContent, "NAV", listaEmpaque.Carrier.Nombre);
            RepTemp(ref htmlContent, "RSV", listaEmpaque.NReserva);
            RepTemp(ref htmlContent, "CNT", cnts);
            RepTemp(ref htmlContent, "FACT", Factura);
            RepTemp(ref htmlContent, "PTOEMB", listaEmpaque.PuertoEmbarque);
            RepTemp(ref htmlContent, "PTODST", listaEmpaque.PuertoDestino);
            RepTemp(ref htmlContent, "BCO", listaEmpaque.Barco.Nombre);
            RepTemp(ref htmlContent, "DUS", listaEmpaque.DUS);
            RepTemp(ref htmlContent, "PSONET", listaEmpaque.PesoNetoTotal.ToString("N2"));
            RepTemp(ref htmlContent, "PSOBRT", listaEmpaque.PesoBrutoTotal.ToString("N2"));

            StringBuilder stringBuilder = new StringBuilder();

            //CABECERA
            stringBuilder.AppendLine("<thead>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine("<th scope='col'>Día</th>");
            stringBuilder.AppendLine("<th scope='col'>Guía de Despacho</th>");
            stringBuilder.AppendLine("<th scope='col'>Contenedor</th>");
            stringBuilder.AppendLine("<th scope='col'>Sello de Línea</th>");
            stringBuilder.AppendLine("<th scope='col'>Peso Neto</th>");
            stringBuilder.AppendLine("<th scope='col'>Peso Bruto</th>");
            stringBuilder.AppendLine("<th scope='col'>Tara</th>");
            stringBuilder.AppendLine("<th scope='col'>VGM</th>");
            stringBuilder.AppendLine("<th scope='col'>Producto</th>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("</thead>");
            //CABECERA
            stringBuilder.AppendLine("<tbody>");
            List<CAL_LECargaGranelContenedor> contenedores = dcSoftwareCalidad.CAL_LECargaGranelContenedor.Where(X => X.IdLECargaGranel == listaEmpaque.IdLECargaGranel).ToList();
            foreach (var contenedor in contenedores)
            {

                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine(string.Format("<td>{0:dd/MM/yyyy}</td>", contenedor.Fecha));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NGuiaDespacho));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NContenedor));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.SelloLinea));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.PesoNeto));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.PesoBruto));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.Tara));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.VGM));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NProducto));
                stringBuilder.AppendLine("</tr>");
            }
            stringBuilder.AppendLine("</tbody>");
            //PIE DE PÁGINA
            stringBuilder.AppendLine("<tfoot>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine("<td style='text-align: right' colspan ='4'><strong>TOTAL</strong>:</td>");
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedores.Sum(X => X.PesoNeto)));
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedores.Sum(X => X.PesoBruto)));
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedores.Sum(X => X.Tara)));
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedores.Sum(X => X.VGM)));
            stringBuilder.AppendLine("<td></td>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine(string.Format("<td class='ultima-linea' style='text-align: left' colspan ='2'><strong>FECHA</strong>: {0:dd/MM/yyyy}</td>", DateTime.Now));
            stringBuilder.AppendLine(string.Format("<td class='ultima-linea' style='text-align: center' colspan ='3'><strong>EMITIDO POR</strong>: {0}</td>", listaEmpaque.CAL_OrdenProduccion.GetUser(listaEmpaque.UserIns).FullName));
            stringBuilder.AppendLine("<td class='ultima-linea' style='text-align: left' colspan ='4'><strong>FIRMA</strong>:</td>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("</tfoot>");

            RepTemp(ref htmlContent, "CONTENEDORES", stringBuilder.ToString());

            try
            {
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/listadeempaque"), string.Format("{0}.html", certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/listadeempaque"), string.Format("{0}.pdf", certkey));
                byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()
                {
                    Orientation = NReco.PdfGenerator.PageOrientation.Landscape,
                    Margins = new NReco.PdfGenerator.PageMargins()
                    {
                        Bottom = 15,
                        Left = 15,
                        Right = 15,
                        Top = 15
                    },
                    Size = NReco.PdfGenerator.PageSize.Letter,
                }).GeneratePdf(htmlContent);
                System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO

                listaEmpaque.certkey = certkey;
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = listaEmpaque.IdLECargaGranel, certkey = certkey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/listadeempaque"), string.Format("IDCERT{0}_{1}.txt", listaEmpaque.IdLECargaGranel, certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine("No se pudo generar el PDF de la lista de empaque");
                writer.WriteLine(ex.ToString());
                writer.Close();

                return RedirectToAction("Index", new { errMsg = "No se pudo generar el PDF de la lista de empaque", okMsg = "" });
            }
        }

        public ActionResult DescargarPDF(int id, string certkey)
        {
            CheckPermisoAndRedirect(387);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/listadeempaque/{0}.pdf", listaEmpaque.certkey));
                return File(pdf, "application/pdf", String.Format("Lista de Empaque {0}-{1}.pdf", listaEmpaque.IdLECargaGranel, listaEmpaque.CAL_OrdenProduccion.LoteComercial));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF de la lista de empaque", okMsg = "" });
            }
        }

        public ActionResult CrearPDF_en(int id)
        {
            CheckPermisoAndRedirect(387);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            string cnts = "";
            if (listaEmpaque.GetTamañoContenedores().Count == 0)
            {
                cnts = "0 cnt.";
            }
            else
            {
                foreach (var tc in listaEmpaque.GetTamañoContenedores())
                {
                    cnts += string.Format("{0} cnt. {1}' ft.<br>", tc.CountContenedor, tc.Tamaño);
                }
            }

            string Factura = "";
            if (string.IsNullOrEmpty(listaEmpaque.NFactura))
            {
                Factura = "";
            }
            else
            {
                Factura = listaEmpaque.NFactura.ToString();
            }

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/lista_de_empaque_carga_a_granel_ingles_template.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "AÑO", DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "LOTE", listaEmpaque.CAL_OrdenProduccion.LoteComercial);
            RepTemp(ref htmlContent, "EMB", listaEmpaque.CAL_OrdenProduccion.CAL_Exportador.Nombre);
            RepTemp(ref htmlContent, "CSG", listaEmpaque.CAL_OrdenProduccion.GetCliente(listaEmpaque.CAL_OrdenProduccion.IdCliente).RazonSocial);
            RepTemp(ref htmlContent, "NAV", listaEmpaque.Carrier.Nombre);
            RepTemp(ref htmlContent, "RSV", listaEmpaque.NReserva);
            RepTemp(ref htmlContent, "CNT", cnts);
            RepTemp(ref htmlContent, "FACT", Factura);
            RepTemp(ref htmlContent, "PTOEMB", listaEmpaque.PuertoEmbarque);
            RepTemp(ref htmlContent, "PTODST", listaEmpaque.PuertoDestino);
            RepTemp(ref htmlContent, "BCO", listaEmpaque.Barco.Nombre);
            RepTemp(ref htmlContent, "DUS", listaEmpaque.DUS);
            RepTemp(ref htmlContent, "PSONET", listaEmpaque.PesoNetoTotal.ToString("N2"));
            RepTemp(ref htmlContent, "PSOBRT", listaEmpaque.PesoBrutoTotal.ToString("N2"));

            StringBuilder stringBuilder = new StringBuilder();

            //CABECERA
            stringBuilder.AppendLine("<thead>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine("<th scope='col'>Day</th>");
            stringBuilder.AppendLine("<th scope='col'>Guide</th>");
            stringBuilder.AppendLine("<th scope='col'>Container</th>");
            stringBuilder.AppendLine("<th scope='col'>Seal</th>");
            stringBuilder.AppendLine("<th scope='col'>Net Weight</th>");
            stringBuilder.AppendLine("<th scope='col'>Gross Weight</th>");
            stringBuilder.AppendLine("<th scope='col'>Tara. Container</th>");
            stringBuilder.AppendLine("<th scope='col'>VGM</th>");
            stringBuilder.AppendLine("<th scope='col'>Product</th>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("</thead>");
            //CABECERA
            stringBuilder.AppendLine("<tbody>");
            List<CAL_LECargaGranelContenedor> contenedores = dcSoftwareCalidad.CAL_LECargaGranelContenedor.Where(X => X.IdLECargaGranel == listaEmpaque.IdLECargaGranel).ToList();
            foreach (var contenedor in contenedores)
            {

                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine(string.Format("<td>{0:dd/MM/yyyy}</td>", contenedor.Fecha));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NGuiaDespacho));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NContenedor));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.SelloLinea));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.PesoNeto));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.PesoBruto));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.Tara));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.VGM));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NProducto));
                stringBuilder.AppendLine("</tr>");
            }
            stringBuilder.AppendLine("</tbody>");
            //PIE DE PÁGINA
            stringBuilder.AppendLine("<tfoot>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine("<td style='text-align: right' colspan ='4'><strong>TOTAL</strong>:</td>");
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedores.Sum(X => X.PesoNeto)));
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedores.Sum(X => X.PesoBruto)));
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedores.Sum(X => X.Tara)));
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedores.Sum(X => X.VGM)));
            stringBuilder.AppendLine("<td></td>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine(string.Format("<td class='ultima-linea' style='text-align: left' colspan ='2'><strong>DATE</strong>: {0:dd/MM/yyyy}</td>", DateTime.Now));
            stringBuilder.AppendLine(string.Format("<td class='ultima-linea' style='text-align: center' colspan ='3'><strong>ISSUED BY</strong>: {0}</td>", listaEmpaque.CAL_OrdenProduccion.GetUser(listaEmpaque.UserIns).FullName));
            stringBuilder.AppendLine("<td class='ultima-linea' style='text-align: left' colspan ='4'><strong>SIGNATURE</strong>:</td>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("</tfoot>");

            RepTemp(ref htmlContent, "CONTENEDORES", stringBuilder.ToString());

            try
            {
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/listadeempaque"), string.Format("{0}.html", certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/listadeempaque"), string.Format("{0}.pdf", certkey));
                byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()
                {
                    Orientation = NReco.PdfGenerator.PageOrientation.Landscape,
                    Margins = new NReco.PdfGenerator.PageMargins()
                    {
                        Bottom = 15,
                        Left = 15,
                        Right = 15,
                        Top = 15
                    },
                    Size = NReco.PdfGenerator.PageSize.Letter,
                }).GeneratePdf(htmlContent);
                System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO

                listaEmpaque.certkey = certkey;
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF_en", new { id = listaEmpaque.IdLECargaGranel, certkey = certkey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/listadeempaque"), string.Format("IDCERT{0}_{1}.txt", listaEmpaque.IdLECargaGranel, certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine("No se pudo generar el PDF de la lista de empaque");
                writer.WriteLine(ex.ToString());
                writer.Close();

                return RedirectToAction("Index", new { errMsg = "No se pudo generar el PDF de la lista de empaque", okMsg = "" });
            }
        }

        public ActionResult DescargarPDF_en(int id, string certkey)
        {
            CheckPermisoAndRedirect(387);
            CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/listadeempaque/{0}.pdf", listaEmpaque.certkey));
                return File(pdf, "application/pdf", String.Format("Packing List {0}-{1}.pdf", listaEmpaque.IdLECargaGranel, listaEmpaque.CAL_OrdenProduccion.LoteComercial));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF de la lista de empaque", okMsg = "" });
            }
        }

        public ActionResult ReasignarContenedor(int id, int IdDespachoCargaGranel)
        {
            CheckPermisoAndRedirect(389);
            CAL_LECargaGranelContenedor contenedor = dcSoftwareCalidad.CAL_LECargaGranelContenedor.SingleOrDefault(X => X.IdLECargaGranel == id && X.IdDespachoCargaGranel == IdDespachoCargaGranel);
            if (contenedor == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
            }

            ViewData["listaEmpaque"] = contenedor.CAL_LECargaGranel;
            return View("ReasignarContenedor", contenedor);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ReasignarContenedor(int id, int IdDespachoCargaGranel, CAL_LECargaGranelContenedor contenedor, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(388);
            //var crear = AgregarContenedor(contenedor, formCollection);
            //var eliminar = EliminarContenedor(id, IdDespachoCargaGranel);
            if (ModelState.IsValid)
            {
                CAL_LECargaGranel listaEmpaqueDelete = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == id && X.Habilitado == true);
                if (listaEmpaqueDelete == null)
                {
                    return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
                }

                CAL_LECargaGranelContenedor contenedorDelete = dcSoftwareCalidad.CAL_LECargaGranelContenedor.SingleOrDefault(X => X.IdLECargaGranel == id && X.IdDespachoCargaGranel == IdDespachoCargaGranel);
                if (contenedorDelete == null)
                {
                    return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
                }

                try
                {
                    // Actualizar Luego
                    contenedor.NContenedor = "";
                    contenedor.NProducto = "";
                    contenedor.Fecha = DateTime.Now;
                    // Rescatados del Form
                    contenedor.PesoBruto = (contenedor.PesoNeto);
                    contenedor.VGM = (contenedor.PesoBruto + contenedor.Tara);
                    contenedor.FechaHoraIns = DateTime.Now;
                    contenedor.IpIns = RemoteAddr();
                    contenedor.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_LECargaGranelContenedor.InsertOnSubmit(contenedor);
                    dcSoftwareCalidad.SubmitChanges();
                    // Actualizado
                    contenedor.NContenedor = contenedor.CAL_DespachoCargaGranel.CAL_RITContenedor.NContenedor;
                    contenedor.NProducto = contenedor.CAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion.CAL_Subproducto.Nombre.ToUpper();
                    contenedor.Fecha = contenedor.CAL_DespachoCargaGranel.FechaHoraIns;

                    // Actualizado - CAL_LECargaGranel
                    CAL_LECargaGranel listaEmpaque = dcSoftwareCalidad.CAL_LECargaGranel.SingleOrDefault(X => X.IdLECargaGranel == contenedor.IdLECargaGranel && X.Habilitado == true);

                    listaEmpaque.Contenedores = "--";
                    listaEmpaque.PesoNetoTotal += contenedor.PesoNeto;
                    listaEmpaque.PesoBrutoTotal += contenedor.PesoBruto;
                    dcSoftwareCalidad.SubmitChanges();
                    //Elimina contenedor
                    listaEmpaqueDelete.PesoNetoTotal -= contenedorDelete.PesoNeto;
                    listaEmpaqueDelete.PesoBrutoTotal -= contenedorDelete.PesoBruto;
                    dcSoftwareCalidad.CAL_LECargaGranelContenedor.DeleteOnSubmit(contenedorDelete);
                    dcSoftwareCalidad.SubmitChanges();

                    listaEmpaqueDelete.Contenedores = "--";
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha Reasignado el contenedor" });
                }
                catch
                {
                    var rv = contenedor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            ViewData["listaEmpaque"] = contenedor.CAL_LECargaGranel;
            //return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha Reasignado el contenedor" });
            return View(contenedor);
        }

        #region --- Funciones PRIVADAS ---

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}