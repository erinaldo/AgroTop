using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Fichas
{
    public class ValoresMermaController : BaseApplicationController
    {
        //
        // GET: /ValoresMerma/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ValoresMermaController()
        {
            SetCurrentModulo(1);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(125);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            string key = Request.QueryString["key"] ?? "";

            IQueryable<LOG_Merma> items = dc.LOG_Merma.Where(x => key == "" || x.Cultivo.Nombre.Contains(key)).OrderBy(a => a.Cultivo.Nombre);

            var pagina = new PaginatedList<LOG_Merma>(items, pageIndex, pageSize);

            ViewData["key"] = key;
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(126);

            var merma = new LOG_Merma();

            SetCultivo(null);
            return View("crear", merma);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(LOG_Merma merma)
        {
            CheckPermisoAndRedirect(126);
            if (ModelState.IsValid)
            {
                try
                {
                    merma.FechaHoraIns = DateTime.Now;
                    merma.IpIns = RemoteAddr();
                    merma.UserIns = User.Identity.Name;
                    dc.LOG_Merma.InsertOnSubmit(merma);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = merma.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            SetCultivo(merma.IdCultivo);
            return View("crear", merma);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(127);

            var merma = dc.LOG_Merma.SingleOrDefault(x => x.IdCultivo == id);
            if (merma == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el cultivo");

            SetCultivo(merma.IdCultivo);
            return View("crear", merma);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(127);

            var merma = dc.LOG_Merma.SingleOrDefault(x => x.IdCultivo == id);
            if (merma == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el cultivo");

            try
            {
                UpdateModel(merma, new string[] { "IdCultivo", "Precio" });

                merma.UserUpd = User.Identity.Name;
                merma.FechaHoraUpd = DateTime.Now;
                merma.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = merma.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            SetCultivo(merma.IdCultivo);
            return View("crear", merma);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(128);

            var merma = dc.LOG_Merma.SingleOrDefault(x => x.IdCultivo == id);
            if (merma == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el cultivo");

            string msgerr = "";
            string msgok = "";
            try
            {
                dc.LOG_Merma.DeleteOnSubmit(merma);
                dc.SubmitChanges();
                msgok = String.Format("La valor de la merma del / de la {0} ha sido eliminado", merma.Cultivo.Nombre);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        private void SetCultivo(int? IdCultivo)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.Cultivo
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdCultivo == IdCultivo && IdCultivo != null),
                    Text = s.Nombre,
                    Value = s.IdCultivo.ToString()
                };
            ViewData["cultivosList"] = selectList;
        }
    }
}
