using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.TrazaTop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Liquidaciones
{
    [WebsiteAuthorize]
    public class SolicitudesContratosController : BaseApplicationController
    {
        AgroFichasDBDataContext context = new AgroFichasDBDataContext();

        public SolicitudesContratosController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(1032);

            SolicitudContratoViewModel viewModel = new SolicitudContratoViewModel()
            {
                SolicitudContratos = context.SolicitudContrato.Where(sc => sc.Verificado.Value).ToList(),
                Permiso = new Permiso()
                {
                    Leer           = CheckPermiso(1032),
                    Crear          = CheckPermiso(1034),
                    Actualizar     = CheckPermiso(1035),
                    Borrar         = CheckPermiso(1036),
                    VerContrato    = CheckPermiso(11),
                    CrearContrato  = CheckPermiso(12),
                    AnularContrato = CheckPermiso(14)
                }
            };

            return View("Index_Modal", viewModel);
        }

        public ActionResult Previsualizar(int id)
        {
            SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == id);
            if (solicitudContrato != null)
            {
                switch (solicitudContrato.IdTipoContrato)
                {
                    case 1:
                        //Acuerdo Comercial
                        Models.TrazaTop.Documentos.AcuerdoComercial acuerdoComercial = new Models.TrazaTop.Documentos.AcuerdoComercial(solicitudContrato.IdSolicitudContrato);
                        if (acuerdoComercial.ExistePlanilla())
                        {
                            ViewData["html"] = acuerdoComercial.CrearHTML();
                        }
                        else
                        {
                            ViewData["html"] = "No existe la planilla para este tipo de contrato";
                        }
                        break;
                    case 2:
                        //Contrato
                        Models.TrazaTop.Documentos.Contrato contrato = new Models.TrazaTop.Documentos.Contrato(solicitudContrato.IdSolicitudContrato);
                        if (contrato.ExistePlanilla())
                        {
                            ViewData["html"] = contrato.CrearHTML();
                        }
                        else
                        {
                            ViewData["html"] = "No existe la planilla para este tipo de contrato";
                        }
                        break;
                    case 3:
                        //Cierre de Negocio
                        Models.TrazaTop.Documentos.CierreNegocio cierreNegocio = new Models.TrazaTop.Documentos.CierreNegocio(solicitudContrato.IdSolicitudContrato);
                        if (cierreNegocio.ExistePlanilla())
                        {
                            ViewData["html"] = cierreNegocio.CrearHTML();
                        }
                        else
                        {
                            ViewData["html"] = "No existe la planilla para este tipo de contrato";
                        }
                        break;
                }
            }

            return View();
        }
    }
}