using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Configuracion

{
    public class PlantasProductivasController : BaseApplicationController
    {
        //
        // GET: /Plantas Productivas/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public PlantasProductivasController()
        {
            SetCurrentModulo(2);//Fichas
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(2011);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            string key = Request.QueryString["key"] ?? "";

            IQueryable<PlantaProduccion> items = dc.PlantaProduccion.Where(x => x.Habilitado == true && (key == "" || x.Nombre.Contains(key) || x.Comuna.Nombre.Contains(key))).OrderBy(a => a.Nombre);

            var pagina = new PaginatedList<PlantaProduccion>(items, pageIndex, pageSize);

            ViewData["key"] = key;

            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(2012),
                                                      CheckPermiso(2011),
                                                      CheckPermiso(2013),
                                                      CheckPermiso(2014));
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(2012);

            var plantaProduccion = new PlantaProduccion();

            SetComunas(null);
            return View("crear", plantaProduccion);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(PlantaProduccion plantaProduccion)
        {
            CheckPermisoAndRedirect(2012);
            if (ModelState.IsValid)
            {
                try
                {

                    plantaProduccion.IdPlantaProduccion = (dc.PlantaProduccion.Max(x => x.IdPlantaProduccion) + 1);
                    plantaProduccion.Habilitado = true;
                    plantaProduccion.FechaHoraIns = DateTime.Now;
                    plantaProduccion.IpIns = RemoteAddr();
                    plantaProduccion.UserIns = User.Identity.Name;
                    dc.PlantaProduccion.InsertOnSubmit(plantaProduccion);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = plantaProduccion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            SetComunas(plantaProduccion.IdComuna);
            return View("crear", plantaProduccion);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(2013);

            var plantaProduccion = dc.PlantaProduccion.SingleOrDefault(x => x.IdPlantaProduccion == id && x.Habilitado == true);
            if (plantaProduccion == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la Planta de Producción");

            SetComunas(plantaProduccion.IdComuna);
            return View("crear", plantaProduccion);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(2012);

            var plantaProduccion = dc.PlantaProduccion.SingleOrDefault(x => x.IdPlantaProduccion == id && x.Habilitado == true);
            if (plantaProduccion == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la Planta de Producción");

            try
            {
                UpdateModel(plantaProduccion, new string[] { "Nombre", "IdComuna"});


                plantaProduccion.UserUpd = User.Identity.Name;
                plantaProduccion.FechaHoraUpd = DateTime.Now;
                plantaProduccion.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = plantaProduccion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            SetComunas(plantaProduccion.IdComuna);
            return View("crear", plantaProduccion);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(2014);

            var plantaProduccion = dc.PlantaProduccion.SingleOrDefault(x => x.IdPlantaProduccion == id && x.Habilitado == true);
            if (plantaProduccion == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la sucursal");

            string msgerr = "";
            string msgok = "";
            try
            {
                plantaProduccion.Habilitado = false;
                plantaProduccion.UserUpd = User.Identity.Name;
                plantaProduccion.FechaHoraUpd = DateTime.Now;
                plantaProduccion.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("La Planta Productiva '{0}' ha sido eliminada", plantaProduccion.Nombre);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        private void SetComunas(int? IdComuna)
        {
            var selectList = Comuna.SelectList(dc, IdComuna);
            ViewData["comunasList"] = selectList;
        }
    }
}
