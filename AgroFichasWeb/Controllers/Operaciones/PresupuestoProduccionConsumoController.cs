using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    [WebsiteAuthorize]
    public class PresupuestoProduccionConsumoController : BaseApplicationController
    {
        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        //
        // GET: /PresupuestoProduccionConsumo/

        public PresupuestoProduccionConsumoController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(237);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OPR_PresupuestoProduccionConsumo> items = dc.OPR_PresupuestoProduccionConsumo.OrderBy(X => X.Año).OrderBy(X => X.Mes);
            var pagina = new PaginatedList<OPR_PresupuestoProduccionConsumo>(items, pageIndex, pageSize);

            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(237);
            OPR_PresupuestoProduccionConsumo presupuestoProduccionConsumo = new OPR_PresupuestoProduccionConsumo();
            return View(presupuestoProduccionConsumo);
        }

        [HttpPost]
        public ActionResult Crear(OPR_PresupuestoProduccionConsumo presupuestoProduccionConsumo)
        {
            CheckPermisoAndRedirect(237);
            if (ModelState.IsValid)
            {
                try
                {
                    presupuestoProduccionConsumo.FechaHoraIns = DateTime.Now;
                    presupuestoProduccionConsumo.IpIns = RemoteAddr();
                    presupuestoProduccionConsumo.UserIns = User.Identity.Name;
                    dc.OPR_PresupuestoProduccionConsumo.InsertOnSubmit(presupuestoProduccionConsumo);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = presupuestoProduccionConsumo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", presupuestoProduccionConsumo);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(237);
            var presupuestoProduccionConsumo = dc.OPR_PresupuestoProduccionConsumo.SingleOrDefault(X => X.IdPresupuestoProduccionConsumo == id);
            if (presupuestoProduccionConsumo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el presupuesto de producción y consumo"); }
            return View("crear", presupuestoProduccionConsumo);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(237);
            var presupuestoProduccionConsumo = dc.OPR_PresupuestoProduccionConsumo.SingleOrDefault(X => X.IdPresupuestoProduccionConsumo == id);
            if (presupuestoProduccionConsumo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el presupuesto de producción y consumo"); }

            try
            {
                UpdateModel(presupuestoProduccionConsumo, new string[] { "Mes", "Año", "ProduccionHojuela", "ProduccionHarina", "ConsumoAvena", "RendimientoTeorico", "ProduccionTonH" });

                presupuestoProduccionConsumo.UserUpd = User.Identity.Name;
                presupuestoProduccionConsumo.FechaHoraUpd = DateTime.Now;
                presupuestoProduccionConsumo.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = presupuestoProduccionConsumo.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", presupuestoProduccionConsumo);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(237);
            var presupuestoProduccionConsumo = dc.OPR_PresupuestoProduccionConsumo.SingleOrDefault(X => X.IdPresupuestoProduccionConsumo == id);
            if (presupuestoProduccionConsumo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el presupuesto de producción y consumo"); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dc.OPR_PresupuestoProduccionConsumo.DeleteOnSubmit(presupuestoProduccionConsumo);
                dc.SubmitChanges();
                okMsg = "El presupuesto ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}
