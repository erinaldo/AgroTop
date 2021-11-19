using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.SoftwareCalidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALDespachosCargaGranelController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        public CALDespachosCargaGranelController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALDespachosCargaGranel
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(356);

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
            List<CAL_DespachoCargaGranel> list = new List<CAL_DespachoCargaGranel>();
            if (IdPlantaProduccionSelect == 0)
            {
                 list = dcSoftwareCalidad.CAL_DespachoCargaGranel.Where(X => X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();

            }
            else
            {
                list = dcSoftwareCalidad.CAL_DespachoCargaGranel.Where(X => X.Habilitado == true && X.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect).OrderByDescending(X => X.FechaHoraIns).ToList();

            }


            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            Permiso permisosUsuario = new Permiso()
            {
                Leer = CheckPermiso(356),
                Crear = CheckPermiso(357),
                Actualizar = CheckPermiso(358),
                Borrar = CheckPermiso(359),
                ReprocesarDespachosCargaGranel = CheckPermiso(386),
                LiberarDespachosCargaGranel = CheckPermiso(397)
            };
            ViewData["permisosUsuario"] = permisosUsuario;
            return View(list);
        }

        public ActionResult Crear(int id, int idDetalleOrdenProduccion, int? IdFichaTecnica)
        {
            CheckPermisoAndRedirect(357);
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == id
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == idDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_DespachoCargaGranel despachoCargaGranel = new CAL_DespachoCargaGranel
            {
                IdOrdenProduccion        = ordenProduccion.IdOrdenProduccion,
                IdDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion
            };

            #region Resuelve Ficha Técnica

            int IdFichaTecnicaSelect = IdFichaTecnica ?? 0; string redirectToAction = "", errMsg = "";
            if (IdFichaTecnicaSelect == 0)
            {
                if (CAL_FT.ResolverFichaTecnica(detalleOrdenProduccion, out IdFichaTecnicaSelect, out redirectToAction, out errMsg))
                {
                    if (redirectToAction == "SeleccionarFichaTecnica")
                        return RedirectToAction(redirectToAction, new { id = ordenProduccion.IdOrdenProduccion, idDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion, ordenProduccion.IdCliente, detalleOrdenProduccion.IdProducto, detalleOrdenProduccion.IdSubproducto });
                }
                else
                {
                    return RedirectToAction(redirectToAction, new { errMsg, okMsg = "" });
                }
            }

            #endregion

            // Ficha técnica resuelta
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.Single(X => X.IdFichaTecnica == IdFichaTecnicaSelect);

            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == detalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            despachoCargaGranel.IdSiloSelect      = null;
            ViewData["ordenProduccion"]           = ordenProduccion;
            ViewData["detalleOrdenProduccion"]    = detalleOrdenProduccion;
            ViewData["cAL_FTParametroAnalisList"] = cAL_FTParametroAnalisList;
            ViewData["cAL_FT"]                    = cAL_FT;
            ViewData["cAL_FTControlVersion"]      = cAL_FTControlVersion;
            return View("Crear", despachoCargaGranel);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Crear(int id, int idDetalleOrdenProduccion, CAL_DespachoCargaGranel despachoCargaGranel, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(357);
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == id
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == idDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.Single(X => X.IdFichaTecnica == despachoCargaGranel.IdFichaTecnica);

            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == detalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            string errMsg = string.Empty;
            string okMsg  = string.Empty;

            despachoCargaGranel.Validate(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(despachoCargaGranel.Container))
                        despachoCargaGranel.Container = string.Empty;
                    if (string.IsNullOrEmpty(despachoCargaGranel.Observaciones))
                        despachoCargaGranel.Observaciones = string.Empty;

                    despachoCargaGranel.Reproceso       = false;
                    despachoCargaGranel.Liberado        = false;
                    despachoCargaGranel.CantidadCargada = detalleOrdenProduccion.CantidadPorContenedor.Value;
                    despachoCargaGranel.Habilitado      = true;
                    despachoCargaGranel.FechaHoraIns    = DateTime.Now;
                    despachoCargaGranel.IpIns           = RemoteAddr();
                    despachoCargaGranel.UserIns         = User.Identity.Name;
                    dcSoftwareCalidad.CAL_DespachoCargaGranel.InsertOnSubmit(despachoCargaGranel);
                    dcSoftwareCalidad.SubmitChanges();

                    foreach (CAL_FTParametroAnalisis cAL_FTParametroAnalisis in cAL_FTParametroAnalisList)
                    {
                        decimal PARAMETROANALISIS = decimal.Parse(formCollection[string.Format("PARAMETROANALISIS__{0}", cAL_FTParametroAnalisis.IdParametroAnalisis)]);

                        CAL_DespachoCargaGranelTest cAL_DespachoCargaGranelTest = new CAL_DespachoCargaGranelTest()
                        {
                            IdDespachoCargaGranel = despachoCargaGranel.IdDespachoCargaGranel,
                            IdFichaTecnica        = cAL_FT.IdFichaTecnica,
                            IdParametroAnalisis   = cAL_FTParametroAnalisis.IdParametroAnalisis,
                            Value                 = PARAMETROANALISIS,
                            Habilitado            = true,
                            UserIns               = User.Identity.Name,
                            FechaHoraIns          = DateTime.Now,
                            IpIns                 = RemoteAddr(),
                        };
                        dcSoftwareCalidad.CAL_DespachoCargaGranelTest.InsertOnSubmit(cAL_DespachoCargaGranelTest);
                    }

                    string[] ids = { };

                    if (!string.IsNullOrEmpty(formCollection["SILO__ALIMENTACION"]))
                        ids = formCollection["SILO__ALIMENTACION"].Split(',');

                    foreach (string idx in ids)
                    {
                        CAL_DespachoCargaGranelSilo despachoCargaGranelSilo = new CAL_DespachoCargaGranelSilo()
                        {
                            IdDespachoCargaGranel = despachoCargaGranel.IdDespachoCargaGranel,
                            IdSilo                = int.Parse(idx),
                            FechaHoraIns          = DateTime.Now,
                            IpIns                 = RemoteAddr(),
                            UserIns               = User.Identity.Name
                        };
                        dcSoftwareCalidad.CAL_DespachoCargaGranelSilo.InsertOnSubmit(despachoCargaGranelSilo);
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha creado el despacho" });
                }
                catch
                {
                    var rv = despachoCargaGranel.GetRuleViolations();
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

            despachoCargaGranel.IdSiloSelect      = null;
            ViewData["ordenProduccion"]           = ordenProduccion;
            ViewData["detalleOrdenProduccion"]    = detalleOrdenProduccion;
            ViewData["cAL_FTParametroAnalisList"] = cAL_FTParametroAnalisList;
            ViewData["cAL_FT"]                    = cAL_FT;
            ViewData["cAL_FTControlVersion"]      = cAL_FTControlVersion;
            return View("Crear", despachoCargaGranel);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(358);
            CAL_DespachoCargaGranel despachoCargaGranel = dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == id && X.Habilitado == true);
            if (despachoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion = despachoCargaGranel.CAL_OrdenProduccion;
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = despachoCargaGranel.CAL_DetalleOrdenProduccion;

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == despachoCargaGranel.IdFichaTecnica && X.Habilitado == true);
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

            List<CAL_DespachoCargaGranelTest> despachoCargaGranelTestList = dcSoftwareCalidad.CAL_DespachoCargaGranelTest.Where(X => X.IdDespachoCargaGranel == despachoCargaGranel.IdDespachoCargaGranel).ToList();

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            despachoCargaGranel.IdSiloSelect        = null;
            ViewData["ordenProduccion"]             = ordenProduccion;
            ViewData["detalleOrdenProduccion"]      = detalleOrdenProduccion;
            ViewData["cAL_FTParametroAnalisList"]   = cAL_FTParametroAnalisList;
            ViewData["cAL_FT"]                      = cAL_FT;
            ViewData["cAL_FTControlVersion"]        = cAL_FTControlVersion;
            ViewData["despachoCargaGranelTestList"] = despachoCargaGranelTestList;
            return View("Editar", despachoCargaGranel);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(358);
            CAL_DespachoCargaGranel despachoCargaGranel = dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == id && X.Habilitado == true);
            if (despachoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion = despachoCargaGranel.CAL_OrdenProduccion;
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = despachoCargaGranel.CAL_DetalleOrdenProduccion;

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == despachoCargaGranel.IdFichaTecnica && X.Habilitado == true);
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

            List<CAL_DespachoCargaGranelTest> despachoCargaGranelTestList = dcSoftwareCalidad.CAL_DespachoCargaGranelTest.Where(X => X.IdDespachoCargaGranel == despachoCargaGranel.IdDespachoCargaGranel).ToList();

            string errMsg = string.Empty;
            string okMsg  = string.Empty;

            despachoCargaGranel.Validate(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(despachoCargaGranel, new string[] { "NContainerDiario", "Container", "KgProductoNoConforme", "Observaciones", "IdContenedor", "Retenido" });

                    if (string.IsNullOrEmpty(despachoCargaGranel.Container))
                        despachoCargaGranel.Container = string.Empty;
                    if (string.IsNullOrEmpty(despachoCargaGranel.Observaciones))
                        despachoCargaGranel.Observaciones = string.Empty;

                    despachoCargaGranel.FechaHoraUpd = DateTime.Now;
                    despachoCargaGranel.IpUpd        = RemoteAddr();
                    despachoCargaGranel.UserUpd      = User.Identity.Name;
                    dcSoftwareCalidad.SubmitChanges();

                    foreach (CAL_FTParametroAnalisis cAL_FTParametroAnalisis in cAL_FTParametroAnalisList)
                    {
                        decimal PARAMETROANALISIS = decimal.Parse(formCollection[string.Format("PARAMETROANALISIS__{0}", cAL_FTParametroAnalisis.IdParametroAnalisis)]);

                        CAL_DespachoCargaGranelTest despachoCargaGranelTest = despachoCargaGranelTestList.SingleOrDefault(X => X.IdParametroAnalisis == cAL_FTParametroAnalisis.IdParametroAnalisis);
                        if (despachoCargaGranelTest != null)
                        {
                            despachoCargaGranelTest.Value        = PARAMETROANALISIS;
                            despachoCargaGranelTest.UserUpd      = User.Identity.Name;
                            despachoCargaGranelTest.FechaHoraUpd = DateTime.Now;
                            despachoCargaGranelTest.IpUpd        = RemoteAddr();
                            dcSoftwareCalidad.SubmitChanges();
                        }
                        else
                        {
                            despachoCargaGranelTest = new CAL_DespachoCargaGranelTest()
                            {
                                IdDespachoCargaGranel = despachoCargaGranel.IdDespachoCargaGranel,
                                IdFichaTecnica        = cAL_FT.IdFichaTecnica,
                                IdParametroAnalisis   = cAL_FTParametroAnalisis.IdParametroAnalisis,
                                Value                 = PARAMETROANALISIS,
                                Habilitado            = true,
                                UserIns               = User.Identity.Name,
                                FechaHoraIns          = DateTime.Now,
                                IpIns = RemoteAddr(),
                            };
                            dcSoftwareCalidad.CAL_DespachoCargaGranelTest.InsertOnSubmit(despachoCargaGranelTest);
                        }
                    }

                    string[] ids = { };

                    if (!string.IsNullOrEmpty(formCollection["SILO__ALIMENTACION"]))
                        ids = formCollection["SILO__ALIMENTACION"].Split(',');

                    foreach (var siloAlimentacion in despachoCargaGranel.CAL_DespachoCargaGranelSilo)
                        if (ids.SingleOrDefault(idx => idx == siloAlimentacion.IdSilo.ToString()) == null)
                            dcSoftwareCalidad.CAL_DespachoCargaGranelSilo.DeleteOnSubmit(siloAlimentacion);

                    foreach (string idx in ids)
                    {
                        if (despachoCargaGranel.CAL_DespachoCargaGranelSilo.SingleOrDefault(X => X.IdSilo == int.Parse(idx)) == null)
                        {
                            CAL_DespachoCargaGranelSilo despachoCargaGranelSilo = new CAL_DespachoCargaGranelSilo()
                            {
                                IdDespachoCargaGranel = despachoCargaGranel.IdDespachoCargaGranel,
                                IdSilo                = int.Parse(idx),
                                FechaHoraIns          = DateTime.Now,
                                IpIns                 = RemoteAddr(),
                                UserIns               = User.Identity.Name
                            };
                            dcSoftwareCalidad.CAL_DespachoCargaGranelSilo.InsertOnSubmit(despachoCargaGranelSilo);
                        }
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha editado el despacho" });
                }
                catch
                {
                    var rv = despachoCargaGranel.GetRuleViolations();
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

            despachoCargaGranel.IdSiloSelect        = null;
            ViewData["ordenProduccion"]             = ordenProduccion;
            ViewData["detalleOrdenProduccion"]      = detalleOrdenProduccion;
            ViewData["cAL_FTParametroAnalisList"]   = cAL_FTParametroAnalisList;
            ViewData["cAL_FT"]                      = cAL_FT;
            ViewData["cAL_FTControlVersion"]        = cAL_FTControlVersion;
            ViewData["despachoCargaGranelTestList"] = despachoCargaGranelTestList;
            return View("Editar", despachoCargaGranel);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(359);
            CAL_DespachoCargaGranel despachoCargaGranel = dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == id && X.Habilitado == true);
            if (despachoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho", okMsg = "" });
            }

            CAL_LECargaGranelContenedor listadeempaque = dcSoftwareCalidad.CAL_LECargaGranelContenedor.SingleOrDefault(X => X.NContenedor == despachoCargaGranel.CAL_RITContenedor.NContenedor);
            if (listadeempaque != null)
            {
                return RedirectToAction("Index", new { errMsg = "Despacho no pudo ser eliminado, verifique que la lista de empaque no contenga contenedor asociado", okMsg = "" });
            }
            string errMsg = "";
            string okMsg = "";

            try
            {
                despachoCargaGranel.Habilitado   = false;
                despachoCargaGranel.UserUpd      = User.Identity.Name;
                despachoCargaGranel.FechaHoraUpd = DateTime.Now;
                despachoCargaGranel.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El despacho {0} ha sido eliminado", despachoCargaGranel.IdDespachoCargaGranel);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }

        public ActionResult Imprimir(int id)
        {
            CheckPermisoAndRedirect(356);
            CAL_DespachoCargaGranel cAL_DespachoCargaGranel = dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == id && X.Habilitado == true);
            if (cAL_DespachoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la carga a granel", okMsg = "" });
            }

            DespachoCargaGranelViewModel despachoCargaGranelViewModel = new DespachoCargaGranelViewModel
            {
                DespachoCargaGranel    = cAL_DespachoCargaGranel,
                DetalleOrdenProduccion = cAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion,
                OrdenProduccion        = cAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion,
                Producto               = cAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion.CAL_Subproducto
            };

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == cAL_DespachoCargaGranel.IdFichaTecnica && X.Habilitado == true);
            if (cAL_FT != null)
            {
                despachoCargaGranelViewModel.FichaTecnica = cAL_FT;
            }
            else
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion != null)
            {
                despachoCargaGranelViewModel.ControlVersion = cAL_FTControlVersion;
            }
            else
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            despachoCargaGranelViewModel.DespachoCargaGranel = dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == cAL_DespachoCargaGranel.IdDespachoCargaGranel && X.Habilitado == true);
            if (despachoCargaGranelViewModel.DespachoCargaGranel != null)
            {
                despachoCargaGranelViewModel.AnalisisDespachoCargaGranelTestList = (from X in dcSoftwareCalidad.CAL_DespachoCargaGranelTest
                                                                                    join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                                    where X.IdDespachoCargaGranel == despachoCargaGranelViewModel.DespachoCargaGranel.IdDespachoCargaGranel
                                                                                    && X.CAL_FTParametroAnalisis.NoAplica == false
                                                                                    && Y.IdProducto == despachoCargaGranelViewModel.DetalleOrdenProduccion.IdProducto
                                                                                    orderby Y.Orden
                                                                                    select X).ToList();
            }
            despachoCargaGranelViewModel.ControlVersion = cAL_FTControlVersion;
            return View("Imprimir", despachoCargaGranelViewModel);
        }

        public ActionResult Liberar(int id)
        {
            CheckPermisoAndRedirect(397);
            CAL_DespachoCargaGranel despachoCargaGranel = dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == id && X.Habilitado == true);
            if (despachoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                despachoCargaGranel.Liberado             = true;
                despachoCargaGranel.UserLiberadoIns      = User.Identity.Name;
                despachoCargaGranel.FechaLiberadoHoraIns = DateTime.Now;
                despachoCargaGranel.IpLiberadoIns        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El despacho {0} ha sido liberado", despachoCargaGranel.IdDespachoCargaGranel);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }

        public ActionResult Reprocesar(int id)
        {
            CheckPermisoAndRedirect(386);
            CAL_DespachoCargaGranel despachoCargaGranel = dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == id && X.Habilitado == true);
            if (despachoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                despachoCargaGranel.Reproceso             = true;
                despachoCargaGranel.UserReprocesoIns      = User.Identity.Name;
                despachoCargaGranel.FechaHoraReprocesoIns = DateTime.Now;
                despachoCargaGranel.IpReprocesoIns        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El despacho {0} ha sido reprocesado", despachoCargaGranel.IdDespachoCargaGranel);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }

        public ActionResult VerCargas(int id)
        {
            CheckPermisoAndRedirect(356);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            ViewData["ordenProduccion"] = detalleOrdenProduccion.CAL_OrdenProduccion;
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            Permiso permisosUsuario = new Permiso()
            {
                Leer                           = CheckPermiso(356),
                Crear                          = CheckPermiso(357),
                Actualizar                     = CheckPermiso(358),
                Borrar                         = CheckPermiso(359),
                ReprocesarDespachosCargaGranel = CheckPermiso(386),
                LiberarDespachosCargaGranel    = CheckPermiso(397)
            };
            ViewData["permisosUsuario"] = permisosUsuario;
            return View("VerCargas", detalleOrdenProduccion);
        }

        public ActionResult VerDespacho(int id)
        {
            CheckPermisoAndRedirect(356);
            CAL_DespachoCargaGranel cAL_DespachoCargaGranel = dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == id && X.Habilitado == true);
            if (cAL_DespachoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la carga a granel", okMsg = "" });
            }

            DespachoCargaGranelViewModel despachoCargaGranelViewModel = new DespachoCargaGranelViewModel
            {
                DetalleOrdenProduccion = cAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion,
                OrdenProduccion        = cAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion,
                Producto               = cAL_DespachoCargaGranel.CAL_DetalleOrdenProduccion.CAL_Subproducto,
                DespachoCargaGranel    = cAL_DespachoCargaGranel
            };
            if (cAL_DespachoCargaGranel != null)
            {
                despachoCargaGranelViewModel.AnalisisDespachoCargaGranelTestList = (from X in dcSoftwareCalidad.CAL_DespachoCargaGranelTest
                                                                                    join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                                    where X.IdDespachoCargaGranel == despachoCargaGranelViewModel.DespachoCargaGranel.IdDespachoCargaGranel
                                                                                    && X.CAL_FTParametroAnalisis.NoAplica == false
                                                                                    && Y.IdProducto == despachoCargaGranelViewModel.DetalleOrdenProduccion.IdProducto
                                                                                    orderby Y.Orden
                                                                                    select X).ToList();
            }

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == cAL_DespachoCargaGranel.IdFichaTecnica && X.Habilitado == true);
            if (cAL_FT != null)
            {
                despachoCargaGranelViewModel.FichaTecnica = cAL_FT;
            }
            else
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion != null)
            {
                despachoCargaGranelViewModel.ControlVersion = cAL_FTControlVersion;
            }

            return View("VerDespacho", despachoCargaGranelViewModel);
        }

        public ActionResult VerOP()
        {
            CheckPermisoAndRedirect(356);
            List<CAL_OrdenProduccion> list = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                              where X.Habilitado
                                              && (X.Autorizado.HasValue && X.Autorizado.Value)
                                              && !X.Terminada
                                              && X.IdTipoOrdenProduccion == 2
                                              select X).ToList();
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            Permiso permisosUsuario = new Permiso()
            {
                Leer                           = CheckPermiso(356),
                Crear                          = CheckPermiso(357),
                Actualizar                     = CheckPermiso(358),
                Borrar                         = CheckPermiso(359),
                ReprocesarDespachosCargaGranel = CheckPermiso(386),
                LiberarDespachosCargaGranel    = CheckPermiso(397)
            };
            ViewData["permisosUsuario"] = permisosUsuario;
            return View("VerOP", list);
        }

        public ActionResult VerProductos(int id)
        {
            CheckPermisoAndRedirect(356);
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == id
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            List<CAL_DetalleOrdenProduccion> list = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion).OrderBy(X => X.IdDetalleOrdenProduccion).ToList();

            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            Permiso permisosUsuario = new Permiso()
            {
                Leer                           = CheckPermiso(356),
                Crear                          = CheckPermiso(357),
                Actualizar                     = CheckPermiso(358),
                Borrar                         = CheckPermiso(359),
                ReprocesarDespachosCargaGranel = CheckPermiso(386),
                LiberarDespachosCargaGranel    = CheckPermiso(397)
            };
            ViewData["permisosUsuario"] = permisosUsuario;
            return View("VerProductos", list);
        }

        public ActionResult SeleccionarFichaTecnica(int id, int idDetalleOrdenProduccion, int IdCliente, int IdProducto, int IdSubproducto)
        {
            CheckPermisoAndRedirect(328);

            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == id
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == idDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            List<CAL_FT> fts = CAL_FT.GetFichasTecnicas(IdCliente, IdProducto, IdSubproducto);

            ViewData["ordenProduccion"]        = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["errMsg"]                 = Request["errMsg"] ?? "";
            ViewData["okMsg"]                  = Request["okMsg"] ?? "";
            return View("SeleccionarFichaTecnica", fts);
        }

        [HttpPost]
        public ActionResult SeleccionarFichaTecnica(int id, int idDetalleOrdenProduccion, int IdCliente, int IdProducto, int IdSubproducto, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(328);

            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == id
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == idDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
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
                    return RedirectToAction("Crear", new { id = ordenProduccion.IdOrdenProduccion, idDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion, cAL_FT.IdFichaTecnica });
                }
            }
            else
            {
                errMsg = "Debes seleccionar una ficha técnica";
            }

            List<CAL_FT> fts = CAL_FT.GetFichasTecnicas(IdCliente, IdProducto, IdSubproducto);

            ViewData["ordenProduccion"]        = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["errMsg"]                 = errMsg;
            ViewData["okMsg"]                  = okMsg;
            return View("SeleccionarFichaTecnica", fts);
        }
    }
}