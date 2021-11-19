using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALAnalisisPeriodicosController : BaseApplicationController
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALAnalisisPeriodicosController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALAnalisisPeriodicos
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(373);
            CAL_AnalisisPeriodico analisisPeriodico = new CAL_AnalisisPeriodico();
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(374),
                                                      CheckPermiso(373),
                                                      CheckPermiso(375),
                                                      CheckPermiso(376));
            return View(analisisPeriodico);
        }

        [HttpPost]
        public ActionResult Index(CAL_AnalisisPeriodico cAL_AnalisisPeriodico, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(373);

            if (ModelState.IsValid)
            {
                string errMsg = "";
                string okMsg  = "";

                try
                {
                    return RedirectToAction("VerOP", new { cAL_AnalisisPeriodico.IdCliente, cAL_AnalisisPeriodico.IdOrdenProduccion, errMsg, okMsg });
                }
                catch
                {
                    var rv = cAL_AnalisisPeriodico.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(373),
                                                      CheckPermiso(374),
                                                      CheckPermiso(375),
                                                      CheckPermiso(376));
            return View(cAL_AnalisisPeriodico);
        }

        public ActionResult VerOP(int IdCliente, int IdOrdenProduccion)
        {
            CheckPermisoAndRedirect(373);

            Cliente cliente = dcAgroFichas.Cliente.SingleOrDefault(X => X.IdCliente == IdCliente && X.Habilitado == true);
            if (cliente == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el cliente", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == IdOrdenProduccion && X.Habilitado == true);

            List<vw_CAL_OrdenProduccionDespacho> list = (from O0 in dcSoftwareCalidad.vw_CAL_OrdenProduccionDespacho
                                                         where (IdOrdenProduccion == 0 || O0.IdOrdenProduccion == IdOrdenProduccion)
                                                         && O0.IdCliente == IdCliente
                                                         select O0).ToList();

            ViewData["cliente"]         = cliente;
            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(373),
                                                      CheckPermiso(374),
                                                      CheckPermiso(375),
                                                      CheckPermiso(376));
            return View("VerOP", list);
        }

        public ActionResult VerAnalisis(int id, int IdTipoAnalisis)
        {
            CheckPermisoAndRedirect(373);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_TipoAnalisis tipoAnalisis = dcSoftwareCalidad.CAL_TipoAnalisis.SingleOrDefault(X => X.IdTipoAnalisis == IdTipoAnalisis);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de análisis", okMsg = "" });
            }

            List<CAL_AnalisisPeriodico> analisisPeriodicos = dcSoftwareCalidad.CAL_AnalisisPeriodico.Where(X => X.IdDetalleOrdenProduccion == detalleOrdenProduccion.IdDetalleOrdenProduccion && X.Habilitado == true && X.IdTipoAnalisis == tipoAnalisis.IdTipoAnalisis).ToList();

            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["tipoAnalisis"]           = tipoAnalisis;
            ViewData["errMsg"]                 = Request["errMsg"];
            ViewData["okMsg"]                  = Request["okMsg"];
            ViewData["permisosUsuario"]        = new Permiso(CheckPermiso(373),
                                                             CheckPermiso(374),
                                                             CheckPermiso(375),
                                                             CheckPermiso(376));
            return View(analisisPeriodicos);
        }

        public ActionResult VerAnalisisDetallado(int id)
        {
            CheckPermisoAndRedirect(373);
            CAL_AnalisisPeriodico analisisPeriodico = dcSoftwareCalidad.CAL_AnalisisPeriodico.SingleOrDefault(X => X.IdAnalisisPeriodico == id && X.Habilitado == true);
            if (analisisPeriodico == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis periódico", okMsg = "" });
            }

            CAL_TipoAnalisis cAL_TipoAnalisis                                 = analisisPeriodico.CAL_TipoAnalisis;
            List<CAL_AnalisisPesticidaTest> analisisPesticidaTestList         = dcSoftwareCalidad.CAL_AnalisisPesticidaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMetalPesadoTest> analisisMetalPesadoTestList     = dcSoftwareCalidad.CAL_AnalisisMetalPesadoTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMicotoxinaTest> analisisMicotoxinaTestList       = dcSoftwareCalidad.CAL_AnalisisMicotoxinaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMicrobiologiaTest> analisisMicrobiologiaTestList = dcSoftwareCalidad.CAL_AnalisisMicrobiologiaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisNutricionalTest> analisisNutricionalTestList     = dcSoftwareCalidad.CAL_AnalisisNutricionalTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();

            ViewData["cAL_TipoAnalisis"]              = cAL_TipoAnalisis;
            ViewData["analisisPesticidaTestList"]     = analisisPesticidaTestList;
            ViewData["analisisPesticidaTestList"]     = analisisPesticidaTestList;
            ViewData["analisisMetalPesadoTestList"]   = analisisMetalPesadoTestList;
            ViewData["analisisMicotoxinaTestList"]    = analisisMicotoxinaTestList;
            ViewData["analisisMicrobiologiaTestList"] = analisisMicrobiologiaTestList;
            ViewData["analisisNutricionalTestList"]   = analisisNutricionalTestList;
            ViewData["errMsg"]                        = Request["errMsg"];
            ViewData["okMsg"]                         = Request["okMsg"];
            ViewData["permisosUsuario"]               = new Permiso(CheckPermiso(373),
                                                                    CheckPermiso(374),
                                                                    CheckPermiso(375),
                                                                    CheckPermiso(376));
            return View(analisisPeriodico);
        }

        public ActionResult CreaAnalisis(int id, int IdTipoAnalisis, int? IdFichaTecnica)
        {
            CheckPermisoAndRedirect(374);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_TipoAnalisis cAL_TipoAnalisis = dcSoftwareCalidad.CAL_TipoAnalisis.SingleOrDefault(X => X.IdTipoAnalisis == IdTipoAnalisis);
            if (cAL_TipoAnalisis == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de análisis", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion = detalleOrdenProduccion.CAL_OrdenProduccion;

            #region Resuelve Ficha Técnica

            int IdFichaTecnicaSelect = IdFichaTecnica ?? 0; string redirectToAction = "", errMsg = "";
            if (IdFichaTecnicaSelect == 0)
            {
                if (CAL_FT.ResolverFichaTecnica(detalleOrdenProduccion, out IdFichaTecnicaSelect, out redirectToAction, out errMsg))
                {
                    if (redirectToAction == "SeleccionarFichaTecnica")
                        return RedirectToAction(redirectToAction, new { id = detalleOrdenProduccion.IdDetalleOrdenProduccion, cAL_TipoAnalisis.IdTipoAnalisis, ordenProduccion.IdCliente, detalleOrdenProduccion.IdProducto, detalleOrdenProduccion.IdSubproducto });
                }
                else
                {
                    return RedirectToAction(redirectToAction, new { errMsg, okMsg = "" });
                }
            }

            #endregion

            // Ficha técnica resuelta
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.Single(X => X.IdFichaTecnica == IdFichaTecnicaSelect);

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            CAL_AnalisisPeriodico analisisPeriodico = new CAL_AnalisisPeriodico()
            {
                IdOrdenProduccion        = ordenProduccion.IdOrdenProduccion,
                IdDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion,
                IdTipoAnalisis           = cAL_TipoAnalisis.IdTipoAnalisis
            };

            CreaParametros(cAL_FT.IdFichaTecnica, cAL_TipoAnalisis.IdTipoAnalisis);
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["ordenProduccion"]        = ordenProduccion;
            ViewData["cAL_TipoAnalisis"]       = cAL_TipoAnalisis;
            ViewData["cAL_FT"]                 = cAL_FT;
            ViewData["cAL_FTControlVersion"]   = cAL_FTControlVersion;
            ViewData["plantaProduccion"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == ordenProduccion.IdPlanta).FirstOrDefault();
            return View(analisisPeriodico);
        }

        [HttpPost]
        public ActionResult CreaAnalisis(int id, int IdTipoAnalisis, CAL_AnalisisPeriodico analisisPeriodico, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(374);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_TipoAnalisis cAL_TipoAnalisis = dcSoftwareCalidad.CAL_TipoAnalisis.SingleOrDefault(X => X.IdTipoAnalisis == IdTipoAnalisis);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de análisis", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion       = detalleOrdenProduccion.CAL_OrdenProduccion;
            CAL_FT cAL_FT                             = dcSoftwareCalidad.CAL_FT.Single(X => X.IdFichaTecnica == analisisPeriodico.IdFichaTecnica);
            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            string errMsg = string.Empty;
            string okMsg  = string.Empty;

            analisisPeriodico.ValidateParametros(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    analisisPeriodico.Habilitado   = true;
                    analisisPeriodico.UserIns      = User.Identity.Name;
                    analisisPeriodico.FechaHoraIns = DateTime.Now;
                    analisisPeriodico.IpIns        = RemoteAddr();
                    dcSoftwareCalidad.CAL_AnalisisPeriodico.InsertOnSubmit(analisisPeriodico);
                    dcSoftwareCalidad.SubmitChanges();

                    switch (cAL_TipoAnalisis.IdTipoAnalisis)
                    {
                        case 1:
                            CreaParametrosMetalesPesados(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                        case 2:
                            CreaParametrosMicotoxinas(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                        case 3:
                            CreaParametrosMicrobiologia(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                        case 4:
                            CreaParametrosNutricionales(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                        case 5:
                            CreaParametrosPesticidas(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                    }

                    okMsg = string.Format("Se ha guardado el {0}", cAL_TipoAnalisis.Descripcion.ToLower());
                    return RedirectToAction("VerAnalisis", new { id = detalleOrdenProduccion.IdDetalleOrdenProduccion, cAL_TipoAnalisis.IdTipoAnalisis, errMsg = "", okMsg });
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }
            }

            CreaParametros(cAL_FT.IdFichaTecnica, cAL_TipoAnalisis.IdTipoAnalisis);
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["ordenProduccion"]        = ordenProduccion;
            ViewData["cAL_TipoAnalisis"]       = cAL_TipoAnalisis;
            ViewData["cAL_FT"]                 = cAL_FT;
            ViewData["cAL_FTControlVersion"]   = cAL_FTControlVersion;
            return View(analisisPeriodico);
        }

        public ActionResult EditarAnalisis(int id)
        {
            CheckPermisoAndRedirect(375);
            CAL_AnalisisPeriodico analisisPeriodico = dcSoftwareCalidad.CAL_AnalisisPeriodico.SingleOrDefault(X => X.IdAnalisisPeriodico == id && X.Habilitado == true);
            if (analisisPeriodico == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis periódico", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion               = analisisPeriodico.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = analisisPeriodico.CAL_DetalleOrdenProduccion;
            CAL_TipoAnalisis cAL_TipoAnalisis                 = analisisPeriodico.CAL_TipoAnalisis;

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == analisisPeriodico.IdFichaTecnica && X.Habilitado == true);
            if (cAL_FT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            analisisPeriodico.IdOrdenProduccion                               = ordenProduccion.IdOrdenProduccion;
            List<CAL_AnalisisPesticidaTest> analisisPesticidaTestList         = dcSoftwareCalidad.CAL_AnalisisPesticidaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMetalPesadoTest> analisisMetalPesadoTestList     = dcSoftwareCalidad.CAL_AnalisisMetalPesadoTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMicotoxinaTest> analisisMicotoxinaTestList       = dcSoftwareCalidad.CAL_AnalisisMicotoxinaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMicrobiologiaTest> analisisMicrobiologiaTestList = dcSoftwareCalidad.CAL_AnalisisMicrobiologiaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisNutricionalTest> analisisNutricionalTestList     = dcSoftwareCalidad.CAL_AnalisisNutricionalTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();

            CreaParametros(cAL_FT.IdFichaTecnica, cAL_TipoAnalisis.IdTipoAnalisis);
            ViewData["detalleOrdenProduccion"]        = detalleOrdenProduccion;
            ViewData["ordenProduccion"]               = ordenProduccion;
            ViewData["cAL_TipoAnalisis"]              = cAL_TipoAnalisis;
            ViewData["cAL_FT"]                        = cAL_FT;
            ViewData["cAL_FTControlVersion"]          = cAL_FTControlVersion;
            ViewData["analisisPesticidaTestList"]     = analisisPesticidaTestList;
            ViewData["analisisMetalPesadoTestList"]   = analisisMetalPesadoTestList;
            ViewData["analisisMicotoxinaTestList"]    = analisisMicotoxinaTestList;
            ViewData["analisisMicrobiologiaTestList"] = analisisMicrobiologiaTestList;
            ViewData["analisisNutricionalTestList"]   = analisisNutricionalTestList;
            ViewData["plantaProduccion"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == ordenProduccion.IdPlanta).FirstOrDefault();
            return View(analisisPeriodico);
        }

        [HttpPost]
        public ActionResult EditarAnalisis(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(375);
            CAL_AnalisisPeriodico analisisPeriodico = dcSoftwareCalidad.CAL_AnalisisPeriodico.SingleOrDefault(X => X.IdAnalisisPeriodico == id && X.Habilitado == true);
            if (analisisPeriodico == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis periódico", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion               = analisisPeriodico.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = analisisPeriodico.CAL_DetalleOrdenProduccion;
            CAL_TipoAnalisis cAL_TipoAnalisis                 = analisisPeriodico.CAL_TipoAnalisis;

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == analisisPeriodico.IdFichaTecnica && X.Habilitado == true);
            if (cAL_FT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            analisisPeriodico.IdOrdenProduccion                               = ordenProduccion.IdOrdenProduccion;
            List<CAL_AnalisisPesticidaTest> analisisPesticidaTestList         = dcSoftwareCalidad.CAL_AnalisisPesticidaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMetalPesadoTest> analisisMetalPesadoTestList     = dcSoftwareCalidad.CAL_AnalisisMetalPesadoTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMicotoxinaTest> analisisMicotoxinaTestList       = dcSoftwareCalidad.CAL_AnalisisMicotoxinaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisMicrobiologiaTest> analisisMicrobiologiaTestList = dcSoftwareCalidad.CAL_AnalisisMicrobiologiaTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();
            List<CAL_AnalisisNutricionalTest> analisisNutricionalTestList     = dcSoftwareCalidad.CAL_AnalisisNutricionalTest.Where(X => X.IdAnalisisPeriodico == analisisPeriodico.IdAnalisisPeriodico && X.Habilitado == true).ToList();

            string errMsg = string.Empty;
            string okMsg  = string.Empty;

            analisisPeriodico.ValidateParametros(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(analisisPeriodico, new string[] { "FechaHoraInsRealizado" });

                    analisisPeriodico.UserUpd      = User.Identity.Name;
                    analisisPeriodico.FechaHoraUpd = DateTime.Now;
                    analisisPeriodico.IpUpd        = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();

                    switch (cAL_TipoAnalisis.IdTipoAnalisis)
                    {
                        case 1:
                            CreaParametrosMetalesPesados(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                        case 2:
                            CreaParametrosMicotoxinas(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                        case 3:
                            CreaParametrosMicrobiologia(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                        case 4:
                            CreaParametrosNutricionales(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                        case 5:
                            CreaParametrosPesticidas(cAL_FT.IdFichaTecnica, analisisPeriodico.IdAnalisisPeriodico, formCollection);
                            break;
                    }

                    okMsg = string.Format("Se ha editado el {0}", cAL_TipoAnalisis.Descripcion.ToLower());
                    return RedirectToAction("VerAnalisis", new { id = detalleOrdenProduccion.IdDetalleOrdenProduccion, cAL_TipoAnalisis.IdTipoAnalisis, errMsg = "", okMsg });
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }
            }

            CreaParametros(cAL_FT.IdFichaTecnica, cAL_TipoAnalisis.IdTipoAnalisis);
            ViewData["detalleOrdenProduccion"]        = detalleOrdenProduccion;
            ViewData["ordenProduccion"]               = ordenProduccion;
            ViewData["cAL_TipoAnalisis"]              = cAL_TipoAnalisis;
            ViewData["cAL_FT"]                        = cAL_FT;
            ViewData["cAL_FTControlVersion"]          = cAL_FTControlVersion;
            ViewData["analisisPesticidaTestList"]     = analisisPesticidaTestList;
            ViewData["analisisMetalPesadoTestList"]   = analisisMetalPesadoTestList;
            ViewData["analisisMicotoxinaTestList"]    = analisisMicotoxinaTestList;
            ViewData["analisisMicrobiologiaTestList"] = analisisMicrobiologiaTestList;
            ViewData["analisisNutricionalTestList"]   = analisisNutricionalTestList;
            return View(analisisPeriodico);
        }

        public ActionResult EliminarAnalisis(int id)
        {
            CheckPermisoAndRedirect(376);
            CAL_AnalisisPeriodico analisisPeriodico = dcSoftwareCalidad.CAL_AnalisisPeriodico.SingleOrDefault(X => X.IdAnalisisPeriodico == id && X.Habilitado == true);
            if (analisisPeriodico == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis periódico", okMsg = "" });
            }

            string errMsg = "";
            string okMsg  = "";

            try
            {
                analisisPeriodico.Habilitado   = false;
                analisisPeriodico.UserUpd      = User.Identity.Name;
                analisisPeriodico.FechaHoraUpd = DateTime.Now;
                analisisPeriodico.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El análisis periódico {0} ha sido eliminado", analisisPeriodico.IdAnalisisPeriodico);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("VerAnalisis", new { id = analisisPeriodico.IdDetalleOrdenProduccion, IdTipoAnalisis = analisisPeriodico.IdTipoAnalisis, errMsg = "", okMsg = okMsg });
        }

        public ActionResult SeleccionarFichaTecnica(int id, int IdTipoAnalisis, int IdCliente, int IdProducto, int IdSubproducto)
        {
            CheckPermisoAndRedirect(374);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_TipoAnalisis cAL_TipoAnalisis = dcSoftwareCalidad.CAL_TipoAnalisis.SingleOrDefault(X => X.IdTipoAnalisis == IdTipoAnalisis);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de análisis", okMsg = "" });
            }

            List<CAL_FT> fts = CAL_FT.GetFichasTecnicas(IdCliente, IdProducto, IdSubproducto);

            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["cAL_TipoAnalisis"]       = cAL_TipoAnalisis;
            ViewData["errMsg"]                 = Request["errMsg"] ?? "";
            ViewData["okMsg"]                  = Request["okMsg"] ?? "";
            return View("SeleccionarFichaTecnica", fts);
        }

        [HttpPost]
        public ActionResult SeleccionarFichaTecnica(int id, int IdTipoAnalisis, int IdCliente, int IdProducto, int IdSubproducto, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(374);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_TipoAnalisis cAL_TipoAnalisis = dcSoftwareCalidad.CAL_TipoAnalisis.SingleOrDefault(X => X.IdTipoAnalisis == IdTipoAnalisis);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de análisis", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            if (string.IsNullOrEmpty(formCollection["IdFichaTecnica"]) == false && int.TryParse(formCollection["IdFichaTecnica"], out int IdFichaTecnica))
            {
                CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == IdFichaTecnica && X.Habilitado == true);
                if (cAL_FT == null)
                {
                    errMsg = "No se ha encontrado la ficha técnica";
                }
                else
                {
                    return RedirectToAction("CreaAnalisis", new { id = detalleOrdenProduccion.IdDetalleOrdenProduccion, cAL_TipoAnalisis.IdTipoAnalisis, cAL_FT.IdFichaTecnica });
                }
            }
            else
            {
                errMsg = "Debes seleccionar una ficha técnica";
            }

            List<CAL_FT> fts = CAL_FT.GetFichasTecnicas(IdCliente, IdProducto, IdSubproducto);

            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["cAL_TipoAnalisis"]       = cAL_TipoAnalisis;
            ViewData["errMsg"]                 = errMsg;
            ViewData["okMsg"]                  = okMsg;
            return View("SeleccionarFichaTecnica", fts);
        }

        #region --- Funciones PRIVADAS ---

        private void CreaParametros(int IdFichaTecnica, int IdTipoAnalisis)
        {
            switch (IdTipoAnalisis)
            {
                case 1:
                    ViewData["parametroList"] = (from X in dcSoftwareCalidad.CAL_FTParametroMetalPesado
                                                 where X.IdFichaTecnica == IdFichaTecnica
                                                 && X.NoAplica == false
                                                 select X).ToList();
                    break;
                case 2:
                    ViewData["parametroList"] = (from X in dcSoftwareCalidad.CAL_FTParametroMicotoxina
                                                 where X.IdFichaTecnica == IdFichaTecnica
                                                 && X.NoAplica == false
                                                 select X).ToList();
                    break;
                case 3:
                    ViewData["parametroList"] = (from X in dcSoftwareCalidad.CAL_FTParametroMicrobiologia
                                                 where X.IdFichaTecnica == IdFichaTecnica
                                                 && X.NoAplica == false
                                                 select X).ToList();
                    break;
                case 4:
                    ViewData["parametroList"] = (from X in dcSoftwareCalidad.CAL_FTParametroNutricional
                                                 where X.IdFichaTecnica == IdFichaTecnica
                                                 && X.NoAplica == false
                                                 select X).ToList();
                    break;
                case 5:
                    ViewData["parametroList"] = (from X in dcSoftwareCalidad.CAL_FTParametroPesticida
                                                 where X.IdFichaTecnica == IdFichaTecnica
                                                 && X.NoAplica == false
                                                 select X).ToList();
                    break;
            }
        }

        private void CreaParametrosPesticidas(int IdFichaTecnica, int IdAnalisisPeriodico, FormCollection formCollection)
        {
            List<CAL_FTParametroPesticida> cAL_FTParametroPesticidaList = (from X in dcSoftwareCalidad.CAL_FTParametroPesticida
                                                                           where X.IdFichaTecnica == IdFichaTecnica
                                                                           && X.NoAplica == false
                                                                           select X).ToList();

            List<CAL_AnalisisPesticidaTest> analisisPesticidaTestList = dcSoftwareCalidad.CAL_AnalisisPesticidaTest.Where(X => X.IdAnalisisPeriodico == IdAnalisisPeriodico && X.Habilitado == true).ToList();

            foreach (CAL_FTParametroPesticida cAL_FTParametroPesticida in cAL_FTParametroPesticidaList)
            {
                decimal PARAMETROPESTICIDA = decimal.Parse(formCollection[string.Format("PARAMETROPESTICIDA__{0}", cAL_FTParametroPesticida.IdParametroPesticida)]);
                bool FormatString = bool.Parse(formCollection[string.Format("HID_NA_{0}", cAL_FTParametroPesticida.IdParametroPesticida)]);

                CAL_AnalisisPesticidaTest analisisPesticidaTest = analisisPesticidaTestList.SingleOrDefault(X => X.IdParametroPesticida == cAL_FTParametroPesticida.IdParametroPesticida);
                if (analisisPesticidaTest != null)
                {
                    analisisPesticidaTest.Value        = PARAMETROPESTICIDA;
                    analisisPesticidaTest.FormatString = FormatString;
                    analisisPesticidaTest.UserUpd      = User.Identity.Name;
                    analisisPesticidaTest.FechaHoraUpd = DateTime.Now;
                    analisisPesticidaTest.IpUpd        = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();
                }
                else
                {
                    analisisPesticidaTest = new CAL_AnalisisPesticidaTest()
                    {
                        IdAnalisisPeriodico  = IdAnalisisPeriodico,
                        IdFichaTecnica       = IdFichaTecnica,
                        IdParametroPesticida = cAL_FTParametroPesticida.IdParametroPesticida,
                        Value                = PARAMETROPESTICIDA,
                        FormatString         = FormatString,
                        Habilitado           = true,
                        UserIns              = User.Identity.Name,
                        FechaHoraIns         = DateTime.Now,
                        IpIns                = RemoteAddr()
                    };
                    dcSoftwareCalidad.CAL_AnalisisPesticidaTest.InsertOnSubmit(analisisPesticidaTest);
                }
            }
            dcSoftwareCalidad.SubmitChanges();
        }

        private void CreaParametrosMetalesPesados(int IdFichaTecnica, int IdAnalisisPeriodico, FormCollection formCollection)
        {
            List<CAL_FTParametroMetalPesado> cAL_FTParametroMetalPesadoList = (from X in dcSoftwareCalidad.CAL_FTParametroMetalPesado
                                                                               where X.IdFichaTecnica == IdFichaTecnica
                                                                               && X.NoAplica == false
                                                                               select X).ToList();

            List<CAL_AnalisisMetalPesadoTest> analisisMetalPesadoTestList = dcSoftwareCalidad.CAL_AnalisisMetalPesadoTest.Where(X => X.IdAnalisisPeriodico == IdAnalisisPeriodico && X.Habilitado == true).ToList();

            foreach (CAL_FTParametroMetalPesado cAL_FTParametroMetalPesado in cAL_FTParametroMetalPesadoList)
            {
                decimal PARAMETROMETALPESADO = decimal.Parse(formCollection[string.Format("PARAMETROMETALPESADO__{0}", cAL_FTParametroMetalPesado.IdParametroMetalPesado)]);
                bool FormatString = bool.Parse(formCollection[string.Format("HID_NA_{0}", cAL_FTParametroMetalPesado.IdParametroMetalPesado)]);

                CAL_AnalisisMetalPesadoTest analisisMetalPesadoTest = analisisMetalPesadoTestList.SingleOrDefault(X => X.IdParametroMetalPesado == cAL_FTParametroMetalPesado.IdParametroMetalPesado);
                if (analisisMetalPesadoTest != null)
                {
                    analisisMetalPesadoTest.Value        = PARAMETROMETALPESADO;
                    analisisMetalPesadoTest.FormatString = FormatString;
                    analisisMetalPesadoTest.UserUpd      = User.Identity.Name;
                    analisisMetalPesadoTest.FechaHoraUpd = DateTime.Now;
                    analisisMetalPesadoTest.IpUpd        = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();
                }
                else
                {
                    analisisMetalPesadoTest = new CAL_AnalisisMetalPesadoTest()
                    {
                        IdAnalisisPeriodico    = IdAnalisisPeriodico,
                        IdFichaTecnica         = IdFichaTecnica,
                        IdParametroMetalPesado = cAL_FTParametroMetalPesado.IdParametroMetalPesado,
                        Value                  = PARAMETROMETALPESADO,
                        FormatString           = FormatString,
                        Habilitado             = true,
                        UserIns                = User.Identity.Name,
                        FechaHoraIns           = DateTime.Now,
                        IpIns                  = RemoteAddr()
                    };
                    dcSoftwareCalidad.CAL_AnalisisMetalPesadoTest.InsertOnSubmit(analisisMetalPesadoTest);
                }
            }
            dcSoftwareCalidad.SubmitChanges();
        }

        private void CreaParametrosMicotoxinas(int IdFichaTecnica, int IdAnalisisPeriodico, FormCollection formCollection)
        {
            List<CAL_FTParametroMicotoxina> cAL_FTParametroMicotoxinaList = (from X in dcSoftwareCalidad.CAL_FTParametroMicotoxina
                                                                             where X.IdFichaTecnica == IdFichaTecnica
                                                                             && X.NoAplica == false
                                                                             select X).ToList();

            List<CAL_AnalisisMicotoxinaTest> analisisMicotoxinaTestList = dcSoftwareCalidad.CAL_AnalisisMicotoxinaTest.Where(X => X.IdAnalisisPeriodico == IdAnalisisPeriodico && X.Habilitado == true).ToList();

            foreach (CAL_FTParametroMicotoxina cAL_FTParametroMicotoxina in cAL_FTParametroMicotoxinaList)
            {
                decimal PARAMETROMICOTOXINA = decimal.Parse(formCollection[string.Format("PARAMETROMICOTOXINA__{0}", cAL_FTParametroMicotoxina.IdParametroMicotoxina)]);
                bool FormatString = bool.Parse(formCollection[string.Format("HID_NA_{0}", cAL_FTParametroMicotoxina.IdParametroMicotoxina)]);

                CAL_AnalisisMicotoxinaTest analisisMicotoxinaTest = analisisMicotoxinaTestList.SingleOrDefault(X => X.IdParametroMicotoxina == cAL_FTParametroMicotoxina.IdParametroMicotoxina);
                if (analisisMicotoxinaTest != null)
                {
                    analisisMicotoxinaTest.Value        = PARAMETROMICOTOXINA;
                    analisisMicotoxinaTest.FormatString = FormatString;
                    analisisMicotoxinaTest.UserUpd      = User.Identity.Name;
                    analisisMicotoxinaTest.FechaHoraUpd = DateTime.Now;
                    analisisMicotoxinaTest.IpUpd        = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();
                }
                else
                {
                    analisisMicotoxinaTest = new CAL_AnalisisMicotoxinaTest()
                    {
                        IdAnalisisPeriodico   = IdAnalisisPeriodico,
                        IdFichaTecnica        = IdFichaTecnica,
                        IdParametroMicotoxina = cAL_FTParametroMicotoxina.IdParametroMicotoxina,
                        Value                 = PARAMETROMICOTOXINA,
                        FormatString          = FormatString,
                        Habilitado            = true,
                        UserIns               = User.Identity.Name,
                        FechaHoraIns          = DateTime.Now,
                        IpIns                 = RemoteAddr()
                    };
                    dcSoftwareCalidad.CAL_AnalisisMicotoxinaTest.InsertOnSubmit(analisisMicotoxinaTest);
                }
            }
            dcSoftwareCalidad.SubmitChanges();
        }

        private void CreaParametrosMicrobiologia(int IdFichaTecnica, int IdAnalisisPeriodico, FormCollection formCollection)
        {
            List<CAL_FTParametroMicrobiologia> cAL_FTParametroMicrobiologiaList = (from X in dcSoftwareCalidad.CAL_FTParametroMicrobiologia
                                                                                   where X.IdFichaTecnica == IdFichaTecnica
                                                                                   && X.NoAplica == false
                                                                                   select X).ToList();

            List<CAL_AnalisisMicrobiologiaTest> analisisMicrobiologiaTestList = dcSoftwareCalidad.CAL_AnalisisMicrobiologiaTest.Where(X => X.IdAnalisisPeriodico == IdAnalisisPeriodico && X.Habilitado == true).ToList();

            foreach (CAL_FTParametroMicrobiologia cAL_FTParametroMicrobiologia in cAL_FTParametroMicrobiologiaList)
            {
                decimal PARAMETROMICROBIOLOGIA = decimal.Parse(formCollection[string.Format("PARAMETROMICROBIOLOGIA__{0}", cAL_FTParametroMicrobiologia.IdParametroMicrobiologia)]);
                bool FormatString = bool.Parse(formCollection[string.Format("HID_NA_{0}", cAL_FTParametroMicrobiologia.IdParametroMicrobiologia)]);

                CAL_AnalisisMicrobiologiaTest analisisMicrobiologiaTest = analisisMicrobiologiaTestList.SingleOrDefault(X => X.IdParametroMicrobiologia == cAL_FTParametroMicrobiologia.IdParametroMicrobiologia);
                if (analisisMicrobiologiaTest != null)
                {
                    analisisMicrobiologiaTest.Value        = PARAMETROMICROBIOLOGIA;
                    analisisMicrobiologiaTest.FormatString = FormatString;
                    analisisMicrobiologiaTest.UserUpd      = User.Identity.Name;
                    analisisMicrobiologiaTest.FechaHoraUpd = DateTime.Now;
                    analisisMicrobiologiaTest.IpUpd        = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();
                }
                else
                {
                    analisisMicrobiologiaTest = new CAL_AnalisisMicrobiologiaTest()
                    {
                        IdAnalisisPeriodico      = IdAnalisisPeriodico,
                        IdFichaTecnica           = IdFichaTecnica,
                        IdParametroMicrobiologia = cAL_FTParametroMicrobiologia.IdParametroMicrobiologia,
                        Value                    = PARAMETROMICROBIOLOGIA,
                        FormatString             = FormatString,
                        Habilitado               = true,
                        UserIns                  = User.Identity.Name,
                        FechaHoraIns             = DateTime.Now,
                        IpIns                    = RemoteAddr()
                    };
                    dcSoftwareCalidad.CAL_AnalisisMicrobiologiaTest.InsertOnSubmit(analisisMicrobiologiaTest);
                }
            }
            dcSoftwareCalidad.SubmitChanges();
        }

        private void CreaParametrosNutricionales(int IdFichaTecnica, int IdAnalisisPeriodico, FormCollection formCollection)
        {
            List<CAL_FTParametroNutricional> cAL_FTParametroNutricionalList = (from X in dcSoftwareCalidad.CAL_FTParametroNutricional
                                                                               where X.IdFichaTecnica == IdFichaTecnica
                                                                               && X.NoAplica == false
                                                                               select X).ToList();

            List<CAL_AnalisisNutricionalTest> analisisNutricionalTestList = dcSoftwareCalidad.CAL_AnalisisNutricionalTest.Where(X => X.IdAnalisisPeriodico == IdAnalisisPeriodico && X.Habilitado == true).ToList();

            foreach (CAL_FTParametroNutricional cAL_FTParametroNutricional in cAL_FTParametroNutricionalList)
            {
                decimal PARAMETRONUTRICIONAL = decimal.Parse(formCollection[string.Format("PARAMETRONUTRICIONAL__{0}", cAL_FTParametroNutricional.IdParametroNutricional)]);
                bool FormatString = bool.Parse(formCollection[string.Format("HID_NA_{0}", cAL_FTParametroNutricional.IdParametroNutricional)]);

                CAL_AnalisisNutricionalTest analisisNutricionalTest = analisisNutricionalTestList.SingleOrDefault(X => X.IdParametroNutricional == cAL_FTParametroNutricional.IdParametroNutricional);
                if (analisisNutricionalTest != null)
                {
                    analisisNutricionalTest.Value        = PARAMETRONUTRICIONAL;
                    analisisNutricionalTest.FormatString = FormatString;
                    analisisNutricionalTest.UserUpd      = User.Identity.Name;
                    analisisNutricionalTest.FechaHoraUpd = DateTime.Now;
                    analisisNutricionalTest.IpUpd        = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();
                }
                else
                {
                    analisisNutricionalTest = new CAL_AnalisisNutricionalTest()
                    {
                        IdAnalisisPeriodico    = IdAnalisisPeriodico,
                        IdFichaTecnica         = IdFichaTecnica,
                        IdParametroNutricional = cAL_FTParametroNutricional.IdParametroNutricional,
                        Value                  = PARAMETRONUTRICIONAL,
                        FormatString           = FormatString,
                        Habilitado             = true,
                        UserIns                = User.Identity.Name,
                        FechaHoraIns           = DateTime.Now,
                        IpIns                  = RemoteAddr()
                    };
                    dcSoftwareCalidad.CAL_AnalisisNutricionalTest.InsertOnSubmit(analisisNutricionalTest);
                }
            }
            dcSoftwareCalidad.SubmitChanges();
        }

        #endregion
    }
}