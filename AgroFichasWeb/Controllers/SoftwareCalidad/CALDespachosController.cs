using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALDespachosController : BaseApplicationController
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private OperacionesDBDataContext dcOperaciones = new OperacionesDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALDespachosController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALDespachos
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(356);
            Permiso permisosUsuario = new Permiso(CheckPermiso(357),
                                                  CheckPermiso(356),
                                                  CheckPermiso(358),
                                                  CheckPermiso(359));
            permisosUsuario.CrearCargaDividida = CheckPermiso(370);
            ViewData["permisosUsuario"] = permisosUsuario;
            return View();
        }
    }
}