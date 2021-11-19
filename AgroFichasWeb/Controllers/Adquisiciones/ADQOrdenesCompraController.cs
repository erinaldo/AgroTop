using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Adquisiciones
{
    [WebsiteAuthorize]
    public class ADQOrdenesCompraController : BaseApplicationController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ADQOrdenesCompraController()
        {
            SetCurrentModulo(11);
        }

        // GET: ADQOrdenCompra
        public ActionResult Index(int? IdEmpresa, int? IdTipoCompra, int? IdCentroCosto, int? IdEstado, string key = "")
        {
            CheckPermisoAndRedirect(450);

            var user = SYS_User.Current();

            var IdEmpresaSelect = IdEmpresa ?? 0;
            var IdTipoCompraSelect = IdTipoCompra ?? 0;
            var IdCentroCostoSelect = IdCentroCosto ?? 0;
            var IdEstadoSelect = IdEstado ?? 0;

            List<ADQ_SolicitudCompra> solicitudesCompra = (from X in dc.ADQ_SolicitudCompra
                                                           //join Y in dc.ADQ_DetalleSolicitudCompra on X.IdSolicitud equals Y.IdSolicitud
                                                           //join Z in dc.ADQ_Proyecto on X.IdSolicitud equals Z.IdSolicitud into proyectoSolicitud
                                                          where (IdEmpresaSelect == 0 || X.IdEmpresa == IdEmpresaSelect) &&
                                                             (IdTipoCompraSelect == 0 || X.IdTipoCompra == IdTipoCompraSelect) &&
                                                             (IdCentroCostoSelect == 0 || X.IdCentroCosto == IdCentroCostoSelect) &&
                                                             (IdEstadoSelect == 0 || X.IdEstado == IdEstadoSelect) &&
                                                             (key == "" ||
                                                              X.IdSolicitud.ToString().Contains(key) ||
                                                              X.Observacion.Contains(key) ||
                                                              X.Proveedor.Contains(key))
                                                       && X.Habilitado == true
                                                       && X.UserIns == user.UserName
                                                       select X).ToList();

            ADQ_SolicitudCompra solicitudCompra = new ADQ_SolicitudCompra();
            solicitudCompra.IdEmpresa = IdEmpresaSelect;
            solicitudCompra.IdTipoCompra = IdTipoCompraSelect;
            solicitudCompra.IdEstado = IdEstadoSelect;
            solicitudCompra.IdCentroCosto = IdCentroCostoSelect;

            ViewData["solicitudCompra"] = solicitudCompra;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            Permiso permisosUsuario = new Permiso();
            permisosUsuario.VerAreaCliente = CheckPermiso(450);

            ViewData["permisosUsuario"] = permisosUsuario;

            return View(solicitudesCompra);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(450);

            ADQ_SolicitudCompra solicitudCompra = new ADQ_SolicitudCompra();
            return View("Crear", solicitudCompra);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Crear(ADQ_SolicitudCompra solicitudCompra, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(450);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(solicitudCompra.Observacion))
                        solicitudCompra.Observacion = "";
                    if (string.IsNullOrEmpty(solicitudCompra.Proveedor))
                        solicitudCompra.Proveedor = "";

                    solicitudCompra.Habilitado = true;
                    solicitudCompra.IdEstado = 1;
                    solicitudCompra.FechaHoraIns = DateTime.Now;
                    solicitudCompra.IpIns = RemoteAddr();
                    solicitudCompra.UserIns = User.Identity.Name;
                    dc.ADQ_SolicitudCompra.InsertOnSubmit(solicitudCompra);
                    dc.SubmitChanges();

                    string errMsg = "";
                    string okMsg = "";

                    //Detalle de Solicitud de OC
                    int currentRow = 1;
                    if (!string.IsNullOrEmpty(formCollection["RowKey"]))
                    {
                        string[] rowKeys = formCollection["RowKey"].Split(new char[] { ',' }).ToArray<string>();
                        foreach (var rowKey in rowKeys)
                        {
                            var codigo_rowData = formCollection[string.Format("codigo_{0}", rowKey)];
                            var cantidad_rowData = formCollection[string.Format("cantidad_{0}", rowKey)];
                            var descripcion_rowData = formCollection[string.Format("descripcion_{0}", rowKey)];
                            var precio_rowData = formCollection[string.Format("precio_{0}", rowKey)];
                            {
                                bool requestOK = true;

                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine("<ul>");


                                //if (!int.TryParse(codigo_rowData, out int Codigo))
                                //{
                                //    stringBuilder.AppendLine("<li>El codigo no es válido</li>");
                                //    requestOK = false;
                                //}

                                if (!int.TryParse(cantidad_rowData, out int Cantidad))
                                {
                                    stringBuilder.AppendLine("<li>La cantidad no es válida</li>");
                                    requestOK = false;
                                }

                                //if (!int.TryParse(descripcion_rowData, out int Descripcion))
                                //{
                                //    stringBuilder.AppendLine("<li>La descripción no es válida</li>");
                                //    requestOK = false;
                                //}

                                if (!int.TryParse(precio_rowData, out int Precio))
                                {
                                    stringBuilder.AppendLine("<li>El precio no es válido</li>");
                                    requestOK = false;
                                }

                               stringBuilder.AppendLine("</ul>");


                                if (requestOK)
                                {
                                    ADQ_DetalleSolicitudCompra detalleSolicitudCompra = new ADQ_DetalleSolicitudCompra();
                                    detalleSolicitudCompra.IdSolicitud = solicitudCompra.IdSolicitud;
                                    detalleSolicitudCompra.FechaHoraIns = DateTime.Now;
                                    detalleSolicitudCompra.IpIns = RemoteAddr();
                                    detalleSolicitudCompra.UserIns = User.Identity.Name;
                                    detalleSolicitudCompra.RowKey = rowKey;
                                    detalleSolicitudCompra.Codigo = codigo_rowData;
                                    detalleSolicitudCompra.Cantidad = Cantidad;
                                    detalleSolicitudCompra.Descripcion = descripcion_rowData;
                                    detalleSolicitudCompra.Precio = Precio;
                                    dc.ADQ_DetalleSolicitudCompra.InsertOnSubmit(detalleSolicitudCompra);
                                    dc.SubmitChanges();
                                }
                                else
                                {
                                    errMsg = stringBuilder.ToString();
                                    okMsg = "La solicitud de orden de compra se ha creado pero con errores";
                                    return RedirectToAction("index", new { errMsg = errMsg, okMsg = okMsg });
                                }

                                currentRow++;
                            }
                        }
                    }
                    {
                        // Reset

                        errMsg = "";
                        okMsg = "";

                        //Proyecto
                        if (!string.IsNullOrEmpty(formCollection["Proyecto"]) || !string.IsNullOrEmpty(formCollection["Numero"]) || !string.IsNullOrEmpty(formCollection["IdDetalleProyecto"]))
                        {

                            ADQ_Proyecto proyecto = new ADQ_Proyecto();
                            proyecto.IdSolicitud = solicitudCompra.IdSolicitud;

                            if (string.IsNullOrEmpty(formCollection["Proyecto"]))
                                proyecto.Nombre = "";
                            else
                                proyecto.Nombre = formCollection["Proyecto"];

                            if (!int.TryParse(formCollection["Numero"], out int Numero))
                                proyecto.Numero = null;
                            else
                                proyecto.Numero = Numero;

                            if (!int.TryParse(formCollection["IdDetalleProyecto"], out int IdDetalleProyecto))
                                proyecto.IdDetalleProyecto = null;
                            else
                                proyecto.IdDetalleProyecto = IdDetalleProyecto;


                            proyecto.FechaHoraIns = DateTime.Now;
                            proyecto.IpIns = RemoteAddr();
                            proyecto.UserIns = User.Identity.Name;
                            dc.ADQ_Proyecto.InsertOnSubmit(proyecto);
                            dc.SubmitChanges();
                        }
                    }

                    //solicitudCompra.NotificarCreacion();

                    return RedirectToAction("index", new { errMsg = errMsg, okMsg = "La solicitud de orden de compra se ha creado" });
                }
                catch
                {
                    var rv = solicitudCompra.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", solicitudCompra);
        }

        public ActionResult Anular(int id)
        {
            CheckPermisoAndRedirect(450);

            ADQ_SolicitudCompra solicitudCompra = dc.ADQ_SolicitudCompra.SingleOrDefault(X => X.IdSolicitud == id && X.Habilitado == true);
            if (solicitudCompra == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la solicitud de órden de compra", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                solicitudCompra.IdEstado = 99;
                solicitudCompra.UserUpd = User.Identity.Name;
                solicitudCompra.FechaHoraUpd = DateTime.Now;
                solicitudCompra.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("La solicitud de órden de compra {0} ha sido anulada", solicitudCompra.IdSolicitud);

                //solicitudCompra.NotificarEliminacion();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }

        public ActionResult SolicitudesOC(int? IdEmpresa, int? IdTipoCompra, int? IdCentroCosto, int? IdEstado, string key = "")
        {
            CheckPermisoAndRedirect(451);

            var user = SYS_User.Current();

            var IdEmpresaSelect = IdEmpresa ?? 0;
            var IdTipoCompraSelect = IdTipoCompra ?? 0;
            var IdCentroCostoSelect = IdCentroCosto ?? 0;
            var IdEstadoSelect = IdEstado ?? 0;

            List<ADQ_SolicitudCompra> solicitudesCompra = (from X in dc.ADQ_SolicitudCompra
                                                           //join Y in dc.ADQ_DetalleSolicitudCompra on X.IdSolicitud equals Y.IdSolicitud
                                                           //join Z in dc.ADQ_Proyecto on X.IdSolicitud equals Z.IdSolicitud into proyectoSolicitud
                                                           where (IdEmpresaSelect == 0 || X.IdEmpresa == IdEmpresaSelect) &&
                                                              (IdTipoCompraSelect == 0 || X.IdTipoCompra == IdTipoCompraSelect) &&
                                                              (IdCentroCostoSelect == 0 || X.IdCentroCosto == IdCentroCostoSelect) &&
                                                              (IdEstadoSelect == 0 || X.IdEstado == IdEstadoSelect) &&
                                                              (key == "" ||
                                                               X.IdSolicitud.ToString().Contains(key) ||
                                                               X.Observacion.Contains(key) ||
                                                               X.Proveedor.Contains(key))
                                                        && X.Habilitado == true
                                                           select X).ToList();

            ADQ_SolicitudCompra solicitudCompra = new ADQ_SolicitudCompra();
            solicitudCompra.IdEmpresa = IdEmpresaSelect;
            solicitudCompra.IdTipoCompra = IdTipoCompraSelect;
            solicitudCompra.IdEstado = IdEstadoSelect;
            solicitudCompra.IdCentroCosto = IdCentroCostoSelect;

            ViewData["solicitudCompra"] = solicitudCompra;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            Permiso permisosUsuario = new Permiso();
            permisosUsuario.VerAreaCliente = CheckPermiso(450);

            ViewData["permisosUsuario"] = permisosUsuario;

            return View(solicitudesCompra);
        }
    }
}