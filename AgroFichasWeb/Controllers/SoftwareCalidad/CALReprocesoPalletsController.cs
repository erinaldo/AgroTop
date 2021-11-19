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
    public class CALReprocesoPalletsController : BaseApplicationController
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private OperacionesDBDataContext dcOperaciones = new OperacionesDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALReprocesoPalletsController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(373);

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
            List<CAL_ReprocesoPallets> list = new List<CAL_ReprocesoPallets>();

            if (IdPlantaProduccionSelect == 0)
            {
                list = (from X in dcSoftwareCalidad.CAL_ReprocesoPallets
                        join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                        where X.Habilitado == true
                        orderby X.IdPale ascending
                        select X).ToList();
            }
            else
            {
                list = (from X in dcSoftwareCalidad.CAL_ReprocesoPallets
                        join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                        where X.Habilitado == true && X.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect
                        orderby X.IdPale ascending
                        select X).ToList();
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            Permiso permisosUsuario = new Permiso(CheckPermiso(374),
                                                      CheckPermiso(373),
                                                      CheckPermiso(375),
                                                      CheckPermiso(376));

            permisosUsuario.AutorizarReproceso = CheckPermiso(377);
            ViewData["permisosUsuario"] = permisosUsuario;
            return View(list);
        }

        public ActionResult CrearReproceso(int id)
        {
            CheckPermisoAndRedirect(374);

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdAnalisisPale == id && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", "CALAnalisisPallets", new { errMsg = "No es un pallet retenido", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == analisisPale.IdOrdenProduccion);
            Cliente cliente = dcAgroFichas.Cliente.SingleOrDefault(X => X.IdCliente == ordenProduccion.IdCliente);
            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == analisisPale.IdPale && X.Habilitado == true);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == pale.IdDetalleOrdenProduccion);

            CAL_ReprocesoPallets reproceso = new CAL_ReprocesoPallets();
            reproceso.IdPale = analisisPale.IdPale;
            reproceso.IdOrdenProduccion = analisisPale.IdOrdenProduccion;
            reproceso.IdCliente = cliente.IdCliente;
            reproceso.IdSubproducto = detalleOrdenProduccion.IdSubproducto;
            reproceso.CantidadPaletizada = pale.CantidadPaletizada;
            reproceso.IdDetalleOrdenProduccion = detalleOrdenProduccion.IdDetalleOrdenProduccion;
            reproceso.IdSubproductoOrigen = detalleOrdenProduccion.IdSubproducto;
            ViewData["plantaProduccion"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == ordenProduccion.IdPlanta).FirstOrDefault();
            return View("CrearReproceso", reproceso);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CrearReproceso(int id, CAL_ReprocesoPallets reproceso, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(374);

            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdAnalisisPale == id && X.Habilitado == true);
            if (analisisPale == null)
            {
                return RedirectToAction("Index", "CALAnalisisPallets", new { errMsg = "No es un pallet retenido", okMsg = "" });
            }

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == analisisPale.IdOrdenProduccion);
            Cliente cliente = dcAgroFichas.Cliente.SingleOrDefault(X => X.IdCliente == ordenProduccion.IdCliente);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion && X.IdSubproducto == reproceso.IdSubproductoOrigen);
            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == analisisPale.IdPale && X.Habilitado == true);

            reproceso.IdPale = analisisPale.IdPale;
            reproceso.IdOrdenProduccion = analisisPale.IdOrdenProduccion;
            reproceso.IdCliente = cliente.IdCliente;
            reproceso.IdSubproducto = detalleOrdenProduccion.IdSubproducto;
            reproceso.CantidadPaletizada = pale.CantidadPaletizada;

            string errMsg = string.Empty;
            string okMsg = string.Empty;

            //reproceso.Validate(ModelState, formCollection);
            if (!(ordenProduccion.IdOrdenProduccion == int.Parse(formCollection["op"]) && cliente.IdCliente == int.Parse(formCollection["idCliente"]) && detalleOrdenProduccion.IdSubproducto == int.Parse(formCollection["subproducto"])))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(reproceso.Observaciones))
                            reproceso.Observaciones = string.Empty;
                        reproceso.IdOrdenProduccion = int.Parse(formCollection["op"]);
                        reproceso.IdCliente = int.Parse(formCollection["idCliente"]);
                        reproceso.IdSubproducto = int.Parse(formCollection["subproducto"]);
                        reproceso.IdDetalleOrdenProduccion = int.Parse(formCollection["detalleOP"]);
                        reproceso.Habilitado = true;
                        reproceso.FechaHoraIns = DateTime.Now;
                        reproceso.IpIns = RemoteAddr();
                        reproceso.UserIns = User.Identity.Name;
                        dcSoftwareCalidad.CAL_ReprocesoPallets.InsertOnSubmit(reproceso);
                        dcSoftwareCalidad.SubmitChanges();

                        return RedirectToAction("Index", "CALAnalisisPallets", new { errMsg = "", okMsg = "Se ha creado el reproceso para revisión" });
                    }
                    catch
                    {
                        var rv = reproceso.GetRuleViolations();
                        if (rv.Count() > 0)
                            ModelState.AddRuleViolations(rv);
                        else
                            throw;
                    }
                }

            }
            else
            {
                return RedirectToAction("Index", "CALAnalisisPallets", new { errMsg = "El reporceso no puede ser igual al pallet", okMsg = "" });
            }

            return View("CrearReproceso", reproceso);
        }

        public ActionResult VerReproceso(int id)
        {
            CheckPermisoAndRedirect(373);

            CAL_ReprocesoPallets reproceso = dcSoftwareCalidad.CAL_ReprocesoPallets.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            if (reproceso == null)
            {
                return RedirectToAction("Index", "CALReprocesoPallets", new { errMsg = "El pallet no existe", okMsg = "" });
            }

            ReprocesoViewModel reprocesoViewModel = new ReprocesoViewModel();

            reprocesoViewModel.CALPale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == id && X.Habilitado == true);
            reprocesoViewModel.CALDetalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == reprocesoViewModel.CALPale.IdDetalleOrdenProduccion);
            reprocesoViewModel.CALOrdenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == reprocesoViewModel.CALDetalleOrdenProduccion.IdOrdenProduccion);
            reprocesoViewModel.Cliente = dcAgroFichas.Cliente.SingleOrDefault(X => X.IdCliente == reprocesoViewModel.CALOrdenProduccion.IdCliente);

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];

            Permiso permisosUsuario = new Permiso(CheckPermiso(374),
                                                      CheckPermiso(373),
                                                      CheckPermiso(375),
                                                      CheckPermiso(376));

            permisosUsuario.AutorizarReproceso = CheckPermiso(377);
            ViewData["permisosUsuario"] = permisosUsuario;
            ViewData["reproceso"] = reproceso;
            return View("VerReproceso", reprocesoViewModel);
        }

        public ActionResult Autorizar(int id)
        {
            CheckPermisoAndRedirect(377);

            CAL_ReprocesoPallets reproceso = dcSoftwareCalidad.CAL_ReprocesoPallets.SingleOrDefault(X => X.IdReprocesoPalet == id && X.Habilitado == true);
            if (reproceso == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el reproceso", okMsg = "" });
            }

            CAL_Pale pale = dcSoftwareCalidad.CAL_Pale.SingleOrDefault(X => X.IdPale == reproceso.IdPale && X.Habilitado == true);

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == reproceso.IdOrdenProduccion && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == pale.IdDetalleOrdenProduccion);
            if (detalleOrdenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                reproceso.Autorizado        = true;
                reproceso.UserAutoriza      = User.Identity.Name;
                reproceso.FechaHoraAutoriza = DateTime.Now;
                reproceso.IpAutoriza        = RemoteAddr();
                reproceso.UserUpd           = User.Identity.Name;
                reproceso.FechaHoraUpd      = DateTime.Now;
                reproceso.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();

                #region INSERT CAL_Pale

                CAL_Pale cAL_Pale = new CAL_Pale()
                {
                    IdTipoPale               = pale.IdTipoPale,
                    IdDetalleOrdenProduccion = reproceso.IdDetalleOrdenProduccion,
                    CantidadPaletizada       = pale.CantidadPaletizada,
                    CodigoInterno            = "",
                    Habilitado               = true,
                    FechaHoraIns             = DateTime.Now,
                    IpIns                    = RemoteAddr(),
                    UserIns                  = User.Identity.Name,
                    IdGrupoEnvasador         = pale.IdGrupoEnvasador,
                    IdControlFechado         = pale.IdControlFechado
                };
                dcSoftwareCalidad.CAL_Pale.InsertOnSubmit(cAL_Pale);
                dcSoftwareCalidad.SubmitChanges();

                #endregion

                #region Creación QR Palé

                string QRCodeData = string.Format("PLE_{0}_DT{1}_PL{2}_{3}_REP_{4}", ordenProduccion.LoteComercial, reproceso.IdDetalleOrdenProduccion, cAL_Pale.IdPale, cAL_Pale.CantidadPaletizada, reproceso.IdPale);

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

                okMsg = String.Format("El reproceso {0} del pallet {1} ha sido autorizada", reproceso.IdReprocesoPalet, reproceso.IdPale);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}