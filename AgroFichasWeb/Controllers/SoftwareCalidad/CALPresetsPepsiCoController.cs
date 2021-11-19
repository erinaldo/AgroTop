using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    public class CALPresetsPepsiCoController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dataContext = new SoftwareCalidadDBDataContext();

        public CALPresetsPepsiCoController()
        {
            SetCurrentModulo(10);
        }

        #region Index

        // GET: CALPresetsPepsiCo
        public ActionResult Index()
        {
            return View();
        }

        #endregion

        #region Parámetros de Análisis

        public ActionResult IndexParametrosAnalisis()
        {
            CheckPermisoAndRedirect(410);
            List<CAL_PresetPepsiCoIncChemicalPhysicalParameter> list = dataContext.CAL_PresetPepsiCoIncChemicalPhysicalParameter.Where(X => X.CAL_FT.Habilitado).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410));
            return View(list);
        }

        public ActionResult CrearParametroAnalisis()
        {
            CheckPermisoAndRedirect(410);
            CAL_PresetPepsiCoIncChemicalPhysicalParameter preset = new CAL_PresetPepsiCoIncChemicalPhysicalParameter();
            return View("CrearParametroAnalisis", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroAnalisis(CAL_PresetPepsiCoIncChemicalPhysicalParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetPepsiCoIncChemicalPhysicalParameter = dataContext.CAL_GetPepsiCoIncSiguienteId(1).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetPepsiCoIncChemicalPhysicalParameter.InsertOnSubmit(preset);
                    dataContext.SubmitChanges();
                    return RedirectToAction("IndexParametrosAnalisis");
                }
                catch
                {
                    var rv = preset.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearParametroAnalisis", preset);
        }

        public ActionResult EditarParametroAnalisis(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncChemicalPhysicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncChemicalPhysicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosAnalisis", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroAnalisis", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroAnalisis(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncChemicalPhysicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncChemicalPhysicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosAnalisis", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroAnalisis", "Specification", "IndiaMethod", "IndiaFreq", "AvenatopMethod", "AvenatopFreq", "SortOrder" });

                dataContext.SubmitChanges();
                return RedirectToAction("IndexParametrosAnalisis");
            }
            catch
            {
                var rv = preset.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("CrearParametroAnalisis", preset);
        }

        public ActionResult EliminarParametroAnalisis(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncChemicalPhysicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncChemicalPhysicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosAnalisis", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetPepsiCoIncChemicalPhysicalParameter.DeleteOnSubmit(preset);
                dataContext.SubmitChanges();
                okMsg = "El preset ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("IndexParametrosAnalisis", new { errMsg, okMsg });
        }

        #endregion

        #region Parámetros de Pesticidas

        public ActionResult IndexParametrosPesticidas()
        {
            CheckPermisoAndRedirect(410);
            List<CAL_PresetPepsiCoIncPesticideParameter> list = dataContext.CAL_PresetPepsiCoIncPesticideParameter.Where(X => X.CAL_FT.Habilitado).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410));
            return View(list);
        }

        public ActionResult CrearParametroPesticidas()
        {
            CheckPermisoAndRedirect(410);
            CAL_PresetPepsiCoIncPesticideParameter preset = new CAL_PresetPepsiCoIncPesticideParameter();
            return View("CrearParametroPesticidas", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroPesticidas(CAL_PresetPepsiCoIncPesticideParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetPepsiCoIncPesticideParameter = dataContext.CAL_GetPepsiCoIncSiguienteId(2).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetPepsiCoIncPesticideParameter.InsertOnSubmit(preset);
                    dataContext.SubmitChanges();
                    return RedirectToAction("IndexParametrosPesticidas");
                }
                catch (Exception ex)
                {
                    var rv = preset.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearParametroPesticidas", preset);
        }

        public ActionResult EditarParametroPesticidas(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncPesticideParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncPesticideParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosPesticidas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroPesticidas", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroPesticidas(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncPesticideParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncPesticideParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosPesticidas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroPesticida", "Specification", "IndiaMethod", "IndiaFreq", "AvenatopMethod", "AvenatopFreq", "SortOrder" });

                dataContext.SubmitChanges();
                return RedirectToAction("IndexParametrosPesticidas");
            }
            catch
            {
                var rv = preset.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("CrearParametroPesticidas", preset);
        }

        public ActionResult EliminarParametroPesticidas(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncPesticideParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncPesticideParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosPesticidas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetPepsiCoIncPesticideParameter.DeleteOnSubmit(preset);
                dataContext.SubmitChanges();
                okMsg = "El preset ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("IndexParametrosPesticidas", new { errMsg, okMsg });
        }

        #endregion

        #region Parámetros de Metales Pesados

        public ActionResult IndexParametrosMetalesPesados()
        {
            CheckPermisoAndRedirect(410);
            List<CAL_PresetPepsiCoIncHeavyMetalParameter> list = dataContext.CAL_PresetPepsiCoIncHeavyMetalParameter.Where(X => X.CAL_FT.Habilitado).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410));
            return View(list);
        }

        public ActionResult CrearParametroMetalesPesados()
        {
            CheckPermisoAndRedirect(410);
            CAL_PresetPepsiCoIncHeavyMetalParameter preset = new CAL_PresetPepsiCoIncHeavyMetalParameter();
            return View("CrearParametroMetalesPesados", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroMetalesPesados(CAL_PresetPepsiCoIncHeavyMetalParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetPepsiCoIncHeavyMetalParameter = dataContext.CAL_GetPepsiCoIncSiguienteId(3).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetPepsiCoIncHeavyMetalParameter.InsertOnSubmit(preset);
                    dataContext.SubmitChanges();
                    return RedirectToAction("IndexParametrosMetalesPesados");
                }
                catch (Exception ex)
                {
                    var rv = preset.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearParametroMetalesPesados", preset);
        }

        public ActionResult EditarParametroMetalesPesados(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncHeavyMetalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncHeavyMetalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMetalesPesados", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroMetalesPesados", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroMetalesPesados(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncHeavyMetalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncHeavyMetalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMetalesPesados", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroMetalPesado", "Specification", "IndiaMethod", "IndiaFreq", "AvenatopMethod", "AvenatopFreq", "SortOrder" });

                dataContext.SubmitChanges();
                return RedirectToAction("IndexParametrosMetalesPesados");
            }
            catch
            {
                var rv = preset.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("CrearParametroMetalesPesados", preset);
        }

        public ActionResult EliminarParametroMetalesPesados(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncHeavyMetalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncHeavyMetalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMetalesPesados", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetPepsiCoIncHeavyMetalParameter.DeleteOnSubmit(preset);
                dataContext.SubmitChanges();
                okMsg = "El preset ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("IndexParametrosMetalesPesados", new { errMsg, okMsg });
        }

        #endregion

        #region Parámetros de Micotoxinas

        public ActionResult IndexParametrosMicotoxinas()
        {
            CheckPermisoAndRedirect(410);
            List<CAL_PresetPepsiCoIncMycotoxinParameter> list = dataContext.CAL_PresetPepsiCoIncMycotoxinParameter.Where(X => X.CAL_FT.Habilitado).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410));
            return View(list);
        }

        public ActionResult CrearParametroMicotoxinas()
        {
            CheckPermisoAndRedirect(410);
            CAL_PresetPepsiCoIncMycotoxinParameter preset = new CAL_PresetPepsiCoIncMycotoxinParameter();
            return View("CrearParametroMicotoxinas", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroMicotoxinas(CAL_PresetPepsiCoIncMycotoxinParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetPepsiCoIncMycotoxinParameter = dataContext.CAL_GetPepsiCoIncSiguienteId(4).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetPepsiCoIncMycotoxinParameter.InsertOnSubmit(preset);
                    dataContext.SubmitChanges();
                    return RedirectToAction("IndexParametrosMicotoxinas");
                }
                catch (Exception ex)
                {
                    var rv = preset.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearParametroMicotoxinas", preset);
        }

        public ActionResult EditarParametroMicotoxinas(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncMycotoxinParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncMycotoxinParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicotoxinas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroMicotoxinas", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroMicotoxinas(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncMycotoxinParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncMycotoxinParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicotoxinas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroMicotoxina", "Specification", "IndiaMethod", "IndiaFreq", "AvenatopMethod", "AvenatopFreq", "SortOrder" });

                dataContext.SubmitChanges();
                return RedirectToAction("IndexParametrosMicotoxinas");
            }
            catch
            {
                var rv = preset.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("CrearParametroMicotoxinas", preset);
        }

        public ActionResult EliminarParametroMicotoxinas(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncMycotoxinParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncMycotoxinParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicotoxinas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetPepsiCoIncMycotoxinParameter.DeleteOnSubmit(preset);
                dataContext.SubmitChanges();
                okMsg = "El preset ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("IndexParametrosMicotoxinas", new { errMsg, okMsg });
        }

        #endregion

        #region Parámetros de Microbiología

        public ActionResult IndexParametrosMicrobiologia()
        {
            CheckPermisoAndRedirect(410);
            List<CAL_PresetPepsiCoIncMicrobiologicalParameter> list = dataContext.CAL_PresetPepsiCoIncMicrobiologicalParameter.Where(X => X.CAL_FT.Habilitado).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410));
            return View(list);
        }

        public ActionResult CrearParametroMicrobiologia()
        {
            CheckPermisoAndRedirect(410);
            CAL_PresetPepsiCoIncMicrobiologicalParameter preset = new CAL_PresetPepsiCoIncMicrobiologicalParameter();
            return View("CrearParametroMicrobiologia", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroMicrobiologia(CAL_PresetPepsiCoIncMicrobiologicalParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetPepsiCoIncMicrobiologicalParameter = dataContext.CAL_GetPepsiCoIncSiguienteId(5).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetPepsiCoIncMicrobiologicalParameter.InsertOnSubmit(preset);
                    dataContext.SubmitChanges();
                    return RedirectToAction("IndexParametrosMicrobiologia");
                }
                catch
                {
                    var rv = preset.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearParametroMicrobiologia", preset);
        }

        public ActionResult EditarParametroMicrobiologia(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncMicrobiologicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncMicrobiologicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicrobiologia", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroMicrobiologia", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroMicrobiologia(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncMicrobiologicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncMicrobiologicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicrobiologia", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroMicrobiologia", "Specification", "IndiaMethod", "IndiaFreq", "AvenatopMethod", "AvenatopFreq", "SortOrder" });

                dataContext.SubmitChanges();
                return RedirectToAction("IndexParametrosMicrobiologia");
            }
            catch
            {
                var rv = preset.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("CrearParametroMicrobiologia", preset);
        }

        public ActionResult EliminarParametroMicrobiologia(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncMicrobiologicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncMicrobiologicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicrobiologia", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetPepsiCoIncMicrobiologicalParameter.DeleteOnSubmit(preset);
                dataContext.SubmitChanges();
                okMsg = "El preset ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("IndexParametrosMicrobiologia", new { errMsg, okMsg });
        }

        #endregion

        #region Parámetros Nutricionales

        public ActionResult IndexParametrosNutricionales()
        {
            CheckPermisoAndRedirect(410);
            List<CAL_PresetPepsiCoIncChemicalParameter> list = dataContext.CAL_PresetPepsiCoIncChemicalParameter.Where(X => X.CAL_FT.Habilitado).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410),
                                                      CheckPermiso(410));
            return View(list);
        }

        public ActionResult CrearParametroNutricionales()
        {
            CheckPermisoAndRedirect(410);
            CAL_PresetPepsiCoIncChemicalParameter preset = new CAL_PresetPepsiCoIncChemicalParameter();
            return View("CrearParametroNutricionales", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroNutricionales(CAL_PresetPepsiCoIncChemicalParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetPepsiCoIncChemicalParameter = dataContext.CAL_GetPepsiCoIncSiguienteId(6).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetPepsiCoIncChemicalParameter.InsertOnSubmit(preset);
                    dataContext.SubmitChanges();
                    return RedirectToAction("IndexParametrosNutricionales");
                }
                catch
                {
                    var rv = preset.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearParametroNutricionales", preset);
        }

        public ActionResult EditarParametroNutricionales(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncChemicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncChemicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosNutricionales", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroNutricionales", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroNutricionales(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncChemicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncChemicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosNutricionales", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroNutricional", "Specification", "IndiaMethod", "IndiaFreq", "AvenatopMethod", "AvenatopFreq", "SortOrder" });

                dataContext.SubmitChanges();
                return RedirectToAction("IndexParametrosNutricionales");
            }
            catch
            {
                var rv = preset.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("CrearParametroNutricionales", preset);
        }

        public ActionResult EliminarParametroNutricionales(int id)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetPepsiCoIncChemicalParameter.SingleOrDefault(X => X.IdPresetPepsiCoIncChemicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosNutricionales", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetPepsiCoIncChemicalParameter.DeleteOnSubmit(preset);
                dataContext.SubmitChanges();
                okMsg = "El preset ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("IndexParametrosNutricionales", new { errMsg, okMsg });
        }

        #endregion
    }
}