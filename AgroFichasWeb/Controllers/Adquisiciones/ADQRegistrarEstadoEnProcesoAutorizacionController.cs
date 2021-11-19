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
    public class ADQRegistrarEstadoEnProcesoAutorizacionController : BaseApplicationController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ADQRegistrarEstadoEnProcesoAutorizacionController()
        {
            SetCurrentModulo(11);
        }

        // GET: ADQRegistrarEstadoEnCotización
        public ActionResult Index(int? IdEmpresa, int? IdTipoCompra, int? IdCentroCosto, int? IdEstado, string key = "")
        {
            CheckPermisoAndRedirect(453);

            var IdEmpresaSelect = IdEmpresa ?? 0;
            var IdTipoCompraSelect = IdTipoCompra ?? 0;
            var IdCentroCostoSelect = IdCentroCosto ?? 0;
            var IdEstadoSelect = IdEstado ?? 0;

            List<ADQ_SolicitudCompra> solicitudesCompra = (from X in dc.ADQ_SolicitudCompra
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
                                                              && X.IdEstado == 2
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

        public ActionResult Registrar(int id)
        {
            CheckPermisoAndRedirect(453);

            ADQ_SolicitudCompra solicitudCompra = dc.ADQ_SolicitudCompra.SingleOrDefault(X => X.IdSolicitud == id && X.Habilitado == true);
            if (solicitudCompra == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la solicitud de órden de compra", okMsg = "" });
            }

            return View("Registrar", solicitudCompra);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Registrar(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(453);
            ADQ_SolicitudCompra aDQ_SolicitudCompra = dc.ADQ_SolicitudCompra.SingleOrDefault(X => X.IdSolicitud == id);
            if (aDQ_SolicitudCompra == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado la solicitud de órden de compra", okMsg = "" }); }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(aDQ_SolicitudCompra, new string[] { "IdEstado" });

                    if (aDQ_SolicitudCompra.ValidacionEnProcesoAutorizacion(aDQ_SolicitudCompra, ModelState))
                    {
                        aDQ_SolicitudCompra.IdEstado = 3;
                        aDQ_SolicitudCompra.FechaHoraEnProcesoAutIns = DateTime.Now;
                        aDQ_SolicitudCompra.FechaHoraUpd = DateTime.Now;
                        aDQ_SolicitudCompra.IpUpd = RemoteAddr();
                        aDQ_SolicitudCompra.UserUpd = User.Identity.Name;
                        dc.SubmitChanges();
                        var okMsg = String.Format("La solicitud de órden de compra {0} ha cambiado de estado con éxito", aDQ_SolicitudCompra.IdSolicitud);
                        return RedirectToAction("Index", new { okMsg = okMsg, errMsg = "" });
                    }
                }
                catch
                {
                    var rv = aDQ_SolicitudCompra.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Registrar", aDQ_SolicitudCompra);
        }
    }
}