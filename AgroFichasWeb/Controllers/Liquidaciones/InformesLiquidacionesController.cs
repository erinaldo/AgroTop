using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Liquidaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Liquidaciones
{
    [WebsiteAuthorize]
    public class InformesLiquidacionesController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public InformesLiquidacionesController()
        {
            SetCurrentModulo(5); //Informes
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ControlCambioMoneda(int? idTemporada, int idEmpresa = 1)
        {
            CheckPermisoAndRedirect(82);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = (from ccm in dc.ConvenioCambioMoneda
                         where ccm.Contrato.IdTemporada == temporada.IdTemporada
                            && ccm.Contrato.IdEmpresa == idEmpresa
                         orderby ccm.Contrato.Agricultor.Nombre
                         select ccm).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] =  dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre;
            ViewData["hideAllEmpresas"] = "1";

            return View(items);
        }

        public ActionResult IntencionContratoPrecioIngresos(int? idTemporada, int? idCultivo)
        {
            CheckPermisoAndRedirect(82);

            //TODO: 2 Sucursal para intención de siembra

            //Temporadas
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out Temporada temporada);

            var conveniosPorSemana = dc.rpt_ConvenioPreciosPorSemana(idTemporada, idCultivo).ToList();
            var ingresosPorSemana = dc.rpt_IngresosPorSemana(idTemporada, idCultivo).ToList();
            

            var data = new List<SemanaAnoBasic>();
            data.AddRange(from x in conveniosPorSemana select new SemanaAnoBasic() { Ano = x.Ano, Semana = x.Semana });
            data.AddRange(from x in conveniosPorSemana select new SemanaAnoBasic() { Ano = x.Ano, Semana = x.Semana });

            var semanas = SemanaAno.FromRange(data);

            var model = new IntencionContratoPrecioIngresosViewModel(dc, temporada.IdTemporada, idCultivo ?? 0, User.Identity.Name);

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idCultivo"] = idCultivo ?? 0;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["cultivoLabel"] = idCultivo.HasValue ? dc.Cultivo.Single(c => c.IdCultivo == idCultivo).Nombre : "(Todos)";
            ViewData["semanas"] = semanas;
            ViewData["conveniosPorSemana"] = conveniosPorSemana;
            ViewData["ingresosPorSemana"] = ingresosPorSemana;

            return View(model);
        }
    }
}
