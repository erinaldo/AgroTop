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
    public class PlanillasContratosController : BaseApplicationController
    {
        AgroFichasDBDataContext context = new AgroFichasDBDataContext();

        public PlanillasContratosController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(1033);

            PlanillaContratoViewModel viewModel = new PlanillaContratoViewModel()
            {
                Temporadas   = context.Temporada.Where(t => t.IdTemporada >= 8).ToList(),
                Cultivos     = (from cc in context.CultivoContrato
                                join cu in context.Cultivo on cc.IdCultivo equals cu.IdCultivo
                                select cu).Distinct().ToList(),
                UserIns      = User.Identity.Name,
                FechaHoraIns = DateTime.Now,
                IpIns        = RemoteAddr(),
                UserUpd      = User.Identity.Name,
                FechaHoraUpd = DateTime.Now,
                IpUpd        = RemoteAddr(),
                Permisos     = new List<Permiso>()
                {

                },
                Permiso      = new Permiso()
                {
                    Leer       = CheckPermiso(1033),
                    Crear      = CheckPermiso(1037),
                    Actualizar = CheckPermiso(1038),
                    Borrar     = CheckPermiso(1039)
                }
            };
            return View(viewModel);
        }
    }
}