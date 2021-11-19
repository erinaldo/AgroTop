using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFTControlVersionController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTControlVersionController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFTControlVersion
        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(331);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_FTControlVersion> list = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == id).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["cAL_FT"] = cAL_FT;
            return View(list);
        }

        public ActionResult Crear(int id)
        {
            CheckPermisoAndRedirect(331);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTControlVersion cAL_FTControlVersion = new CAL_FTControlVersion();
            cAL_FTControlVersion.IdFichaTecnica = cAL_FT.IdFichaTecnica;
            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", cAL_FTControlVersion);
        }

        [HttpPost]
        public ActionResult Crear(int id, CAL_FTControlVersion cAL_FTControlVersion, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(331);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            if (ModelState.IsValid)
            {
                try
                {
                    cAL_FTControlVersion.IdFichaTecnica = cAL_FT.IdFichaTecnica;
                    cAL_FTControlVersion.Subversion = 0;
                    cAL_FTControlVersion.FechaHoraIns = DateTime.Now;
                    cAL_FTControlVersion.IpIns = RemoteAddr();
                    cAL_FTControlVersion.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_FTControlVersion.InsertOnSubmit(cAL_FTControlVersion);
                    dcSoftwareCalidad.SubmitChanges();
                    return RedirectToAction("Index", new { id = id });
                }
                catch
                {
                    var rv = cAL_FTControlVersion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", cAL_FTControlVersion);
        }

        public ActionResult Eliminar(int id, int IdControlVersion)
        {
            CheckPermisoAndRedirect(331);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.SingleOrDefault(X => X.IdControlVersion == IdControlVersion);
            if (cAL_FTControlVersion == null) return RedirectToAction("Index", "CALFTControlVersion", new { errMsg = "No se ha encontrado el control de versión", okMsg = "" });

            string errMsg = "";
            string okMsg = "";

            try
            {
                dcSoftwareCalidad.CAL_FTControlVersion.DeleteOnSubmit(cAL_FTControlVersion);
                dcSoftwareCalidad.SubmitChanges();
                okMsg = "El control de versión ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Editar(int id, int IdControlVersion)
        {
            CheckPermisoAndRedirect(331);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.SingleOrDefault(X => X.IdFichaTecnica == id && X.IdControlVersion == IdControlVersion);
            cAL_FTControlVersion.IdFichaTecnica = cAL_FT.IdFichaTecnica;
            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", cAL_FTControlVersion);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formCollection, int IdControlVersion)
        {
            CheckPermisoAndRedirect(331);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.SingleOrDefault(X => X.IdFichaTecnica == id && X.IdControlVersion == IdControlVersion);
            if (cAL_FTControlVersion == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado el Control de versiones", okMsg = "" });

            if (TryUpdateModel(cAL_FTControlVersion, new string[] { "Fecha", "Version", "IdItem", "Cambios", "IdMotivo", "IdSolicitante" }))
            {
                try
                {
                    dcSoftwareCalidad.SubmitChanges();
                    return RedirectToAction("index", new { id = id});
                }
                catch
                {
                    var rv = cAL_FTControlVersion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", cAL_FTControlVersion);
        }
    }
}