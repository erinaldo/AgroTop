using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALResponsablesAreasController : BaseApplicationController
    { 
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALResponsablesAreasController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(278);

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
            List<vw_CAL_ResponsableArea> list = new List<vw_CAL_ResponsableArea>();
            if (IdPlantaProduccionSelect == 0)
            {
                list = dc.vw_CAL_ResponsableArea.ToList();
            }
            else
            {
                list = dc.vw_CAL_ResponsableArea.Where(X => X.IdPlanta == IdPlantaProduccionSelect).ToList();
            }
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(278),
                                                      CheckPermiso(279),
                                                      CheckPermiso(280),
                                                      CheckPermiso(281));
            return View(list);
        }

        public ActionResult Editar(int id, int? userID)
        {
            CheckPermisoAndRedirect(279);

            CAL_ResponsableArea responsableArea = null;

            if (userID.HasValue)
            {
                responsableArea = dc.CAL_ResponsableArea.SingleOrDefault(X => X.IdResponsableArea == id && X.UserID == userID);
                if (responsableArea == null)
                {
                    return RedirectToAction("Index", new { errMsg = "No se ha encontrado el responsable del área", okMsg = "" });
                }
            }
            else
            {
                responsableArea = dc.CAL_ResponsableArea.SingleOrDefault(X => X.IdResponsableArea == id);
            }

            return View("Editar", responsableArea);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(279);
            CAL_ResponsableArea responsableArea = dc.CAL_ResponsableArea.SingleOrDefault(X => X.IdResponsableArea == id );
            if (responsableArea == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el responsable del área", okMsg = "" });
            }

            try
            {
                UpdateModel(responsableArea, new string[] {  "IdArea","UserID" });

                //responsableArea.UserID = responsableArea.UserID;
                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = responsableArea.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Editar", responsableArea);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(280);
            var responsableArea = dc.CAL_ResponsableArea.SingleOrDefault(X => X.IdResponsableArea == id);
            if (responsableArea == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el responsable del área", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                responsableArea.UserID = null;
                responsableArea.UserUpd = User.Identity.Name;
                responsableArea.FechaHoraUpd = DateTime.Now;
                responsableArea.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El responsable {0} ha sido eliminado", responsableArea.UserID);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}
