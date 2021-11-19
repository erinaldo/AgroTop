using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class DensidadesController : BaseApplicationController
    {
        //
        // GET: /Densidades/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public DensidadesController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(156);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OPR_Densidad> items = dc.OPR_Densidad.Where(x => x.Habilitado == true).OrderBy(a => a.IdDensidad);
            var pagina = new PaginatedList<OPR_Densidad>(items, pageIndex, pageSize);

            ViewData["MensajeError"]           = Request["MensajeError"];
            ViewData["MensajeExito"]           = Request["MensajeExito"];
            ViewData["PuedeCrear"]             = CheckPermiso(157);
            ViewData["PuedeEditar"]            = CheckPermiso(158);
            ViewData["PuedeEliminar"]          = CheckPermiso(159);
            ViewData["PuedeVerTipoDensidades"] = CheckPermiso(160);
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(157);
            var densidad = new OPR_Densidad();
            ViewData["tipoDensidadList"] = SetTipoDensidad(null);
            return View("crear", densidad);
        }

        [HttpPost]
        public ActionResult Crear(OPR_Densidad densidad)
        {
            CheckPermisoAndRedirect(157);
            if (ModelState.IsValid)
            {
                try
                {
                    densidad.Habilitado = true;
                    densidad.FechaHoraIns = DateTime.Now;
                    densidad.IpIns = RemoteAddr();
                    densidad.UserIns = User.Identity.Name;
                    dc.OPR_Densidad.InsertOnSubmit(densidad);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = densidad.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["tipoDensidadList"] = SetTipoDensidad(densidad.IdTipoDensidad);
            return View("crear", densidad);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(158);
            var densidad = dc.OPR_Densidad.SingleOrDefault(x => x.IdDensidad == id && x.Habilitado == true);
            if (densidad == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la densidad"); }
            ViewData["tipoDensidadList"] = SetTipoDensidad(densidad.IdTipoDensidad);
            return View("crear", densidad);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(158);

            var densidad = dc.OPR_Densidad.SingleOrDefault(x => x.IdDensidad == id && x.Habilitado == true);
            if (densidad == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la densidad"); }

            try
            {
                UpdateModel(densidad, new string[] { "IdTipoDensidad", "Valor" });

                densidad.UserUpd = User.Identity.Name;
                densidad.FechaHoraUpd = DateTime.Now;
                densidad.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = densidad.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["tipoDensidadList"] = SetTipoDensidad(densidad.IdTipoDensidad);
            return View("crear", densidad);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(159);

            var densidad = dc.OPR_Densidad.SingleOrDefault(x => x.IdDensidad == id && x.Habilitado == true);
            if (densidad == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la densidad"); }

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                densidad.Habilitado = false;
                densidad.UserUpd = User.Identity.Name;
                densidad.FechaHoraUpd = DateTime.Now;
                densidad.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                MensajeExito = String.Format("La densidad {0} ha sido eliminada", densidad.OPR_TipoDensidad.Descripcion);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }

        private IEnumerable<SelectListItem> SetTipoDensidad(int? IdTipoDensidad)
        {
            IEnumerable<SelectListItem> selectList =
                from X in dc.OPR_TipoDensidad
                where X.Habilitado == true
                orderby X.Descripcion
                select new SelectListItem
                {
                    Selected = (X.IdTipoDensidad == IdTipoDensidad && IdTipoDensidad != null),
                    Text = string.Format("{0} ({1})", X.Descripcion, X.Sigla),
                    Value = X.IdTipoDensidad.ToString()
                };
            return selectList;
        }
    }
}