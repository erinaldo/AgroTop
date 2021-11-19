using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ZXing;
using ZXing.QrCode;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALPaleController : BaseApplicationController
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALPaleController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            //304 Ver pallets
            //305 Crear pallet
            //306 Editar pallet
            //307 Eliminar pallet
            CheckPermisoAndRedirect(304);
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
            List<CAL_OrdenProduccion> list = new List<CAL_OrdenProduccion>();
            if (IdPlantaProduccionSelect == 0)
            {
                list = dcSoftwareCalidad.CAL_OrdenProduccion.Where(X => X.IdTipoOrdenProduccion == 1 && X.Autorizado.Value && X.Habilitado == true).ToList();
            }
            else
            {
                list = dcSoftwareCalidad.CAL_OrdenProduccion.Where(X => X.IdTipoOrdenProduccion == 1 && X.Autorizado.Value && X.Habilitado == true && X.IdPlanta == IdPlantaProduccionSelect).ToList();
            }

            ViewData["PlantasProduccion"] = plantas;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            
            Permiso permisosUsuario = new Permiso(CheckPermiso(305), CheckPermiso(304), CheckPermiso(306), CheckPermiso(307));
            permisosUsuario.CrearAnalisisPallet = CheckPermiso(328);
            ViewData["permisosUsuario"] = permisosUsuario;
            return View(list);
        }

        public ActionResult CrearPale(int id, int idDetalleOrdenProduccion)
        {
            CheckPermisoAndRedirect(305);

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

            // Resuelve turno actual en base a la jornada activa
            CAL_Turno turno = ResolveTurno3(dcSoftwareCalidad);
            CAL_GetTurno2AnteriorSiguienteResult AnteriorSiguiente = dcSoftwareCalidad.CAL_GetTurno2AnteriorSiguiente(DateTime.Now).SingleOrDefault();
            CAL_Turno2 turno2 = dcSoftwareCalidad.CAL_Turno2.SingleOrDefault(X => X.IdTurno == AnteriorSiguiente.TAct && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);

            // Trae el último grupo envasador registrado en el turno actual
            CAL_GrupoEnvasador grupoEnvasador = dcSoftwareCalidad.CAL_GrupoEnvasador.FirstOrDefault(X => X.FechaHoraIns.Date == DateTime.Now.Date && X.IdTurno2 == turno2.IdTurno && X.Habilitado == true && X.IdPlanta == ordenProduccion.IdPlanta);
            if (grupoEnvasador == null)
            {
                return RedirectToAction("RegistrarEnvasadoresTurno", new { id, idDetalleOrdenProduccion });
            }

            CAL_Pale pale = new CAL_Pale
            {
                IdGrupoEnvasador = grupoEnvasador.IdGrupoEnvasador
            };

            // Recordando el último pallet creado asociado al detalle del producto para agilizar su creación
            // Esto pensado que crearán 30-40 pallets al inicio del turno
            CAL_Pale ultimoPale = dcSoftwareCalidad.CAL_Pale.Where(X => X.IdDetalleOrdenProduccion == detalleOrdenProduccion.IdDetalleOrdenProduccion).OrderByDescending(X => X.IdPale).FirstOrDefault();
            if (ultimoPale != null)
            {
                pale.UltimoPallet_IdTipoPale = ultimoPale.IdTipoPale;
                pale.UltimoPallet_CantidadPaletizada = ultimoPale.CantidadPaletizada;
                pale.UltimoPallet_IdControlFechado = ultimoPale.IdControlFechado.Value;
                pale.UltimoPallet_CntMax = ultimoPale.CAL_TipoPale.CntMax;
                pale.UltimoPallet_CntPallet = (int)Math.Ceiling((double)ultimoPale.CantidadPaletizada / (double)ultimoPale.CAL_TipoPale.CntMax);
            }
            else
            {
                pale.UltimoPallet_IdTipoPale = 0;
                pale.UltimoPallet_CantidadPaletizada = 0;
                pale.UltimoPallet_IdControlFechado = 0;
                pale.UltimoPallet_CntMax = 0;
                pale.UltimoPallet_CntPallet = 0;
            }
            ViewData["errMsg"] = Request["errMsg"] ?? "";
            ViewData["okMsg"] = Request["okMsg"] ?? "";
            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            return View("CrearPale", pale);
        }

        [HttpPost]
        public ActionResult CrearPale(int id, int idDetalleOrdenProduccion, CAL_Pale pale, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(305);
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
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

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(formCollection["CntPallets"]) == false && int.TryParse(formCollection["CntPallets"], out int CntPallets))
                    {
                        for (int i = 0; i < pale.CntPallets; i++)
                        {
                            CAL_Pale pale2 = new CAL_Pale();

                            pale2.CantidadPaletizada = int.Parse(formCollection["CantidadPaletizada"]);
                            pale2.IdTipoPale = int.Parse(formCollection["IdTipoPale"]);
                            pale2.IdControlFechado = int.Parse(formCollection["IdControlFechado"]);
                            pale2.IdGrupoEnvasador = int.Parse(formCollection["IdGrupoEnvasador"]);
                            pale2.IdDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion;
                            pale2.CodigoInterno = "";
                            pale2.Habilitado = true;
                            pale2.FechaHoraIns = DateTime.Now;
                            pale2.IpIns = RemoteAddr();
                            pale2.UserIns = User.Identity.Name;
                            pale2.CntMax = pale.CntMax;
                            pale2.CntPallets = pale.CntPallets;
                            dcSoftwareCalidad.CAL_Pale.InsertOnSubmit(pale2);
                            dcSoftwareCalidad.SubmitChanges();

                            #region Creación QR Palé

                            string QRCodeData = string.Format("PLE_{0}_DT{1}_PL{2}_{3}", ordenProduccion.LoteComercial, detalleOrdenProduccion.IdDetalleOrdenProduccion, pale2.IdPale, pale2.CantidadPaletizada);

                            QrCodeEncodingOptions options = new QrCodeEncodingOptions()
                            {
                                DisableECI = true,
                                CharacterSet = "UTF-8",
                                Width = 250,
                                Height = 250,
                            };

                            BarcodeWriter writer = new BarcodeWriter()
                            {
                                Format = BarcodeFormat.QR_CODE,
                                Options = options
                            };

                            Bitmap QRCode = new Bitmap(writer.Write(QRCodeData));

                            Models.ImageConverter imageConverter = new Models.ImageConverter();
                            byte[] byteArrayQRCode = imageConverter.imageToByteArray(QRCode);

                            #endregion

                            pale2.CodigoInterno = QRCodeData;
                            pale2.QR_CODE = byteArrayQRCode;
                            dcSoftwareCalidad.SubmitChanges();
                        }
                        return RedirectToAction("Ver", new { id = ordenProduccion.IdOrdenProduccion });
                    }
                    else
                    {
                        errMsg = "Debes seleccionar cantidad de pallet entre 1, 5 o 10";
                    }
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
            ViewData["errMsg"] = errMsg;
            ViewData["okMsg"] = okMsg;
            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["detalleOrdenProduccion"] = detalleOrdenProduccion;
            return View("CrearPale", pale);
        }

        public ActionResult EditarPallet(int id)
        {
            CheckPermisoAndRedirect(306);
            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            pale.CntMax = pale.CAL_TipoPale.CntMax;
            return View("EditarPallet", pale);
        }

        [HttpPost]
        public ActionResult EditarPallet(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(306);

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(pale, new string[] { "CantidadPaletizada", "CntMax" });

                    pale.UserUpd = User.Identity.Name;
                    pale.FechaHoraUpd = DateTime.Now;
                    pale.IpUpd = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("VerPales", new { id = pale.IdDetalleOrdenProduccion, errMsg = "", okMsg = "Se ha editado el pallet" });
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

            return View("EditarPallet", pale);
        }

        public ActionResult Eliminar(int id, int IdOrdenProduccion)
        {
            CheckPermisoAndRedirect(307);
            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                pale.CntMax = pale.CAL_TipoPale.CntMax;
                pale.Habilitado = false;
                pale.UserUpd = User.Identity.Name;
                pale.FechaHoraUpd = DateTime.Now;
                pale.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El pallet {0} ha sido eliminado", pale.IdPale);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Ver", new { id = IdOrdenProduccion, errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult ImprimirEtiqueta(int id)
        {
            CheckPermisoAndRedirect(304);
            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (pale == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el pallet", okMsg = "" });
            }

            return View("ImprimirEtiqueta", pale);
        }

        public ActionResult ImprimirEtiquetas(int id)
        {
            CheckPermisoAndRedirect(304);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            List<CAL_Pale> pales = dcSoftwareCalidad.CAL_Pale.Where(X => X.IdDetalleOrdenProduccion == id && X.Habilitado == true).ToList();

            return View("ImprimirEtiquetas", pales);
        }

        public ActionResult RegistrarEnvasadoresTurno(int id, int idDetalleOrdenProduccion)
        {
            CheckPermisoAndRedirect(305);

            // Resuelve turno actual en base a la jornada activa
            CAL_Turno turno = ResolveTurno3(dcSoftwareCalidad);
            CAL_GetTurno2AnteriorSiguienteResult AnteriorSiguiente = dcSoftwareCalidad.CAL_GetTurno2AnteriorSiguiente(DateTime.Now).SingleOrDefault();
            CAL_Turno2 turno2 = dcSoftwareCalidad.CAL_Turno2.SingleOrDefault(X => X.IdTurno == AnteriorSiguiente.TAct && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);

            CAL_GrupoEnvasador grupoEnvasador = new CAL_GrupoEnvasador
            {
                CAL_Turno = turno,
                CAL_Turno2 = turno2
            };
            return View("RegistrarEnvasadoresTurno", grupoEnvasador);
        }

        [HttpPost]
        public ActionResult RegistrarEnvasadoresTurno(int id, int idDetalleOrdenProduccion, CAL_GrupoEnvasador grupoEnvasador, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(305);

            // Resuelve turno actual en base a la jornada activa
            CAL_Turno turno = ResolveTurno3(dcSoftwareCalidad);
            CAL_GetTurno2AnteriorSiguienteResult AnteriorSiguiente = dcSoftwareCalidad.CAL_GetTurno2AnteriorSiguiente(DateTime.Now).SingleOrDefault();
            CAL_Turno2 turno2 = dcSoftwareCalidad.CAL_Turno2.SingleOrDefault(X => X.IdTurno == AnteriorSiguiente.TAct && X.CAL_TipoTurno.CAL_TipoJornada.JornadaActiva);

            if (grupoEnvasador.ValidacionEnvasadores(ModelState, System.Web.HttpContext.Current))
            {
                var detalle = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(X => X.IdDetalleOrdenProduccion == idDetalleOrdenProduccion).FirstOrDefault();
                CAL_GrupoEnvasador grupoEnvasador1 = new CAL_GrupoEnvasador()
                {
                    IdTurno = turno.IdTurno,
                    IdTurno2 = AnteriorSiguiente.TAct,
                    IdTipoGrupoEnvasador = 1,
                    Nombre = string.Format("{0:dd/MM/yyyy} {1}", DateTime.Now, turno2.CAL_TipoTurno.Descripcion),
                    Habilitado = true,
                    UserIns = User.Identity.Name,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr(),
                    IdPlanta = detalle.CAL_OrdenProduccion.IdPlanta
                };
                dcSoftwareCalidad.CAL_GrupoEnvasador.InsertOnSubmit(grupoEnvasador1);
                dcSoftwareCalidad.SubmitChanges();

                List<SYS_User> users = new List<SYS_User>();

                string[] ids = formCollection["chkEnvasador"].Split(new char[] { ',' }).ToArray<string>();
                foreach (var idx in ids)
                {
                    SYS_User sYS_User = dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserID == idx.ToInt(0));
                    if (sYS_User != null)
                    {
                        CAL_Envasador envasador = new CAL_Envasador()
                        {
                            IdGrupoEnvasador = grupoEnvasador1.IdGrupoEnvasador,
                            UserID = sYS_User.UserID,
                            Habilitado = true,
                            UserIns = User.Identity.Name,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        };
                        dcSoftwareCalidad.CAL_Envasador.InsertOnSubmit(envasador);
                    }

                    users.Add(sYS_User);
                }

                grupoEnvasador1.IdTipoGrupoEnvasador = grupoEnvasador1.GetTipoEnvasadores(users);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("CrearPale", new { id, idDetalleOrdenProduccion });
            }

            grupoEnvasador.CAL_Turno = turno;
            return View("RegistrarEnvasadoresTurno", grupoEnvasador);
        }

        public ActionResult Ver(int id)
        {
            CheckPermisoAndRedirect(304);
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            List<CAL_DetalleOrdenProduccion> list = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion).OrderBy(X => X.IdDetalleOrdenProduccion).ToList();

            Permiso permisosUsuario = new Permiso(CheckPermiso(305), CheckPermiso(304), CheckPermiso(306), CheckPermiso(307));
            permisosUsuario.CrearAnalisisPallet = CheckPermiso(328);

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = permisosUsuario;
            ViewData["ordenProduccion"] = ordenProduccion;
            return View("VerProductos", list);
        }

        public ActionResult AdministrarPallets(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(304);

            //List<CAL_DetalleOrdenProduccion> list = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion).OrderBy(X => X.IdDetalleOrdenProduccion).ToList();


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



            List<CAL_DetalleOrdenProduccion> list = (from detalle in dcSoftwareCalidad.CAL_DetalleOrdenProduccion
                                                     join orden in dcSoftwareCalidad.CAL_OrdenProduccion on detalle.IdOrdenProduccion equals orden.IdOrdenProduccion
                                                     where orden.IdTipoOrdenProduccion == 1
                                                     && orden.Autorizado.Value 
                                                     && orden.Habilitado == true 
                                                     && orden.IdPlanta == IdPlantaProduccionSelect
                                                     select detalle).OrderBy(CD => CD.CAL_OrdenProduccion.LoteComercial).ToList();

            Permiso permisosUsuario = new Permiso(CheckPermiso(305), CheckPermiso(304), CheckPermiso(306), CheckPermiso(307));
            permisosUsuario.CrearAnalisisPallet = CheckPermiso(328);

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = permisosUsuario;
            ViewData["PlantasProduccion"] = plantas;

            return View(list);
        }

        public ActionResult VerPales(int id)
        {
            CheckPermisoAndRedirect(304);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            Permiso permisosUsuario = new Permiso(CheckPermiso(305), CheckPermiso(304), CheckPermiso(306), CheckPermiso(307));
            permisosUsuario.CrearAnalisisPallet = CheckPermiso(328);
            permisosUsuario.Actualizar = CheckPermiso(329);
            permisosUsuario.VerAnalisisPale = CheckPermiso(327);

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = permisosUsuario;
            ViewData["ordenProduccion"] = detalleOrdenProduccion.CAL_OrdenProduccion;
            return View("VerPales", detalleOrdenProduccion);
        }
    }
}