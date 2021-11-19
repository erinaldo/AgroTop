using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.ControlTiempo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.ControlTiempo
{
    [WebsiteAuthorize]
    public class InformesControlTiempoController : BaseApplicationController
    {
        //
        // GET: /InformesControlTiempo/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public InformesControlTiempoController()
        {
            SetCurrentModulo(9);
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(242);
            return View();
        }

        private IEnumerable<SelectListItem> GetPlantas(int? IdPlantaProduccion)
        {
            IEnumerable<SelectListItem> plantas = from us in dc.PlantaUsuario
                                                  join pl in dc.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
                                                  where us.UserID == CurrentUser.UserID
                                                  && pl.Habilitado
                                                  select new SelectListItem
                                                  {
                                                      Value = pl.IdPlantaProduccion.ToString(),
                                                      Text = pl.Nombre,
                                                      Selected = IdPlantaProduccion == pl.IdPlantaProduccion
                                                  };
            return plantas;
        }

        public ActionResult InformeGeneral(int? IdEstado, int? IdIncremento, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(242);

            int IdEstadoSelect     = IdEstado ?? 4;
            int IdIncrementoSelect = IdIncremento ?? -1;
            int IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            IEnumerable<SelectListItem> plantas = GetPlantas(IdPlantaProduccionSelect);

            ViewData["PlantasProduccion"] = plantas;

            InformeGeneralViewModel model = new InformeGeneralViewModel();
            model.InformeGeneral = dc.rpt_CTR_ControlDeTiempo(IdEstadoSelect, IdIncrementoSelect, IdPlantaProduccionSelect).ToList();
            model.IdEstado = IdEstadoSelect;
            model.IdIncremento = IdIncrementoSelect;

            return View(model);
        }

        public ActionResult InformeTiempoRomana(int? IdEmpresa, DateTime? Fecha, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(242);

            var IdEmpresaSelect = IdEmpresa ?? 1;
            var FechaSelect = (Fecha.HasValue ? Fecha.Value : DateTime.Now);

            int IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            IEnumerable<SelectListItem> plantas = GetPlantas(IdPlantaProduccionSelect);

            ViewData["PlantasProduccion"] = plantas;

            var controlesTiempo = dc.rpt_CTR_DetalleControlDiarioRomanaPorEmpresa(IdEmpresaSelect, FechaSelect, IdPlantaProduccionSelect).ToList();

            CTR_ControlTiempo controlTiempo = new CTR_ControlTiempo();
            controlTiempo.IdEmpresa = IdEmpresaSelect;
            controlTiempo.FechaHoraIns = FechaSelect;


            ViewData["controlTiempo"] = controlTiempo;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(225),
                                                      CheckPermiso(224),
                                                      CheckPermiso(226),
                                                      CheckPermiso(227));

            return View(controlesTiempo);
        }

        public ActionResult PrintInformeTiempoRomana(int? IdEmpresa, DateTime? Fecha, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(242);

            var IdEmpresaSelect = IdEmpresa ?? 1;
            var FechaSelect = (Fecha.HasValue ? Fecha.Value : DateTime.Now);

            int IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            IEnumerable<SelectListItem> plantas = GetPlantas(IdPlantaProduccionSelect);

            var controlesTiempo = dc.rpt_CTR_DetalleControlDiarioRomanaPorEmpresa(IdEmpresa, FechaSelect, IdPlantaProduccionSelect).ToList();

            CTR_ControlTiempo controlTiempo = new CTR_ControlTiempo();
            controlTiempo.IdEmpresa = IdEmpresaSelect;
            controlTiempo.FechaHoraIns = FechaSelect;


            ViewData["controlTiempo"] = controlTiempo;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(225),
                                                      CheckPermiso(224),
                                                      CheckPermiso(226),
                                                      CheckPermiso(227));

            return View(controlesTiempo);
        }

        public ActionResult InformeDiarioCamionesPlanta(int? IdEmpresa, int? IdProducto, int? IdEnvase, DateTime? FechaDesde, DateTime? FechaHasta, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(242);

            var IdEmpresaSelect  = IdEmpresa ?? 0;
            var IdProductoSelect = IdProducto ?? 0;
            var IdEnvaseSelect   = IdEnvase ?? 0;
            var FechaDesdeSelect = (FechaDesde.HasValue ? FechaDesde.Value : DateTime.Now);
            var FechaHastaSelect = (FechaHasta.HasValue ? FechaHasta.Value : DateTime.Now);

            //var FechaSelect = (Fecha.HasValue ? Fecha.Value : DateTime.Now);

            int IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            IEnumerable<SelectListItem> plantas = GetPlantas(IdPlantaProduccionSelect);

            ViewData["PlantasProduccion"] = plantas;


            var controlesTiempo = dc.rpt_CTR_DetalleControlDiarioPorEmpresa(IdEmpresaSelect, FechaDesdeSelect, FechaHastaSelect, IdProductoSelect, IdEnvaseSelect, IdPlantaProduccionSelect).ToList();

            CTR_ControlTiempo controlTiempo = new CTR_ControlTiempo();
            controlTiempo.IdEmpresa = IdEmpresaSelect;
            controlTiempo.IdProducto = IdProductoSelect;
            controlTiempo.FechaDesde = FechaDesdeSelect;
            controlTiempo.FechaHasta = FechaHastaSelect;

            ViewData["IdEmpresa"] = IdEmpresaSelect;
            ViewData["FechaDesde"] = FechaDesdeSelect;
            ViewData["FechaHasta"] = FechaHastaSelect;
            ViewData["IdProducto"] = IdProductoSelect;
            ViewData["IdEnvase"] = IdEnvaseSelect;

            ViewData["controlTiempo"] = controlTiempo;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(225),
                                                      CheckPermiso(224),
                                                      CheckPermiso(226),
                                                      CheckPermiso(227));

            return View(controlesTiempo);
        }

        public ActionResult InformeDiarioCamionesPlantaExport(int? IdEmpresa, int? IdProducto, int? IdEnvase, DateTime? FechaDesde, DateTime? FechaHasta, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(242);
            var IdEmpresaSelect = IdEmpresa ?? 1;
            var IdProductoSelect = IdProducto ?? 0;
            var IdEnvaseSelect = IdEnvase ?? 0;
            var FechaDesdeSelect = (FechaDesde.HasValue ? FechaDesde.Value : DateTime.Now);
            var FechaHastaSelect = (FechaHasta.HasValue ? FechaHasta.Value : DateTime.Now);

            var items = dc.rpt_CTR_DetalleControlDiarioPorEmpresa(IdEmpresaSelect,FechaDesdeSelect, FechaHastaSelect, IdProductoSelect, IdEnvaseSelect, IdPlantaProduccion).ToList();

            return new CsvActionResult<rpt_CTR_DetalleControlDiarioPorEmpresaResult>(items.ToList(), "DetalleControlDiarioPorEmpresa.csv", 1, ';', null);
        }
    }
}