using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.SoftwareCalidad;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALInformesController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALInformesController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALInformes
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(318);
            ViewData["permisosUsuario"] = new Permiso()
            {
                CALInformes = CheckPermiso(318),
                CALInformesPaletizacionPorTurno = CheckPermiso(317)
            };
            return View();
        }

        public ActionResult PaletizacionPorTurno(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(317);

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

            List<vw_CAL_SoloTurnoPallet> list = dcSoftwareCalidad.vw_CAL_SoloTurnoPallet.Where(X => X.IdPlanta == IdPlantaProduccionSelect).OrderByDescending(X => X.Date).ThenByDescending(X => X.IdTurno).ToList();

            ViewData["PlantasProduccion"] = plantas;
            return View("PaletizacionPorTurnoIndex", list);
        }

        [HttpPost]
        public ActionResult PaletizacionPorTurno(DateTime dateTime, int IdTurno)
        {
            CheckPermisoAndRedirect(317);
            List<CAL_Pale> pales = (from X in dcSoftwareCalidad.vw_CAL_TurnoPallet
                                    join Y in dcSoftwareCalidad.CAL_Pale
                                    on X.IdPale equals Y.IdPale
                                    where X.Date == dateTime.Date
                                    select Y).ToList();

            List<CAL_OrdenProduccion> ordenesProduccion = pales.DistinctBy(X => X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdOrdenProduccion).Select(X => X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion).ToList();

            InformeViewModel informeViewModel = new InformeViewModel
            {
                dateTime          = dateTime,
                OrdenesProduccion = ordenesProduccion,
                Pales             = pales,
                Turno             = dcSoftwareCalidad.CAL_Turno.Single(X => X.IdTurno == IdTurno)
            };

            return View("PaletizacionPorTurnoData", informeViewModel);
        }
    }
}