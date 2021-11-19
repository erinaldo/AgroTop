using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.OrdenesCompra
{
    public class MaterialesController : BaseApplicationController
    {
        //
        // GET: /Materiales/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public MaterialesController()
        {
            SetCurrentModulo(8);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(218);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<OC_Material> items = dc.OC_Material.Where(X => X.Habilitado == true);

            var pagina = new PaginatedList<OC_Material>(items, pageIndex, pageSize);

            var puedeEditar = CheckPermiso(220);
            var puedeEliminar = CheckPermiso(221);
            var columnas = 5 + (puedeEditar ? 1 : 0) + (puedeEliminar ? 1 : 0);
            ViewData["puedeEditar"] = puedeEditar;
            ViewData["puedeEliminar"] = puedeEliminar;
            ViewData["columnas"] = columnas;
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(219);
            var material = new OC_Material();
            SetProyectos(null);
            SetEmpresas(null);
            return View("crear", material);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(OC_Material material)
        {
            CheckPermisoAndRedirect(219);
            if (ModelState.IsValid)
            {
                try
                {
                    material.FechaHoraIns = DateTime.Now;
                    material.IpIns = RemoteAddr();
                    material.UserIns = User.Identity.Name;
                    dc.OC_Material.InsertOnSubmit(material);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = material.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            SetProyectos(material.IdProyecto);
            SetEmpresas(material.IdEmpresa);
            return View("crear", material);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(220);

            var material = dc.OC_Material.SingleOrDefault(X => X.IdMaterial == id);
            if (material == null)
            {
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el material");
            }

            SetProyectos(material.IdProyecto);
            SetEmpresas(material.IdEmpresa);
            return View("crear", material);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(220);

            var material = dc.OC_Material.SingleOrDefault(X => X.IdMaterial == id);
            if (material == null)
            {
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el material");
            }

            try
            {
                UpdateModel(material, new string[] { "IdProyecto", "IdEmpresa", "CodigoMaterial", "Descripcion" });

                material.UserUpd = User.Identity.Name;
                material.FechaHoraUpd = DateTime.Now;
                material.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                return RedirectToAction("index");
            }
            catch
            {
                var rv = material.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            SetProyectos(material.IdProyecto);
            SetEmpresas(material.IdEmpresa);
            return View("crear", material);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(221);

            var material = dc.OC_Material.SingleOrDefault(X => X.IdMaterial == id);
            if (material == null)
            {
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el material");
            }

            string msgErr = "";
            string msgOk = "";

            try
            {
                material.Habilitado = false;
                material.UserUpd = User.Identity.Name;
                material.FechaHoraUpd = DateTime.Now;
                material.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgOk = String.Format("El material {0} ha sido eliminado", material.Descripcion);
            }
            catch (Exception ex)
            {
                msgErr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgErr, msgok = msgOk });
        }

        private void SetProyectos(int? IdProyecto)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.OC_Proyecto
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdProyecto == IdProyecto && IdProyecto != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdProyecto.ToString()
                                                     };
            ViewData["proyectosList"] = selectList;
        }

        private void SetEmpresas(int? IdEmpresa)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.Empresa
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdEmpresa == IdEmpresa && IdEmpresa != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdEmpresa.ToString()
                                                     };
            ViewData["empresasList"] = selectList;
        }
    }
}
