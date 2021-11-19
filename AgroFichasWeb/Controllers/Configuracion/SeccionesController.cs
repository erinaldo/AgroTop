using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Configuracion
{
    public class SeccionesController : BaseApplicationController
    {
        //
        // GET: /Secciones/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public SeccionesController()
        {
            SetCurrentModulo(2);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(1008);

            var seccion = new Seccion();

            return View("crear", seccion);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(Seccion seccion)
        {
            CheckPermisoAndRedirect(1008);
            if (ModelState.IsValid)
            {
                try
                {
                    seccion.Habilitado = true;
                    seccion.FechaHoraIns = DateTime.Now;
                    seccion.IpIns = RemoteAddr();
                    seccion.UserIns = User.Identity.Name;
                    dc.Seccion.InsertOnSubmit(seccion);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = seccion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", seccion);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(1009);

            var seccion = dc.Seccion.SingleOrDefault(x => x.IdSeccion == id && x.Habilitado == true);
            if (seccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la sección"); }

            return View("crear", seccion);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(1009);

            var seccion = dc.Seccion.SingleOrDefault(x => x.IdSeccion == id && x.Habilitado == true);
            if (seccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la sección"); }

            try
            {
                UpdateModel(seccion, new string[] { "Descripcion" });

                seccion.UserUpd = User.Identity.Name;
                seccion.FechaHoraUpd = DateTime.Now;
                seccion.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = seccion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", seccion);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(1010);

            var seccion = dc.Seccion.SingleOrDefault(x => x.IdSeccion == id && x.Habilitado == true);
            if (seccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la sección"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                seccion.Habilitado = false;
                seccion.UserUpd = User.Identity.Name;
                seccion.FechaHoraUpd = DateTime.Now;
                seccion.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("La sección {0} ha sido eliminada", seccion.Descripcion);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(1007);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<Seccion> items = dc.Seccion.Where(x => x.Habilitado == true).OrderBy(a => a.Descripcion);

            var pagina = new PaginatedList<Seccion>(items, pageIndex, pageSize);

            return View(pagina);
        }

    }
}
