using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFTParametroMicrobiologiaController : BaseApplicationController
    {
        // GET: CALFTParametroMicrobiologia

        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTParametroMicrobiologiaController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(323);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            List<CAL_FTParametroMicrobiologia> list = (from X in dcSoftwareCalidad.CAL_FTParametroMicrobiologia
                                                       join Y in dcSoftwareCalidad.CAL_ParametroMicrobiologia on X.IdParametroMicrobiologia equals Y.IdParametroMicrobiologia
                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                       && Y.Habilitado == true
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
            List<CAL_ParametroMicrobiologia> parametroMicrobiologia = GetParametroMicrobiologia(cAL_FT);
            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroMicrobiologia"] = parametroMicrobiologia;
            return View("Crear");
        }

        [HttpPost]
        public ActionResult Crear(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(324);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_ParametroMicrobiologia> parametroMicrobiologia = GetParametroMicrobiologia(cAL_FT);

            if (ModelState.IsValid)
            {
                foreach (CAL_ParametroMicrobiologia cAL_ParametroMicrobiologia in parametroMicrobiologia)
                {
                    if (!string.IsNullOrEmpty(formCollection[string.Format("PARAM_min_{0}", cAL_ParametroMicrobiologia.IdParametroMicrobiologia)]) &&
                        !string.IsNullOrEmpty(formCollection[string.Format("PARAM_max_{0}", cAL_ParametroMicrobiologia.IdParametroMicrobiologia)]))
                    {
                        if ((decimal.TryParse(formCollection[string.Format("PARAM_min_{0}", cAL_ParametroMicrobiologia.IdParametroMicrobiologia)], out decimal PARAM_min)) &&
                            (decimal.TryParse(formCollection[string.Format("PARAM_max_{0}", cAL_ParametroMicrobiologia.IdParametroMicrobiologia)], out decimal PARAM_max)))
                        {
                            CAL_FTParametroMicrobiologia cAL_FTParametroMicrobiologia = new CAL_FTParametroMicrobiologia()
                            {
                                IdFichaTecnica = cAL_FT.IdFichaTecnica,
                                IdParametroMicrobiologia = cAL_ParametroMicrobiologia.IdParametroMicrobiologia,
                                MinValidValue = PARAM_min,
                                MaxValidValue = PARAM_max,
                                MinAutValue = 0,
                                MaxAutValue = 0,
                                AccionAutValue = "",
                                NoAplica = bool.Parse(formCollection[string.Format("HID_NA_{0}", cAL_ParametroMicrobiologia.IdParametroMicrobiologia)]),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = CurrentUser.UserName
                            };
                            dcSoftwareCalidad.CAL_FTParametroMicrobiologia.InsertOnSubmit(cAL_FTParametroMicrobiologia);
                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }

                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }

            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroMicrobiologia"] = parametroMicrobiologia;
            return View("Crear");
        }

        public ActionResult Editar(int id, int IdParametroMicrobiologia)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroMicrobiologia cAL_FTParametroMicrobiologia = dcSoftwareCalidad.CAL_FTParametroMicrobiologia.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroMicrobiologia == IdParametroMicrobiologia);
            if (cAL_FTParametroMicrobiologia == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de microbiología", okMsg = "" }); }

            return View("Editar", cAL_FTParametroMicrobiologia);
        }

        [HttpPost]
        public ActionResult Editar(int id, int IdParametroMicrobiologia, FormCollection formValues)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroMicrobiologia cAL_FTParametroMicrobiologia = dcSoftwareCalidad.CAL_FTParametroMicrobiologia.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroMicrobiologia == IdParametroMicrobiologia);
            if (cAL_FTParametroMicrobiologia == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de microbiología", okMsg = "" }); }

            try
            {
                UpdateModel(cAL_FTParametroMicrobiologia, new string[] { "MinValidValue", "MaxValidValue", "MinAutValue", "MaxAutValue", "AccionAutValue", "NoAplica" });

                cAL_FTParametroMicrobiologia.UserUpd = User.Identity.Name;
                cAL_FTParametroMicrobiologia.FechaHoraUpd = DateTime.Now;
                cAL_FTParametroMicrobiologia.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }
            catch
            {
                var rv = cAL_FTParametroMicrobiologia.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Editar", cAL_FTParametroMicrobiologia);
        }

        #region --- Funciones PRIVADAS ---

        private List<CAL_ParametroMicrobiologia> GetParametroMicrobiologia(CAL_FT cAL_FT)
        {
            List<CAL_ParametroMicrobiologia> parametroMicrobiologiaList = new List<CAL_ParametroMicrobiologia>();
            List<CAL_ParametroMicrobiologia> parametroMicrobiologia = dcSoftwareCalidad.CAL_ParametroMicrobiologia.Where(X => X.Habilitado == true).ToList();
            foreach (CAL_ParametroMicrobiologia cAL_ParametroMicrobiologia in parametroMicrobiologia)
            {
                if (dcSoftwareCalidad.CAL_FTParametroMicrobiologia.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).SingleOrDefault(X => X.IdParametroMicrobiologia == cAL_ParametroMicrobiologia.IdParametroMicrobiologia) == null)
                {
                    parametroMicrobiologiaList.Add(cAL_ParametroMicrobiologia);
                }
            }

            return parametroMicrobiologiaList;
        }

        #endregion
    }
}