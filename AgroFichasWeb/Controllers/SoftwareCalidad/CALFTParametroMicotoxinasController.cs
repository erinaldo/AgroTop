using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFTParametroMicotoxinasController : BaseApplicationController
    {
        // GET: CALFTParametroMicotoxinas

        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTParametroMicotoxinasController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(323);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            List<CAL_FTParametroMicotoxina> list = (from X in dcSoftwareCalidad.CAL_FTParametroMicotoxina
                                                    join Y in dcSoftwareCalidad.CAL_ParametroMicotoxina on X.IdParametroMicotoxina equals Y.IdParametroMicotoxina
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
            List<CAL_ParametroMicotoxina> parametroMicotoxinas = GetParametroMicotoxinas(cAL_FT);
            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroMicotoxinas"] = parametroMicotoxinas;
            return View("Crear");
        }

        [HttpPost]
        public ActionResult Crear(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(324);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_ParametroMicotoxina> parametroMicotoxinas = GetParametroMicotoxinas(cAL_FT);

            if (ModelState.IsValid)
            {
                foreach (CAL_ParametroMicotoxina parametroMicotoxina in parametroMicotoxinas)
                {
                    if (!string.IsNullOrEmpty(formCollection[string.Format("PARAM_min_{0}", parametroMicotoxina.IdParametroMicotoxina)]) &&
                        !string.IsNullOrEmpty(formCollection[string.Format("PARAM_max_{0}", parametroMicotoxina.IdParametroMicotoxina)]))
                    {
                        if ((decimal.TryParse(formCollection[string.Format("PARAM_min_{0}", parametroMicotoxina.IdParametroMicotoxina)], out decimal PARAM_min)) &&
                            (decimal.TryParse(formCollection[string.Format("PARAM_max_{0}", parametroMicotoxina.IdParametroMicotoxina)], out decimal PARAM_max)))
                        {
                            CAL_FTParametroMicotoxina cAL_FTParametroMicotoxina = new CAL_FTParametroMicotoxina()
                            {
                                IdFichaTecnica = cAL_FT.IdFichaTecnica,
                                IdParametroMicotoxina = parametroMicotoxina.IdParametroMicotoxina,
                                MinValidValue = PARAM_min,
                                MaxValidValue = PARAM_max,
                                MinAutValue = 0,
                                MaxAutValue = 0,
                                AccionAutValue = "",
                                NoAplica = bool.Parse(formCollection[string.Format("HID_NA_{0}", parametroMicotoxina.IdParametroMicotoxina)]),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = CurrentUser.UserName
                            };
                            dcSoftwareCalidad.CAL_FTParametroMicotoxina.InsertOnSubmit(cAL_FTParametroMicotoxina);
                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }

                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }

            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroMicotoxinas"] = parametroMicotoxinas;
            return View("Crear");
        }

        public ActionResult Editar(int id, int IdParametroMicotoxina)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroMicotoxina cAL_FTParametroMicotoxina = dcSoftwareCalidad.CAL_FTParametroMicotoxina.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroMicotoxina == IdParametroMicotoxina);
            if (cAL_FTParametroMicotoxina == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de micotoxina", okMsg = "" }); }

            return View("Editar", cAL_FTParametroMicotoxina);
        }

        [HttpPost]
        public ActionResult Editar(int id, int IdParametroMicotoxina, FormCollection formValues)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroMicotoxina cAL_FTParametroMicotoxina = dcSoftwareCalidad.CAL_FTParametroMicotoxina.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroMicotoxina == IdParametroMicotoxina);
            if (cAL_FTParametroMicotoxina == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de micotoxina", okMsg = "" }); }

            try
            {
                UpdateModel(cAL_FTParametroMicotoxina, new string[] { "MinValidValue", "MaxValidValue", "MinAutValue", "MaxAutValue", "AccionAutValue", "NoAplica" });

                cAL_FTParametroMicotoxina.UserUpd = User.Identity.Name;
                cAL_FTParametroMicotoxina.FechaHoraUpd = DateTime.Now;
                cAL_FTParametroMicotoxina.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }
            catch
            {
                var rv = cAL_FTParametroMicotoxina.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Editar", cAL_FTParametroMicotoxina);
        }

        #region --- Funciones PRIVADAS ---

        private List<CAL_ParametroMicotoxina> GetParametroMicotoxinas(CAL_FT cAL_FT)
        {
            List<CAL_ParametroMicotoxina> parametroMicotoxinasList = new List<CAL_ParametroMicotoxina>();
            List<CAL_ParametroMicotoxina> parametroMicotoxinas = dcSoftwareCalidad.CAL_ParametroMicotoxina.Where(X => X.Habilitado == true).ToList();
            foreach (CAL_ParametroMicotoxina parametroMicotoxina in parametroMicotoxinas)
            {
                if (dcSoftwareCalidad.CAL_FTParametroMicotoxina.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).SingleOrDefault(X => X.IdParametroMicotoxina == parametroMicotoxina.IdParametroMicotoxina) == null)
                {
                    parametroMicotoxinasList.Add(parametroMicotoxina);
                }
            }

            return parametroMicotoxinasList;
        }

        #endregion
    }
}