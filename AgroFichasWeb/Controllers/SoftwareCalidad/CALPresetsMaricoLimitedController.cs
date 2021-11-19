using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    public class CALPresetsMaricoLimitedController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dataContext = new SoftwareCalidadDBDataContext();

        public CALPresetsMaricoLimitedController()
        {
            SetCurrentModulo(10);
        }

        #region Index

        // GET: CALPresetsMaricoLimited
        public ActionResult Index()
        {
            return View();
        }

        #endregion

        #region Parámetros de Análisis

        public ActionResult IndexParametrosAnalisis()
        {
            CheckPermisoAndRedirect(410);
            List<CAL_PresetMaricoLimitedChemicalPhysicalParameter> list = dataContext.CAL_PresetMaricoLimitedChemicalPhysicalParameter.Where(X => X.CAL_FT.Habilitado).ToList();
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
            CAL_PresetMaricoLimitedChemicalPhysicalParameter preset = new CAL_PresetMaricoLimitedChemicalPhysicalParameter();
            return View("CrearParametroAnalisis", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroAnalisis(CAL_PresetMaricoLimitedChemicalPhysicalParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetMaricoLimitedChemicalPhysicalParameter = dataContext.CAL_GetMaricoLimitedSiguienteId(1).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetMaricoLimitedChemicalPhysicalParameter.InsertOnSubmit(preset);
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
            var preset = dataContext.CAL_PresetMaricoLimitedChemicalPhysicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedChemicalPhysicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosAnalisis", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroAnalisis", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroAnalisis(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetMaricoLimitedChemicalPhysicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedChemicalPhysicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosAnalisis", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroAnalisis", "MaricoSpec" });

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
            var preset = dataContext.CAL_PresetMaricoLimitedChemicalPhysicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedChemicalPhysicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosAnalisis", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetMaricoLimitedChemicalPhysicalParameter.DeleteOnSubmit(preset);
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
            List<CAL_PresetMaricoLimitedPesticideParameter> list = dataContext.CAL_PresetMaricoLimitedPesticideParameter.Where(X => X.CAL_FT.Habilitado).ToList();
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
            CAL_PresetMaricoLimitedPesticideParameter preset = new CAL_PresetMaricoLimitedPesticideParameter();
            return View("CrearParametroPesticidas", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroPesticidas(CAL_PresetMaricoLimitedPesticideParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetMaricoLimitedPesticideParameter = dataContext.CAL_GetMaricoLimitedSiguienteId(2).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetMaricoLimitedPesticideParameter.InsertOnSubmit(preset);
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
            var preset = dataContext.CAL_PresetMaricoLimitedPesticideParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedPesticideParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosPesticidas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroPesticidas", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroPesticidas(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetMaricoLimitedPesticideParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedPesticideParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosPesticidas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroPesticida", "MaricoSpec" });

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
            var preset = dataContext.CAL_PresetMaricoLimitedPesticideParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedPesticideParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosPesticidas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetMaricoLimitedPesticideParameter.DeleteOnSubmit(preset);
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
            List<CAL_PresetMaricoLimitedHeavyMetalParameter> list = dataContext.CAL_PresetMaricoLimitedHeavyMetalParameter.Where(X => X.CAL_FT.Habilitado).ToList();
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
            CAL_PresetMaricoLimitedHeavyMetalParameter preset = new CAL_PresetMaricoLimitedHeavyMetalParameter();
            return View("CrearParametroMetalesPesados", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroMetalesPesados(CAL_PresetMaricoLimitedHeavyMetalParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetMaricoLimitedHeavyMetalParameter = dataContext.CAL_GetMaricoLimitedSiguienteId(3).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetMaricoLimitedHeavyMetalParameter.InsertOnSubmit(preset);
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
            var preset = dataContext.CAL_PresetMaricoLimitedHeavyMetalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedHeavyMetalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMetalesPesados", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroMetalesPesados", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroMetalesPesados(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetMaricoLimitedHeavyMetalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedHeavyMetalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMetalesPesados", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroMetalPesado", "MaricoSpec" });

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
            var preset = dataContext.CAL_PresetMaricoLimitedHeavyMetalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedHeavyMetalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMetalesPesados", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetMaricoLimitedHeavyMetalParameter.DeleteOnSubmit(preset);
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
            List<CAL_PresetMaricoLimitedMycotoxinParameter> list = dataContext.CAL_PresetMaricoLimitedMycotoxinParameter.Where(X => X.CAL_FT.Habilitado).ToList();
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
            CAL_PresetMaricoLimitedMycotoxinParameter preset = new CAL_PresetMaricoLimitedMycotoxinParameter();
            return View("CrearParametroMicotoxinas", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroMicotoxinas(CAL_PresetMaricoLimitedMycotoxinParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetMaricoLimitedMycotoxinParameter = dataContext.CAL_GetMaricoLimitedSiguienteId(4).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetMaricoLimitedMycotoxinParameter.InsertOnSubmit(preset);
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
            var preset = dataContext.CAL_PresetMaricoLimitedMycotoxinParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedMycotoxinParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicotoxinas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroMicotoxinas", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroMicotoxinas(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetMaricoLimitedMycotoxinParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedMycotoxinParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicotoxinas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroMicotoxina", "MaricoSpec" });

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
            var preset = dataContext.CAL_PresetMaricoLimitedMycotoxinParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedMycotoxinParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicotoxinas", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetMaricoLimitedMycotoxinParameter.DeleteOnSubmit(preset);
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
            List<CAL_PresetMaricoLimitedMicrobiologicalParameter> list = dataContext.CAL_PresetMaricoLimitedMicrobiologicalParameter.Where(X => X.CAL_FT.Habilitado).ToList();
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
            CAL_PresetMaricoLimitedMicrobiologicalParameter preset = new CAL_PresetMaricoLimitedMicrobiologicalParameter();
            return View("CrearParametroMicrobiologia", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroMicrobiologia(CAL_PresetMaricoLimitedMicrobiologicalParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetMaricoLimitedMicrobiologicalParameter = dataContext.CAL_GetMaricoLimitedSiguienteId(5).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetMaricoLimitedMicrobiologicalParameter.InsertOnSubmit(preset);
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
            var preset = dataContext.CAL_PresetMaricoLimitedMicrobiologicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedMicrobiologicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicrobiologia", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroMicrobiologia", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroMicrobiologia(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetMaricoLimitedMicrobiologicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedMicrobiologicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicrobiologia", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroMicrobiologia", "MaricoSpec" });

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
            var preset = dataContext.CAL_PresetMaricoLimitedMicrobiologicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedMicrobiologicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosMicrobiologia", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetMaricoLimitedMicrobiologicalParameter.DeleteOnSubmit(preset);
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
            List<CAL_PresetMaricoLimitedChemicalParameter> list = dataContext.CAL_PresetMaricoLimitedChemicalParameter.Where(X => X.CAL_FT.Habilitado).ToList();
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
            CAL_PresetMaricoLimitedChemicalParameter preset = new CAL_PresetMaricoLimitedChemicalParameter();
            return View("CrearParametroNutricionales", preset);
        }

        [HttpPost]
        public ActionResult CrearParametroNutricionales(CAL_PresetMaricoLimitedChemicalParameter preset)
        {
            CheckPermisoAndRedirect(410);
            if (ModelState.IsValid)
            {
                try
                {
                    preset.IdPresetMaricoLimitedChemicalParameter = dataContext.CAL_GetMaricoLimitedSiguienteId(6).Single().SiguienteId.Value;
                    preset.FechaHoraIns = DateTime.Now;
                    preset.IpIns = RemoteAddr();
                    preset.UserIns = User.Identity.Name;
                    dataContext.CAL_PresetMaricoLimitedChemicalParameter.InsertOnSubmit(preset);
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
            var preset = dataContext.CAL_PresetMaricoLimitedChemicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedChemicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosNutricionales", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }
            return View("CrearParametroNutricionales", preset);
        }

        [HttpPost]
        public ActionResult EditarParametroNutricionales(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(410);
            var preset = dataContext.CAL_PresetMaricoLimitedChemicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedChemicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosNutricionales", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            try
            {
                UpdateModel(preset, new string[] { "IdFichaTecnica", "IdParametroNutricional", "MaricoSpec" });

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
            var preset = dataContext.CAL_PresetMaricoLimitedChemicalParameter.SingleOrDefault(X => X.IdPresetMaricoLimitedChemicalParameter == id);
            if (preset == null) { return RedirectToAction("IndexParametrosNutricionales", new { errMsg = "No se ha encontrado el preset", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dataContext.CAL_PresetMaricoLimitedChemicalParameter.DeleteOnSubmit(preset);
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