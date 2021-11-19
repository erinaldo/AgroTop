using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Response;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
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
    public class CALLEPalletsController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALLEPalletsController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALLEPallets
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
            List<CAL_LEPallets> list = new List<CAL_LEPallets>();

            if (IdPlantaProduccionSelect == 0) { 

                list = dcSoftwareCalidad.CAL_LEPallets.Where(X => X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).OrderByDescending(X => X.IdLEPallets).ToList();

            }
            else
            {
                list = dcSoftwareCalidad.CAL_LEPallets.Where(X => X.Habilitado == true && X.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect).OrderByDescending(X => X.FechaHoraIns).OrderByDescending(X => X.IdLEPallets).ToList();
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
            CAL_LEPallets listaEmpaque = new CAL_LEPallets();
            return View(listaEmpaque);
        }



        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(389);
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
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
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
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
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
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
                okMsg = String.Format("La lista de empaque {0} ha sido eliminada", listaEmpaque.IdLEPallets);
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
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            List<CAL_LEPalletsContenedor> list = dcSoftwareCalidad.CAL_LEPalletsContenedor.Where(X => X.IdLEPallets == listaEmpaque.IdLEPallets).ToList();

            ViewData["listaEmpaque"] = listaEmpaque;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(388),
                                                      CheckPermiso(387),
                                                      CheckPermiso(389),
                                                      CheckPermiso(390));
            return View(list);
        }

        public ActionResult AgregarContenedor(int id, int IdOrdenProduccion)
        {
            CheckPermisoAndRedirect(388);

            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == IdOrdenProduccion
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            CAL_LEPalletsContenedor contenedor = new CAL_LEPalletsContenedor();
            contenedor.IdLEPallets = listaEmpaque.IdLEPallets;

            ViewData["listaEmpaque"] = listaEmpaque;
            ViewData["ordenProduccion"] = ordenProduccion;
            return View(contenedor);
        }
        [HttpPost]
        public ActionResult Crear(CAL_LEPallets listaEmpaque, FormCollection formCollection)
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
                    dcSoftwareCalidad.CAL_LEPallets.InsertOnSubmit(listaEmpaque);
                    dcSoftwareCalidad.SubmitChanges();

                    listaEmpaque.NotificarCreacion();

                    CAL_LEPalletsContenedor palletsContenedor = new CAL_LEPalletsContenedor();

                    List<SelectContenedoresListaEmpaquePallets> selectList = palletsContenedor.GetContenedoresList(listaEmpaque.IdOrdenProduccion);

                    if (selectList.Count() > 0)
                    {
                        foreach (var item in selectList.ToList())
                        {
                            CAL_LEPallets objEmpaque = (from c in dcSoftwareCalidad.CAL_LEPallets select c).OrderByDescending(C=> C.IdLEPallets).FirstOrDefault();
                            CAL_LEPalletsContenedor obj = new CAL_LEPalletsContenedor();
                            obj.Fecha = DateTime.Now;
                            obj.NProducto = "";
                            CAL_GetSacosParaListaEmpaque_PalletsResult CntSacosPallet = dcSoftwareCalidad.CAL_GetSacosParaListaEmpaque_Pallets(listaEmpaque.IdOrdenProduccion, Convert.ToInt32(item.Value)).FirstOrDefault();
                            int CntSacos = 0;
                            if (CntSacosPallet.Sacos != null)
                            {
                                CntSacos = CntSacosPallet.Sacos.Value;
                            }
                            obj.IdLEPallets = objEmpaque.IdLEPallets;
                            obj.IdContenedor = Convert.ToInt32(item.Value);
                            obj.NContenedor = item.Text;
                            obj.PesoNeto = 0;
                            obj.PesoBruto = 0;
                            obj.SelloLinea = "0";
                            obj.Tara = 0;
                            obj.PesoBruto = 0;
                            obj.VGM = 0;
                            obj.FechaHoraIns = DateTime.Now;
                            obj.IpIns = RemoteAddr();
                            obj.UserIns = User.Identity.Name;
                            dcSoftwareCalidad.CAL_LEPalletsContenedor.InsertOnSubmit(obj);
                            dcSoftwareCalidad.SubmitChanges();
                            obj.Fecha = obj.FechaHoraIns;
                            obj.NProducto = obj.GetSubproducto(listaEmpaque.IdOrdenProduccion, Convert.ToInt32(item.Value));

                            CAL_LEPallets listaEmpaque_ = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == objEmpaque.IdLEPallets && X.Habilitado == true);
                            listaEmpaque_.Contenedores = "--";
                            listaEmpaque_.PesoNetoTotal += obj.PesoNeto;
                            listaEmpaque_.PesoBrutoTotal += obj.PesoBruto;

                            dcSoftwareCalidad.SubmitChanges();
                        }
                        string errMsg = "";
                        string okMsg = "Se ha creado correctamente la lista de empque con sus mantenedores";
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
        public ActionResult AgregarContenedor(CAL_LEPalletsContenedor contenedor, FormCollection formCollection, int IdOrdenProduccion)
        {
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == IdOrdenProduccion
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();

            CheckPermisoAndRedirect(388);
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar Luego
                    contenedor.Fecha = DateTime.Now;
                    contenedor.NProducto = "";
                    // Rescatados del Form
                    CAL_GetSacosParaListaEmpaque_PalletsResult CntSacosPallet = dcSoftwareCalidad.CAL_GetSacosParaListaEmpaque_Pallets(ordenProduccion.IdOrdenProduccion, contenedor.IdContenedor).FirstOrDefault();
                    int CntSacos = 0;
                    if (CntSacosPallet != null)
                    {
                        CntSacos = CntSacosPallet.Sacos.Value;
                    }

                    contenedor.PesoBruto = (contenedor.PesoNeto + (CntSacos * 0.08m)); //cnt sacos * 80 grs.
                    contenedor.VGM = (contenedor.PesoBruto + contenedor.Tara);
                    contenedor.FechaHoraIns = DateTime.Now;
                    contenedor.IpIns = RemoteAddr();
                    contenedor.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_LEPalletsContenedor.InsertOnSubmit(contenedor);
                    dcSoftwareCalidad.SubmitChanges();
                    // Actualizado
                    contenedor.Fecha = contenedor.FechaHoraIns;
                    contenedor.NProducto = contenedor.GetSubproducto(ordenProduccion.IdOrdenProduccion, contenedor.IdContenedor);

                    // Actualizado - CAL_LECargaGranel
                    CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == contenedor.IdLEPallets && X.Habilitado == true);

                    //int J = 0;
                    //List<CAL_GetTamañoContenedoresParaListaEmpaque_PalletsResult> tamaños = dcSoftwareCalidad.CAL_GetTamañoContenedoresParaListaEmpaque_Pallets(contenedor.CAL_LEPallets.IdOrdenProduccion).ToList();
                    //string contenedores = "";
                    //foreach (CAL_GetTamañoContenedoresParaListaEmpaque_PalletsResult tamaño in tamaños)
                    //{
                    //    contenedores += string.Format("{0} cont. {1}' ft.", tamaño.CountTamaño, tamaño.Tamaño);
                    //    J++;
                    //    if (J != tamaños.Count)
                    //        contenedores += ", ";
                    //}

                    listaEmpaque.Contenedores = "--";
                    listaEmpaque.PesoNetoTotal += contenedor.PesoNeto;
                    listaEmpaque.PesoBrutoTotal += contenedor.PesoBruto;
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Contenedores", new { id = listaEmpaque.IdLEPallets });
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
            ViewData["listaEmpaque"] = contenedor.CAL_LEPallets;
            ViewData["ordenProduccion"] = ordenProduccion;
            return View(contenedor);
        }

        public ActionResult EditarContenedor(int id, int IdContenedor)
        {
            CheckPermisoAndRedirect(389);
            CAL_LEPalletsContenedor contenedor = dcSoftwareCalidad.CAL_LEPalletsContenedor.SingleOrDefault(X => X.IdLEPallets == id && X.IdContenedor == IdContenedor);
            if (contenedor == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
            }

            contenedor.EditPesoBruto = contenedor.PesoBruto;
            contenedor.EditPesoNeto = contenedor.PesoNeto;
            ViewData["listaEmpaque"] = contenedor.CAL_LEPallets;
            return View("EditarContenedor", contenedor);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditarContenedor(int id, int IdContenedor, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(389);
            CAL_LEPalletsContenedor contenedor = dcSoftwareCalidad.CAL_LEPalletsContenedor.SingleOrDefault(X => X.IdLEPallets == id && X.IdContenedor == IdContenedor);
            if (contenedor == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(contenedor, new string[] { "NGuiaDespacho", "SelloLinea", "PesoNeto", "Tara", "EditPesoNeto", "EditPesoBruto" });

                    CAL_GetSacosParaListaEmpaque_PalletsResult CntSacosPallet = dcSoftwareCalidad.CAL_GetSacosParaListaEmpaque_Pallets(contenedor.CAL_LEPallets.IdOrdenProduccion, contenedor.IdContenedor).FirstOrDefault();
                    int CntSacos = 0;
                    if (CntSacosPallet != null)
                    {
                        CntSacos = CntSacosPallet.Sacos.Value;
                    }

                    contenedor.PesoBruto = (contenedor.PesoNeto + (CntSacos * 0.08m)); //cnt sacos * 80 grs.
                    contenedor.VGM = (contenedor.PesoBruto + contenedor.Tara);

                    CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == contenedor.IdLEPallets && X.Habilitado == true);
                    List<CAL_LEPalletsContenedor> contenedores = dcSoftwareCalidad.CAL_LEPalletsContenedor.Where(X => X.IdLEPallets == contenedor.IdLEPallets).ToList();
                    listaEmpaque.PesoNetoTotal -= contenedor.EditPesoNeto;
                    listaEmpaque.PesoBrutoTotal -= contenedor.EditPesoBruto;
                    listaEmpaque.PesoNetoTotal += contenedor.PesoNeto;
                    listaEmpaque.PesoBrutoTotal += contenedor.PesoBruto;

                    contenedor.FechaHoraUpd = DateTime.Now;
                    contenedor.IpUpd = RemoteAddr();
                    contenedor.UserUpd = User.Identity.Name;
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Contenedores", new { id = listaEmpaque.IdLEPallets, errMsg = "", okMsg = "Se ha editado el contenedor" });
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

            ViewData["listaEmpaque"] = contenedor.CAL_LEPallets;
            return View("EditarContenedor", contenedor);
        }

        public ActionResult EliminarContenedor(int id, int IdContenedor)
        {
            CheckPermisoAndRedirect(390);
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            CAL_LEPalletsContenedor contenedor = dcSoftwareCalidad.CAL_LEPalletsContenedor.SingleOrDefault(X => X.IdLEPallets == id && X.IdContenedor == IdContenedor);
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
                dcSoftwareCalidad.CAL_LEPalletsContenedor.DeleteOnSubmit(contenedor);
                dcSoftwareCalidad.SubmitChanges();

                //int J = 0;
                //List<CAL_GetTamañoContenedoresParaListaEmpaque_PalletsResult> tamaños = dcSoftwareCalidad.CAL_GetTamañoContenedoresParaListaEmpaque_Pallets(listaEmpaque.IdOrdenProduccion).ToList();
                //string contenedores = "";
                //foreach (CAL_GetTamañoContenedoresParaListaEmpaque_PalletsResult tamaño in tamaños)
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
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
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

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/lista_de_empaque_pallets_template.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA" , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "AÑO"   , DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "LOTE"  , listaEmpaque.CAL_OrdenProduccion.LoteComercial);
            RepTemp(ref htmlContent, "EMB"   , listaEmpaque.CAL_OrdenProduccion.CAL_Exportador.Nombre);
            RepTemp(ref htmlContent, "CSG"   , listaEmpaque.CAL_OrdenProduccion.GetCliente(listaEmpaque.CAL_OrdenProduccion.IdCliente).RazonSocial);
            RepTemp(ref htmlContent, "NAV"   , listaEmpaque.Carrier.Nombre);
            RepTemp(ref htmlContent, "RSV"   , listaEmpaque.NReserva);
            RepTemp(ref htmlContent, "CNT"   , cnts);
            RepTemp(ref htmlContent, "FACT"  , Factura);
            RepTemp(ref htmlContent, "PTOEMB", listaEmpaque.PuertoEmbarque);
            RepTemp(ref htmlContent, "PTODST", listaEmpaque.PuertoDestino);
            RepTemp(ref htmlContent, "BCO"   , listaEmpaque.Barco.Nombre);
            RepTemp(ref htmlContent, "DUS"   , listaEmpaque.DUS);
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
            stringBuilder.AppendLine("<th scope='col'>Sacos</th>");
            stringBuilder.AppendLine("<th scope='col'>Producto</th>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("</thead>");
            //CABECERA
            stringBuilder.AppendLine("<tbody>");
            List<CAL_LEPalletsContenedor> contenedores = dcSoftwareCalidad.CAL_LEPalletsContenedor.Where(X => X.IdLEPallets == listaEmpaque.IdLEPallets).ToList();
            List<CAL_GetTotalProductosYSacosParaListaEmpaque_PalletsResult> productos = dcSoftwareCalidad.CAL_GetTotalProductosYSacosParaListaEmpaque_Pallets(listaEmpaque.IdOrdenProduccion).ToList();
            foreach (var contenedor in contenedores)
            {
                List<CAL_GetProductosYSacosParaListaEmpaque_PalletsResult> productosContenedor = dcSoftwareCalidad.CAL_GetProductosYSacosParaListaEmpaque_Pallets(listaEmpaque.IdOrdenProduccion, contenedor.IdContenedor).ToList();
                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine(string.Format("<td>{0:dd/MM/yyyy}</td>", contenedor.Fecha));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NGuiaDespacho));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NContenedor));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.SelloLinea));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.PesoNeto));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.PesoBruto));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.Tara));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.VGM));
                stringBuilder.AppendLine("<td>");
                stringBuilder.AppendLine("<table style='margin: 0 auto;'>");
                foreach (CAL_GetProductosYSacosParaListaEmpaque_PalletsResult producto in productosContenedor)
                {
                    stringBuilder.AppendLine("<tr>");
                    stringBuilder.AppendLine(string.Format("<td style='border: none;'>{0}</td>", producto.Sacos));
                    stringBuilder.AppendLine("</tr>");
                }
                stringBuilder.AppendLine("</table>");
                stringBuilder.AppendLine("</td>");
                stringBuilder.AppendLine("<td>");
                stringBuilder.AppendLine("<table style='margin: 0 auto;'>");
                foreach (CAL_GetProductosYSacosParaListaEmpaque_PalletsResult producto in productosContenedor)
                {
                    stringBuilder.AppendLine("<tr>");
                    stringBuilder.AppendLine(string.Format("<td style='border: none;'>{0}</td>", producto.Subproducto));
                    stringBuilder.AppendLine("</tr>");
                }
                stringBuilder.AppendLine("</table>");
                stringBuilder.AppendLine("</td>");
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
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", productos.Sum(X => X.Sacos)));
            stringBuilder.AppendLine("<td></td>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine(string.Format("<td class='ultima-linea' style='text-align: left' colspan ='2'><strong>FECHA</strong>: {0:dd/MM/yyyy}</td>", DateTime.Now));
            stringBuilder.AppendLine(string.Format("<td class='ultima-linea' style='text-align: center' colspan ='4'><strong>EMITIDO POR</strong>: {0}</td>", listaEmpaque.CAL_OrdenProduccion.GetUser(listaEmpaque.UserIns).FullName));
            stringBuilder.AppendLine("<td class='ultima-linea' style='text-align: left' colspan ='5'><strong>FIRMA</strong>:</td>");
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

                return RedirectToAction("DescargarPDF", new { id = listaEmpaque.IdLEPallets, certkey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/listadeempaque"), string.Format("IDCERT{0}_{1}.txt", listaEmpaque.IdLEPallets, certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine("No se pudo generar el PDF de la lista de empaque");
                writer.WriteLine(ex.ToString());
                writer.Close();

                return RedirectToAction("Index", new { errMsg = "No se pudo generar el PDF de la lista de empaque", okMsg = "" });
            }
        }

        public ActionResult CrearPDF_en(int id)
        {
            CheckPermisoAndRedirect(387);
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
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

            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/lista_de_empaque_pallets_ingles_template.html"), Encoding.UTF8);
            RepTemp(ref htmlContent, "FECHA" , DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "AÑO"   , DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "LOTE"  , listaEmpaque.CAL_OrdenProduccion.LoteComercial);
            RepTemp(ref htmlContent, "EMB"   , listaEmpaque.CAL_OrdenProduccion.CAL_Exportador.Nombre);
            RepTemp(ref htmlContent, "CSG"   , listaEmpaque.CAL_OrdenProduccion.GetCliente(listaEmpaque.CAL_OrdenProduccion.IdCliente).RazonSocial);
            RepTemp(ref htmlContent, "NAV"   , listaEmpaque.Carrier.Nombre);
            RepTemp(ref htmlContent, "RSV"   , listaEmpaque.NReserva);
            RepTemp(ref htmlContent, "CNT"   , cnts);
            RepTemp(ref htmlContent, "FACT"  , Factura);
            RepTemp(ref htmlContent, "PTOEMB", listaEmpaque.PuertoEmbarque);
            RepTemp(ref htmlContent, "PTODST", listaEmpaque.PuertoDestino);
            RepTemp(ref htmlContent, "BCO"   , listaEmpaque.Barco.Nombre);
            RepTemp(ref htmlContent, "DUS"   , listaEmpaque.DUS);
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
            stringBuilder.AppendLine("<th scope='col'>Bags</th>");
            stringBuilder.AppendLine("<th scope='col'>Product</th>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("</thead>");
            //CABECERA
            stringBuilder.AppendLine("<tbody>");
            List<CAL_LEPalletsContenedor> contenedores = dcSoftwareCalidad.CAL_LEPalletsContenedor.Where(X => X.IdLEPallets == listaEmpaque.IdLEPallets).ToList();
            List<CAL_GetTotalProductosYSacosParaListaEmpaque_PalletsResult> productos = dcSoftwareCalidad.CAL_GetTotalProductosYSacosParaListaEmpaque_Pallets(listaEmpaque.IdOrdenProduccion).ToList();
            foreach (var contenedor in contenedores)
            {
                List<CAL_GetProductosYSacosParaListaEmpaque_PalletsResult> productosContenedor = dcSoftwareCalidad.CAL_GetProductosYSacosParaListaEmpaque_Pallets(listaEmpaque.IdOrdenProduccion, contenedor.IdContenedor).ToList();
                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine(string.Format("<td>{0:dd/MM/yyyy}</td>", contenedor.Fecha));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NGuiaDespacho));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.NContenedor));
                stringBuilder.AppendLine(string.Format("<td>{0}</td>", contenedor.SelloLinea));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.PesoNeto));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.PesoBruto));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.Tara));
                stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", contenedor.VGM));
                stringBuilder.AppendLine("<td>");
                stringBuilder.AppendLine("<table style='margin: 0 auto;'>");
                foreach (CAL_GetProductosYSacosParaListaEmpaque_PalletsResult producto in productosContenedor)
                {
                    stringBuilder.AppendLine("<tr>");
                    stringBuilder.AppendLine(string.Format("<td style='border: none;'>{0}</td>", producto.Sacos));
                    stringBuilder.AppendLine("</tr>");
                }
                stringBuilder.AppendLine("</table>");
                stringBuilder.AppendLine("</td>");
                stringBuilder.AppendLine("<td>");
                stringBuilder.AppendLine("<table style='margin: 0 auto;'>");
                foreach (CAL_GetProductosYSacosParaListaEmpaque_PalletsResult producto in productosContenedor)
                {
                    stringBuilder.AppendLine("<tr>");
                    stringBuilder.AppendLine(string.Format("<td style='border: none;'>{0}</td>", producto.Subproducto));
                    stringBuilder.AppendLine("</tr>");
                }
                stringBuilder.AppendLine("</table>");
                stringBuilder.AppendLine("</td>");
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
            stringBuilder.AppendLine(string.Format("<td>{0:N0}</td>", productos.Sum(X => X.Sacos)));
            stringBuilder.AppendLine("<td></td>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine(string.Format("<td class='ultima-linea' style='text-align: left' colspan ='2'><strong>DATE</strong>: {0:dd/MM/yyyy}</td>", DateTime.Now));
            stringBuilder.AppendLine(string.Format("<td class='ultima-linea' style='text-align: center' colspan ='4'><strong>ISSUED BY</strong>: {0}</td>", listaEmpaque.CAL_OrdenProduccion.GetUser(listaEmpaque.UserIns).FullName));
            stringBuilder.AppendLine("<td class='ultima-linea' style='text-align: left' colspan ='5'><strong>SIGNATURE</strong>:</td>");
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

                return RedirectToAction("DescargarPDF_en", new { id = listaEmpaque.IdLEPallets, certkey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/listadeempaque"), string.Format("IDCERT{0}_{1}.txt", listaEmpaque.IdLEPallets, certkey)), FileMode.OpenOrCreate, FileAccess.Write);
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
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/listadeempaque/{0}.pdf", listaEmpaque.certkey));
                return File(pdf, "application/pdf", String.Format("Lista de Empaque {0}-{1}.pdf", listaEmpaque.IdLEPallets, listaEmpaque.CAL_OrdenProduccion.LoteComercial));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF de la lista de empaque", okMsg = "" });
            }
        }

        public ActionResult DescargarPDF_en(int id, string certkey)
        {
            CheckPermisoAndRedirect(387);
            CAL_LEPallets listaEmpaque = dcSoftwareCalidad.CAL_LEPallets.SingleOrDefault(X => X.IdLEPallets == id && X.Habilitado == true);
            if (listaEmpaque == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la lista de empaque", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/listadeempaque/{0}.pdf", listaEmpaque.certkey));
                return File(pdf, "application/pdf", String.Format("Packing List {0}-{1}.pdf", listaEmpaque.IdLEPallets, listaEmpaque.CAL_OrdenProduccion.LoteComercial));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF de la lista de empaque", okMsg = "" });
            }
        }

        #region --- Funciones PRIVADAS ---

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}