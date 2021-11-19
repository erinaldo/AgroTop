using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class BalanzasController : BaseApplicationController
    {
        //
        // GET: /Balanzas/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public BalanzasController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(165);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OPR_Balanza> items = dc.OPR_Balanza.OrderBy(a => a.Descripcion);
            var pagina = new PaginatedList<OPR_Balanza>(items, pageIndex, pageSize);

            ViewData["MensajeError"]  = Request["MensajeError"];
            ViewData["MensajeExito"]  = Request["MensajeExito"];
            ViewData["PuedeCrear"]    = CheckPermiso(166);
            ViewData["PuedeEditar"]   = CheckPermiso(167);
            ViewData["PuedeEliminar"] = CheckPermiso(168);
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(166);
            var balanza = new OPR_Balanza();
            return View("crear", balanza);
        }

        [HttpPost]
        public ActionResult Crear(OPR_Balanza balanza)
        {
            CheckPermisoAndRedirect(166);
            if (ModelState.IsValid)
            {
                try
                {
                    balanza.Habilitado = true;
                    balanza.FechaHoraIns = DateTime.Now;
                    balanza.IpIns = RemoteAddr();
                    balanza.UserIns = User.Identity.Name;
                    dc.OPR_Balanza.InsertOnSubmit(balanza);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = balanza.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", balanza);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(167);
            var balanza = dc.OPR_Balanza.SingleOrDefault(x => x.IdBalanza == id);
            if (balanza == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la balanza"); }
            return View("crear", balanza);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(167);
            var balanza = dc.OPR_Balanza.SingleOrDefault(x => x.IdBalanza == id);
            if (balanza == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la balanza"); }

            try
            {
                UpdateModel(balanza, new string[] { "Descripcion", "Habilitado" });

                balanza.UserUpd = User.Identity.Name;
                balanza.FechaHoraUpd = DateTime.Now;
                balanza.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = balanza.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", balanza);
        }
    }
}
