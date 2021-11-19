using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFTParametroPesticidaController : BaseApplicationController
    {
        // GET: CALFTParametroPesticida

        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTParametroPesticidaController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(323);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            List<CAL_FTParametroPesticida> list = (from X in dcSoftwareCalidad.CAL_FTParametroPesticida
                                                   join Y in dcSoftwareCalidad.CAL_ParametroPesticida on X.IdParametroPesticida equals Y.IdParametroPesticida
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
            List<CAL_ParametroPesticida> parametroPesticidas = GetParametroPesticidas(cAL_FT);
            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroPesticidas"] = parametroPesticidas;
            return View("Crear");
        }

        [HttpPost]
        public ActionResult Crear(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(324);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_ParametroPesticida> parametroPesticidas = GetParametroPesticidas(cAL_FT);

            if (ModelState.IsValid)
            {
                foreach (CAL_ParametroPesticida parametroPesticida in parametroPesticidas)
                {
                    if (!string.IsNullOrEmpty(formCollection[string.Format("PARAM_min_{0}", parametroPesticida.IdParametroPesticida)]) &&
                        !string.IsNullOrEmpty(formCollection[string.Format("PARAM_max_{0}", parametroPesticida.IdParametroPesticida)]))
                    {
                        if ((decimal.TryParse(formCollection[string.Format("PARAM_min_{0}", parametroPesticida.IdParametroPesticida)], out decimal PARAM_min)) &&
                            (decimal.TryParse(formCollection[string.Format("PARAM_max_{0}", parametroPesticida.IdParametroPesticida)], out decimal PARAM_max)))
                        {
                            CAL_FTParametroPesticida cAL_FTParametroPesticida = new CAL_FTParametroPesticida()
                            {
                                IdFichaTecnica = cAL_FT.IdFichaTecnica,
                                IdParametroPesticida = parametroPesticida.IdParametroPesticida,
                                MinValidValue = PARAM_min,
                                MaxValidValue = PARAM_max,
                                MinAutValue = 0,
                                MaxAutValue = 0,
                                AccionAutValue = "",
                                NoAplica = bool.Parse(formCollection[string.Format("HID_NA_{0}", parametroPesticida.IdParametroPesticida)]),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = CurrentUser.UserName
                            };
                            dcSoftwareCalidad.CAL_FTParametroPesticida.InsertOnSubmit(cAL_FTParametroPesticida);
                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }

                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }

            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroPesticidas"] = parametroPesticidas;
            return View("Crear");
        }

        public ActionResult Editar(int id, int IdParametroPesticida)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroPesticida cAL_FTParametroPesticida = dcSoftwareCalidad.CAL_FTParametroPesticida.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroPesticida == IdParametroPesticida);
            if (cAL_FTParametroPesticida == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de pesticida", okMsg = "" }); }

            return View("Editar", cAL_FTParametroPesticida);
        }

        [HttpPost]
        public ActionResult Editar(int id, int IdParametroPesticida, FormCollection formValues)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroPesticida cAL_FTParametroPesticida = dcSoftwareCalidad.CAL_FTParametroPesticida.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroPesticida == IdParametroPesticida);
            if (cAL_FTParametroPesticida == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de pesticida", okMsg = "" }); }

            try
            {
                UpdateModel(cAL_FTParametroPesticida, new string[] { "MinValidValue", "MaxValidValue", "MinAutValue", "MaxAutValue", "AccionAutValue", "NoAplica" });

                cAL_FTParametroPesticida.UserUpd = User.Identity.Name;
                cAL_FTParametroPesticida.FechaHoraUpd = DateTime.Now;
                cAL_FTParametroPesticida.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }
            catch
            {
                var rv = cAL_FTParametroPesticida.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Editar", cAL_FTParametroPesticida);
        }

        #region --- Funciones PRIVADAS ---

        private List<CAL_ParametroPesticida> GetParametroPesticidas(CAL_FT cAL_FT)
        {
            List<CAL_ParametroPesticida> parametroPesticidasList = new List<CAL_ParametroPesticida>();
            List<CAL_ParametroPesticida> parametroPesticidas = dcSoftwareCalidad.CAL_ParametroPesticida.Where(X => X.Habilitado == true).ToList();
            foreach (CAL_ParametroPesticida parametroPesticida in parametroPesticidas)
            {
                if (dcSoftwareCalidad.CAL_FTParametroPesticida.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).SingleOrDefault(X => X.IdParametroPesticida == parametroPesticida.IdParametroPesticida) == null)
                {
                    parametroPesticidasList.Add(parametroPesticida);
                }
            }

            return parametroPesticidasList;
        }

        #endregion
    }
}