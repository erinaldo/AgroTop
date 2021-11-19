using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Response;
using DotnetDaddy.DocumentViewer;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALOrdenesProduccionController : BaseApplicationController
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        #region Órdenes de Producción

        public CALOrdenesProduccionController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(293);

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
            List<CAL_OrdenProduccion> list = new List<CAL_OrdenProduccion>();
            if (IdPlantaProduccionSelect == 0)
            {
                list = dcSoftwareCalidad.CAL_OrdenProduccion.Where(X => X.Habilitado == true).ToList();
            }
            else
            {
                list = dcSoftwareCalidad.CAL_OrdenProduccion.Where(X => X.Habilitado == true && X.IdPlanta == IdPlantaProduccionSelect).ToList();
            }
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299),
                CerrarOP = CheckPermiso(405),
            };
            return View(list);
        }

        public ActionResult Autorizar(int id)
        {
            CheckPermisoAndRedirect(298);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                ordenProduccion.Autorizado = true;
                ordenProduccion.AutorizadoAuto = false;
                ordenProduccion.UserAutoriza = User.Identity.Name;
                ordenProduccion.FechaHoraAutoriza = DateTime.Now;
                ordenProduccion.IpAutoriza = RemoteAddr();

                ordenProduccion.UserUpd = User.Identity.Name;
                ordenProduccion.FechaHoraUpd = DateTime.Now;
                ordenProduccion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("La orden de producción {0} ha sido autorizada", ordenProduccion.LoteComercial);

                ordenProduccion.NotificarAutorizar();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(294);
            CAL_OrdenProduccion ordenProduccion = new CAL_OrdenProduccion();
            ordenProduccion.Fecha = DateTime.Now;
            ordenProduccion.FechaZarpe = DateTime.Now;
            ordenProduccion.InicioProduccion = DateTime.Now;
            ordenProduccion.TerminoProduccion = DateTime.Now;
            return View("Crear", ordenProduccion);
        }

        [HttpPost]
        public ActionResult Crear(CAL_OrdenProduccion ordenProduccion, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(294);
            if (ModelState.IsValid)
            {
                try
                {
                    ordenProduccion.LoteComercial = ordenProduccion.LoteComercial.ToUpper();
                    if (string.IsNullOrEmpty(ordenProduccion.FichaTecnica))
                        ordenProduccion.FichaTecnica = "DE ACUERDO A LA FICHA TÉCNICA DEL CLIENTE";
                    if (string.IsNullOrEmpty(ordenProduccion.Observaciones))
                        ordenProduccion.Observaciones = "";
                    if (string.IsNullOrEmpty(ordenProduccion.NumeroViaje))
                        ordenProduccion.NumeroViaje = "";
                    ordenProduccion.IdTipoOrdenProduccion = 1;
                    ordenProduccion.InicioProduccion = DateTime.Now;
                    ordenProduccion.TerminoProduccion = DateTime.Now;
                    ordenProduccion.Autorizado = false;
                    ordenProduccion.Terminada = false;
                    ordenProduccion.AutorizadoAuto = false;
                    ordenProduccion.Habilitado = true;
                    ordenProduccion.FechaHoraIns = DateTime.Now;
                    ordenProduccion.IpIns = RemoteAddr();
                    ordenProduccion.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_OrdenProduccion.InsertOnSubmit(ordenProduccion);
                    dcSoftwareCalidad.SubmitChanges();

                    string errMsg = "";
                    string okMsg = "";

                    //Detalle de la Orden de Producción
                    int currentRow = 1;
                    if (!string.IsNullOrEmpty(formCollection["RowKey"]))
                    {
                        string[] rowKeys = formCollection["RowKey"].Split(new char[] { ',' }).ToArray<string>();
                        foreach (var rowKey in rowKeys)
                        {
                            var idProducto_rowData = formCollection[string.Format("idProducto_{0}", rowKey)];
                            var Espesor_rowData = formCollection[string.Format("Espesor_{0}", rowKey)];
                            var Saco_rowData = formCollection[string.Format("Saco_{0}", rowKey)];
                            var PesoSaco_rowData = formCollection[string.Format("PesoSaco_{0}", rowKey)];
                            var Contenedor_rowData = formCollection[string.Format("Contenedor_{0}", rowKey)];
                            var cntCont_rowData = formCollection[string.Format("cntCont_{0}", rowKey)];
                            var SacosPorContenedor_rowData = formCollection[string.Format("SacosPorContenedor_{0}", rowKey)];
                            var cntSaco_rowData = formCollection[string.Format("cntSaco_{0}", rowKey)];
                            var cntProd_rowData = formCollection[string.Format("cntProd_{0}", rowKey)];

                            {
                                bool requestOK = true;

                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine(string.Format("<p>PRODUCTO {0}</p>", currentRow));
                                stringBuilder.AppendLine("<ul>");

                                if (!int.TryParse(idProducto_rowData, out int idProducto))
                                {
                                    stringBuilder.AppendLine("<li>El producto no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(Espesor_rowData, out int Espesor)) { }

                                if (!int.TryParse(Saco_rowData, out int Saco))
                                {
                                    stringBuilder.AppendLine("<li>El saco no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(PesoSaco_rowData, out int PesoSaco))
                                {
                                    stringBuilder.AppendLine("<li>El peso saco no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(Contenedor_rowData, out int Contenedor))
                                {
                                    stringBuilder.AppendLine("<li>El contenedor no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(SacosPorContenedor_rowData, out int SacosPorContenedor))
                                {
                                    stringBuilder.AppendLine("<li>Los sacos por contenedor no son válidos</li>");
                                    requestOK = false;
                                }

                                if (!decimal.TryParse(cntCont_rowData, out decimal cntCont))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de contenedor/ es no es válida</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(cntSaco_rowData, out int cntSaco))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de sacos no es válido</li>");
                                    requestOK = false;
                                }

                                if (!decimal.TryParse(cntProd_rowData, out decimal cntProd))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de producto no es válida</li>");
                                    requestOK = false;
                                }

                                CAL_Producto cAL_Producto = (from S1 in dcSoftwareCalidad.CAL_Subproducto
                                                             join P1 in dcSoftwareCalidad.CAL_Producto on S1.IdProducto equals P1.IdProducto
                                                             where S1.IdSubproducto == idProducto
                                                             && S1.Habilitado == true
                                                             && P1.Habilitado == true
                                                             select P1).SingleOrDefault();
                                if (cAL_Producto == null)
                                {
                                    stringBuilder.AppendLine("<li>El producto no existe</li>");
                                    requestOK = false;
                                }

                                stringBuilder.AppendLine("</ul>");


                                if (requestOK)
                                {
                                    CAL_DetalleOrdenProduccion detalleOrdenProduccion = new CAL_DetalleOrdenProduccion();
                                    detalleOrdenProduccion.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;
                                    detalleOrdenProduccion.FechaHoraIns = DateTime.Now;
                                    detalleOrdenProduccion.IpIns = RemoteAddr();
                                    detalleOrdenProduccion.UserIns = User.Identity.Name;
                                    detalleOrdenProduccion.RowKey = rowKey;
                                    detalleOrdenProduccion.IdProducto = cAL_Producto.IdProducto;
                                    detalleOrdenProduccion.IdSubproducto = idProducto;
                                    detalleOrdenProduccion.CantidadProducto = cntProd;
                                    if (Espesor != 0)
                                        detalleOrdenProduccion.IdEspesorProducto = Espesor;
                                    detalleOrdenProduccion.IdSaco = Saco;
                                    detalleOrdenProduccion.IdContenedor = Contenedor;
                                    detalleOrdenProduccion.CantidadContenedores = cntCont;
                                    detalleOrdenProduccion.CantidadSacos = cntSaco;
                                    detalleOrdenProduccion.IdPesoSaco = PesoSaco;
                                    detalleOrdenProduccion.SacosPorContenedor = SacosPorContenedor;
                                    dcSoftwareCalidad.CAL_DetalleOrdenProduccion.InsertOnSubmit(detalleOrdenProduccion);
                                    dcSoftwareCalidad.SubmitChanges();
                                }
                                else
                                {
                                    errMsg = stringBuilder.ToString();
                                    okMsg = "La orden de producción se ha creado pero con errores";
                                    return RedirectToAction("index", new { errMsg = errMsg, okMsg = okMsg });
                                }

                                currentRow++;
                            }
                        }
                    }
                    {
                        // Reset
                        bool requestOK = true;
                        errMsg = "";
                        okMsg = "";
                        currentRow = 1;

                        //Transportes
                        if (!string.IsNullOrEmpty(formCollection["idsTransportistas"]))
                        {
                            string[] idsTransportistas = formCollection["idsTransportistas"].Split(new char[] { ',' }).ToArray<string>();
                            foreach (var idTransportista in idsTransportistas)
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine(string.Format("<p>TRANSPORTISTA {0}</p>", currentRow));
                                stringBuilder.AppendLine("<ul>");

                                if (!int.TryParse(idTransportista, out int result))
                                {
                                    stringBuilder.AppendLine("<li>El transportista no es válido</li>");
                                    requestOK = false;
                                }

                                LOG_Transportista lOG_Transportista = dcAgroFichas.LOG_Transportista.SingleOrDefault(X => X.IdTransportista == result);
                                if (lOG_Transportista == null)
                                {
                                    stringBuilder.AppendLine("<li>El transportista no existe</li>");
                                    requestOK = false;
                                }

                                stringBuilder.AppendLine("</ul>");

                                if (requestOK)
                                {
                                    CAL_TransporteTerrestre transporteTerrestre = new CAL_TransporteTerrestre();
                                    transporteTerrestre.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;
                                    transporteTerrestre.FechaHoraIns = DateTime.Now;
                                    transporteTerrestre.IpIns = RemoteAddr();
                                    transporteTerrestre.UserIns = User.Identity.Name;
                                    transporteTerrestre.IdTransportista = result;
                                    dcSoftwareCalidad.CAL_TransporteTerrestre.InsertOnSubmit(transporteTerrestre);
                                    dcSoftwareCalidad.SubmitChanges();
                                }
                                else
                                {
                                    errMsg = stringBuilder.ToString();
                                    okMsg = "La orden de producción se ha creado pero con errores";
                                    return RedirectToAction("index", new { errMsg = Url.Encode(errMsg), okMsg = okMsg });
                                }

                                currentRow++;
                            }
                        }
                    }

                    ordenProduccion.NotificarCreacion();

                    return RedirectToAction("index", new { errMsg = errMsg, okMsg = "La orden de producción se ha creado" });
                }
                catch
                {
                    var rv = ordenProduccion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", ordenProduccion);
        }

        //GRANEL
        public ActionResult CrearGranel()
        {
            CheckPermisoAndRedirect(294);
            CAL_OrdenProduccion ordenProduccion = new CAL_OrdenProduccion();
            ordenProduccion.Fecha = DateTime.Now;
            ordenProduccion.FechaZarpe = DateTime.Now;
            ordenProduccion.InicioProduccion = DateTime.Now;
            ordenProduccion.TerminoProduccion = DateTime.Now;
            return View("CrearGranel", ordenProduccion);
        }

        [HttpPost]
        public ActionResult CrearGranel(CAL_OrdenProduccion ordenProduccion, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(294);
            if (ModelState.IsValid)
            {
                try
                {
                    ordenProduccion.LoteComercial = ordenProduccion.LoteComercial.ToUpper();
                    if (string.IsNullOrEmpty(ordenProduccion.FichaTecnica))
                        ordenProduccion.FichaTecnica = "DE ACUERDO A LA FICHA TÉCNICA DEL CLIENTE";
                    if (string.IsNullOrEmpty(ordenProduccion.Observaciones))
                        ordenProduccion.Observaciones = "";
                    if (string.IsNullOrEmpty(ordenProduccion.NumeroViaje))
                        ordenProduccion.NumeroViaje = "";

                    ordenProduccion.IdTipoOrdenProduccion = 2;
                    ordenProduccion.InicioProduccion = DateTime.Now;
                    ordenProduccion.TerminoProduccion = DateTime.Now;
                    ordenProduccion.Autorizado = false;
                    ordenProduccion.AutorizadoAuto = false;
                    ordenProduccion.Terminada = false;
                    ordenProduccion.Habilitado = true;
                    ordenProduccion.FechaHoraIns = DateTime.Now;
                    ordenProduccion.IpIns = RemoteAddr();
                    ordenProduccion.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_OrdenProduccion.InsertOnSubmit(ordenProduccion);
                    dcSoftwareCalidad.SubmitChanges();

                    string errMsg = "";
                    string okMsg = "";

                    //Detalle de la Orden de Producción
                    int currentRow = 1;
                    if (!string.IsNullOrEmpty(formCollection["RowKey"]))
                    {
                        string[] rowKeys = formCollection["RowKey"].Split(new char[] { ',' }).ToArray<string>();
                        foreach (var rowKey in rowKeys)
                        {
                            var idProducto_rowData = formCollection[string.Format("idProducto_{0}", rowKey)];
                            var Contenedor_rowData = formCollection[string.Format("Contenedor_{0}", rowKey)];
                            var cntProd_rowData = formCollection[string.Format("cntProd_{0}", rowKey)];
                            var cntPorCont_rowData = formCollection[string.Format("cntPorCont_{0}", rowKey)];
                            var cntCont_rowData = formCollection[string.Format("cntCont_{0}", rowKey)];
                            {
                                bool requestOK = true;

                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine(string.Format("<p>PRODUCTO {0}</p>", currentRow));
                                stringBuilder.AppendLine("<ul>");

                                if (!int.TryParse(idProducto_rowData, out int idProducto))
                                {
                                    stringBuilder.AppendLine("<li>El producto no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(Contenedor_rowData, out int Contenedor))
                                {
                                    stringBuilder.AppendLine("<li>El contenedor no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(cntCont_rowData, out int cntCont))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de contenedor/ es no es válida</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(cntPorCont_rowData, out int cntPorCont))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad por contenedor no es válida</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(cntProd_rowData, out int cntProd))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de producto no es válida</li>");
                                    requestOK = false;
                                }

                                CAL_Producto cAL_Producto = (from S1 in dcSoftwareCalidad.CAL_Subproducto
                                                             join P1 in dcSoftwareCalidad.CAL_Producto on S1.IdProducto equals P1.IdProducto
                                                             where S1.IdSubproducto == idProducto
                                                             && S1.Habilitado == true
                                                             && P1.Habilitado == true
                                                             select P1).SingleOrDefault();
                                if (cAL_Producto == null)
                                {
                                    stringBuilder.AppendLine("<li>El producto no existe</li>");
                                    requestOK = false;
                                }

                                stringBuilder.AppendLine("</ul>");


                                if (requestOK)
                                {
                                    CAL_DetalleOrdenProduccion detalleOrdenProduccionGranel = new CAL_DetalleOrdenProduccion();
                                    detalleOrdenProduccionGranel.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;
                                    detalleOrdenProduccionGranel.FechaHoraIns = DateTime.Now;
                                    detalleOrdenProduccionGranel.IpIns = RemoteAddr();
                                    detalleOrdenProduccionGranel.UserIns = User.Identity.Name;
                                    detalleOrdenProduccionGranel.RowKey = rowKey;
                                    detalleOrdenProduccionGranel.IdProducto = cAL_Producto.IdProducto;
                                    detalleOrdenProduccionGranel.IdSubproducto = idProducto;
                                    detalleOrdenProduccionGranel.CantidadProducto = cntProd;
                                    detalleOrdenProduccionGranel.IdContenedor = Contenedor;
                                    detalleOrdenProduccionGranel.CantidadPorContenedor = cntPorCont;
                                    detalleOrdenProduccionGranel.CantidadContenedores = cntCont;
                                    dcSoftwareCalidad.CAL_DetalleOrdenProduccion.InsertOnSubmit(detalleOrdenProduccionGranel);
                                    dcSoftwareCalidad.SubmitChanges();
                                }
                                else
                                {
                                    errMsg = stringBuilder.ToString();
                                    okMsg = "La orden de producción granel se ha creado pero con errores";
                                    return RedirectToAction("index", new { errMsg = errMsg, okMsg = okMsg });
                                }

                                currentRow++;
                            }
                        }
                    }

                    {
                        // Reset
                        bool requestOK = true;
                        errMsg = "";
                        okMsg = "";
                        currentRow = 1;

                        //Transportes
                        if (!string.IsNullOrEmpty(formCollection["idsTransportistas"]))
                        {
                            string[] idsTransportistas = formCollection["idsTransportistas"].Split(new char[] { ',' }).ToArray<string>();
                            foreach (var idTransportista in idsTransportistas)
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine(string.Format("<p>TRANSPORTISTA {0}</p>", currentRow));
                                stringBuilder.AppendLine("<ul>");

                                if (!int.TryParse(idTransportista, out int result))
                                {
                                    stringBuilder.AppendLine("<li>El transportista no es válido</li>");
                                    requestOK = false;
                                }

                                LOG_Transportista lOG_Transportista = dcAgroFichas.LOG_Transportista.SingleOrDefault(X => X.IdTransportista == result);
                                if (lOG_Transportista == null)
                                {
                                    stringBuilder.AppendLine("<li>El transportista no existe</li>");
                                    requestOK = false;
                                }

                                stringBuilder.AppendLine("</ul>");

                                if (requestOK)
                                {
                                    CAL_TransporteTerrestre transporteTerrestre = new CAL_TransporteTerrestre();
                                    transporteTerrestre.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;
                                    transporteTerrestre.FechaHoraIns = DateTime.Now;
                                    transporteTerrestre.IpIns = RemoteAddr();
                                    transporteTerrestre.UserIns = User.Identity.Name;
                                    transporteTerrestre.IdTransportista = result;
                                    dcSoftwareCalidad.CAL_TransporteTerrestre.InsertOnSubmit(transporteTerrestre);
                                    dcSoftwareCalidad.SubmitChanges();
                                }
                                else
                                {
                                    errMsg = stringBuilder.ToString();
                                    okMsg = "La orden de producción se ha creado pero con errores";
                                    return RedirectToAction("index", new { errMsg = Url.Encode(errMsg), okMsg = okMsg });
                                }

                                currentRow++;
                            }
                        }
                    }

                    ordenProduccion.NotificarCreacion();

                    return RedirectToAction("index", new { errMsg = errMsg, okMsg = "La orden de producción se ha creado" });
                }
                catch
                {
                    var rv = ordenProduccion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearGranel", ordenProduccion);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(295);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299)
            };
            return View("Editar", ordenProduccion);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(295);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            try
            {
                UpdateModel(ordenProduccion, new string[] { "LoteComercial", "Fecha", "IdExportador", "IdCliente", "PaisCodigo", "IdCarrier", "IdBarco", "NumeroViaje", "InspeccionSAG", "InicioProduccion", "TerminoProduccion", "FechaZarpe", "Fumigacion", "Observaciones", "IdPlanta" });

                ordenProduccion.LoteComercial = ordenProduccion.LoteComercial.ToUpper();
                if (string.IsNullOrEmpty(ordenProduccion.Observaciones))
                    ordenProduccion.Observaciones = "";
                if (string.IsNullOrEmpty(ordenProduccion.NumeroViaje))
                    ordenProduccion.NumeroViaje = "";

                ordenProduccion.Autorizado = false;
                ordenProduccion.AutorizadoAuto = false;
                ordenProduccion.UserAutoriza = User.Identity.Name;
                ordenProduccion.FechaHoraAutoriza = DateTime.Now;
                ordenProduccion.IpAutoriza = RemoteAddr();
                ordenProduccion.UserUpd = User.Identity.Name;
                ordenProduccion.FechaHoraUpd = DateTime.Now;
                ordenProduccion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();

                string errMsg = "";
                string okMsg = "";

                //Detalle de la Orden de Producción
                int currentRow = 1;
                if (!string.IsNullOrEmpty(formCollection["RowKey"]))
                {
                    string[] rowKeys = formCollection["RowKey"].Split(new char[] { ',' }).ToArray<string>();
                    foreach (var rowKey in rowKeys)
                    {
                        CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.RowKey == rowKey);
                        if (detalleOrdenProduccion == null)
                        {
                            var idProducto_rowData = formCollection[string.Format("idProducto_{0}", rowKey)];
                            var Espesor_rowData = formCollection[string.Format("Espesor_{0}", rowKey)];
                            var Saco_rowData = formCollection[string.Format("Saco_{0}", rowKey)];
                            var PesoSaco_rowData = formCollection[string.Format("PesoSaco_{0}", rowKey)];
                            var Contenedor_rowData = formCollection[string.Format("Contenedor_{0}", rowKey)];
                            var cntCont_rowData = formCollection[string.Format("cntCont_{0}", rowKey)];
                            var SacosPorContenedor_rowData = formCollection[string.Format("SacosPorContenedor_{0}", rowKey)];
                            var cntSaco_rowData = formCollection[string.Format("cntSaco_{0}", rowKey)];
                            var cntProd_rowData = formCollection[string.Format("cntProd_{0}", rowKey)];

                            {
                                bool requestOK = true;

                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine(string.Format("<p>PRODUCTO {0}</p>", currentRow));
                                stringBuilder.AppendLine("<ul>");

                                if (!int.TryParse(idProducto_rowData, out int idProducto))
                                {
                                    stringBuilder.AppendLine("<li>El producto no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(Espesor_rowData, out int Espesor)) { }

                                if (!int.TryParse(Saco_rowData, out int Saco))
                                {
                                    stringBuilder.AppendLine("<li>El saco no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(PesoSaco_rowData, out int PesoSaco))
                                {
                                    stringBuilder.AppendLine("<li>El peso saco no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(Contenedor_rowData, out int Contenedor))
                                {
                                    stringBuilder.AppendLine("<li>El contenedor no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(SacosPorContenedor_rowData, out int SacosPorContenedor))
                                {
                                    stringBuilder.AppendLine("<li>Los sacos por contenedor no son válidos</li>");
                                    requestOK = false;
                                }

                                if (!decimal.TryParse(cntCont_rowData, out decimal cntCont))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de contenedor/ es no es válida</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(cntSaco_rowData, out int cntSaco))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de sacos no es válido</li>");
                                    requestOK = false;
                                }

                                if (!decimal.TryParse(cntProd_rowData, out decimal cntProd))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de producto no es válida</li>");
                                    requestOK = false;
                                }

                                CAL_Producto cAL_Producto = (from S1 in dcSoftwareCalidad.CAL_Subproducto
                                                             join P1 in dcSoftwareCalidad.CAL_Producto on S1.IdProducto equals P1.IdProducto
                                                             where S1.IdSubproducto == idProducto
                                                             && S1.Habilitado == true
                                                             && P1.Habilitado == true
                                                             select P1).SingleOrDefault();
                                if (cAL_Producto == null)
                                {
                                    stringBuilder.AppendLine("<li>El producto no existe</li>");
                                    requestOK = false;
                                }

                                stringBuilder.AppendLine("</ul>");

                                if (requestOK)
                                {
                                    detalleOrdenProduccion = new CAL_DetalleOrdenProduccion();
                                    detalleOrdenProduccion.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;
                                    detalleOrdenProduccion.FechaHoraIns = DateTime.Now;
                                    detalleOrdenProduccion.IpIns = RemoteAddr();
                                    detalleOrdenProduccion.UserIns = User.Identity.Name;
                                    detalleOrdenProduccion.RowKey = rowKey;
                                    detalleOrdenProduccion.IdProducto = cAL_Producto.IdProducto;
                                    detalleOrdenProduccion.IdSubproducto = idProducto;
                                    detalleOrdenProduccion.CantidadProducto = cntProd;
                                    if (Espesor != 0)
                                        detalleOrdenProduccion.IdEspesorProducto = Espesor;
                                    detalleOrdenProduccion.IdSaco = Saco;
                                    detalleOrdenProduccion.IdContenedor = Contenedor;
                                    detalleOrdenProduccion.CantidadContenedores = cntCont;
                                    detalleOrdenProduccion.CantidadSacos = cntSaco;
                                    detalleOrdenProduccion.IdPesoSaco = PesoSaco;
                                    detalleOrdenProduccion.SacosPorContenedor = SacosPorContenedor;
                                    dcSoftwareCalidad.CAL_DetalleOrdenProduccion.InsertOnSubmit(detalleOrdenProduccion);
                                    dcSoftwareCalidad.SubmitChanges();
                                }
                                else
                                {
                                    errMsg = stringBuilder.ToString();
                                    okMsg = "La orden de producción se ha editado pero con errores";
                                    return RedirectToAction("index", new { errMsg = Url.Encode(errMsg), okMsg = okMsg });
                                }
                            }

                            currentRow++;
                        }
                    }
                }
                {
                    // Reset
                    bool requestOK = true;
                    errMsg = "";
                    okMsg = "";
                    currentRow = 1;

                    //Transportes
                    if (!string.IsNullOrEmpty(formCollection["idsTransportistas"]))
                    {
                        List<CAL_TransporteTerrestre> transporteTerrestreList = dcSoftwareCalidad.CAL_TransporteTerrestre.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion).ToList();
                        string[] idsTransportistas = formCollection["idsTransportistas"].Split(new char[] { ',' }).ToArray<string>();
                        foreach (var transporteTerrestre in transporteTerrestreList)
                        {
                            if (idsTransportistas.Contains(transporteTerrestre.IdTransportista.ToString()) == false)
                            {
                                dcSoftwareCalidad.CAL_TransporteTerrestre.DeleteOnSubmit(transporteTerrestre);
                                dcSoftwareCalidad.SubmitChanges();
                            }
                        }

                        foreach (var idTransportista in idsTransportistas)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine(string.Format("<p>TRANSPORTISTA {0}</p>", currentRow));
                            stringBuilder.AppendLine("<ul>");

                            if (!int.TryParse(idTransportista, out int result))
                            {
                                stringBuilder.AppendLine("<li>El transportista no es válido</li>");
                                requestOK = false;
                            }

                            LOG_Transportista lOG_Transportista = dcAgroFichas.LOG_Transportista.SingleOrDefault(X => X.IdTransportista == result);
                            if (lOG_Transportista == null)
                            {
                                stringBuilder.AppendLine("<li>El transportista no existe</li>");
                                requestOK = false;
                            }

                            stringBuilder.AppendLine("</ul>");

                            if (requestOK)
                            {
                                CAL_TransporteTerrestre transporteTerrestre = dcSoftwareCalidad.CAL_TransporteTerrestre.SingleOrDefault(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion && X.IdTransportista == result);
                                if (transporteTerrestre == null)
                                {
                                    transporteTerrestre = new CAL_TransporteTerrestre();
                                    transporteTerrestre.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;
                                    transporteTerrestre.IdTransportista = result;
                                    transporteTerrestre.FechaHoraIns = DateTime.Now;
                                    transporteTerrestre.IpIns = RemoteAddr();
                                    transporteTerrestre.UserIns = User.Identity.Name;
                                    dcSoftwareCalidad.CAL_TransporteTerrestre.InsertOnSubmit(transporteTerrestre);
                                    dcSoftwareCalidad.SubmitChanges();
                                }
                            }
                            else
                            {
                                errMsg = stringBuilder.ToString();
                                okMsg = "La orden de producción se ha editado pero con errores.";
                                return RedirectToAction("index", new { errMsg = errMsg, okMsg = okMsg });
                            }

                            currentRow++;
                        }
                    }
                    else
                    {
                        List<CAL_TransporteTerrestre> transporteTerrestreList = dcSoftwareCalidad.CAL_TransporteTerrestre.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion).ToList();
                        foreach (var transporteTerrestre in transporteTerrestreList)
                        {
                            dcSoftwareCalidad.CAL_TransporteTerrestre.DeleteOnSubmit(transporteTerrestre);
                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }

                ordenProduccion.NotificarEdicion();

                return RedirectToAction("index", new { errMsg = errMsg, okMsg = "La orden de producción se ha editado" });
            }
            catch
            {
                var rv = ordenProduccion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299)
            };
            return View("Editar", ordenProduccion);
        }

        public ActionResult EditarGranel(int id)
        {
            CheckPermisoAndRedirect(295);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299)
            };
            return View("EditarGranel", ordenProduccion);
        }

        [HttpPost]
        public ActionResult EditarGranel(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(295);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            try
            {
                UpdateModel(ordenProduccion, new string[] { "LoteComercial", "Fecha", "IdExportador", "IdCliente", "PaisCodigo", "IdCarrier", "IdBarco", "NumeroViaje", "InspeccionSAG", "InicioProduccion", "TerminoProduccion", "FechaZarpe", "Fumigacion", "Observaciones" });

                ordenProduccion.LoteComercial = ordenProduccion.LoteComercial.ToUpper();
                if (string.IsNullOrEmpty(ordenProduccion.Observaciones))
                    ordenProduccion.Observaciones = "";
                if (string.IsNullOrEmpty(ordenProduccion.NumeroViaje))
                    ordenProduccion.NumeroViaje = "";

                ordenProduccion.Autorizado = false;
                ordenProduccion.AutorizadoAuto = false;
                ordenProduccion.UserAutoriza = User.Identity.Name;
                ordenProduccion.FechaHoraAutoriza = DateTime.Now;
                ordenProduccion.IpAutoriza = RemoteAddr();
                ordenProduccion.UserUpd = User.Identity.Name;
                ordenProduccion.FechaHoraUpd = DateTime.Now;
                ordenProduccion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();

                string errMsg = "";
                string okMsg = "";

                //Detalle de la Orden de Producción
                int currentRow = 1;
                if (!string.IsNullOrEmpty(formCollection["RowKey"]))
                {
                    string[] rowKeys = formCollection["RowKey"].Split(new char[] { ',' }).ToArray<string>();
                    foreach (var rowKey in rowKeys)
                    {
                        CAL_DetalleOrdenProduccion detalleOrdenProduccionGranel = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.RowKey == rowKey);
                        if (detalleOrdenProduccionGranel == null)
                        {
                            var idProducto_rowData = formCollection[string.Format("idProducto_{0}", rowKey)];
                            var Contenedor_rowData = formCollection[string.Format("Contenedor_{0}", rowKey)];
                            var cntProd_rowData = formCollection[string.Format("cntProd_{0}", rowKey)];
                            var cntPorCont_rowData = formCollection[string.Format("cntPorCont_{0}", rowKey)];
                            var cntCont_rowData = formCollection[string.Format("cntCont_{0}", rowKey)];

                            {
                                bool requestOK = true;

                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine(string.Format("<p>PRODUCTO {0}</p>", currentRow));
                                stringBuilder.AppendLine("<ul>");


                                if (!int.TryParse(idProducto_rowData, out int idProducto))
                                {
                                    stringBuilder.AppendLine("<li>El producto no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(Contenedor_rowData, out int Contenedor))
                                {
                                    stringBuilder.AppendLine("<li>El contenedor no es válido</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(cntCont_rowData, out int cntCont))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de contenedor/ es no es válida</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(cntPorCont_rowData, out int cntPorCont))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad por contenedor no es válida</li>");
                                    requestOK = false;
                                }

                                if (!int.TryParse(cntProd_rowData, out int cntProd))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad de producto no es válida</li>");
                                    requestOK = false;
                                }

                                CAL_Producto cAL_Producto = (from S1 in dcSoftwareCalidad.CAL_Subproducto
                                                             join P1 in dcSoftwareCalidad.CAL_Producto on S1.IdProducto equals P1.IdProducto
                                                             where S1.IdSubproducto == idProducto
                                                             && S1.Habilitado == true
                                                             && P1.Habilitado == true
                                                             select P1).SingleOrDefault();
                                if (cAL_Producto == null)
                                {
                                    stringBuilder.AppendLine("<li>El producto no existe</li>");
                                    requestOK = false;
                                }

                                stringBuilder.AppendLine("</ul>");

                                if (requestOK)
                                {
                                    detalleOrdenProduccionGranel = new CAL_DetalleOrdenProduccion();
                                    detalleOrdenProduccionGranel.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;
                                    detalleOrdenProduccionGranel.FechaHoraIns = DateTime.Now;
                                    detalleOrdenProduccionGranel.IpIns = RemoteAddr();
                                    detalleOrdenProduccionGranel.UserIns = User.Identity.Name;
                                    detalleOrdenProduccionGranel.RowKey = rowKey;
                                    detalleOrdenProduccionGranel.IdProducto = cAL_Producto.IdProducto;
                                    detalleOrdenProduccionGranel.IdSubproducto = idProducto;
                                    detalleOrdenProduccionGranel.CantidadProducto = cntProd;
                                    detalleOrdenProduccionGranel.IdContenedor = Contenedor;
                                    detalleOrdenProduccionGranel.CantidadPorContenedor = cntPorCont;
                                    detalleOrdenProduccionGranel.CantidadContenedores = cntCont;
                                    dcSoftwareCalidad.CAL_DetalleOrdenProduccion.InsertOnSubmit(detalleOrdenProduccionGranel);
                                    dcSoftwareCalidad.SubmitChanges();
                                }
                                else
                                {
                                    errMsg = stringBuilder.ToString();
                                    okMsg = "La orden de producción se ha editado pero con errores";
                                    return RedirectToAction("index", new { errMsg = Url.Encode(errMsg), okMsg = okMsg });
                                }
                            }

                            currentRow++;
                        }
                    }
                }
                {
                    // Reset
                    bool requestOK = true;
                    errMsg = "";
                    okMsg = "";
                    currentRow = 1;

                    //Transportes
                    if (!string.IsNullOrEmpty(formCollection["idsTransportistas"]))
                    {
                        List<CAL_TransporteTerrestre> transporteTerrestreList = dcSoftwareCalidad.CAL_TransporteTerrestre.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion).ToList();
                        string[] idsTransportistas = formCollection["idsTransportistas"].Split(new char[] { ',' }).ToArray<string>();
                        foreach (var transporteTerrestre in transporteTerrestreList)
                        {
                            if (idsTransportistas.Contains(transporteTerrestre.IdTransportista.ToString()) == false)
                            {
                                dcSoftwareCalidad.CAL_TransporteTerrestre.DeleteOnSubmit(transporteTerrestre);
                                dcSoftwareCalidad.SubmitChanges();
                            }
                        }

                        foreach (var idTransportista in idsTransportistas)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine(string.Format("<p>TRANSPORTISTA {0}</p>", currentRow));
                            stringBuilder.AppendLine("<ul>");

                            if (!int.TryParse(idTransportista, out int result))
                            {
                                stringBuilder.AppendLine("<li>El transportista no es válido</li>");
                                requestOK = false;
                            }

                            LOG_Transportista lOG_Transportista = dcAgroFichas.LOG_Transportista.SingleOrDefault(X => X.IdTransportista == result);
                            if (lOG_Transportista == null)
                            {
                                stringBuilder.AppendLine("<li>El transportista no existe</li>");
                                requestOK = false;
                            }

                            stringBuilder.AppendLine("</ul>");

                            if (requestOK)
                            {
                                CAL_TransporteTerrestre transporteTerrestre = dcSoftwareCalidad.CAL_TransporteTerrestre.SingleOrDefault(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion && X.IdTransportista == result);
                                if (transporteTerrestre == null)
                                {
                                    transporteTerrestre = new CAL_TransporteTerrestre();
                                    transporteTerrestre.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;
                                    transporteTerrestre.IdTransportista = result;
                                    transporteTerrestre.FechaHoraIns = DateTime.Now;
                                    transporteTerrestre.IpIns = RemoteAddr();
                                    transporteTerrestre.UserIns = User.Identity.Name;
                                    dcSoftwareCalidad.CAL_TransporteTerrestre.InsertOnSubmit(transporteTerrestre);
                                    dcSoftwareCalidad.SubmitChanges();
                                }
                            }
                            else
                            {
                                errMsg = stringBuilder.ToString();
                                okMsg = "La orden de producción se ha editado pero con errores.";
                                return RedirectToAction("index", new { errMsg = errMsg, okMsg = okMsg });
                            }

                            currentRow++;
                        }
                    }
                    else
                    {
                        List<CAL_TransporteTerrestre> transporteTerrestreList = dcSoftwareCalidad.CAL_TransporteTerrestre.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion).ToList();
                        foreach (var transporteTerrestre in transporteTerrestreList)
                        {
                            dcSoftwareCalidad.CAL_TransporteTerrestre.DeleteOnSubmit(transporteTerrestre);
                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }

                ordenProduccion.NotificarEdicion();

                return RedirectToAction("index", new { errMsg = errMsg, okMsg = "La orden de producción se ha editado" });
            }
            catch
            {
                var rv = ordenProduccion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299)
            };
            return View("EditarGranel", ordenProduccion);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(296);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                ordenProduccion.Habilitado = false;
                ordenProduccion.UserUpd = User.Identity.Name;
                ordenProduccion.FechaHoraUpd = DateTime.Now;
                ordenProduccion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("La orden de producción {0} ha sido eliminado", ordenProduccion.LoteComercial);

                ordenProduccion.NotificarEliminacion();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Print(int id)
        {
            CheckPermisoAndRedirect(293);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299)
            };
            return View(ordenProduccion);
        }

        public ActionResult Ver(int id)
        {
            CheckPermisoAndRedirect(293);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299)
            };
            return View(ordenProduccion);
        }

        public ActionResult Cerrar(int id)
        {
            CheckPermisoAndRedirect(296);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                ordenProduccion.Terminada = true;
                ordenProduccion.UserUpd = User.Identity.Name;
                ordenProduccion.FechaHoraUpd = DateTime.Now;
                ordenProduccion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("La orden de producción {0} ha sido cerrada", ordenProduccion.LoteComercial);

                ordenProduccion.NotificarEliminacion();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Clonar(int id)
        {
            CheckPermisoAndRedirect(294);

            CAL_OrdenProduccion op = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (op == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la Orden de Produción", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dcSoftwareCalidad.CAL_ClonarUnaOrdenDeProduccion(op.IdOrdenProduccion, User.Identity.Name, DateTime.Now, RemoteAddr(), op.IdPlanta);
                okMsg = String.Format("La Orden de Produccón con el Lote {0} ha sido clonada", op.LoteComercial);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }

        public ActionResult CambiarHaciaGranel(int id)
        {

            CheckPermisoAndRedirect(295);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true && X.IdTipoOrdenProduccion == 1);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299)
            };

            return View(ordenProduccion);
        }

        public ActionResult CambiarHaciaEnvasado(int id)
        {
            CheckPermisoAndRedirect(295);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true && X.IdTipoOrdenProduccion == 2);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297),
                AutorizarOP = CheckPermiso(298),
                AdminFechasProduccion = CheckPermiso(299)
            };

            return View(ordenProduccion);
        }

        #endregion

        #region Re-Notificar

        public ActionResult Renotificarcreacion(int id)
        {
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.FirstOrDefault(X => X.IdOrdenProduccion == id && !X.Autorizado.Value && !X.Terminada);
            if (ordenProduccion == null) return Json(new Response { IsSuccess = false, Message = "No existe la orden de producción." }, JsonRequestBehavior.AllowGet);

            if (ordenProduccion.NotificarCreacion())
            {
                return Json(new Response { IsSuccess = true, Message = string.Format("Se ha re-notificado correctamente la orden de produción N° {0}.", ordenProduccion.IdOrdenProduccion) }, JsonRequestBehavior.AllowGet);
            }
            return Json(new Response { IsSuccess = false, Message = string.Format("No se ha podido re-notificar la orden de producción N° {0}.", ordenProduccion.IdOrdenProduccion) }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Espesores
        public ActionResult CrearEspesor(CAL_EspesorProducto espesor)
        {

            CheckPermisoAndRedirect(269);
            if (ModelState.IsValid)
            {
                CheckPermisoAndRedirect(294);
                try
                {
                    if (string.IsNullOrEmpty(espesor.Observaciones))
                        espesor.Observaciones = "";

                    espesor.Avg = Convert.ToDecimal((espesor.Max + espesor.Min) / 2);
                    espesor.Habilitado = true;
                    espesor.FechHoraIns = DateTime.Now;
                    espesor.IpIns = RemoteAddr();
                    espesor.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_EspesorProducto.InsertOnSubmit(espesor);
                    dcSoftwareCalidad.SubmitChanges();
                    return Json(new Response { IsSuccess = true, Message = "El espesor ha sido ingresado correctamente" }, JsonRequestBehavior.AllowGet);
                }
                catch
                {
                    var rv = espesor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return Json(new Response { IsSuccess = false, Message = "Algo ha salido mal a la hora de ingresar, por favor verifique que no existan espacios y que los números esten correctamente escritos." }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Fichas Técnicas

        public ActionResult ActivarFichaTecnica(int id, int IdFichaTecnica)
        {
            CheckPermisoAndRedirect(297);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_FichaTecnica fichaTecnica = dcSoftwareCalidad.CAL_FichaTecnica.SingleOrDefault(X => X.IdFichaTecnica == IdFichaTecnica);
            if (fichaTecnica == null)
            {
                return RedirectToAction("FichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                CAL_Subproducto subproducto = dcSoftwareCalidad.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == fichaTecnica.IdSubproducto);
                List<CAL_FichaTecnica> fichaTecnicas = dcSoftwareCalidad.CAL_FichaTecnica.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion && X.IdSubproducto == subproducto.IdSubproducto).ToList();
                foreach (CAL_FichaTecnica cAL_FichaTecnica in fichaTecnicas)
                {
                    cAL_FichaTecnica.Activa = false;
                    dcSoftwareCalidad.SubmitChanges();
                }

                fichaTecnica.Activa = true;
                dcSoftwareCalidad.SubmitChanges();

                okMsg = string.Format("La ficha técnica v{0} del producto {1} ha sido activada", fichaTecnica.Version, subproducto.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("FichasTecnicas", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult CrearFichaTecnica(int id)
        {
            CheckPermisoAndRedirect(297);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_FichaTecnica fichaTecnica = new CAL_FichaTecnica();
            fichaTecnica.IdOrdenProduccion = id;
            return View("CrearFichaTecnica", fichaTecnica);
        }

        [HttpPost]
        public ActionResult CrearFichaTecnica(int id, CAL_FichaTecnica fichaTecnica, HttpPostedFileBase postedFile)
        {
            CheckPermisoAndRedirect(297);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string errMsg = "";
                    string okMsg = "";

                    string path = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/{0}/", ordenProduccion.LoteComercial));
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    if (postedFile != null)
                    {
                        CAL_Subproducto subproducto = dcSoftwareCalidad.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == fichaTecnica.IdSubproducto);

                        string fileName = string.Format("vigente_{3:dd-MM-yy hh_mm_ss}_FT_LC_{0}_{4}_v{1}.{2}", ordenProduccion.LoteComercial, fichaTecnica.Version, Path.GetExtension(postedFile.FileName).Replace(".", ""), DateTime.Now, subproducto.Nombre.Replace(" ", "_").Replace("/", "-"));
                        postedFile.SaveAs(path + fileName);

                        List<CAL_FichaTecnica> fichaTecnicas = dcSoftwareCalidad.CAL_FichaTecnica.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion && X.IdSubproducto == fichaTecnica.IdSubproducto).ToList();
                        foreach (CAL_FichaTecnica cAL_FichaTecnica in fichaTecnicas)
                        {
                            cAL_FichaTecnica.Activa = false;
                            dcAgroFichas.SubmitChanges();
                        }

                        fichaTecnica.IdOrdenProduccion = id;
                        fichaTecnica.NombreArchivo = fileName;
                        fichaTecnica.Activa = true;
                        fichaTecnica.Habilitado = true;
                        fichaTecnica.FechaHoraIns = DateTime.Now;
                        fichaTecnica.IpIns = RemoteAddr();
                        fichaTecnica.UserIns = User.Identity.Name;
                        dcSoftwareCalidad.CAL_FichaTecnica.InsertOnSubmit(fichaTecnica);
                        dcSoftwareCalidad.SubmitChanges();

                        okMsg = "La ficha técnica ha sido subida";
                    }
                    else
                    {
                        errMsg = "No se ha podido subir la ficha técnica";
                    }

                    return RedirectToAction("FichasTecnicas", new { id = id, errMsg = errMsg, okMsg = okMsg });
                }
                catch
                {
                    var rv = fichaTecnica.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            fichaTecnica.IdOrdenProduccion = id;
            return View("CrearFichaTecnica", fichaTecnica);
        }

        public ActionResult DescargarFichaTecnica(int id, int IdFichaTecnica)
        {
            CheckPermisoAndRedirect(297);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_FichaTecnica fichaTecnica = dcSoftwareCalidad.CAL_FichaTecnica.SingleOrDefault(X => X.IdFichaTecnica == IdFichaTecnica);
            if (fichaTecnica == null)
            {
                return RedirectToAction("FichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            CAL_Subproducto subproducto = dcSoftwareCalidad.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == fichaTecnica.IdSubproducto);

            string docFile = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/{0}/{1}", ordenProduccion.LoteComercial, fichaTecnica.NombreArchivo));
            string contentType = "";
            switch (Path.GetExtension(docFile))
            {
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                default:
                    contentType = "application/octet-stream";
                    break;
            }

            return File(docFile, contentType, string.Format("FT_LC_{0}_{4}_v{1}.{2}", ordenProduccion.LoteComercial, fichaTecnica.Version, Path.GetExtension(docFile).Replace(".", ""), DateTime.Now, subproducto.Nombre.Replace(" ", "_").Replace("/", "-")));
        }

        public ActionResult EliminarFichaTecnica(int id, int IdFichaTecnica)
        {
            CheckPermisoAndRedirect(297);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_FichaTecnica fichaTecnica = dcSoftwareCalidad.CAL_FichaTecnica.SingleOrDefault(X => X.IdFichaTecnica == IdFichaTecnica);
            if (fichaTecnica == null)
            {
                return RedirectToAction("FichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                CAL_Subproducto subproducto = dcSoftwareCalidad.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == fichaTecnica.IdSubproducto);

                string path = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/{0}/borrado", ordenProduccion.LoteComercial));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string sourceFileName = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/{0}/{1}", ordenProduccion.LoteComercial, fichaTecnica.NombreArchivo));
                string destFileName = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/{0}/borrado/{1}", ordenProduccion.LoteComercial, fichaTecnica.NombreArchivo));

                System.IO.File.Move(sourceFileName, destFileName.Replace("vigente", "borrado"));

                dcSoftwareCalidad.CAL_FichaTecnica.DeleteOnSubmit(fichaTecnica);
                dcSoftwareCalidad.SubmitChanges();
                okMsg = "La ficha técnica ha sido eliminada";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("FichasTecnicas", new { id = id, errMsg = errMsg, okMsg = okMsg });

        }

        public ActionResult FichasTecnicas(int id)
        {
            CheckPermisoAndRedirect(297);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            List<CAL_FichaTecnica> list = dcSoftwareCalidad.CAL_FichaTecnica.Where(X => X.IdOrdenProduccion == id && X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(293),
                Crear = CheckPermiso(294),
                Actualizar = CheckPermiso(295),
                Borrar = CheckPermiso(296),
                AdminFichaTecnicas = CheckPermiso(297)
            };

            ViewData["IdOrdenProduccion"] = id;
            ViewData["LoteComercial"] = ordenProduccion.LoteComercial;

            // Init the main viewer object.
            var viewer = new DocViewer { ID = "ctlDoc", IncludeJQuery = false, DebugMode = false, BasePath = "/", FitType = "", Zoom = 40 };

            // Get the required client side script and css
            ViewBag.ViewerScripts = viewer.ReferenceScripts();
            ViewBag.ViewerCSS = viewer.ReferenceCss();
            ViewBag.ViewerID = viewer.ClientID;
            ViewBag.ViewerObject = viewer.JsObject;

            // Get final Init arguments to render the viewer, remove the empty viewing
            ViewBag.ViewerInit = viewer.GetAjaxInitArguments("").Replace("objctlDoc.View('');", "");

            string path = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/{0}/", ordenProduccion.LoteComercial));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Now loop through the files
            var filesToView = new DirectoryInfo(path).GetFiles();
            var fileToken = new Hashtable();
            var fileCount = 1;

            foreach (var file in filesToView)
            {
                CAL_FichaTecnica fichaTecnica = list.SingleOrDefault(X => X.NombreArchivo == file.Name);
                var index = list.FindIndex(X => X.NombreArchivo == file.Name);
                if (fichaTecnica != null)
                {
                    var viewerDummy = new DocViewer { ID = "viewerDummy" + fileCount, IncludeJQuery = false, DebugMode = false, BasePath = "/", FitType = "", Zoom = 50 };

                    // If only you can to explicitly open each document, before hand.
                    var token = viewerDummy.OpenDocument(file.FullName);

                    if (!token.IsNullOrWhiteSpace())
                    {
                        list[index].doc = new DictionaryEntry(file.Name, token);
                        fileCount++;
                    }
                }
            }

            ViewData["fileToken"] = fileToken;
            return View(list);
        }

        #endregion

        #region Fechas de Inicio y Término de Producción

        public ActionResult FechasProduccion(int id)
        {
            CheckPermisoAndRedirect(299);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            return View("FechasProduccion", ordenProduccion);
        }

        [HttpPost]
        public ActionResult FechasProduccion(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(299);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                UpdateModel(ordenProduccion, new string[] { "InicioProduccion", "TerminoProduccion" });

                ordenProduccion.UserUpd = User.Identity.Name;
                ordenProduccion.FechaHoraUpd = DateTime.Now;
                ordenProduccion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();

                okMsg = string.Format("Las fechas de inicio y término de producción se han asignado a la O/P {0}", ordenProduccion.LoteComercial);

                return RedirectToAction("index", new { errMsg = errMsg, okMsg = okMsg });
            }
            catch
            {
                var rv = ordenProduccion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("FechasProduccion", ordenProduccion);
        }

        #endregion
    }
}