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
    public class QuimicosController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public QuimicosController()
        {
            SetCurrentModulo(1); //Fichas;

            ViewData["tipos"] = dc.TipoRecomendacion.Where(tr => tr.IdTipoRecomendacion != 3).OrderBy(tr => tr.IdTipoRecomendacion);

            ViewData["ums"] = (from um in dc.UM
                               orderby um.IdUM
                               select new SelectListItem()
                               {
                                   Text = um.Nombre,
                                   Value = um.IdUM.ToString(),
                                   Selected = false
                               }).ToList();

        }

        public ActionResult Index(int? id, int tipo, string msg)
        {
            CheckPermisoAndRedirect(4);
            int pageSize = 1000; // this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var items = dc.Quimico.Where(q => q.IdTipoRecomendacion == tipo).OrderBy(a => a.Nombre);
            var pagina = new PaginatedList<Quimico>(items, pageIndex, pageSize);

            ViewData["tipo"] = dc.TipoRecomendacion.Single(tr => tr.IdTipoRecomendacion == tipo);

            if (msg != null)
                ModelState.AddRuleViolations(new List<RuleViolation> () { new RuleViolation(msg, "Eliminar") });

            return View(pagina);
        }

        public ActionResult Crear(int tipo)
        {
            CheckPermisoAndRedirect(4);
            var quimico = new Quimico
            {
                IdTipoRecomendacion = tipo,
                Habilitado = true
            };
            return View("Quimico", quimico);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(Quimico quimico)
        {
            CheckPermisoAndRedirect(4);
            if (ModelState.IsValid)
            {
                try
                {
                    quimico.UserIns = User.Identity.Name;
                    quimico.FechaHoraIns = DateTime.Now;
                    quimico.IpIns = RemoteAddr();
                    dc.Quimico.InsertOnSubmit(quimico);
                    dc.SubmitChanges();
                    return RedirectToAction("Index", new { tipo = quimico.IdTipoRecomendacion });
                }
                catch
                {
                    var rv = quimico.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            return View("Quimico", quimico);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(4);
            var quimico = dc.Quimico.SingleOrDefault(p => p.IdQuimico == id);
            if (quimico == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Químico Not Found");
         
            return View("Quimico", quimico);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(4);
            var quimico = dc.Quimico.SingleOrDefault(p => p.IdQuimico == id);

            try
            {
                UpdateModel(quimico, new string [] { "Nombre", "Dosis", "IdUM", "Habilitado" });

                quimico.UserUpd = User.Identity.Name;
                quimico.FechaHoraUpd = DateTime.Now;
                quimico.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("Index", new { tipo = quimico.IdTipoRecomendacion });
            }
            catch
            {
                var rv = quimico.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Quimico", quimico);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(4);
            var quimico = dc.Quimico.SingleOrDefault(p => p.IdQuimico == id);
            int tipo = quimico.IdTipoRecomendacion;
            
            string msg = "";
            try
            {
                if (quimico != null)
                {
                    dc.Quimico.DeleteOnSubmit(quimico);
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK_Recomendacion_Quimico"))
                    msg = "No es posible elimnar el químico porque tiene al menos una recomendación asociada";
                else
                    msg = ex.Message;
            }

            return RedirectToAction("Index", new { tipo = tipo, msg = msg });
        }

    }
}
