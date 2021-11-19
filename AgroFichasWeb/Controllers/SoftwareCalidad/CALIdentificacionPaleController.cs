using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.SoftwareCalidad;
using AgroFichasWeb.ViewModels.SoftwareCalidad;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALIdentificacionPaleController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALIdentificacionPaleController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(304);
            CALIdentificacionPale cALIdentificacionPale = new CALIdentificacionPale();
            return View(cALIdentificacionPale);
        }

        [HttpPost]
        public ActionResult Index(CALIdentificacionPale cALIdentificacionPale)
        {
            CheckPermisoAndRedirect(304);

            string errMsg = "";
            //string okMsg = "";

            if (cALIdentificacionPale.ValidacionIdentificacionPale(ModelState, System.Web.HttpContext.Current))
            {
                PaleViewModel paleViewModel = new PaleViewModel();
                CAL_Pale cAL_Pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.CodigoInterno == cALIdentificacionPale.QRCode);
                if (cAL_Pale != null)
                {
                    paleViewModel.Pale                   = cAL_Pale;
                    paleViewModel.DetalleOrdenProduccion = cAL_Pale.CAL_DetalleOrdenProduccion;
                    paleViewModel.OrdenProduccion        = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
                    paleViewModel.Producto               = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_Subproducto;

                    CAL_EspesorProducto cAL_EspesorProducto = dcSoftwareCalidad.CAL_EspesorProducto.SingleOrDefault(X => X.IdEspesorProducto == cAL_Pale.CAL_DetalleOrdenProduccion.IdEspesorProducto);
                    if (cAL_EspesorProducto != null)
                        paleViewModel.EspesorProducto = cAL_EspesorProducto;

                    paleViewModel.Saco = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_Saco;

                    CAL_GrupoEnvasador grupoEnvasador = dcSoftwareCalidad.CAL_GrupoEnvasador.SingleOrDefault(X => X.IdGrupoEnvasador == cAL_Pale.IdGrupoEnvasador);
                    paleViewModel.GrupoEnvasador = grupoEnvasador;

                    if (grupoEnvasador != null)
                    {
                        CAL_Turno2 turno2 = dcSoftwareCalidad.CAL_Turno2.SingleOrDefault(X => X.IdTurno == grupoEnvasador.IdTurno2);
                        if (turno2 != null)
                            paleViewModel.Turno2 = turno2;
                    }
                    else
                        paleViewModel.Turno2 = ResolveTurno2(dcSoftwareCalidad, cAL_Pale.FechaHoraIns);

                    paleViewModel.CALIdentificacionPale = new CALIdentificacionPale();

                    ViewData["permisosUsuario"] = new Permiso()
                    {
                        CrearAnalisisPallet = CheckPermiso(328)
                    };

                    paleViewModel.AnalisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == cAL_Pale.IdPale);
                    if (paleViewModel.AnalisisPale != null)
                    {
                        paleViewModel.AnalisisPaleTestList = dcSoftwareCalidad.CAL_AnalisisPaleTest.Where(X => X.IdAnalisisPale == paleViewModel.AnalisisPale.IdAnalisisPale).ToList();
                        paleViewModel.TurnoAnalisisPale    = CALResolveTurno(dcSoftwareCalidad, paleViewModel.AnalisisPale.FechaHoraIns);
                    }

                    return View("Pale", paleViewModel);
                }
                else
                {
                    errMsg = "No se encuentra el pallet con QR-Code escaneado";
                    ModelState.AddModelError("QRCode", errMsg);
                }
            }

            return View(cALIdentificacionPale);
        }

        public ActionResult Imprimir(int id)
        {
            CheckPermisoAndRedirect(304);

            CAL_Pale cAL_Pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (cAL_Pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            PaleViewModel paleViewModel = new PaleViewModel
            {
                Pale                   = cAL_Pale,
                DetalleOrdenProduccion = cAL_Pale.CAL_DetalleOrdenProduccion,
                OrdenProduccion        = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion,
                Producto               = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_Subproducto
            };

            CAL_EspesorProducto cAL_EspesorProducto = dcSoftwareCalidad.CAL_EspesorProducto.SingleOrDefault(X => X.IdEspesorProducto == cAL_Pale.CAL_DetalleOrdenProduccion.IdEspesorProducto);
            if (cAL_EspesorProducto != null)
                paleViewModel.EspesorProducto = cAL_EspesorProducto;

            paleViewModel.Saco = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_Saco;

            CAL_GrupoEnvasador grupoEnvasador = dcSoftwareCalidad.CAL_GrupoEnvasador.SingleOrDefault(X => X.IdGrupoEnvasador == cAL_Pale.IdGrupoEnvasador);
            paleViewModel.GrupoEnvasador = grupoEnvasador;

            if (grupoEnvasador != null)
            {
                CAL_Turno turno = dcSoftwareCalidad.CAL_Turno.SingleOrDefault(X => X.IdTurno == grupoEnvasador.IdTurno);
                if (turno != null)
                    paleViewModel.Turno = turno;
            }
            else
                paleViewModel.Turno = CALResolveTurno(dcSoftwareCalidad, cAL_Pale.FechaHoraIns);

            paleViewModel.CALIdentificacionPale = new CALIdentificacionPale();

            paleViewModel.AnalisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == cAL_Pale.IdPale && X.Habilitado == true);
            if (paleViewModel.AnalisisPale != null)
            {
                paleViewModel.AnalisisPaleTestList = dcSoftwareCalidad.CAL_AnalisisPaleTest.Where(X => X.IdAnalisisPale == paleViewModel.AnalisisPale.IdAnalisisPale).ToList();
                paleViewModel.TurnoAnalisisPale    = CALResolveTurno(dcSoftwareCalidad, paleViewModel.AnalisisPale.FechaHoraIns);
            }

            return View("Imprimir", paleViewModel);
        }
    }
}