using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers
{
    
    public class BaseApplicationController : Controller
    {
        private int defaultPageSize;
        public int DefaultPageSize
        {
            get
            {
                return defaultPageSize;
            }
        }

        public SYS_User CurrentUser
        {
            get
            {
                return SYS_User.Current();
            }
        }

        public BaseApplicationController()
        {
            defaultPageSize = int.Parse(ConfigurationManager.AppSettings["Admin.DefaultPageSize"]);
        }

        public void SetCurrentModulo(int idModulo)
        {
            if (CurrentUser != null)
                CurrentUser.IdModuloActivo = idModulo;
        }

        public string RemoteAddr()
        {
            return HttpContext.Request.ServerVariables["REMOTE_ADDR"];
        }

        public void CheckPermisoAndRedirect(int[] permisos)
        {
            foreach (var idPermiso in permisos)
                if (CurrentUser.HasPermiso(idPermiso))
                    return;

            RedirectToFirstMenu();
        }
        
        public void CheckPermisoAndRedirect(int idPermiso)
        {
            if (!CurrentUser.HasPermiso(idPermiso))
            {
                RedirectToFirstMenu();
            }
        }

        public bool CheckPermiso(int idPermiso)
        {
            return CurrentUser.HasPermiso(idPermiso);
        }

        public void CheckPermisoAndSucursalRedirect(int idPermiso, int idSucursal, string zoneToken)
        {
            if (!CurrentUser.HasPermiso(idPermiso) || !CurrentUser.TieneAccesoSucursal(idSucursal, zoneToken))
            {
                RedirectToFirstMenu();
            }
        }

        public void RedirectToFirstMenu()
        {
            foreach (var modulo in CurrentUser.Modulos)
            {
                var firstMenu = CurrentUser.ItemsMenu(modulo.ID).FirstOrDefault();
                if (firstMenu != null)
                    Response.Redirect(firstMenu.Url, true);
            }

            Response.Redirect("~/Account/Logon");
        }

        protected List<Temporada> ResolveTemporadas(AgroFichasDBDataContext dc, int? idTemporada, out Temporada temporada)
        {
            var temporadas = dc.Temporada.OrderBy(t => t.IdTemporada).ToList();

            if (idTemporada.HasValue)
                temporada = temporadas.Single(t => t.IdTemporada == idTemporada.Value);
            else
                temporada = dc.Temporada.Where(t => t.Activa).OrderByDescending(t => t.IdTemporada).First();

            return temporadas;
        }

        protected CAL_Turno TurnoAnteriorSiguiente(SoftwareCalidadDBDataContext dc, DateTime fecha) 
        {
            var ts = new TimeSpan(fecha.Hour, fecha.Minute, fecha.Second);

            var turno = dc.CAL_Turno.SingleOrDefault(X => X.DiaDeLaSemana == (((int)fecha.DayOfWeek) == 0 ? 7 : (int)fecha.DayOfWeek) && (ts >= X.InicioTurno && ts <= X.FinTurno) && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);
            if (turno == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Turno Not Found");
            return turno;
        }

        protected CAL_Turno ResolveTurno3(SoftwareCalidadDBDataContext dc)
        {
            var dt = DateTime.Now;
            var ts = new TimeSpan(dt.Hour, dt.Minute, dt.Second);

            var turno = dc.CAL_Turno.SingleOrDefault(X => X.DiaDeLaSemana == (((int)dt.DayOfWeek) == 0 ? 7 : (int)dt.DayOfWeek) && (ts >= X.InicioTurno && ts <= X.FinTurno) && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);
            if (turno == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Turno Not Found");

            return turno;
        }

        protected CAL_Turno2 ResolveTurno2(SoftwareCalidadDBDataContext dc, DateTime dt)
        {
            CAL_GetTurno2AnteriorSiguienteResult AnteriorSiguiente = dc.CAL_GetTurno2AnteriorSiguiente(dt).SingleOrDefault();
            CAL_Turno2 turno2 = dc.CAL_Turno2.SingleOrDefault(X => X.IdTurno == AnteriorSiguiente.TAct && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);
            if (turno2 == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Turno Not Found");

            return turno2;
        }

        protected CAL_Turno CALResolveTurno(SoftwareCalidadDBDataContext dc, DateTime dt)
        {
            var ts = new TimeSpan(dt.Hour, dt.Minute, dt.Second);

            var turno = dc.CAL_Turno.SingleOrDefault(X => X.DiaDeLaSemana == (((int)dt.DayOfWeek) == 0 ? 7 : (int)dt.DayOfWeek) && (ts >= X.InicioTurno && ts <= X.FinTurno) && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);
            if (turno == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Turno Not Found");

            return turno;
        }

        protected OPR_Turno ResolveTurno(OperacionesDBDataContext dc)
        {
            var dt = DateTime.Now;
            var ts = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            var turno = dc.OPR_Turno.SingleOrDefault(X => X.DiaDeLaSemana == (((int)dt.DayOfWeek) == 0 ? 7 : (int)dt.DayOfWeek) && (ts >= X.InicioTurno && ts <= X.FinTurno));
            if (turno == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Turno Not Found");

            return turno;
        }

        protected OPR_Turno ResolveTurno(OperacionesDBDataContext dc, DateTime dt)
        {
            var ts = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            var turno = dc.OPR_Turno.SingleOrDefault(X => X.DiaDeLaSemana == (((int)dt.DayOfWeek) == 0 ? 7 : (int)dt.DayOfWeek) && (ts >= X.InicioTurno && ts <= X.FinTurno));
            if (turno == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Turno Not Found");

            return turno;
        }

        protected OPR_RegistroTurno GetTurnoAnterior(OperacionesDBDataContext dc, OPR_RegistroTurno registroTurno)
        {
            var turno = registroTurno.OPR_Turno;

            var d = turno.DiaDeLaSemana;
            var dt = DateTime.Now;
            var t = turno.IdTipoTurno;

            if (t == 1)
            {
                d -= 1;
                if (d == 0)
                    d = 7;
                dt = dt.AddDays(-1);
            }

            if (dt.Date != DateTime.Now.Date)
            {
                if (t == 1)
                {
                    if (d <= 6)
                    {
                        t = 4; //t + 1
                    }
                    else if (d == 7)
                    {
                        t = 3; //t + 1
                    }
                }
            }

            if (t > 1)
            {
                t -= 1;
            }

            OPR_RegistroTurno registroTurnoAnterior = null;
            var turnoAnterior = dc.OPR_Turno.Single(X => X.IdTipoTurno == t && X.DiaDeLaSemana == d);
            if (turnoAnterior.IdTipoTurno >= turno.IdTipoTurno)
            {
                var dtAux = registroTurno.FechaHoraIns.AddDays(-1);
                registroTurnoAnterior = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == dtAux.Date && X.IdTurno == turnoAnterior.IdTurno && X.Habilitado == true);
            }
            else
            {
                registroTurnoAnterior = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == registroTurno.FechaHoraIns.Date && X.IdTurno == turnoAnterior.IdTurno && X.Habilitado == true);
            }

            return registroTurnoAnterior;
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
