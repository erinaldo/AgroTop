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
using MoreLinq;
using AgroFichasWeb.Controllers.Filters;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALSacosDañadosController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        public CALSacosDañadosController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALSacosDañados
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(398);//Ver Sacos Dañados

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

            

            SacosDañadosViewModel SacosDañadosViewModel = new SacosDañadosViewModel()
            {
                SacosDañados = dcSoftwareCalidad.CAL_GetLoteSacosDañados().Where(X => X.IdPlanta == IdPlantaProduccionSelect).ToList(),
                DetalleSacosDañados = dcSoftwareCalidad.CAL_GetDetalleLoteSacosDañados().Where(X => X.IdPlantaProduccion == IdPlantaProduccionSelect).ToList()
            };

            ViewData["permisosUsuario"] = new Permiso()
            {
                VerSacosDañados = CheckPermiso(398),//Ver Sacos Dañados
                VerReprocesoSacosDañados = CheckPermiso(399),//Ver Reproceso Sacos Dañados
                CrearReprocesoSacosDañados = CheckPermiso(400)//Crear Reproceso Sacos Dañados
            };
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            return View(SacosDañadosViewModel);
        }

        public ActionResult CrearReprocesoSacosDañados(int id)
        {
            CheckPermisoAndRedirect(400);//Crear Reproceso Sacos Dañados
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_ReprocesoSacosDañados reprocesoSacosDañados = new CAL_ReprocesoSacosDañados()
            {
                IdOrdenProduccionOrigen = ordenProduccion.IdOrdenProduccion
            };

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            return View("CrearReprocesoSacosDañados", reprocesoSacosDañados);
        }

        [HttpPost]
        public ActionResult CrearReprocesoSacosDañados(int id, CAL_ReprocesoSacosDañados reprocesoSacosDañados, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(400);//Crear Reproceso Sacos Dañados
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    reprocesoSacosDañados.IdOrdenProduccionDestino = reprocesoSacosDañados.IdOrdenProduccionReproceso;
                    reprocesoSacosDañados.IdOrdenProduccionOrigen = ordenProduccion.IdOrdenProduccion;
                    reprocesoSacosDañados.Habilitado = true;
                    reprocesoSacosDañados.FechaHoraIns = DateTime.Now;
                    reprocesoSacosDañados.IpIns = RemoteAddr();
                    reprocesoSacosDañados.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_ReprocesoSacosDañados.InsertOnSubmit(reprocesoSacosDañados);
                    dcSoftwareCalidad.SubmitChanges();

                    foreach (string SacoDañado in reprocesoSacosDañados.SacosDañados)
                    {
                        string[] X = SacoDañado.Split(new char[] { ',' });
                        if (X.Length == 2)
                        {
                            var ParseOK = true;
                            if (!int.TryParse(X[0], out int IdPale)) { ParseOK = false; }
                            if (!int.TryParse(X[1], out int SacosDañados)) { ParseOK = false; }
                            if (ParseOK)
                            {
                                CAL_ReprocesoSacosDañadosPallets reprocesoSacosDañadosPallets = new CAL_ReprocesoSacosDañadosPallets()
                                {
                                    IdReprocesoSacosDañados = reprocesoSacosDañados.IdReprocesoSacosDañados,
                                    IdPale = IdPale,
                                    SacosDañados = SacosDañados,
                                    SacosUtilizados = 0,
                                    SacosDisponibles = SacosDañados,
                                    Habilitado = true,
                                    FechaHoraIns = DateTime.Now,
                                    IpIns = RemoteAddr(),
                                    UserIns = User.Identity.Name,
                                };
                                dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPallets.InsertOnSubmit(reprocesoSacosDañadosPallets);
                            }
                            else
                            {
                                return RedirectToAction("Index", "CALSacosDañados", new { errMsg = "Ha ocurrido un error mientras se procesaban sus sacos dañados seleccionados", okMsg = "" });
                            }
                        }
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", "CALSacosDañados", new { errMsg = "", okMsg = "Se ha creado el reproceso para revisión" });
                }
                catch
                {
                    var rv = reprocesoSacosDañados.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            reprocesoSacosDañados.IdOrdenProduccionOrigen = ordenProduccion.IdOrdenProduccion;
            reprocesoSacosDañados.IdOrdenProduccionReproceso = 0;
            reprocesoSacosDañados.SacosDañados = new List<string>();
            reprocesoSacosDañados.TotalSacosDañados = 0;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            return View("CrearReprocesoSacosDañados", reprocesoSacosDañados);
        }

        public ActionResult VerReprocesoSacosDañados(int? id)
        {
            CheckPermisoAndRedirect(399);//Ver Reproceso Sacos Dañados
            List<CAL_ReprocesoSacosDañados> list = dcSoftwareCalidad.CAL_ReprocesoSacosDañados.Where(X => X.IdOrdenProduccionOrigen == id && X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            Permiso permisosUsuario = new Permiso(CheckPermiso(400),//Crear Reproceso Sacos Dañados
                                                  CheckPermiso(399),//Ver Reproceso Sacos Dañados
                                                  CheckPermiso(401),//Editar Reproceso Sacos Dañados
                                                  CheckPermiso(402));//Eliminar Reproceso Sacos Dañados
            permisosUsuario.AutorizarReprocesoSacosDañados = CheckPermiso(403);//Autorizar Reproceso Sacos Dañados
            permisosUsuario.PaletizarSacosDañadosReprocesados = CheckPermiso(404);//Paletizar Sacos Dañados Reprocesados
            ViewData["permisosUsuario"] = permisosUsuario;
            return View(list);
        }

        public ActionResult Autorizar(int id)
        {
            CheckPermisoAndRedirect(403);
            CAL_ReprocesoSacosDañados reprocesoSacosDañados = dcSoftwareCalidad.CAL_ReprocesoSacosDañados.SingleOrDefault(X => X.IdReprocesoSacosDañados == id && X.Habilitado == true);
            if (reprocesoSacosDañados == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                reprocesoSacosDañados.Autorizado = true;
                reprocesoSacosDañados.UserAutoriza = User.Identity.Name;
                reprocesoSacosDañados.FechaHoraAutoriza = DateTime.Now;
                reprocesoSacosDañados.IpAutoriza = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();

                okMsg = String.Format("El reproceso {0} ha sido autorizado", reprocesoSacosDañados.IdReprocesoSacosDañados);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("VerReprocesoSacosDañados", new { id = reprocesoSacosDañados.IdOrdenProduccionOrigen, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Paletizar(int id)
        {
            CheckPermisoAndRedirect(404);
            CAL_ReprocesoSacosDañados reprocesoSacosDañados = dcSoftwareCalidad.CAL_ReprocesoSacosDañados.SingleOrDefault(X => X.IdReprocesoSacosDañados == id && X.Habilitado == true);
            if (reprocesoSacosDañados == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }

            List<CAL_ReprocesoSacosDañadosPallets> list = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPallets.Where(X => X.IdReprocesoSacosDañados == reprocesoSacosDañados.IdReprocesoSacosDañados && X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            Permiso permisosUsuario = new Permiso();
            permisosUsuario.PaletizarSacosDañadosReprocesados = CheckPermiso(404);//Paletizar Sacos Dañados Reprocesados
            ViewData["permisosUsuario"] = permisosUsuario;
            return View(list);
        }

        public ActionResult CrearPallet(int id)
        {
            CheckPermisoAndRedirect(404);
            CAL_ReprocesoSacosDañadosPallets reprocesoSacosDañadosPallets = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPallets.SingleOrDefault(X => X.IdReprocesoSacosDañadosPallets == id && X.Habilitado == true);
            if (reprocesoSacosDañadosPallets == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }
            CAL_Pale pale = new CAL_Pale();
            ViewData["reprocesoSacosDañadosPallets"] = reprocesoSacosDañadosPallets;
            return View(pale);
        }

        [HttpPost]
        public ActionResult CrearPallet(int id, CAL_Pale nuevo_pallet, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(404);
            var pallet_de_origen = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPallets.SingleOrDefault(X => X.IdReprocesoSacosDañadosPallets == id && X.Habilitado == true);
            if (pallet_de_origen == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (pallet_de_origen.HaySacosDisponibles.Value)
                    {
                        if (nuevo_pallet.CntCargada > pallet_de_origen.SacosDisponibles)
                        {
                            return RedirectToAction("Paletizar", new { id = pallet_de_origen.IdReprocesoSacosDañados, errMsg = "La cantidad cargada no debe superar a la cantidad disponible", okMsg = "" });
                        }
                        else
                        {
                            nuevo_pallet.IdControlFechado           = 1;
                            nuevo_pallet.IdGrupoEnvasador           = null;
                            nuevo_pallet.IdDetalleOrdenProduccion   = pallet_de_origen.CAL_ReprocesoSacosDañados.CAL_DetalleOrdenProduccion.IdDetalleOrdenProduccion;
                            nuevo_pallet.CantidadPaletizada         = nuevo_pallet.CntCargada;
                            nuevo_pallet.CodigoInterno              = "";
                            nuevo_pallet.Habilitado                 = true;
                            nuevo_pallet.FechaHoraIns               = DateTime.Now;
                            nuevo_pallet.IpIns                      = RemoteAddr();
                            nuevo_pallet.UserIns                    = User.Identity.Name;
                            nuevo_pallet.VieneReprocesoSacosDañados = true;
                            dcSoftwareCalidad.CAL_Pale.InsertOnSubmit(nuevo_pallet);
                            dcSoftwareCalidad.SubmitChanges();

                            #region Creación QR Pallet

                            string QRCodeData = string.Format("PLE_{0}_DT{1}_PL{2}_{3}_REP_SCO_DANADOS", pallet_de_origen.CAL_ReprocesoSacosDañados.CAL_OrdenProduccion1.LoteComercial, pallet_de_origen.CAL_ReprocesoSacosDañados.CAL_DetalleOrdenProduccion.IdDetalleOrdenProduccion, nuevo_pallet.IdPale, nuevo_pallet.CntMax);

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

                            nuevo_pallet.CodigoInterno = QRCodeData;
                            nuevo_pallet.QR_CODE       = byteArrayQRCode;
                            dcSoftwareCalidad.SubmitChanges();

                            #endregion

                            var detalle = new CAL_ReprocesoSacosDañadosPalletsDetalle()
                            {
                                IdReprocesoSacosDañadosPallets = pallet_de_origen.IdReprocesoSacosDañadosPallets,
                                IdPale                         = nuevo_pallet.IdPale,
                                CntCargada                     = nuevo_pallet.CntCargada,
                                CntMax                         = nuevo_pallet.CAL_TipoPale.CntMax,
                                CntDisponible                  = (nuevo_pallet.CntMax - nuevo_pallet.CntCargada),
                                Habilitado                     = true,
                                FechaHoraIns                   = DateTime.Now,
                                IpIns                          = RemoteAddr(),
                                UserIns                        = User.Identity.Name,
                            };
                            dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPalletsDetalle.InsertOnSubmit(detalle);
                            dcSoftwareCalidad.SubmitChanges();

                            pallet_de_origen.SacosDisponibles = pallet_de_origen.SacosDisponibles - nuevo_pallet.CntCargada;
                            pallet_de_origen.SacosUtilizados  = pallet_de_origen.SacosUtilizados + nuevo_pallet.CntCargada;
                            dcSoftwareCalidad.SubmitChanges();

                            return RedirectToAction("Paletizar", new { id = pallet_de_origen.IdReprocesoSacosDañados, errMsg = "", okMsg = "" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Paletizar", new { id = pallet_de_origen.IdReprocesoSacosDañados, errMsg = "No hay sacos dañados disponibles para paletizar", okMsg = "" });
                    }
                }
                catch
                {
                    var rv = nuevo_pallet.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View(nuevo_pallet);
        }

        public ActionResult UsarPalletExistente(int id)
        {
            CheckPermisoAndRedirect(404);
            CAL_ReprocesoSacosDañadosPallets reprocesoSacosDañadosPallets = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPallets.SingleOrDefault(X => X.IdReprocesoSacosDañadosPallets == id && X.Habilitado == true);
            if (reprocesoSacosDañadosPallets == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }

            List<CAL_Pale> pallets = (from X in dcSoftwareCalidad.CAL_Pale
                                      join Y in dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPalletsDetalle on X.IdPale equals Y.IdPale
                                      where X.CAL_DetalleOrdenProduccion.IdSubproducto == reprocesoSacosDañadosPallets.CAL_Pale.CAL_DetalleOrdenProduccion.IdSubproducto
                                      && (X.VieneReprocesoSacosDañados.HasValue && X.VieneReprocesoSacosDañados.Value)
                                      && X.Habilitado == true
                                      && !Y.Lleno.Value
                                      select X).DistinctBy(X => X.IdPale).ToList();

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            Permiso permisosUsuario = new Permiso();
            permisosUsuario.PaletizarSacosDañadosReprocesados = CheckPermiso(404);//Paletizar Sacos Dañados Reprocesados
            ViewData["permisosUsuario"] = permisosUsuario;
            ViewData["reprocesoSacosDañadosPallets"] = reprocesoSacosDañadosPallets;
            return View(pallets);
        }

        public ActionResult UsarEstePallet(int id, int IdReprocesoSacosDañadosPallets)
        {
            CheckPermisoAndRedirect(404);
            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            CAL_ReprocesoSacosDañadosPallets reprocesoSacosDañadosPallets = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPallets.SingleOrDefault(X => X.IdReprocesoSacosDañadosPallets == IdReprocesoSacosDañadosPallets && X.Habilitado == true);
            if (reprocesoSacosDañadosPallets == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }

            pale.IdReprocesoSacosDañadosPallets = reprocesoSacosDañadosPallets.IdReprocesoSacosDañadosPallets;
            pale.CntMax = pale.CAL_TipoPale.CntMax;
            ViewData["reprocesoSacosDañadosPallets"] = reprocesoSacosDañadosPallets;
            return View(pale);
        }

        [HttpPost]
        public ActionResult UsarEstePallet(int id, int IdReprocesoSacosDañadosPallets, CAL_Pale pallet_existente, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(404);
            CAL_ReprocesoSacosDañadosPallets pallet_de_origen = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPallets.SingleOrDefault(X => X.IdReprocesoSacosDañadosPallets == IdReprocesoSacosDañadosPallets && X.Habilitado == true);
            if (pallet_de_origen == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }

            pallet_existente.ValidateUsarEstePallet(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    if (pallet_de_origen.HaySacosDisponibles.Value)
                    {
                        int cnt_disponible = 0;
                        List<CAL_ReprocesoSacosDañadosPalletsDetalle> detalles = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPalletsDetalle.Where(X => X.IdPale == pallet_existente.IdPale).ToList();
                        if (detalles.Count > 0)
                            cnt_disponible = detalles.Last().CntDisponible;

                        var detalle = new CAL_ReprocesoSacosDañadosPalletsDetalle()
                        {
                            IdReprocesoSacosDañadosPallets = pallet_de_origen.IdReprocesoSacosDañadosPallets,
                            IdPale = pallet_existente.IdPale,
                            CntCargada = pallet_existente.CntCargada,
                            CntMax = pallet_existente.CntMax,
                            CntDisponible = (cnt_disponible - pallet_existente.CntCargada),
                            Habilitado = true,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name,
                        };
                        dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPalletsDetalle.InsertOnSubmit(detalle);

                        pallet_de_origen.SacosDisponibles = pallet_de_origen.SacosDisponibles - pallet_existente.CntCargada;
                        pallet_de_origen.SacosUtilizados = pallet_de_origen.SacosUtilizados + pallet_existente.CntCargada;

                        CAL_Pale cAL_Pale1 = dcSoftwareCalidad.CAL_Pale.Single(X => X.IdPale == pallet_existente.IdPale);
                        cAL_Pale1.CntCargada = pallet_existente.CntCargada;
                        cAL_Pale1.CntMax = pallet_existente.CntMax;
                        cAL_Pale1.CantidadPaletizada += pallet_existente.CntCargada;
                        dcSoftwareCalidad.SubmitChanges();

                        return RedirectToAction("Paletizar", new { id = pallet_de_origen.IdReprocesoSacosDañados, errMsg = "", okMsg = "" });
                    }
                    else
                    {
                        return RedirectToAction("Paletizar", new { id = pallet_de_origen.IdReprocesoSacosDañados, errMsg = "No hay sacos dañados disponibles para paletizar", okMsg = "" });
                    }
                }
                catch
                {
                    var rv = pallet_existente.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            CAL_Pale cAL_Pale = dcSoftwareCalidad.CAL_Pale.Single(X => X.IdPale == pallet_existente.IdPale);
            pallet_existente.CodigoInterno = cAL_Pale.CodigoInterno;
            pallet_existente.CntMax = cAL_Pale.CAL_TipoPale.CntMax;
            pallet_existente.IdPale = pallet_de_origen.IdReprocesoSacosDañadosPallets;
            pallet_existente.IdReprocesoSacosDañadosPallets = pallet_de_origen.IdReprocesoSacosDañadosPallets;
            ViewData["reprocesoSacosDañadosPallets"] = pallet_de_origen;
            return View(pallet_existente);
        }

        public ActionResult VerPaletizado(int id)
        {
            CheckPermisoAndRedirect(404);
            CAL_ReprocesoSacosDañadosPallets reprocesoSacosDañadosPallets = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPallets.SingleOrDefault(X => X.IdReprocesoSacosDañadosPallets == id && X.Habilitado == true);
            if (reprocesoSacosDañadosPallets == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }

            List<CAL_ReprocesoSacosDañadosPalletsDetalle> list = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPalletsDetalle.Where(X => X.IdReprocesoSacosDañadosPallets == reprocesoSacosDañadosPallets.IdReprocesoSacosDañadosPallets && X.Habilitado == true).ToList();
            ViewData["reprocesoSacosDañadosPallets"] = reprocesoSacosDañadosPallets;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            Permiso permisosUsuario = new Permiso();
            permisosUsuario.PaletizarSacosDañadosReprocesados = CheckPermiso(404);//Paletizar Sacos Dañados Reprocesados
            ViewData["permisosUsuario"] = permisosUsuario;
            return View(list);
        }

        public ActionResult VerHistorialPallet(int id)
        {
            CheckPermisoAndRedirect(404);
            CAL_Pale cAL_Pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (cAL_Pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            List<CAL_ReprocesoSacosDañadosPalletsDetalle> list = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPalletsDetalle.Where(X => X.IdPale == cAL_Pale.IdPale && X.Habilitado == true).ToList();

            Permiso permisosUsuario = new Permiso();
            permisosUsuario.PaletizarSacosDañadosReprocesados = CheckPermiso(404);//Paletizar Sacos Dañados Reprocesados

            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = permisosUsuario;
            ViewData["cAL_Pale"]        = cAL_Pale;
            return View(list);
        }
    }
}