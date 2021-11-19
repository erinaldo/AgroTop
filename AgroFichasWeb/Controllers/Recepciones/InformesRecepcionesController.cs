using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Recepciones
{
    [WebsiteAuthorize]
    public class InformesRecepcionesController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public InformesRecepcionesController()
        {
            SetCurrentModulo(5); //Informes
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DetalleRecepciones(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, string key = "", int run = 0)
        {
            CheckPermisoAndRedirect(63);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            List<rpt_DetalleRecepcionesResult> items; 
            if (run == 1)
            {
                items = dc.rpt_DetalleRecepciones(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo, key.Trim()).ToList();
            }
            else
            {
                items = new List<rpt_DetalleRecepcionesResult>();
            }

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["key"] = key;

            return View(items);
        }

        public ActionResult DetalleRecepcionesExport(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, string key = "")
        {
            CheckPermisoAndRedirect(63);
            
            if (idTemporada == 0)
                idTemporada = null;
            if (idEmpresa == 0)
                idEmpresa = null;
            if (idSucursal == 0)
                idSucursal = null;
            if (idCultivo == 0)
                idCultivo = null;

            var items = dc.rpt_DetalleRecepciones(idTemporada, idEmpresa, idSucursal, idCultivo, key.Trim()).ToList();
            return new CsvActionResult<rpt_DetalleRecepcionesResult>(items, "DetalleRecepciones.csv", 1, ';');
        }

        public ActionResult TiempoProcesoPlanta(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, string key = "")
        {
            CheckPermisoAndRedirect(65);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_DetalleRecepciones(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo, key.Trim()).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["key"] = key;

            return View(items);
        }

        public ActionResult TiempoProcesoPlantaExport(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, string key = "")
        {
            CheckPermisoAndRedirect(65);

            if (idTemporada == 0)
                idTemporada = null;
            if (idEmpresa == 0)
                idEmpresa = null;
            if (idSucursal == 0)
                idSucursal = null;
            if (idCultivo == 0)
                idCultivo = null;

            var items = dc.rpt_DetalleRecepciones(idTemporada, idEmpresa, idSucursal, idCultivo, key.Trim()).ToList();

            return new CsvActionResult<rpt_DetalleRecepcionesResult>(items, "TiempoProcesoPlanta.csv", 1, ';');
        }

        public ActionResult ResultadosAnalisis(int? idTemporada, int? idEmpresa, int? idSucursal, int idCultivo, string key = "")
        {
            CheckPermisoAndRedirect(66);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_DetalleRecepciones(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo, key.Trim()).ToList();

            var valores = dc.rpt_ValoresAnalisisRecepciones(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo, key).ToList();

            int[] idsParametros = valores.Where(v => v.Valor.HasValue).Select(v => v.IdParametroAnalisis).Distinct().ToArray();
            var parametros = dc.ParametroAnalisis.Where(pa => idsParametros.Contains(pa.IdParametroAnalisis)).ToList();

            ViewData["valoresAnalisis"] = valores;
            ViewData["parametros"] = parametros;

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["hideAllCultivos"] = "1";
            ViewData["idCultivo"] = idCultivo;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["cultivoLabel"] = dc.Cultivo.Single(c => c.IdCultivo == idCultivo).Nombre;
            ViewData["key"] = key;

            return View(items);
        }

        public ActionResult RecepcionesValorizadas(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, string key = "")
        {
            CheckPermisoAndRedirect(67);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_DetalleRecepciones(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo, key.Trim()).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["key"] = key;

            return View(items);
        }

        public ActionResult CoberturaTipoCambio(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, string key = "")
        {
            CheckPermisoAndRedirect(71);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_CoberturaTipoCambio(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo, key.Trim()).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["key"] = key;

            return View(items);
        }

        public ActionResult RecepcionesPorDia(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo)
        {
            CheckPermisoAndRedirect(73);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_RecepcionesPorDia(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";

            return View(items);
        }

        public ActionResult RecepcionesPorDiaExport(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo)
        {
            CheckPermisoAndRedirect(73);

            if (idTemporada == 0)
                idTemporada = null;
            if (idEmpresa == 0)
                idEmpresa = null;
            if (idSucursal == 0)
                idSucursal = null;

            var items = dc.rpt_RecepcionesPorDia(idTemporada, idEmpresa, idSucursal, idCultivo).ToList();

            return new CsvActionResult<rpt_RecepcionesPorDiaResult>(items, "RecepcionesPorDia.csv", 1, ',');
        }

        public ActionResult CumplimientoContratos(int? idTemporada, int idCultivo)
        {
            CheckPermisoAndRedirect(75);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_CumplimientoContrato(temporada.IdTemporada, idCultivo).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["hideAllCultivos"] = "1";
            ViewData["idCultivo"] = idCultivo;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["cultivoLabel"] = dc.Cultivo.Single(c => c.IdCultivo == idCultivo).Nombre;

            return View(items);
        }

        public ActionResult RecepcionesPorDiaYBodega(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, int idPeso = 1)
        {
            CheckPermisoAndRedirect(76);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_RecepcionesPorDiaYBodega(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["idCultivo"] = idCultivo ?? 0;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["cultivoLabel"] = idCultivo.HasValue ? dc.Cultivo.Single(c => c.IdCultivo == idCultivo).Nombre : "(Todos)";
            ViewData["idPeso"] = idPeso;
            ViewData["pesoLabel"] = idPeso == 1 ? "Peso Standard" : "Peso Neto";

            return View(items);
        }

        public ActionResult RecepcionesPorDiaYManga(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, int idPeso = 1)
        {
            CheckPermisoAndRedirect(83);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_RecepcionesPorDiaYManga(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["idCultivo"] = idCultivo ?? 0;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["cultivoLabel"] = idCultivo.HasValue ? dc.Cultivo.Single(c => c.IdCultivo == idCultivo).Nombre : "(Todos)";
            ViewData["idPeso"] = idPeso;
            ViewData["pesoLabel"] = idPeso == 1 ? "Peso Standard" : "Peso Neto";
            return View(items);
        }

        public ActionResult RecepcionesMangas(int? idTemporada, int? idEmpresa, int? idSucursal)
        {
            CheckPermisoAndRedirect(86);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_RecepcionesMangas(temporada.IdTemporada, idEmpresa, idSucursal).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            return View(items);
        }

        public ActionResult ComposicionMangas(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo, int idPeso = 1)
        {
            CheckPermisoAndRedirect(84);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_ComposicionManga(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["idCultivo"] = idCultivo ?? 0;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["cultivoLabel"] = idCultivo.HasValue ? dc.Cultivo.Single(c => c.IdCultivo == idCultivo).Nombre : "(Todos)";
            ViewData["idPeso"] = idPeso;
            ViewData["pesoLabel"] = idPeso == 1 ? "Peso Standard" : "Peso Neto";
            return View(items);
        }

        public ActionResult RecepcionesPorRut(int? idTemporada, int? idEmpresa, int? idSucursal, string key = "")
        {
            CheckPermisoAndRedirect(77);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_RecepcionesPorRut(temporada.IdTemporada, idEmpresa, idSucursal, null, key.Trim()).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["key"] = key;

            return View(items);
        }

        public ActionResult RecepcionesPorRutExport(int? idTemporada, int? idEmpresa, int? idSucursal, string key = "")
        {
            CheckPermisoAndRedirect(77);

            if (idTemporada == 0)
                idTemporada = null;
            if (idEmpresa == 0)
                idEmpresa = null;
            if (idSucursal == 0)
                idSucursal = null;

            var items = dc.rpt_RecepcionesPorRut(idTemporada, idEmpresa, idSucursal, null, key.Trim()).ToList();

            return new CsvActionResult<rpt_RecepcionesPorRutResult>(items, "RecepcionesPorRut.csv", 1, ';');
        }

        public ActionResult AnalisisDiariosAnalista(int? idTemporada, int? idEmpresa, int? idSucursal, int? idCultivo)
        {
            CheckPermisoAndRedirect(1001);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var items = dc.rpt_AnalisisPorDiaYAnalista(temporada.IdTemporada, idEmpresa, idSucursal, idCultivo).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["empresaLabel"] = idEmpresa.HasValue ? dc.Empresa.Single(e => e.IdEmpresa == idEmpresa).Nombre : "(Todas)";
            ViewData["idSucursal"] = idSucursal ?? 0;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["sucursalLabel"] = idSucursal.HasValue ? dc.Sucursal.Single(e => e.IdSucursal == idSucursal).Nombre : "(Todas)";
            ViewData["idCultivo"] = idCultivo ?? 0;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["cultivoLabel"] = idCultivo.HasValue ? dc.Cultivo.Single(c => c.IdCultivo == idCultivo).Nombre : "(Todos)";

            return View(items);
        }

    }
}
