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
    public class CALFTParametroNutricionalesController : BaseApplicationController
    {
        // GET: CALFTParametroNutricionales

        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTParametroNutricionalesController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(323);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            List<CAL_FTParametroNutricional> list = (from X in dcSoftwareCalidad.CAL_FTParametroNutricional
                                                     join Y in dcSoftwareCalidad.CAL_ParametroNutricional on X.IdParametroNutricional equals Y.IdParametroNutricional
                                                     where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                     && Y.Habilitado == true
                                                     orderby Y.Nombre
                                                     select X).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["cAL_FT"] = cAL_FT;
            ViewData["permisosUsuario"] = new Permiso()
            {
                Crear = CheckPermiso(324),
                Leer = CheckPermiso(323),
                Actualizar = CheckPermiso(325),
                Borrar = CheckPermiso(326)
            };
            return View(list);
        }

        public ActionResult Crear(int id)
        {
            CheckPermisoAndRedirect(324);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_ParametroNutricional> parametroNutricionales = GetParametroNutricionales(cAL_FT);
            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroNutricionales"] = parametroNutricionales;
            return View("Crear");
        }

        [HttpPost]
        public ActionResult Crear(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(324);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_ParametroNutricional> parametroNutricionales = GetParametroNutricionales(cAL_FT);

            if (ModelState.IsValid)
            {
                foreach (CAL_ParametroNutricional parametroNutricional in parametroNutricionales)
                {
                    if (!string.IsNullOrEmpty(formCollection[string.Format("PARAM_min_{0}", parametroNutricional.IdParametroNutricional)]) &&
                        !string.IsNullOrEmpty(formCollection[string.Format("PARAM_max_{0}", parametroNutricional.IdParametroNutricional)]))
                    {
                        if ((decimal.TryParse(formCollection[string.Format("PARAM_min_{0}", parametroNutricional.IdParametroNutricional)], out decimal PARAM_min)) &&
                            (decimal.TryParse(formCollection[string.Format("PARAM_max_{0}", parametroNutricional.IdParametroNutricional)], out decimal PARAM_max)))
                        {
                            CAL_FTParametroNutricional cAL_FTParametroNutricional = new CAL_FTParametroNutricional()
                            {
                                IdFichaTecnica = cAL_FT.IdFichaTecnica,
                                IdParametroNutricional = parametroNutricional.IdParametroNutricional,
                                MinValidValue = PARAM_min,
                                MaxValidValue = PARAM_max,
                                MinAutValue = 0,
                                MaxAutValue = 0,
                                AccionAutValue = "",
                                NoAplica = bool.Parse(formCollection[string.Format("HID_NA_{0}", parametroNutricional.IdParametroNutricional)]),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = CurrentUser.UserName
                            };
                            dcSoftwareCalidad.CAL_FTParametroNutricional.InsertOnSubmit(cAL_FTParametroNutricional);
                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }

                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }

            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroNutricionales"] = parametroNutricionales;
            return View("Crear");
        }

        public ActionResult Editar(int id, int IdParametroNutricional)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroNutricional cAL_FTParametroNutricional = dcSoftwareCalidad.CAL_FTParametroNutricional.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroNutricional == IdParametroNutricional);
            if (cAL_FTParametroNutricional == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro nutricional", okMsg = "" }); }

            return View("Editar", cAL_FTParametroNutricional);
        }

        [HttpPost]
        public ActionResult Editar(int id, int IdParametroNutricional, FormCollection formValues)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroNutricional cAL_FTParametroNutricional = dcSoftwareCalidad.CAL_FTParametroNutricional.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroNutricional == IdParametroNutricional);
            if (cAL_FTParametroNutricional == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro nutricional", okMsg = "" }); }

            try
            {
                UpdateModel(cAL_FTParametroNutricional, new string[] { "MinValidValue", "MaxValidValue", "MinAutValue", "MaxAutValue", "AccionAutValue", "NoAplica" });

                cAL_FTParametroNutricional.UserUpd = User.Identity.Name;
                cAL_FTParametroNutricional.FechaHoraUpd = DateTime.Now;
                cAL_FTParametroNutricional.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }
            catch
            {
                var rv = cAL_FTParametroNutricional.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Editar", cAL_FTParametroNutricional);
        }

        #region --- Funciones PRIVADAS ---

        private List<CAL_ParametroNutricional> GetParametroNutricionales(CAL_FT cAL_FT)
        {
            List<CAL_ParametroNutricional> parametroNutricionalesList = new List<CAL_ParametroNutricional>();
            List<CAL_ParametroNutricional> parametroNutricionales = dcSoftwareCalidad.CAL_ParametroNutricional.Where(X => X.Habilitado == true).ToList();
            foreach (CAL_ParametroNutricional parametroNutricional in parametroNutricionales)
            {
                if (dcSoftwareCalidad.CAL_FTParametroNutricional.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).SingleOrDefault(X => X.IdParametroNutricional == parametroNutricional.IdParametroNutricional) == null)
                {
                    parametroNutricionalesList.Add(parametroNutricional);
                }
            }

            return parametroNutricionalesList.OrderBy(X => X.Nombre).ToList();
        }

        #endregion
    }
}