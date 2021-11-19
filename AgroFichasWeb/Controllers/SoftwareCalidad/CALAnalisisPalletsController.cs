using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.SoftwareCalidad;
using AgroFichasWeb.ViewModels.SoftwareCalidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALAnalisisPalletsController : BaseApplicationController
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALAnalisisPalletsController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALAnalisisPallets
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(327);

            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = from us in dcAgroFichas.PlantaUsuario
                                                  join pl in dcAgroFichas.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
                                                  where us.UserID == CurrentUser.UserID
                                                  && pl.Habilitado
                                                  select new SelectListItem
                                                  {
                                                      Value = pl.IdPlantaProduccion.ToString(),
                                                      Text = pl.Nombre,
                                                      Selected = IdPlantaProduccionSelect == pl.IdPlantaProduccion
                                                  };
            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();


            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            List<CAL_AnalisisPale> list = new List<CAL_AnalisisPale>();
            if (IdPlantaProduccionSelect == 0)
            {
                list = (from X in dcSoftwareCalidad.CAL_AnalisisPale
                        join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                        where X.Habilitado == true
                        orderby X.IdPale ascending
                        select X).ToList();
            }
            else
            {
                list = (from X in dcSoftwareCalidad.CAL_AnalisisPale
                        join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                        where X.Habilitado == true && X.CAL_Pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect
                        orderby X.IdPale ascending
                        select X).ToList();
            }

            /*List<CAL_AnalisisPale> list = (from X in dcSoftwareCalidad.CAL_AnalisisPale
                                           join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                                           where X.Habilitado == true
                                           orderby X.IdPale ascending
                                           select X).ToList();*/


            Permiso permisosUsuario = new Permiso(CheckPermiso(328),
                                                      CheckPermiso(327),
                                                      CheckPermiso(329),
                                                      CheckPermiso(330));
            permisosUsuario.LiberarRetenidoPallet = CheckPermiso(371);

            ViewData["errMsg"]          = Request["errMsg"] ?? "";
            ViewData["okMsg"]           = Request["okMsg"] ?? "";
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = permisosUsuario;
            return View(list);
        }

        public ActionResult Crear(int id, int? IdFichaTecnica)
        {
            CheckPermisoAndRedirect(328);

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            CAL_AnalisisPale analisisPale = new CAL_AnalisisPale
            {
                IdOrdenProduccion        = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdOrdenProduccion,
                IdDetalleOrdenProduccion = pale.CAL_DetalleOrdenProduccion.IdDetalleOrdenProduccion,
                IdPale                   = pale.IdPale
            };

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = pale.CAL_DetalleOrdenProduccion;
            CAL_OrdenProduccion ordenProduccion               = detalleOrdenProduccion.CAL_OrdenProduccion;

            #region Resuelve Ficha Técnica

            int IdFichaTecnicaSelect = IdFichaTecnica ?? 0; string redirectToAction = "", errMsg = "";
            if (IdFichaTecnicaSelect == 0)
            {
                if (CAL_FT.ResolverFichaTecnica(detalleOrdenProduccion, out IdFichaTecnicaSelect, out redirectToAction, out errMsg))
                {
                    if (redirectToAction == "SeleccionarFichaTecnica")
                        return RedirectToAction(redirectToAction, new { id = pale.IdPale, ordenProduccion.IdCliente, detalleOrdenProduccion.IdProducto, detalleOrdenProduccion.IdSubproducto });
                }
                else
                {
                    return RedirectToAction(redirectToAction, new { errMsg, okMsg = "" });
                }
            }

            #endregion

            // Ficha técnica resuelta
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.Single(X => X.IdFichaTecnica == IdFichaTecnicaSelect);

            #region Validación de Ficha Técnica

            if (cAL_FT.ValidaFichaTecnica(pale, out errMsg) == false)
                return RedirectToAction("Index", new { errMsg, okMsg = "" });

            #endregion

            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == pale.CAL_DetalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            CAL_FTControlVersion  cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            ViewData["ordenProduccion"]           = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            ViewData["detalleOrdenProduccion"]    = pale.CAL_DetalleOrdenProduccion;
            ViewData["pale"]                      = pale;
            ViewData["cAL_FTParametroAnalisList"] = cAL_FTParametroAnalisList;
            ViewData["cAL_FT"]                    = cAL_FT;
            ViewData["cAL_FTControlVersion"]      = cAL_FTControlVersion;
            ViewData["plantaProduccion"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == ordenProduccion.IdPlanta).FirstOrDefault();
            return View("Crear", analisisPale);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Crear(int id, CAL_AnalisisPale analisisPale, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(328);
            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion                     = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            CAL_DetalleOrdenProduccion detalleOrdenProduccion       = pale.CAL_DetalleOrdenProduccion;
            CAL_FT cAL_FT                                           = dcSoftwareCalidad.CAL_FT.Single(X => X.IdFichaTecnica == analisisPale.IdFichaTecnica);
            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == detalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            string errMsg = string.Empty;
            string okMsg  = string.Empty;

            analisisPale.Validate(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(analisisPale.Observaciones))
                        analisisPale.Observaciones = string.Empty;

                    analisisPale.IdAnalisisPalePadre = pale.IdPale;
                    analisisPale.Habilitado   = true;
                    analisisPale.FechaHoraIns = DateTime.Now;
                    analisisPale.IpIns        = RemoteAddr();
                    analisisPale.UserIns      = User.Identity.Name;
                    dcSoftwareCalidad.CAL_AnalisisPale.InsertOnSubmit(analisisPale);
                    dcSoftwareCalidad.SubmitChanges();

                    foreach (CAL_FTParametroAnalisis cAL_FTParametroAnalisis in cAL_FTParametroAnalisList)
                    {
                        decimal PARAMETROANALISIS = decimal.Parse(formCollection[string.Format("PARAMETROANALISIS__{0}", cAL_FTParametroAnalisis.IdParametroAnalisis)]);

                        CAL_AnalisisPaleTest cAL_AnalisisPaleTest = new CAL_AnalisisPaleTest()
                        {
                            IdAnalisisPale      = analisisPale.IdAnalisisPale,
                            IdFichaTecnica      = cAL_FT.IdFichaTecnica,
                            IdParametroAnalisis = cAL_FTParametroAnalisis.IdParametroAnalisis,
                            Value               = PARAMETROANALISIS,
                            Habilitado          = true,
                            UserIns             = User.Identity.Name,
                            FechaHoraIns        = DateTime.Now,
                            IpIns               = RemoteAddr(),
                        };
                        dcSoftwareCalidad.CAL_AnalisisPaleTest.InsertOnSubmit(cAL_AnalisisPaleTest);
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    // Reset
                    bool requestOK = true;
                    errMsg = "";
                    okMsg = "";
                    int currentRow = 1;

                    if (!string.IsNullOrEmpty(formCollection["IdsPallets"]))
                    {
                        string[] idsPallets = formCollection["IdsPallets"].Split(new char[] { ',' }).ToArray<string>();
                        foreach (var idPallet in idsPallets)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine(string.Format("<p>PALLET {0}</p>", currentRow));
                            stringBuilder.AppendLine("<ul>");

                            if (!int.TryParse(idPallet, out int result))
                            {
                                stringBuilder.AppendLine("<li>El pallet no es válido</li>");
                                requestOK = false;
                            }

                            CAL_Pale cAL_Pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == result && X.Habilitado == true);
                            if (cAL_Pale == null)
                            {
                                stringBuilder.AppendLine("<li>El pallet no existe</li>");
                                requestOK = false;
                            }

                            stringBuilder.AppendLine("</ul>");

                            if (requestOK)
                            {
                                dcSoftwareCalidad.CAL_ClonarUnAnalisisPalet(analisisPale.IdAnalisisPale, result);
                            }
                            else
                            {
                                errMsg = stringBuilder.ToString();
                                okMsg = "La los análisis se ha creado pero con errores";
                                return RedirectToAction("index", new { errMsg = Url.Encode(errMsg), okMsg = okMsg });
                            }

                            currentRow++;
                        }
                    }

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha creado el análisis del pallet" });
                }
                catch
                {
                    var rv = pale.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            ViewData["ordenProduccion"]           = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            ViewData["detalleOrdenProduccion"]    = pale.CAL_DetalleOrdenProduccion;
            ViewData["pale"]                      = pale;
            ViewData["cAL_FTParametroAnalisList"] = cAL_FTParametroAnalisList;
            ViewData["cAL_FT"]                    = cAL_FT;
            ViewData["cAL_FTControlVersion"]      = cAL_FTControlVersion;
            return View("Crear", analisisPale);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(329);
            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdAnalisisPale == id && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            CAL_Pale pale                                     = analisisPale.CAL_Pale;
            CAL_OrdenProduccion ordenProduccion               = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = pale.CAL_DetalleOrdenProduccion;

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == analisisPale.IdFichaTecnica && X.Habilitado == true);
            if (cAL_FT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == detalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            List<CAL_AnalisisPaleTest> analisisPaleTestList = dcSoftwareCalidad.CAL_AnalisisPaleTest.Where(X => X.IdAnalisisPale == analisisPale.IdAnalisisPale).ToList();

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            ViewData["ordenProduccion"]           = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            ViewData["detalleOrdenProduccion"]    = pale.CAL_DetalleOrdenProduccion;
            ViewData["pale"]                      = pale;
            ViewData["cAL_FTParametroAnalisList"] = cAL_FTParametroAnalisList;
            ViewData["cAL_FT"]                    = cAL_FT;
            ViewData["cAL_FTControlVersion"]      = cAL_FTControlVersion;
            ViewData["analisisPaleTestList"]      = analisisPaleTestList;
            return View("Editar", analisisPale);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(329);
            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdAnalisisPale == id &&  X.Habilitado == true );
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            CAL_Pale pale                                     = analisisPale.CAL_Pale;
            CAL_OrdenProduccion ordenProduccion               = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = pale.CAL_DetalleOrdenProduccion;

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == analisisPale.IdFichaTecnica && X.Habilitado == true);
            if (cAL_FT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == detalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            List<CAL_AnalisisPaleTest> analisisPaleTestList = dcSoftwareCalidad.CAL_AnalisisPaleTest.Where(X => X.IdAnalisisPale == analisisPale.IdAnalisisPale).ToList();

            string errMsg = string.Empty;
            string okMsg = string.Empty;

            analisisPale.Validate(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(analisisPale, new string[] { "FechaEtiquetado", "MuestraEspesor1", "MuestraEspesor2", "MuestraEspesor3", "MuestraEspesor4", "MuestraEspesor5", "MuestraEspesor6", "MuestraEspesor7", "MuestraEspesor8", "MuestraEspesor9", "MuestraEspesor10", "PromedioMuestraEspesor", "SacosDetectorMetales", "Observaciones", "Retenido" });

                    if (string.IsNullOrEmpty(analisisPale.Observaciones))
                        analisisPale.Observaciones = string.Empty;

                    analisisPale.FechaHoraUpd = DateTime.Now;
                    analisisPale.IpUpd        = RemoteAddr();
                    analisisPale.UserUpd      = User.Identity.Name;
                    dcSoftwareCalidad.SubmitChanges();

                    foreach (CAL_FTParametroAnalisis cAL_FTParametroAnalisis in cAL_FTParametroAnalisList)
                    {
                        decimal PARAMETROANALISIS = decimal.Parse(formCollection[string.Format("PARAMETROANALISIS__{0}", cAL_FTParametroAnalisis.IdParametroAnalisis)]);

                        CAL_AnalisisPaleTest cAL_AnalisisPaleTest = analisisPaleTestList.SingleOrDefault(X => X.IdParametroAnalisis == cAL_FTParametroAnalisis.IdParametroAnalisis);
                        if (cAL_AnalisisPaleTest != null)
                        {
                            cAL_AnalisisPaleTest.Value        = PARAMETROANALISIS;
                            cAL_AnalisisPaleTest.UserUpd      = User.Identity.Name;
                            cAL_AnalisisPaleTest.FechaHoraUpd = DateTime.Now;
                            cAL_AnalisisPaleTest.IpUpd        = RemoteAddr();
                            dcSoftwareCalidad.SubmitChanges();
                        }
                        else
                        {
                            cAL_AnalisisPaleTest = new CAL_AnalisisPaleTest()
                            {
                                IdAnalisisPale      = analisisPale.IdAnalisisPale,
                                IdFichaTecnica      = cAL_FT.IdFichaTecnica,
                                IdParametroAnalisis = cAL_FTParametroAnalisis.IdParametroAnalisis,
                                Value               = PARAMETROANALISIS,
                                Habilitado          = true,
                                UserIns             = User.Identity.Name,
                                FechaHoraIns        = DateTime.Now,
                                IpIns               = RemoteAddr(),
                            };
                            dcSoftwareCalidad.CAL_AnalisisPaleTest.InsertOnSubmit(cAL_AnalisisPaleTest);
                        }
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha editado el análisis del pallet" });
                }
                catch
                {
                    var rv = pale.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            ViewData["ordenProduccion"]           = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            ViewData["detalleOrdenProduccion"]    = pale.CAL_DetalleOrdenProduccion;
            ViewData["pale"]                      = pale;
            ViewData["cAL_FTParametroAnalisList"] = cAL_FTParametroAnalisList;
            ViewData["cAL_FT"]                    = cAL_FT;
            ViewData["cAL_FTControlVersion"]      = cAL_FTControlVersion;
            ViewData["analisisPaleTestList"]      = analisisPaleTestList;

            return View("Editar", analisisPale);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(330);

            CAL_AnalisisPale cAL_AnalisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdAnalisisPale == id && X.Habilitado == true);
            if (cAL_AnalisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cAL_AnalisisPale.Habilitado   = false;
                cAL_AnalisisPale.UserUpd      = User.Identity.Name;
                cAL_AnalisisPale.FechaHoraUpd = DateTime.Now;
                cAL_AnalisisPale.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El análisis del pallet {0} ha sido eliminado", cAL_AnalisisPale.IdAnalisisPale);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult VerAnalisisPale(int id)
        {
            CheckPermisoAndRedirect(327);
            CAL_Pale cAL_Pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (cAL_Pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            PaleViewModel paleViewModel = new PaleViewModel
            {
                Pale                   = cAL_Pale,
                DetalleOrdenProduccion = cAL_Pale.CAL_DetalleOrdenProduccion,
                OrdenProduccion        = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion,
                Producto               = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_Subproducto
            };

            CAL_EspesorProducto cAL_EspesorProducto = dcSoftwareCalidad.CAL_EspesorProducto.SingleOrDefault(X => X.IdEspesorProducto == cAL_Pale.CAL_DetalleOrdenProduccion.IdEspesorProducto);
            if (cAL_EspesorProducto != null)
                paleViewModel.EspesorProducto = cAL_EspesorProducto;

            paleViewModel.Saco = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_Saco;

            CAL_GrupoEnvasador grupoEnvasador = dcSoftwareCalidad.CAL_GrupoEnvasador.SingleOrDefault(X => X.IdGrupoEnvasador == cAL_Pale.IdGrupoEnvasador);
            paleViewModel.GrupoEnvasador = grupoEnvasador;

            if (grupoEnvasador != null)
            {
                CAL_Turno turno = dcSoftwareCalidad.CAL_Turno.SingleOrDefault(X => X.IdTurno == grupoEnvasador.IdTurno);
                if (turno != null)
                    paleViewModel.Turno = turno;
            }
            else
                paleViewModel.Turno = CALResolveTurno(dcSoftwareCalidad, cAL_Pale.FechaHoraIns);

            if (grupoEnvasador != null)
            {
                CAL_Turno2 turno2 = dcSoftwareCalidad.CAL_Turno2.SingleOrDefault(X => X.IdTurno == grupoEnvasador.IdTurno2);
                if (turno2 != null)
                    paleViewModel.Turno2 = turno2;
            }
            else
                paleViewModel.Turno2 = ResolveTurno2(dcSoftwareCalidad, cAL_Pale.FechaHoraIns);

            paleViewModel.CALIdentificacionPale = new CALIdentificacionPale();
            paleViewModel.AnalisisPale          = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == cAL_Pale.IdPale && X.Habilitado == true);
            if (paleViewModel.AnalisisPale != null)
            {
                paleViewModel.AnalisisPaleTestList = (from X in dcSoftwareCalidad.CAL_AnalisisPaleTest
                                                      join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                      where X.IdAnalisisPale == paleViewModel.AnalisisPale.IdAnalisisPale
                                                      && X.CAL_FTParametroAnalisis.NoAplica == false
                                                      && Y.IdProducto == paleViewModel.DetalleOrdenProduccion.IdProducto
                                                      orderby Y.Orden
                                                      select X).ToList();
                paleViewModel.Turno2 = ResolveTurno2(dcSoftwareCalidad, paleViewModel.AnalisisPale.FechaHoraIns);
            }

            ViewData["permisosUsuario"] = new Permiso()
            {
                CrearAnalisisPallet = CheckPermiso(328)
            };

            ViewBag.Section = "ResultadoAnalisis"; //#ResultadoAnalisis      

            return View("../CALIdentificacionPale/Pale", paleViewModel);
        }

        public ActionResult Liberar(int id)
        {
            CheckPermisoAndRedirect(371);

            CAL_AnalisisPale analisisiLiberado = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdAnalisisPale == id && X.Habilitado == true);
            if (analisisiLiberado == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el analisis para liberarlo", okMsg = "" });
            }

            string errMsg = "";
            string okMsg  = "";

            try
            {
                analisisiLiberado.Liberado          = true;
                analisisiLiberado.UserLiberado      = User.Identity.Name;
                analisisiLiberado.FechaHoraLiberado = DateTime.Now;
                analisisiLiberado.IpLiberado        = RemoteAddr();
                analisisiLiberado.UserUpd           = User.Identity.Name;
                analisisiLiberado.FechaHoraUpd      = DateTime.Now;
                analisisiLiberado.IpUpd             = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();

                okMsg = String.Format("El pallet {0} ha sido liberado", analisisiLiberado.IdPale);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }

        public ActionResult SeleccionarFichaTecnica(int id, int IdCliente, int IdProducto, int IdSubproducto)
        {
            CheckPermisoAndRedirect(328);

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            List<CAL_FT> fts = CAL_FT.GetFichasTecnicas(IdCliente, IdProducto, IdSubproducto);

            ViewData["pale"]   = pale;
            ViewData["errMsg"] = Request["errMsg"] ?? "";
            ViewData["okMsg"]  = Request["okMsg"] ?? "";
            return View("SeleccionarFichaTecnica", fts);
        }

        [HttpPost]
        public ActionResult SeleccionarFichaTecnica(int id, int IdCliente, int IdProducto, int IdSubproducto, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(328);

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
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
                    return RedirectToAction("Crear", new { id = pale.IdPale, cAL_FT.IdFichaTecnica });
                }
            }
            else
            {
                errMsg = "Debes seleccionar una ficha técnica";
            }

            List<CAL_FT> fts = CAL_FT.GetFichasTecnicas(IdCliente, IdProducto, IdSubproducto);

            ViewData["pale"]   = pale;
            ViewData["errMsg"] = errMsg;
            ViewData["okMsg"]  = okMsg;
            return View("SeleccionarFichaTecnica", fts);
        }
    }
}