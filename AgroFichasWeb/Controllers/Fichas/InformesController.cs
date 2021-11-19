using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Informes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class InformesController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public InformesController()
        {
            SetCurrentModulo(5); //Informes;
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(new int[] { 10, 223 });
            return View();
        }

        public ActionResult VisitasPorSemana(int? id, string cultivoFilterValue, string soloContrato = "")
        {
            CheckPermisoAndRedirect(10);

            var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();
            Temporada temporada;
            if (id.HasValue)
                temporada = temporadas.Single(t => t.IdTemporada == id.Value);
            else
                temporada = dc.Temporada.OrderByDescending(t => t.IdTemporada).First();

            var variedadFilter = cultivoFilterValue ?? "";

            var index = dc.rpt_VisitasPorSemana_Index(temporada.IdTemporada, variedadFilter, soloContrato == "1");
            var data = dc.rpt_VisitasPorSemana_Data(temporada.IdTemporada, variedadFilter, soloContrato == "1").ToList();
            var cultivos = dc.rpt_SuperficiePorCultivoPorAgricultor(temporada.IdTemporada, null, variedadFilter).ToList();
            var cultivosSemana = dc.rpt_VisitasPorSemana_Cultivos(temporada.IdTemporada, variedadFilter).ToList();
            var contratos = dc.rpt_VisitasPorSemana_Contratos(temporada.IdTemporada, variedadFilter).ToList();
            
            var semanas = SemanaAno.FromRange(data);

            ViewData["variedadFilter"] = variedadFilter;
            ViewData["variedadFilterDescription"] = VariedadFilterDescription(variedadFilter);
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["index"] = index;
            ViewData["data"] = data;
            ViewData["cultivos"] = cultivos;
            ViewData["cultivosSemana"] = cultivosSemana;
            ViewData["contratos"] = contratos;
            ViewData["semanas"] = semanas;
            ViewData["soloContrato"] = soloContrato == "1";
            return View();
        }

        private string VariedadFilterDescription(string variedadFilter)
        {
            if (variedadFilter == "")
                return "Todos los Cultivos";

            int[] idsVariedades = Array.ConvertAll((variedadFilter ?? "").Split(',').Where(item => item != "").ToArray(), int.Parse);

            var variedades = dc.Variedad.Where(v => idsVariedades.Contains(v.IdVariedad)).ToList();
            var cultivos = from variedad in variedades group variedad by variedad.IdCultivo into cultivo select new { IdCultivo = cultivo.Key, Variedades = cultivo.Count() };

            string s = "";
            foreach (var cultivo in cultivos)
            {
                var dbCultivo = dc.Cultivo.Single(c => c.IdCultivo == cultivo.IdCultivo);

                if (dbCultivo.Variedad.Count() == cultivo.Variedades)
                {
                    s += "* Todas las variedades de " + dbCultivo.Nombre;
                }
                else
                {
                    s += "* " + dbCultivo.Nombre + ": ";
                    string t = "";
                    foreach (var variedad in variedades.Where(v => v.IdCultivo == cultivo.IdCultivo))
                        t += (t != "" ? ", " : "") + variedad.Nombre;

                    s += t;
                }
                s += "<br/>";
            }

            return s;
        }

        public ActionResult VisitasPorSemana_Detalle(int idAgricultor, int idTemporada, int? userID, int? semana, int? ano, string cultivoFilter)
        {
            CheckPermisoAndRedirect(10);
            
            var ids = new List<int>();
            foreach (var id in dc.rpt_VisitasPorSemana_Semana(idAgricultor, semana, ano, idTemporada, userID))
            {
                ids.Add(id.IdFicha);
            }
            
            var fichas = dc.Ficha.Where(f => ids.Contains(f.IdFicha)).ToList();

            int[] idsVariedades = Array.ConvertAll((cultivoFilter ?? "").Split(',').Where(item => item != "").ToArray(), int.Parse);

            var model = new VisitasPorSemana_DetalleViewModel()
            {
                Agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == idAgricultor),
                Temporada = dc.Temporada.SingleOrDefault(t => t.IdTemporada == idTemporada),
                UsuarioIngreso = dc.SYS_User.SingleOrDefault(u => u.UserID == userID),
                Semana = semana,
                Fichas = fichas.Where(f => f.TieneVariedades(idsVariedades)).ToList()
            };

            return View(model);
        }

        public ActionResult VisitasPorSemanaAgenteAsignado(int? id, int? idCultivo, int? userId)
        {
            CheckPermisoAndRedirect(10);
            var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();
            Temporada temporada;
            if (id.HasValue)
                temporada = temporadas.Single(t => t.IdTemporada == id.Value);
            else
                temporada = dc.Temporada.OrderByDescending(t => t.IdTemporada).First();

            var agentes = dc.SYS_User.OrderBy(u => u.FullName).ToList();
            SYS_User agente;
            if (userId.HasValue)
                agente = agentes.Single(u => u.UserID == userId.Value);
            else
                agente = agentes.First();

            var idCultivoSelect = (idCultivo ?? 0);

            var index = dc.rpt_VisitasAgentePorSemana_Index(temporada.IdTemporada, agente.UserID, idCultivoSelect); 
            var data = dc.rpt_VisitasAgentePorSemana_Data(temporada.IdTemporada, agente.UserID, idCultivoSelect).ToList();
            var cultivos = dc.rpt_SuperficiePorCultivoPorAgricultor(temporada.IdTemporada, null, "").ToList();
            var cultivosSemana = dc.rpt_VisitasAgentePorSemana_Cultivos(temporada.IdTemporada, agente.UserID, idCultivoSelect).ToList();

            var semanas = SemanaAno.FromRange(data);

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["agentes"] = agentes;
            ViewData["agente"] = agente;
            ViewData["index"] = index;
            ViewData["data"] = data;
            ViewData["cultivos"] = cultivos;
            ViewData["cultivosSemana"] = cultivosSemana;
            ViewData["semanas"] = semanas;
            ViewData["variedadFilter"] = idCultivoSelect;
            return View();
        }

        public ActionResult VisitasPorSemanaAgenteIngreso(int? id, int? userId)
        {
            CheckPermisoAndRedirect(10);
            var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();
            Temporada temporada;
            if (id.HasValue)
                temporada = temporadas.Single(t => t.IdTemporada == id.Value);
            else
                temporada = dc.Temporada.OrderByDescending(t => t.IdTemporada).First();

            var agentes = dc.SYS_User.OrderBy(u => u.FullName).ToList();
            SYS_User agente;
            if (userId.HasValue)
                agente = agentes.Single(u => u.UserID == userId.Value);
            else
                agente = agentes.First();

            var index = dc.rpt_VisitasAgenteIngresoPorSemana_Index(temporada.IdTemporada, agente.UserID);
            var data = dc.rpt_VisitasAgenteIngresoPorSemana_Data(temporada.IdTemporada, agente.UserID).ToList();
            var cultivos = dc.rpt_SuperficiePorCultivoPorAgricultor(temporada.IdTemporada, null, "").ToList();
            var cultivosSemana = dc.rpt_VisitasAgenteIngresoPorSemana_Cultivos(temporada.IdTemporada, agente.UserID).ToList();

            var semanas = SemanaAno.FromRange(data);

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["agentes"] = agentes;
            ViewData["agente"] = agente;
            ViewData["index"] = index;
            ViewData["data"] = data;
            ViewData["cultivos"] = cultivos;
            ViewData["cultivosSemana"] = cultivosSemana;
            ViewData["semanas"] = semanas;
            return View();
        }

        public ActionResult VisitasPorSemanaAgenteIngresoPorAgente(int? id)
        {
            CheckPermisoAndRedirect(223);

            SYS_User agente = CurrentUser;
            if (agente.IdSeccion == 3)
            {
                var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();
                Temporada temporada;
                if (id.HasValue)
                    temporada = temporadas.Single(t => t.IdTemporada == id.Value);
                else
                    temporada = dc.Temporada.OrderByDescending(t => t.IdTemporada).First();

                var index = dc.rpt_VisitasAgenteIngresoPorSemana_Index(temporada.IdTemporada, agente.UserID);
                var data = dc.rpt_VisitasAgenteIngresoPorSemana_Data(temporada.IdTemporada, agente.UserID).ToList();
                var cultivos = dc.rpt_SuperficiePorCultivoPorAgricultor(temporada.IdTemporada, null, "").ToList();
                var cultivosSemana = dc.rpt_VisitasAgenteIngresoPorSemana_Cultivos(temporada.IdTemporada, agente.UserID).ToList();

                var semanas = SemanaAno.FromRange(data);

                ViewData["temporadas"] = temporadas;
                ViewData["temporada"] = temporada;
                ViewData["agente"] = agente;
                ViewData["index"] = index;
                ViewData["data"] = data;
                ViewData["cultivos"] = cultivos;
                ViewData["cultivosSemana"] = cultivosSemana;
                ViewData["semanas"] = semanas;
                return View();
            }
            else
            {
                return RedirectToAction("Error", new { errorMsg = "Solo los asesores agrícolas pueden ver este informe, si eres un asesor agrícola y no puedes ver este informe solicita al administrador del sistema que cambie la sección de tu usuario." });
            }
        }

        public ActionResult VisitasPorSemanaCultivoAgente(int? id)
        {
            CheckPermisoAndRedirect(10);

            var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();
            Temporada temporada;

            if (id.HasValue)
                temporada = temporadas.Single(t => t.IdTemporada == id.Value);
            else
                temporada = dc.Temporada.OrderByDescending(t => t.IdTemporada).First();

            var data = dc.rpt_VisitaPorSemanaCultivoAgente_Data(temporada.IdTemporada).ToList();

            var semanas = SemanaAno.FromRange(data);
            
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["data"] = data;
            ViewData["semanas"] = semanas;

            return View();
        }

        public ActionResult VisitasPorSemanaCultivoAgente_Excel(int? id)
        {
            CheckPermisoAndRedirect(10);

            var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();
            Temporada temporada;

            if (id.HasValue)
                temporada = temporadas.Single(t => t.IdTemporada == id.Value);
            else
                temporada = dc.Temporada.OrderByDescending(t => t.IdTemporada).First();

            var rpt = new VisitasPorSemanaCultivoAgente_Excel();
            var result = rpt.RunReport(temporada);

            return File(rpt.RunReport(temporada), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VisitasPorSemanaCultivoAgente.xlsx");
        }

        public ActionResult Recomendaciones(int? id, int? idCultivo)
        {
            CheckPermisoAndRedirect(10);

            var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();
            Temporada temporada;

            if (id.HasValue)
                temporada = temporadas.Single(t => t.IdTemporada == id.Value);
            else
                temporada = dc.Temporada.OrderByDescending(t => t.IdTemporada).First();

            var data = dc.rpt_Recomendaciones(temporada.IdTemporada, idCultivo).ToList();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["idCultivo"] = idCultivo ?? 0;
            ViewData["cultivos"] = dc.Cultivo.ToList();
            ViewData["cultivoLabel"] = idCultivo.HasValue ? dc.Cultivo.Single(c => c.IdCultivo == idCultivo).Nombre : "(Todos)";

        
            return View(data);

        }

        public ActionResult Recomendaciones_Excel(int? id, int? idCultivo)
        {
            CheckPermisoAndRedirect(10);

            var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();
            Temporada temporada;

            if (id.HasValue)
                temporada = temporadas.Single(t => t.IdTemporada == id.Value);
            else
                temporada = dc.Temporada.OrderByDescending(t => t.IdTemporada).First();

            if (idCultivo == 0)
                idCultivo = null;
            var data = dc.rpt_Recomendaciones(temporada.IdTemporada, idCultivo).ToList();

            return new ExcelActionResult<rpt_RecomendacionesResult>(data, "Recomendaciones.xlsx");
        }

        public ActionResult MapaPredios()
        {
            CheckPermisoAndRedirect(10);

            var predios = dc.Predio.Where(p => p.Lat != null && p.Lon != null).ToList();

            return View(predios);
        }

        public ActionResult AgricultoresRelacionados(string rutPadre, string rutHijo, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            CheckPermisoAndRedirect(142);

            List<rpt_AgricultoresRelacionadosResult> informe = dc.rpt_AgricultoresRelacionados(rutPadre, rutHijo, fechaDesde, fechaHasta).ToList();

            ViewData["rutPadre"] = rutPadre;
            ViewData["rutHijo"] = rutHijo;
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            return View(informe);
        }

        public ActionResult AgricultoresRelacionados_Excel(string rutPadre, string rutHijo, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            CheckPermisoAndRedirect(142);

            List<rpt_AgricultoresRelacionadosResult> informe = dc.rpt_AgricultoresRelacionados(rutPadre, rutHijo, fechaDesde, fechaHasta).ToList();

            return new ExcelActionResult<rpt_AgricultoresRelacionadosResult>(informe, "AgricultoresRelacionados.xlsx");
        }

        public ActionResult Error(string errorMsg)
        {
            ViewData["errorMsg"] = errorMsg;
            return View();
        }
    }

    public class MesAno
    {
        public int Mes { get; set; }
        public int Ano { get; set; }

        public int CuentaSemanas { get; set; }

        public static List<MesAno> FromSemanas(List<SemanaAno> semanas)
        {
            var result = (from sem in semanas
                          group sem.Semana by new { sem.Mes, sem.Ano } into ma
                          select new MesAno { Ano = ma.Key.Ano, Mes = ma.Key.Mes, CuentaSemanas = ma.Count() }).ToList();

            return result;
        }
    }

    public class SemanaAnoBasic : ISemanaAnoSource
    {
        public int? Ano { get; set; }
        public int? Semana { get; set; }
    }
    public class SemanaAno
    {
        public int Semana { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }

        public string MesDecription()
        {
            return new DateTime(Ano, Mes, 1).ToString("MMM yy");
        }


        public static List<SemanaAno> FromRange<T>(List<T> data) where T : ISemanaAnoSource
        {
            var minAno = data.Min(d => d.Ano) ?? DateTime.Now.Year;
            var maxAno = data.Max(d => d.Ano) ?? DateTime.Now.Year;

            var minSemana = data.Where(d => d.Ano == minAno).Min(d => d.Semana) ?? 1;
            var maxSemana = data.Where(d => d.Ano == maxAno).Max(d => d.Semana) ?? 1;

            return FromRange(minSemana, minAno, maxSemana, maxAno);
        }

        public static List<SemanaAno> FromRange(int minSemana, int minAno, int maxSemana, int maxAno)
        {
            var semanas = new List<SemanaAno>();

            for (int ano = minAno; ano <= maxAno; ano++)
            {
                int startWeek = (ano == minAno) ? minSemana : 1;
                int endWeek = (ano == maxAno) ? maxSemana : ControllerHelpers.LastWeek(ano);
                for (int week = startWeek; week <= endWeek; week++)
                {
                    semanas.Add(new SemanaAno() { Ano = ano, Semana = week, Mes = ControllerHelpers.GetMonth(ano, week) });
                }
            }

            return semanas;
        }
    }
}
