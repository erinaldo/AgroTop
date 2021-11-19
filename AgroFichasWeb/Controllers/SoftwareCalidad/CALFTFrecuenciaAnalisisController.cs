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
    public class CALFTFrecuenciaAnalisisController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTFrecuenciaAnalisisController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFTFrecuenciaAnalisis
        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(385);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_FTFrecuenciaAnalisis> list = dcSoftwareCalidad.CAL_FTFrecuenciaAnalisis.Where(X => X.IdFichaTecnica == id).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["cAL_FT"] = cAL_FT;
            return View(list);
        }

        public ActionResult Crear(int id)
        {
            CheckPermisoAndRedirect(385);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTFrecuenciaAnalisis frecuenciaAnalisis = new CAL_FTFrecuenciaAnalisis();
            frecuenciaAnalisis.IdFichaTecnica = cAL_FT.IdFichaTecnica;
            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", frecuenciaAnalisis);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Crear(int id, CAL_FTFrecuenciaAnalisis frecuenciaAnalisis, FormCollection formCollection, HttpPostedFileBase postedFile)
        {
            CheckPermisoAndRedirect(385);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            if (ModelState.IsValid)
            {
                try
                {
                    frecuenciaAnalisis.IdFichaTecnica = cAL_FT.IdFichaTecnica;
                    frecuenciaAnalisis.FechaHoraIns = DateTime.Now;
                    frecuenciaAnalisis.IpIns = RemoteAddr();
                    frecuenciaAnalisis.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_FTFrecuenciaAnalisis.InsertOnSubmit(frecuenciaAnalisis);
                    dcSoftwareCalidad.SubmitChanges();
                    return RedirectToAction("Index", new { id = id });
                }
                catch
                {
                    var rv = frecuenciaAnalisis.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", frecuenciaAnalisis);
        }

        public ActionResult Eliminar(int id, int IdTipoAnalisis)
        {
            CheckPermisoAndRedirect(385);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            CAL_FTFrecuenciaAnalisis frecuenciaAnalisis = dcSoftwareCalidad.CAL_FTFrecuenciaAnalisis.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdTipoAnalisis == IdTipoAnalisis);
            if (frecuenciaAnalisis == null) return RedirectToAction("Index", "CALFTFrecuenciaAnalisis", new { errMsg = "No se ha encontrado la frecuencia de análisis de la ficha técnica", okMsg = "" });

            string errMsg = "";
            string okMsg = "";

            try
            {
                dcSoftwareCalidad.CAL_FTFrecuenciaAnalisis.DeleteOnSubmit(frecuenciaAnalisis);
                dcSoftwareCalidad.SubmitChanges();
                okMsg = "La frecuencia de análisis de la ficha técnica ha sido eliminada";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }
    }
}