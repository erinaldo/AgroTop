using AgroFichasWeb.Models;
using AgroFichasWeb.Models.SoftwareCalidad;
using AgroFichasWeb.ViewModels.SoftwareCalidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ZXing;
using ZXing.QrCode;
using System.Drawing;
using AgroFichasWeb.Controllers.Filters;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALDespachosPalletsController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        public CALDespachosPalletsController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALDespachosPallets
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
            List<CAL_DespachoPale> cAL_DespachoPaleList = new List<CAL_DespachoPale>();
            if (IdPlantaProduccionSelect == 0)
            {
                cAL_DespachoPaleList = cAL_DespachoPaleList = (from X in dcSoftwareCalidad.CAL_DespachoPale
                                               join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                                               where X.Habilitado == true
                                               orderby X.IdPale ascending
                                               select X).ToList();
            }
            else
            {
                cAL_DespachoPaleList = cAL_DespachoPaleList = (from X in dcSoftwareCalidad.CAL_DespachoPale
                                               join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                                               where X.Habilitado == true && X.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect
                                               orderby X.IdPale ascending
                                               select X).ToList();
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"]  = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            Permiso permisosUsuario = new Permiso(CheckPermiso(357),
                                                  CheckPermiso(356),
                                                  CheckPermiso(358),
                                                  CheckPermiso(359))
            {
                CrearCargaDividida = CheckPermiso(370)
            };
            ViewData["permisosUsuario"] = permisosUsuario;
            return View(cAL_DespachoPaleList);
        }

        public ActionResult CrearCargaDividida(int IdOrdenProduccion, int idDetalleOrdenProduccion, int IdPale)
        {
            CheckPermisoAndRedirect(370);
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == IdOrdenProduccion
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

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);

            CAL_DespachoPale despachoPale = new CAL_DespachoPale
            {
                IdOrdenProduccion        = ordenProduccion.IdOrdenProduccion,
                IdDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion,
                IdPale                   = IdPale
            };

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            despachoPale.CantidadPaletizada    = pale.CantidadPaletizada;
            ViewData["ordenProduccion"]        = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["pale"]                   = pale;
            ViewData["analisisPale"]           = analisisPale;
            return View(despachoPale);
        }

        [HttpPost]
        public ActionResult CrearCargaDividida(int IdOrdenProduccion, int idDetalleOrdenProduccion, int IdPale, CAL_DespachoPale despachoPale, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(370);
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == IdOrdenProduccion
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

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            string errMsg = string.Empty;
            string okMsg  = string.Empty;

            despachoPale.Validate(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    pale.CantidadPaletizada = int.Parse(formCollection["PrimeraCarga"]);
                    pale.UserUpd            = User.Identity.Name;
                    pale.FechaHoraUpd       = DateTime.Now;
                    pale.IpUpd              = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();

                    if (!string.IsNullOrEmpty(formCollection["HiddenSegundaCarga"]))
                    {
                        #region INSERT CAL_Pale
                        CAL_Pale cAL_Pale = new CAL_Pale()
                        {
                            IdTipoPale               = pale.IdTipoPale,
                            IdDetalleOrdenProduccion = pale.IdDetalleOrdenProduccion,
                            CantidadPaletizada       = int.Parse(formCollection["HiddenSegundaCarga"]),
                            CodigoInterno            = "",
                            Habilitado               = true,
                            FechaHoraIns             = DateTime.Now,
                            IpIns                    = RemoteAddr(),
                            UserIns                  = User.Identity.Name,
                            IdGrupoEnvasador         = pale.IdGrupoEnvasador,
                            IdControlFechado         = pale.IdControlFechado,
                        };
                        dcSoftwareCalidad.CAL_Pale.InsertOnSubmit(cAL_Pale);
                        dcSoftwareCalidad.SubmitChanges();

                        #endregion

                        #region Creación QR Palé

                        string QRCodeData = string.Format("PLE_{0}_DT{1}_PL{2}_{3}_DIV_{4}", ordenProduccion.LoteComercial, detalleOrdenProduccion.IdDetalleOrdenProduccion, cAL_Pale.IdPale, cAL_Pale.CantidadPaletizada, pale.IdPale);

                        QrCodeEncodingOptions options = new QrCodeEncodingOptions()
                        {
                            DisableECI   = true,
                            CharacterSet = "UTF-8",
                            Width        = 250,
                            Height       = 250,
                        };

                        BarcodeWriter writer = new BarcodeWriter()
                        {
                            Format  = BarcodeFormat.QR_CODE,
                            Options = options
                        };

                        Bitmap QRCode = new Bitmap(writer.Write(QRCodeData));

                        Models.ImageConverter imageConverter = new Models.ImageConverter();
                        byte[] byteArrayQRCode = imageConverter.imageToByteArray(QRCode);

                        cAL_Pale.CodigoInterno = QRCodeData;
                        cAL_Pale.QR_CODE       = byteArrayQRCode;
                        dcSoftwareCalidad.SubmitChanges();

                        #endregion

                        #region INSERT CAL_AnalisisPale

                        CAL_AnalisisPale cAL_AnalisisPale = new CAL_AnalisisPale()
                        {
                            IdOrdenProduccion        = analisisPale.IdOrdenProduccion,
                            IdDetalleOrdenProduccion = analisisPale.IdDetalleOrdenProduccion,
                            IdPale                   = cAL_Pale.IdPale,
                            FechaEtiquetado          = analisisPale.FechaEtiquetado,
                            MuestraEspesor1          = analisisPale.MuestraEspesor1,
                            MuestraEspesor2          = analisisPale.MuestraEspesor2,
                            MuestraEspesor3          = analisisPale.MuestraEspesor3,
                            MuestraEspesor4          = analisisPale.MuestraEspesor4,
                            MuestraEspesor5          = analisisPale.MuestraEspesor5,
                            MuestraEspesor6          = analisisPale.MuestraEspesor6,
                            MuestraEspesor7          = analisisPale.MuestraEspesor7,
                            MuestraEspesor8          = analisisPale.MuestraEspesor8,
                            MuestraEspesor9          = analisisPale.MuestraEspesor9,
                            MuestraEspesor10         = analisisPale.MuestraEspesor10,
                            PromedioMuestraEspesor   = analisisPale.PromedioMuestraEspesor,
                            SacosDetectorMetales     = analisisPale.SacosDetectorMetales,
                            Observaciones            = analisisPale.Observaciones,
                            Retenido                 = analisisPale.Retenido,
                            Habilitado               = true,
                            FechaHoraIns             = DateTime.Now,
                            IpIns                    = RemoteAddr(),
                            UserIns                  = User.Identity.Name,
                            IdFichaTecnica           = analisisPale.IdFichaTecnica
                        };
                        dcSoftwareCalidad.CAL_AnalisisPale.InsertOnSubmit(cAL_AnalisisPale);
                        dcSoftwareCalidad.SubmitChanges();

                        #endregion

                        #region INSERT CAL_AnalisisPaleTest

                        List<CAL_AnalisisPaleTest> cAL_AnalisisPaleTestList = (from X in dcSoftwareCalidad.CAL_AnalisisPaleTest
                                                                               where X.IdAnalisisPale == analisisPale.IdAnalisisPale
                                                                               orderby X.IdParametroAnalisis ascending
                                                                               select X).ToList();

                        foreach (CAL_AnalisisPaleTest analisisPaleTest in cAL_AnalisisPaleTestList)
                        {
                            CAL_AnalisisPaleTest cAL_AnalisisPaleTest = new CAL_AnalisisPaleTest()
                            {
                                IdAnalisisPale      = cAL_AnalisisPale.IdAnalisisPale,
                                IdFichaTecnica      = analisisPaleTest.IdFichaTecnica,
                                IdParametroAnalisis = analisisPaleTest.IdParametroAnalisis,
                                Value               = analisisPaleTest.Value,
                                Habilitado          = true,
                                UserIns             = User.Identity.Name,
                                FechaHoraIns        = DateTime.Now,
                                IpIns               = RemoteAddr(),
                            };
                            dcSoftwareCalidad.CAL_AnalisisPaleTest.InsertOnSubmit(cAL_AnalisisPaleTest);
                        }
                        dcSoftwareCalidad.SubmitChanges();

                        #endregion
                    }

                    return RedirectToAction("VerProductos", new { id = IdOrdenProduccion, errMsg = "", okMsg = "Se ha creado la división de carga" });
                }
                catch
                {
                    var rv = despachoPale.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            despachoPale.CantidadPaletizada    = pale.CantidadPaletizada;
            ViewData["ordenProduccion"]        = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["analisisPale"]           = analisisPale;
            ViewData["pale"]                   = pale;
            return RedirectToAction("CrearCargaDividida", new { IdOrdenProduccion, idDetalleOrdenProduccion, IdPale });
        }

        public ActionResult CrearDespachoPallet(int IdOrdenProduccion, int idDetalleOrdenProduccion, int IdPale)
        {
            CheckPermisoAndRedirect(357);
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == IdOrdenProduccion
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

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);

            CAL_DespachoPale despachopale = dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);
            if (despachopale != null)
            {
                return RedirectToAction("QRDespachoPallet", new { errMsg = "PALLET YA FUE DESPACHADO, No se puede volver a despachar", okMsg = "" });

            }

            CAL_DespachoPale despachoPallets = new CAL_DespachoPale
            {
                IdOrdenProduccion        = ordenProduccion.IdOrdenProduccion,
                IdDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion,
                IdPale                   = IdPale
            };

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            ViewData["ordenProduccion"]        = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["pale"]                   = pale;
            ViewData["analisisPale"]           = analisisPale;
            return View("CrearDespachoPallet", despachoPallets);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CrearDespachoPallet(int IdOrdenProduccion, int idDetalleOrdenProduccion, int IdPale, CAL_DespachoPale despachoPale, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(357);
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == IdOrdenProduccion
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

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            CAL_DespachoPale despachopale = dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);
            if (despachopale != null)
            {
                return RedirectToAction("QRDespachoPallet", new { errMsg = "PALLET YA FUE DESPACHADO, No se puede volver a despachar", okMsg = "" });

            }

            string errMsg = string.Empty;
            string okMsg  = string.Empty;

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(despachoPale.Observaciones))
                        despachoPale.Observaciones = string.Empty;

                    despachoPale.SacosDañados = int.Parse(formCollection["SacosDañados"]);
                    despachoPale.SacosTotales = int.Parse(formCollection["SacosTotales"]);
                    despachoPale.Habilitado   = true;
                    despachoPale.FechaHoraIns = DateTime.Now;
                    despachoPale.IpIns        = RemoteAddr();
                    despachoPale.UserIns      = User.Identity.Name;
                    dcSoftwareCalidad.CAL_DespachoPale.InsertOnSubmit(despachoPale);
                    dcSoftwareCalidad.SubmitChanges();

                    CAL_PosicionContenedorYalero posicionContenedorYalero = dcSoftwareCalidad.CAL_PosicionContenedorYalero.SingleOrDefault(X => X.IdContenedor == int.Parse(formCollection["IdContenedor"]));
                    if (!string.IsNullOrEmpty(formCollection["posicionContenedor"]) && posicionContenedorYalero == null)
                    {
                        CAL_PosicionContenedorYalero cAL_PosicionContenedorYalero = new CAL_PosicionContenedorYalero()
                        {
                            IdContenedor = int.Parse(formCollection["IdContenedor"]),
                            IdPosicion   = int.Parse(formCollection["posicionContenedor"]),
                            FechaHoraIns = DateTime.Now,
                            IpIns        = RemoteAddr(),
                            UserIns      = CurrentUser.UserName
                        };
                        dcSoftwareCalidad.CAL_PosicionContenedorYalero.InsertOnSubmit(cAL_PosicionContenedorYalero);
                        dcSoftwareCalidad.SubmitChanges();
                    }

                    return RedirectToAction("QRDespachoPallet", new { id = IdOrdenProduccion, errMsg = "", okMsg = "Se ha creado el despacho" });
                }
                catch
                {
                    var rv = despachoPale.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["ordenProduccion"]        = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["analisisPale"]           = analisisPale;
            ViewData["pale"]                   = pale;
            return View("CrearDespachoPallet", despachoPale);
        }

        public ActionResult EditarDespachoPallet(int id)
        {
            CheckPermisoAndRedirect(358);

            CAL_DespachoPale despachoPale = dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdDespachoPale == id && X.Habilitado == true);
            if (despachoPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho pallet", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == despachoPale.IdOrdenProduccion
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == despachoPale.IdDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == despachoPale.IdPale && X.Habilitado == true);

            CAL_DespachoPale despachoPallets = new CAL_DespachoPale
            {
                IdOrdenProduccion = ordenProduccion.IdOrdenProduccion,
                IdDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion,
                IdPale = despachoPale.IdPale
            };

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == despachoPale.IdPale && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["pale"] = pale;
            ViewData["analisisPale"] = analisisPale;
            return View("CrearDespachoPallet", despachoPale);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditarDespachoPallet(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(358);
            CAL_DespachoPale despachoPale = dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdDespachoPale == id && X.Habilitado == true);
            if (despachoPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho pallet", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                   where X.IdOrdenProduccion == despachoPale.IdOrdenProduccion
                                                   && X.Habilitado
                                                   && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                   && !X.Terminada
                                                   select X).SingleOrDefault();
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == despachoPale.IdDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == despachoPale.IdPale && X.Habilitado == true);

            CAL_DespachoPale despachoPallets = new CAL_DespachoPale
            {
                IdOrdenProduccion = ordenProduccion.IdOrdenProduccion,
                IdDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion,
                IdPale = despachoPale.IdPale
            };

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == despachoPale.IdPale && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            string errMsg = string.Empty;
            string okMsg  = string.Empty;

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(despachoPale, new string[] { "SacosDañados", "SacosTotales", "Observaciones", "IdContenedor" });

                    if (string.IsNullOrEmpty(despachoPale.Observaciones))
                        despachoPale.Observaciones = string.Empty;

                    despachoPale.FechaHoraUpd = DateTime.Now;
                    despachoPale.IpUpd        = RemoteAddr();
                    despachoPale.UserUpd      = User.Identity.Name;
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha editado el despacho" });
                }
                catch
                {
                    var rv = despachoPale.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            ViewData["pale"] = pale;
            ViewData["analisisPale"] = analisisPale;
            return View("CrearDespachoPallet", despachoPale);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(359);
            CAL_DespachoPale despachoPale = dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdDespachoPale == id && X.Habilitado == true);
            if (despachoPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho pallet", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                despachoPale.Habilitado   = false;
                despachoPale.UserUpd      = User.Identity.Name;
                despachoPale.FechaHoraUpd = DateTime.Now;
                despachoPale.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El despacho {0} ha sido eliminado", despachoPale.IdDespachoPale);
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
            CAL_DespachoPale despachoPale = dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdDespachoPale == id && X.Habilitado == true);
            if (despachoPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho pallet", okMsg = "" });
            }
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == despachoPale.IdOrdenProduccion && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == despachoPale.IdDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == despachoPale.IdPale && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            List<CAL_AnalisisPaleTest> analisisPaleTestList = dcSoftwareCalidad.CAL_AnalisisPaleTest.Where(X => X.IdAnalisisPale == analisisPale.IdAnalisisPale).ToList();

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == analisisPale.IdFichaTecnica && X.Habilitado == true);
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

            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == detalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            ViewData["ordenProduccion"]           = ordenProduccion;
            ViewData["detalleOrdenProduccion"]    = detalleOrdenProduccion;
            ViewData["cAL_FT"]                    = cAL_FT;
            ViewData["cAL_FTControlVersion"]      = cAL_FTControlVersion;
            ViewData["cAL_FTParametroAnalisList"] = cAL_FTParametroAnalisList;
            ViewData["analisisPaleTestList"]      = analisisPaleTestList;
            ViewData["analisisPale"]              = analisisPale;
            ViewData["errMsg"]                    = Request["errMsg"];
            ViewData["okMsg"]                     = Request["okMsg"];
            ViewData["permisosUsuario"]           = new Permiso(CheckPermiso(357),
                                                                CheckPermiso(356),
                                                                CheckPermiso(358),
                                                                CheckPermiso(359));
            return View("Imprimir", despachoPale);
        }

        public ActionResult QRCargaDivididaPallet()
        {
            CheckPermisoAndRedirect(370);
            CALIdentificacionPaleDespacho qrDespacho = new CALIdentificacionPaleDespacho();
            return View(qrDespacho);
        }

        [HttpPost]
        public ActionResult QRCargaDivididaPallet(CALIdentificacionPaleDespacho cALIdentificacionPaleDespacho)
        {
            CheckPermisoAndRedirect(370);

            string errMsg = "";

            if (cALIdentificacionPaleDespacho.ValidacionIdentificacionPale(ModelState, System.Web.HttpContext.Current))
            {
                PaleViewModel paleViewModel = new PaleViewModel();
                CAL_Pale cAL_Pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.CodigoInterno == cALIdentificacionPaleDespacho.QRCode);
                if (cAL_Pale != null)
                {
                    paleViewModel.Pale                   = cAL_Pale;
                    paleViewModel.DetalleOrdenProduccion = cAL_Pale.CAL_DetalleOrdenProduccion;
                    paleViewModel.OrdenProduccion        = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;

                    ViewData["permisosUsuario"] = new Permiso()
                    {
                        Crear = CheckPermiso(357)
                    };
                    return RedirectToAction("CrearCargaDividida", new { paleViewModel.OrdenProduccion.IdOrdenProduccion, idDetalleOrdenProduccion = paleViewModel.DetalleOrdenProduccion.IdDetalleOrdenProduccion, paleViewModel.Pale.IdPale });
                }
                else
                {
                    errMsg = "No se encuentra el pallet con QR-Code escaneado";
                    ModelState.AddModelError("QRCode", errMsg);
                }
            }

            return View(cALIdentificacionPaleDespacho);
        }

        public ActionResult QRDespachoPallet()
        {
            CheckPermisoAndRedirect(356);
            CALIdentificacionPaleDespacho qrDespacho = new CALIdentificacionPaleDespacho();
            return View(qrDespacho);
        }

        [HttpPost]
        public ActionResult QRDespachoPallet(CALIdentificacionPaleDespacho cALIdentificacionPaleDespacho)
        {
            CheckPermisoAndRedirect(356);

            string errMsg = "";

            if (cALIdentificacionPaleDespacho.ValidacionIdentificacionPale(ModelState, System.Web.HttpContext.Current))
            {
                PaleViewModel paleViewModel = new PaleViewModel();
                CAL_Pale cAL_Pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.CodigoInterno == cALIdentificacionPaleDespacho.QRCode);
                if (cAL_Pale != null)
                {
                    paleViewModel.Pale                   = cAL_Pale;
                    paleViewModel.DetalleOrdenProduccion = cAL_Pale.CAL_DetalleOrdenProduccion;
                    paleViewModel.OrdenProduccion        = cAL_Pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;

                    ViewData["permisosUsuario"] = new Permiso()
                    {
                        Crear = CheckPermiso(357)
                    };
                    return RedirectToAction("CrearDespachoPallet", new { paleViewModel.OrdenProduccion.IdOrdenProduccion, idDetalleOrdenProduccion = paleViewModel.DetalleOrdenProduccion.IdDetalleOrdenProduccion, paleViewModel.Pale.IdPale });
                }
                else
                {
                    errMsg = "No se encuentra el pallet con QR-Code escaneado";
                    ModelState.AddModelError("QRCode", errMsg);
                }
            }

            return View(cALIdentificacionPaleDespacho);
        }

        public ActionResult VerDespacho(int id)
        {
            CheckPermisoAndRedirect(356);
            CAL_DespachoPale despachoPale = dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdDespachoPale == id && X.Habilitado == true);
            if (despachoPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el despacho pallet", okMsg = "" });
            }
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == despachoPale.IdOrdenProduccion && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == despachoPale.IdDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == despachoPale.IdPale && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el análisis del pallet", okMsg = "" });
            }

            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == analisisPale.IdFichaTecnica && X.Habilitado == true);
            if (cAL_FT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            List<CAL_AnalisisPaleTest> analisisPaleTestList = dcSoftwareCalidad.CAL_AnalisisPaleTest.Where(X => X.IdAnalisisPale == analisisPale.IdAnalisisPale).ToList();

            CAL_FTControlVersion cAL_FTControlVersion = dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == cAL_FT.IdFichaTecnica).OrderByDescending(X => X.Version).FirstOrDefault();
            if (cAL_FTControlVersion == null)
            {
                cAL_FTControlVersion = new CAL_FTControlVersion()
                {
                    Version = 0
                };
            }

            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == detalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            ViewData["ordenProduccion"]           = ordenProduccion;
            ViewData["detalleOrdenProduccion"]    = detalleOrdenProduccion;
            ViewData["cAL_FT"]                    = cAL_FT;
            ViewData["cAL_FTControlVersion"]      = cAL_FTControlVersion;
            ViewData["cAL_FTParametroAnalisList"] = cAL_FTParametroAnalisList;
            ViewData["analisisPaleTestList"]      = analisisPaleTestList;
            ViewData["analisisPale"]              = analisisPale;
            ViewData["errMsg"]                    = Request["errMsg"];
            ViewData["okMsg"]                     = Request["okMsg"];
            ViewData["permisosUsuario"]           = new Permiso(CheckPermiso(357),
                                                                CheckPermiso(356),
                                                                CheckPermiso(358),
                                                                CheckPermiso(359));
            return View("VerDespacho", despachoPale);
        }

        public ActionResult VerOP()
        {
            CheckPermisoAndRedirect(356);
            DespachoPalletsViewModel despachoPalletsViewModel = new DespachoPalletsViewModel
            {
                AnalisisCompleto = dcSoftwareCalidad.CAL_GetOrdenProduccionAnalisisCompleto().ToList()
            };
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(357),
                                                      CheckPermiso(356),
                                                      CheckPermiso(358),
                                                      CheckPermiso(359));
            return View("VerOP", despachoPalletsViewModel);
        }

        public ActionResult VerPallets(int id)
        {
            CheckPermisoAndRedirect(356);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            detalleOrdenProduccion.AvanceCargaPallet = dcSoftwareCalidad.CAL_GetPorductoAvanceCargaPale(detalleOrdenProduccion.IdDetalleOrdenProduccion).ToList();

            ViewData["ordenProduccion"] = detalleOrdenProduccion.CAL_OrdenProduccion;
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            Permiso permisosUsuario = new Permiso(CheckPermiso(357),
                                                  CheckPermiso(356),
                                                  CheckPermiso(358),
                                                  CheckPermiso(359))
            {
                CrearAnalisisPallet = CheckPermiso(357),
                VerAnalisisPale     = CheckPermiso(356),
                VerCargaDividida    = CheckPermiso(369),
                CrearCargaDividida  = CheckPermiso(370)
            };
            ViewData["permisosUsuario"] = permisosUsuario;
            return View("VerPallets", detalleOrdenProduccion);
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
            DespachoPalletsViewModel despachoPalletsViewModel = new DespachoPalletsViewModel
            {
                AvanceCargaPallet                = dcSoftwareCalidad.CAL_GetPorductosAvanceCargaPale(id).ToList(),
                ContenedoresAvanceCargaPale      = dcSoftwareCalidad.CAL_GetAvanceCargaPalletsPorContenedor(id).ToList(),
                ContenedoresAvanceCargaPaleTotal = dcSoftwareCalidad.CAL_GetAvanceCargaTotalPalletsPorContenedor(id).ToList()
            };

            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            Permiso permisosUsuario = new Permiso(CheckPermiso(357),
                                                  CheckPermiso(356),
                                                  CheckPermiso(358),
                                                  CheckPermiso(359))
            {
                CrearCargaDividida = CheckPermiso(370)
            };
            ViewData["permisosUsuario"] = permisosUsuario;
            return View("VerProductos", despachoPalletsViewModel);
        }
    }
}