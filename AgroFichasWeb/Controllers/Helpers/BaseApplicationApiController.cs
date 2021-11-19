using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers
{
    public class BaseApplicationApiController : ApiController
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

        public BaseApplicationApiController()
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
            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
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
                    HttpContext.Current.Response.Redirect(firstMenu.Url, true);
            }

            HttpContext.Current.Response.Redirect("~/Account/Logon");
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
    }
}
