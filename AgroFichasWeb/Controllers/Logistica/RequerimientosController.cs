using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using AgroFichasWeb.ViewModels.Logistica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class RequerimientosController : BaseApplicationController
    {
        //
        // GET: /Requerimientos/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public RequerimientosController()
        {
            SetCurrentModulo(6);//Logística y Corretaje
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Anular(int id)
        {
            CheckPermisoAndRedirect(115);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == id && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                requerimiento.IdEstado = 99;
                requerimiento.UserUpd = User.Identity.Name;
                requerimiento.FechaHoraUpd = DateTime.Now;
                requerimiento.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("El requerimiento N° {0} ha sido anulado", requerimiento.IdRequerimiento);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Buscar(int id, string keyword)
        {
            CheckPermisoAndRedirect(112);

            var requerimiento = LogisticaHelper.GetRequerimiento(id); // Cualquiera
            if (requerimiento == null) { throw new HttpException(404, "No se ha encontrado el requerimiento"); }

            DetalleRequerimientoViewModel model = new DetalleRequerimientoViewModel(requerimiento.IdRequerimiento, keyword);

            model.Requerimiento = requerimiento;
            model.Columnas = 7;
            model.MostrarCrear = CheckPermiso(113);
            model.ErrorMessage = Request["msgerr"];
            model.OKMessage = Request["msgok"];

            if (CheckPermiso(114))
            {
                model.MostrarEditar = true;
                model.Columnas++;
            }

            if (CheckPermiso(115))
            {
                model.MostrarEliminar = true;
                model.Columnas++;
            }

            ViewData["keyword"] = keyword;
            return View("detalle", model);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(113);

            var requerimiento = new LOG_Requerimiento();

            SetEmpresas(null);
            SetTipoMovimientos(null);
            return View("crear", requerimiento);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(LOG_Requerimiento requerimiento)
        {
            CheckPermisoAndRedirect(113);
            if (ModelState.IsValid)
            {
                try
                {
                    requerimiento.IdEstado = 1;
                    requerimiento.CantidadTotalKg = 0;
                    requerimiento.FechaHoraIns = DateTime.Now;
                    requerimiento.IpIns = RemoteAddr();
                    requerimiento.UserIns = User.Identity.Name;
                    dc.LOG_Requerimiento.InsertOnSubmit(requerimiento);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = requerimiento.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            SetEmpresas(requerimiento.IdEmpresa);
            SetTipoMovimientos(requerimiento.IdTipoMovimiento);
            return View("crear", requerimiento);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Detalle(int id)
        {
            CheckPermisoAndRedirect(112);

            var requerimiento = LogisticaHelper.GetRequerimiento(id);
            if (requerimiento == null) { throw new HttpException(404, "No se ha encontrado el requerimiento"); }

            DetalleRequerimientoViewModel model = new DetalleRequerimientoViewModel(requerimiento.IdRequerimiento);

            model.Requerimiento = requerimiento;
            model.Columnas = 7;
            model.MostrarCrear = CheckPermiso(113);
            model.ErrorMessage = Request["msgerr"];
            model.OKMessage = Request["msgok"];

            if (CheckPermiso(114))
            {
                model.MostrarEditar = true;
                model.Columnas++;
            }

            if (CheckPermiso(115))
            {
                model.MostrarEliminar = true;
                model.Columnas++;
            }

            return View("detalle", model);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(114);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == id && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            SetEmpresas(requerimiento.IdEmpresa);
            SetTipoMovimientos(requerimiento.IdTipoMovimiento);
            return View("crear", requerimiento);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(114);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == id && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            try
            {
                UpdateModel(requerimiento, new string[] { "IdEmpresa", "IdTipoMovimiento", "IdTipoRequerimiento", "Glosa", "FechaInicio", "FechaVencimiento", "UserUpd", "FechaHoraUpd", "IpUpd" });

                requerimiento.UserUpd = User.Identity.Name;
                requerimiento.FechaHoraUpd = DateTime.Now;
                requerimiento.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = requerimiento.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            SetEmpresas(requerimiento.IdEmpresa);
            SetTipoMovimientos(requerimiento.IdTipoMovimiento);
            return View("crear", requerimiento);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(112);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            bool MostrarAnular = false;
            bool MostrarEditar = false;
            bool MostrarLiquidar = false;
            int Columnas = 7;

            if (CheckPermiso(114))
            {
                MostrarEditar = true;
                Columnas++;
            }

            if (CheckPermiso(115))
            {
                MostrarAnular = true;
                Columnas++;
            }

            if (CheckPermiso(123))
            {
                MostrarLiquidar = true;
                Columnas++;
            }

            string key = Request.QueryString["key"] ?? "";

            IQueryable<LOG_Requerimiento> items = dc.LOG_Requerimiento.Where(x => (key == "" || x.IdRequerimiento.ToString().Contains(key) || x.Glosa.ToString().Contains(key))).OrderByDescending(a => a.IdRequerimiento);

            var pagina = new PaginatedList<LOG_Requerimiento>(items, pageIndex, pageSize);

            ViewData["Columnas"] = Columnas;
            ViewData["MostrarAnular"] = MostrarAnular;
            ViewData["MostrarEditar"] = MostrarEditar;
            ViewData["MostrarLiquidar"] = MostrarLiquidar;
            ViewData["key"] = key;
            return View(pagina);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult MarcarLiquidado(int id)
        {
            CheckPermisoAndRedirect(113);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == id);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                requerimiento.IdEstado = 3;
                requerimiento.UserUpd = User.Identity.Name;
                requerimiento.FechaHoraUpd = DateTime.Now;
                requerimiento.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                msgok = String.Format("El requerimiento N° {0} ha sido marcado como liquidado", id);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult MarcarListoParaLiquidar(int id)
        {
            CheckPermisoAndRedirect(113);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == id);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                requerimiento.IdEstado = 2;
                requerimiento.UserUpd = User.Identity.Name;
                requerimiento.FechaHoraUpd = DateTime.Now;
                requerimiento.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                msgok = String.Format("El requerimiento N° {0} ha sido marcado como listo para liquidar", id);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        private void SetEmpresas(int? IdEmpresa)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.Empresa
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdEmpresa == IdEmpresa && IdEmpresa != null),
                    Text = s.Nombre,
                    Value = s.IdEmpresa.ToString()
                };
            ViewData["empresasList"] = selectList;
        }

        private void SetTipoMovimientos(int? IdTipoMovimiento)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_TipoMovimiento
                orderby s.Descripcion
                select new SelectListItem
                {
                    Selected = (s.IdTipoMovimiento == IdTipoMovimiento && IdTipoMovimiento != null),
                    Text = s.Descripcion,
                    Value = s.IdTipoMovimiento.ToString()
                };
            ViewData["tipoMovimientosList"] = selectList;
        }
    }
}
