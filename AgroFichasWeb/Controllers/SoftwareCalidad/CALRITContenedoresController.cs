using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALRITContenedoresController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALRITContenedoresController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALRITContenedores
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(360);

            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = from us in dcAgroFichas.PlantaUsuario
                                                  join pl in dcAgroFichas.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
                                                  where us.UserID == CurrentUser.UserID
                                                  && pl.Habilitado
                                                  select new SelectListItem
                                                  {
                                                      Value = pl.IdPlantaProduccion.ToString(),
                                                      Text = pl.Nombre,
                                                      Selected = IdPlantaProduccionSelect == pl.IdPlantaProduccion
                                                  };
            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }
            List<CAL_RITContenedor> list = new List<CAL_RITContenedor>();
            if (IdPlantaProduccionSelect == 0)
            {
                list = dc.CAL_RITContenedor.Where(X => X.Habilitado == true).ToList();
            }
            else
            {
                list = dc.CAL_RITContenedor.Where(X => X.Habilitado == true && X.IdPlanta == IdPlantaProduccionSelect).ToList();
            }
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(361),
                                                      CheckPermiso(360),
                                                      CheckPermiso(362),
                                                      CheckPermiso(363));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(361);
            CAL_RITContenedor ritContenedor = new CAL_RITContenedor();
            return View("Crear", ritContenedor);
        }

        [HttpPost]
        public ActionResult Crear(CAL_RITContenedor contenedor)
        {
            CheckPermisoAndRedirect(361);
            if (ModelState.IsValid)
            {
                try
                {
                    contenedor.NContenedor = contenedor.NContenedor.ToUpperInvariant();

                    contenedor.Habilitado = true;
                    contenedor.FechaHoraIns = DateTime.Now;
                    contenedor.IpIns = RemoteAddr();
                    contenedor.UserIns = User.Identity.Name;
                    dc.CAL_RITContenedor.InsertOnSubmit(contenedor);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = contenedor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("Crear", contenedor);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(362);

            CAL_RITContenedor contenedor = dc.CAL_RITContenedor.SingleOrDefault(X => X.IdContenedor == id && X.Habilitado == true);
            if (contenedor == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
            }

            return View("Crear", contenedor);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(362);
            var contenedor = dc.CAL_RITContenedor.SingleOrDefault(X => X.IdContenedor == id && X.Habilitado == true);
            if (contenedor == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" }); }

            try
            {
                UpdateModel(contenedor, new string[] { "NContenedor", "IdTransportista","IdPlanta" });

                contenedor.NContenedor = contenedor.NContenedor.ToUpperInvariant();

                contenedor.UserUpd = User.Identity.Name;
                contenedor.FechaHoraUpd = DateTime.Now;
                contenedor.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = contenedor.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Crear", contenedor);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(363);
            var contenedor = dc.CAL_RITContenedor.SingleOrDefault(X => X.IdContenedor == id && X.Habilitado == true);
            if (contenedor == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                contenedor.Habilitado = false;
                contenedor.UserUpd = User.Identity.Name;
                contenedor.FechaHoraUpd = DateTime.Now;
                contenedor.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El contenedor {0} ha sido eliminado", contenedor.NContenedor);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}