using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFTParametroMetalesPesadosController : BaseApplicationController
    {
        // GET: CALFTParametroMetalesPesados

        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTParametroMetalesPesadosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(323);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            List<CAL_FTParametroMetalPesado> list = (from X in dcSoftwareCalidad.CAL_FTParametroMetalPesado
                                                     join Y in dcSoftwareCalidad.CAL_ParametroMetalPesado on X.IdParametroMetalPesado equals Y.IdParametroMetalPesado
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
            List<CAL_ParametroMetalPesado> parametroMetalesPesados = GetParametroMetalesPesados(cAL_FT);
            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroMetalesPesados"] = parametroMetalesPesados;
            return View("Crear");
        }

        [HttpPost]
        public ActionResult Crear(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(324);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_ParametroMetalPesado> parametroMetalesPesados = GetParametroMetalesPesados(cAL_FT);

            if (ModelState.IsValid)
            {
                foreach (CAL_ParametroMetalPesado parametroMetalPesado in parametroMetalesPesados)
                {
                    if (!string.IsNullOrEmpty(formCollection[string.Format("PARAM_min_{0}", parametroMetalPesado.IdParametroMetalPesado)]) &&
                        !string.IsNullOrEmpty(formCollection[string.Format("PARAM_max_{0}", parametroMetalPesado.IdParametroMetalPesado)]))
                    {
                        if ((decimal.TryParse(formCollection[string.Format("PARAM_min_{0}", parametroMetalPesado.IdParametroMetalPesado)], out decimal PARAM_min)) &&
                            (decimal.TryParse(formCollection[string.Format("PARAM_max_{0}", parametroMetalPesado.IdParametroMetalPesado)], out decimal PARAM_max)))
                        {
                            CAL_FTParametroMetalPesado cAL_FTParametroMetalPesado = new CAL_FTParametroMetalPesado()
                            {
                                IdFichaTecnica = cAL_FT.IdFichaTecnica,
                                IdParametroMetalPesado = parametroMetalPesado.IdParametroMetalPesado,
                                MinValidValue = PARAM_min,
                                MaxValidValue = PARAM_max,
                                MinAutValue = 0,
                                MaxAutValue = 0,
                                AccionAutValue = "",
                                NoAplica = bool.Parse(formCollection[string.Format("HID_NA_{0}", parametroMetalPesado.IdParametroMetalPesado)]),
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = CurrentUser.UserName
                            };
                            dcSoftwareCalidad.CAL_FTParametroMetalPesado.InsertOnSubmit(cAL_FTParametroMetalPesado);
                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }

                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }

            ViewData["cAL_FT"] = cAL_FT;
            ViewData["parametroMetalesPesados"] = parametroMetalesPesados;
            return View("Crear");
        }

        public ActionResult Editar(int id, int IdParametroMetalPesado)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroMetalPesado cAL_FTParametroMetalPesado = dcSoftwareCalidad.CAL_FTParametroMetalPesado.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroMetalPesado == IdParametroMetalPesado);
            if (cAL_FTParametroMetalPesado == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de metal pesado", okMsg = "" }); }

            return View("Editar", cAL_FTParametroMetalPesado);
        }

        [HttpPost]
        public ActionResult Editar(int id, int IdParametroMetalPesado, FormCollection formValues)
        {
            CheckPermisoAndRedirect(325);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTParametroMetalPesado cAL_FTParametroMetalPesado = dcSoftwareCalidad.CAL_FTParametroMetalPesado.SingleOrDefault(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica && X.IdParametroMetalPesado == IdParametroMetalPesado);
            if (cAL_FTParametroMetalPesado == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el parámetro de metal pesado", okMsg = "" }); }

            try
            {
                UpdateModel(cAL_FTParametroMetalPesado, new string[] { "MinValidValue", "MaxValidValue", "MinAutValue", "MaxAutValue", "AccionAutValue", "NoAplica" });

                cAL_FTParametroMetalPesado.UserUpd = User.Identity.Name;
                cAL_FTParametroMetalPesado.FechaHoraUpd = DateTime.Now;
                cAL_FTParametroMetalPesado.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Index", new { id = cAL_FT.IdFichaTecnica });
            }
            catch
            {
                var rv = cAL_FTParametroMetalPesado.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Editar", cAL_FTParametroMetalPesado);
        }

        #region --- Funciones PRIVADAS ---

        private List<CAL_ParametroMetalPesado> GetParametroMetalesPesados(CAL_FT cAL_FT)
        {
            List<CAL_ParametroMetalPesado> parametroMetalesPesadosList = new List<CAL_ParametroMetalPesado>();
            List<CAL_ParametroMetalPesado> parametroMetalesPesados = dcSoftwareCalidad.CAL_ParametroMetalPesado.Where(X => X.Habilitado == true).ToList();
            foreach (CAL_ParametroMetalPesado parametroMetalPesado in parametroMetalesPesados)
            {
                if (dcSoftwareCalidad.CAL_FTParametroMetalPesado.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).SingleOrDefault(X => X.IdParametroMetalPesado == parametroMetalPesado.IdParametroMetalPesado) == null)
                {
                    parametroMetalesPesadosList.Add(parametroMetalPesado);
                }
            }

            return parametroMetalesPesadosList;
        }

        #endregion
    }
}