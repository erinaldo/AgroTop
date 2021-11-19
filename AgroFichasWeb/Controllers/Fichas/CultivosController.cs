using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class CultivosController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public CultivosController()
        {
            SetCurrentModulo(1); //Fichas;

            ViewData["tipos"] = dc.Cultivo.OrderBy(cu => cu.IdCultivo);
        }

        public ActionResult Index(int? id, int tipo, string msg)
        {
            CheckPermisoAndRedirect(7);
            int pageSize = 1000; // this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;


            var items = dc.Variedad.Where(q => q.IdCultivo == tipo).OrderBy(a => a.Nombre);
            var pagina = new PaginatedList<Variedad>(items, pageIndex, pageSize);

            ViewData["tipo"] = dc.Cultivo.Single(cu => cu.IdCultivo == tipo);

            if (msg != null)
                ModelState.AddRuleViolations(new List<RuleViolation>() { new RuleViolation(msg, "Eliminar") });

            return View(pagina);
        }

        public ActionResult Crear(int tipo)
        {
            CheckPermisoAndRedirect(7);
            var variedad = new Variedad
            {
                IdCultivo = tipo,
                Habilitado = true
            };
            ViewData["tipo"] = dc.Cultivo.Single(c => c.IdCultivo == tipo).Nombre;
            return View("Variedad", variedad);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(Variedad variedad)
        {
            CheckPermisoAndRedirect(7);
            if (ModelState.IsValid)
            {
                try
                {
                    variedad.IdVariedad = dc.Variedad.Max(q => q.IdVariedad) + 1;
                    variedad.UserIns = User.Identity.Name;
                    variedad.FechaHoraIns = DateTime.Now;
                    variedad.IpIns = RemoteAddr();
                    dc.Variedad.InsertOnSubmit(variedad);
                    dc.SubmitChanges();
                    return RedirectToAction("Index", new { tipo = variedad.IdCultivo });
                }
                catch
                {
                    var rv = variedad.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            return View("Variedad", variedad);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(7);
            var variedad = dc.Variedad.SingleOrDefault(p => p.IdVariedad == id);
            if (variedad == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Variedad Not Found");

            return View("Variedad", variedad);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(7);
            var variedad = dc.Variedad.SingleOrDefault(p => p.IdVariedad == id);

            try
            {
                UpdateModel(variedad, new string[] { "Nombre", "Habilitado" });

                variedad.UserUpd = User.Identity.Name;
                variedad.FechaHoraUpd = DateTime.Now;
                variedad.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("Index", new { tipo = variedad.IdCultivo });
            }
            catch
            {
                var rv = variedad.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Varidad", variedad);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(7);
            var variedad = dc.Variedad.SingleOrDefault(p => p.IdVariedad == id);
            int tipo = variedad.IdCultivo;

            string msg = "";
            try
            {
                if (variedad != null)
                {
                    dc.Variedad.DeleteOnSubmit(variedad);
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK_Siembra_Variedad"))
                    msg = "No es posible elimnar la variedad porque tiene al menos un registro de siembra asociado";
                else
                    msg = ex.Message;
            }

            return RedirectToAction("Index", new { tipo = tipo, msg = msg });
        }

    }
}
