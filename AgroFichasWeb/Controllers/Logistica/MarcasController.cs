using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class MarcasController : BaseApplicationController
    {
        //
        // GET: /Marcas/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public MarcasController()
        {
            SetCurrentModulo(6);//Logística y Corretaje
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(101);

            var marca = new LOG_Marca();

            return View("crear", marca);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(LOG_Marca marca)
        {
            CheckPermisoAndRedirect(101);
            if (ModelState.IsValid)
            {
                try
                {
                    marca.Habilitado = true;
                    marca.FechaHoraIns = DateTime.Now;
                    marca.IpIns = RemoteAddr();
                    marca.UserIns = User.Identity.Name;
                    dc.LOG_Marca.InsertOnSubmit(marca);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = marca.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", marca);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(102);

            var marca = dc.LOG_Marca.SingleOrDefault(x => x.IdMarca == id && x.Habilitado == true);
            if (marca == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la marca"); }

            return View("crear", marca);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(102);

            var marca = dc.LOG_Marca.SingleOrDefault(x => x.IdMarca == id && x.Habilitado == true);
            if (marca == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la marca"); }

            try
            {
                UpdateModel(marca, new string[] { "Nombre" });

                marca.UserUpd = User.Identity.Name;
                marca.FechaHoraUpd = DateTime.Now;
                marca.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = marca.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", marca);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(103);

            var marca = dc.LOG_Marca.SingleOrDefault(x => x.IdMarca == id && x.Habilitado == true);
            if (marca == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la marca"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                marca.Habilitado = false;
                marca.UserUpd = User.Identity.Name;
                marca.FechaHoraUpd = DateTime.Now;
                marca.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("La marca {0} ha sido eliminada", marca.Nombre);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(100);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<LOG_Marca> items = dc.LOG_Marca.Where(x => x.Habilitado == true).OrderBy(a => a.Nombre);

            var pagina = new PaginatedList<LOG_Marca>(items, pageIndex, pageSize);

            return View(pagina);
        }
    }
}
