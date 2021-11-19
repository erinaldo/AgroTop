using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFichasTecnicasController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFichasTecnicasController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFichasTecnicas
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(323);
            List<CAL_FT> list           = dcSoftwareCalidad.CAL_FT.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Crear                   = CheckPermiso(324),
                Leer                    = CheckPermiso(323),
                Actualizar              = CheckPermiso(325),
                Borrar                  = CheckPermiso(326),
                AdminControlVersiones   = CheckPermiso(331),
                AdminDctos              = CheckPermiso(339),
                AdminFrecuenciaAnalisis = CheckPermiso(385)
            };
            return View(list);
        }

        public ActionResult CrearPaso1()
        {
            CheckPermisoAndRedirect(324);
            CAL_FT ft = new CAL_FT();
            return View("CrearPaso1", ft);
        }

        [HttpPost]
        public ActionResult CrearPaso1(CAL_FT ft)
        {
            CheckPermisoAndRedirect(324);
            if (ModelState.IsValid)
            {
                try
                {
                    CAL_Producto cAL_Producto = dcSoftwareCalidad.CAL_Producto.SingleOrDefault(X => X.IdProducto == ft.IdProducto && X.Habilitado == true);
                    if (cAL_Producto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado la familia de productos", okMsg = "" }); }

                    CAL_Subproducto cAL_Subproducto = dcSoftwareCalidad.CAL_Subproducto.SingleOrDefault(X => X.IdProducto == cAL_Producto.IdProducto && X.IdSubproducto == ft.IdSubproducto && X.Habilitado == true);
                    if (cAL_Subproducto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el subproducto", okMsg = "" }); }

                    return RedirectToAction("CrearPaso2", new { cAL_Producto.IdProducto, IdSubproducto = cAL_Subproducto.IdSubproducto });
                }
                catch
                {
                    var rv = ft.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearPaso1", ft);
        }

        public ActionResult CrearPaso2(int IdProducto, int IdSubproducto)
        {
            CheckPermisoAndRedirect(324);

            CAL_Producto cAL_Producto = dcSoftwareCalidad.CAL_Producto.SingleOrDefault(X => X.IdProducto == IdProducto && X.Habilitado == true);
            if (cAL_Producto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado la familia de productos", okMsg = "" }); }

            CAL_Subproducto cAL_Subproducto = dcSoftwareCalidad.CAL_Subproducto.SingleOrDefault(X => X.IdProducto == cAL_Producto.IdProducto && X.IdSubproducto == IdSubproducto && X.Habilitado == true);
            if (cAL_Subproducto == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el subproducto", okMsg = "" }); }

            CAL_FT ft = new CAL_FT
            {
                IdProducto      = cAL_Producto.IdProducto,
                CAL_Producto    = cAL_Producto,
                IdSubproducto   = cAL_Subproducto.IdSubproducto,
                CAL_Subproducto = cAL_Subproducto
            };

            List<CAL_ParametroAnalisis> list = (from X in dcSoftwareCalidad.CAL_ParametroAnalisisProducto
                                                where X.IdProducto == cAL_Producto.IdProducto
                                                && X.CAL_ParametroAnalisis.Habilitado == true
                                                orderby X.Orden
                                                select X.CAL_ParametroAnalisis).ToList();
            ViewData["list"] = list;
            return View("CrearPaso2", ft);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CrearPaso2(CAL_FT ft, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(324);
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(ft.Observacion))
                        ft.Observacion = string.Empty;

                    ft.VerificacionCliente = false;
                    ft.Version             = 0;
                    ft.Habilitado          = true;
                    ft.FechaHoraIns        = DateTime.Now;
                    ft.IpIns               = RemoteAddr();
                    ft.UserIns             = User.Identity.Name;
                    dcSoftwareCalidad.CAL_FT.InsertOnSubmit(ft);
                    dcSoftwareCalidad.SubmitChanges();

                    if (!string.IsNullOrEmpty(formCollection["NEWID"]) && !ft.Granel)
                    {
                        string[] NEWIDs = formCollection["NEWID"].Split(new char[] { ',' });
                        foreach (string NEWID in NEWIDs)
                        {
                            string Sacos       = formCollection[string.Format("Sacos_{0}",       NEWID)] ?? "";
                            string pesoSaco    = formCollection[string.Format("pesoSaco_{0}",    NEWID)] ?? "";
                            string ctrlFechado = formCollection[string.Format("ctrlFechado_{0}", NEWID)] ?? "";
                            string hilo        = formCollection[string.Format("hilo_{0}",        NEWID)] ?? "";

                            CAL_FTSaco cAL_FTSaco = dcSoftwareCalidad.CAL_FTSaco.SingleOrDefault(X => X.IdFichaTecnica == ft.IdFichaTecnica && X.NEWID == NEWID);
                            if (cAL_FTSaco == null)
                            {
                                cAL_FTSaco = new CAL_FTSaco()
                                {
                                    IdFichaTecnica   = ft.IdFichaTecnica,
                                    IdSaco           = int.Parse(Sacos),
                                    Peso             = decimal.Parse(pesoSaco),
                                    IdControlFechado = int.Parse(ctrlFechado),
                                    ColorHilo        = hilo,
                                    FechaHoraIns     = DateTime.Now,
                                    IpIns            = RemoteAddr(),
                                    UserIns          = CurrentUser.UserName,
                                    NEWID            = NEWID
                                };
                                dcSoftwareCalidad.CAL_FTSaco.InsertOnSubmit(cAL_FTSaco);
                                dcSoftwareCalidad.SubmitChanges();
                            }
                        }
                    }

                    if (ft.Granel)
                    {
                        List<CAL_FTSaco> sacoList = dcSoftwareCalidad.CAL_FTSaco.Where(X => X.IdFichaTecnica == ft.IdFichaTecnica).ToList();

                        foreach (var saco in sacoList)
                            dcSoftwareCalidad.CAL_FTSaco.DeleteOnSubmit(saco);

                        dcSoftwareCalidad.SubmitChanges();
                    }

                    List<CAL_ParametroAnalisis> parametroAnalisisList = (from X in dcSoftwareCalidad.CAL_ParametroAnalisisProducto
                                                                         where X.IdProducto == ft.IdProducto
                                                                         orderby X.Orden
                                                                         select X.CAL_ParametroAnalisis).ToList();

                    foreach (CAL_ParametroAnalisis parametroAnalisis in parametroAnalisisList)
                    {
                        if (!string.IsNullOrEmpty(formCollection[string.Format("PARAM_min_{0}", parametroAnalisis.IdParametroAnalisis)]) &&
                            !string.IsNullOrEmpty(formCollection[string.Format("PARAM_max_{0}", parametroAnalisis.IdParametroAnalisis)]))
                        {
                            if ((decimal.TryParse(formCollection[string.Format("PARAM_min_{0}", parametroAnalisis.IdParametroAnalisis)], out decimal PARAM_min)) &&
                                (decimal.TryParse(formCollection[string.Format("PARAM_max_{0}", parametroAnalisis.IdParametroAnalisis)], out decimal PARAM_max)))
                            {
                                CAL_FTParametroAnalisis cAL_FTParametroAnalisis = new CAL_FTParametroAnalisis()
                                {
                                    IdFichaTecnica      = ft.IdFichaTecnica,
                                    IdParametroAnalisis = parametroAnalisis.IdParametroAnalisis,
                                    MinValidValue       = PARAM_min,
                                    MaxValidValue       = PARAM_max,
                                    MinAutValue         = 0,
                                    MaxAutValue         = 0,
                                    AccionAutValue      = "",
                                    NoAplica            = bool.Parse(formCollection[string.Format("HID_NA_{0}", parametroAnalisis.IdParametroAnalisis)]),
                                    FechaHoraIns        = DateTime.Now,
                                    IpIns               = RemoteAddr(),
                                    UserIns             = CurrentUser.UserName
                                };
                                dcSoftwareCalidad.CAL_FTParametroAnalisis.InsertOnSubmit(cAL_FTParametroAnalisis);
                                dcSoftwareCalidad.SubmitChanges();
                            }
                        }
                    }
                    ft.NotificarCreacion();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = ft.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ft.CAL_Producto                  = dcSoftwareCalidad.CAL_Producto.Single(X => X.IdProducto == ft.IdProducto);
            ft.CAL_Subproducto               = dcSoftwareCalidad.CAL_Subproducto.Single(X => X.IdSubproducto == ft.IdSubproducto);
            List<CAL_ParametroAnalisis> list = (from X in dcSoftwareCalidad.CAL_ParametroAnalisisProducto
                                                where X.IdProducto == ft.IdProducto
                                                orderby X.Orden
                                                select X.CAL_ParametroAnalisis).ToList();
            ViewData["list"] = list;
            return View("CrearPaso2", ft);
        }

        public ActionResult Ver(int id)
        {
            CheckPermisoAndRedirect(323);
            CAL_FT fichaTecnica = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (fichaTecnica == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            return View(fichaTecnica);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(325);

            CAL_FT fichaTecnica = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (fichaTecnica == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            List<CAL_ParametroAnalisis> list = (from X in dcSoftwareCalidad.CAL_ParametroAnalisisProducto
                                                where X.IdProducto == fichaTecnica.IdProducto
                                                orderby X.Orden
                                                select X.CAL_ParametroAnalisis).ToList();
            ViewData["list"]   = list;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"]  = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Crear                 = CheckPermiso(324),
                Leer                  = CheckPermiso(323),
                Actualizar            = CheckPermiso(325),
                Borrar                = CheckPermiso(326),
                AdminControlVersiones = CheckPermiso(331)
            };
            return View("Editar", fichaTecnica);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(325);

            CAL_FT fichaTecnica = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (fichaTecnica == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            try
            {
                UpdateModel(fichaTecnica, new string[] { "Codigo", "IdCliente", "IdProducto", "IdSubproducto", "PaisCodigo", "Sag", "Fumigacion", "PesoTotalPickingTest", "Granel", "Observacion", "VerificacionCliente", "VidaUtil", "IdTemperatura", "HumedadRelativa" });

                if (string.IsNullOrEmpty(fichaTecnica.Observacion))
                    fichaTecnica.Observacion = string.Empty;

                fichaTecnica.Codigo       = fichaTecnica.Codigo.ToUpper();
                fichaTecnica.Activa       = false;
                fichaTecnica.UserUpd      = User.Identity.Name;
                fichaTecnica.FechaHoraUpd = DateTime.Now;
                fichaTecnica.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();

                string errMsg = "";

                //SACOS
                if (!string.IsNullOrEmpty(formCollection["NEWID"]) && !fichaTecnica.Granel)
                {
                    List<CAL_FTSaco> sacoList = dcSoftwareCalidad.CAL_FTSaco.Where(X => X.IdFichaTecnica == fichaTecnica.IdFichaTecnica).ToList();

                    string[] NEWIDs = { };

                    if (formCollection["NEWID"] != null && formCollection["NEWID"] != "")
                        NEWIDs = formCollection["NEWID"].Split(',');

                    foreach (var saco in sacoList)
                        if (NEWIDs.SingleOrDefault(idx => idx == saco.NEWID) == null)
                            dcSoftwareCalidad.CAL_FTSaco.DeleteOnSubmit(saco);

                    dcSoftwareCalidad.SubmitChanges();

                    foreach (string NEWID in NEWIDs)
                    {
                        string Sacos       = formCollection[string.Format("Sacos_{0}", NEWID)] ?? "";
                        string pesoSaco    = formCollection[string.Format("pesoSaco_{0}", NEWID)] ?? "";
                        string ctrlFechado = formCollection[string.Format("ctrlFechado_{0}", NEWID)] ?? "";
                        string hilo        = formCollection[string.Format("hilo_{0}", NEWID)] ?? "";

                        CAL_FTSaco cAL_FTSaco = dcSoftwareCalidad.CAL_FTSaco.SingleOrDefault(X => X.IdFichaTecnica == fichaTecnica.IdFichaTecnica && X.NEWID == NEWID);
                        if (cAL_FTSaco == null)
                        {
                            cAL_FTSaco = new CAL_FTSaco()
                            {
                                IdFichaTecnica   = fichaTecnica.IdFichaTecnica,
                                IdSaco           = int.Parse(Sacos),
                                Peso             = decimal.Parse(pesoSaco),
                                IdControlFechado = int.Parse(ctrlFechado),
                                ColorHilo        = hilo,
                                FechaHoraIns     = DateTime.Now,
                                IpIns            = RemoteAddr(),
                                UserIns          = CurrentUser.UserName,
                                NEWID            = NEWID
                            };
                            dcSoftwareCalidad.CAL_FTSaco.InsertOnSubmit(cAL_FTSaco);
                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }
                else
                {
                    // Si los borran todos de la lista
                    List<CAL_FTSaco> sacoList = dcSoftwareCalidad.CAL_FTSaco.Where(X => X.IdFichaTecnica == fichaTecnica.IdFichaTecnica).ToList();

                    foreach (var saco in sacoList)
                        dcSoftwareCalidad.CAL_FTSaco.DeleteOnSubmit(saco);

                    dcSoftwareCalidad.SubmitChanges();
                }

                // Si la ficha técnica fuera granel se borran todos los sacos
                if (fichaTecnica.Granel)
                {
                    List<CAL_FTSaco> sacoList = dcSoftwareCalidad.CAL_FTSaco.Where(X => X.IdFichaTecnica == fichaTecnica.IdFichaTecnica).ToList();

                    foreach (var saco in sacoList)
                        dcSoftwareCalidad.CAL_FTSaco.DeleteOnSubmit(saco);

                    dcSoftwareCalidad.SubmitChanges();
                }

                List<CAL_ParametroAnalisis> parametroAnalisisList = (from X in dcSoftwareCalidad.CAL_ParametroAnalisisProducto
                                                                     where X.IdProducto == fichaTecnica.IdProducto
                                                                     orderby X.Orden
                                                                     select X.CAL_ParametroAnalisis).ToList();

                foreach (CAL_ParametroAnalisis parametroAnalisis in parametroAnalisisList)
                {
                    if (!string.IsNullOrEmpty(formCollection[string.Format("PARAM_min_{0}", parametroAnalisis.IdParametroAnalisis)]) &&
                        !string.IsNullOrEmpty(formCollection[string.Format("PARAM_max_{0}", parametroAnalisis.IdParametroAnalisis)]))
                    {
                        if ((decimal.TryParse(formCollection[string.Format("PARAM_min_{0}", parametroAnalisis.IdParametroAnalisis)], out decimal PARAM_min)) &&
                            (decimal.TryParse(formCollection[string.Format("PARAM_max_{0}", parametroAnalisis.IdParametroAnalisis)], out decimal PARAM_max)))
                        {
                            CAL_FTParametroAnalisis cAL_FTParametroAnalisis = dcSoftwareCalidad.CAL_FTParametroAnalisis.SingleOrDefault(X => X.IdFichaTecnica == id && X.IdParametroAnalisis == parametroAnalisis.IdParametroAnalisis);
                            if (cAL_FTParametroAnalisis != null)
                            {
                                cAL_FTParametroAnalisis.MinValidValue  = PARAM_min;
                                cAL_FTParametroAnalisis.MaxValidValue  = PARAM_max;
                                cAL_FTParametroAnalisis.MinAutValue    = 0;
                                cAL_FTParametroAnalisis.MaxAutValue    = 0;
                                cAL_FTParametroAnalisis.AccionAutValue = "";
                                cAL_FTParametroAnalisis.NoAplica = bool.Parse(formCollection[string.Format("HID_NA_{0}", parametroAnalisis.IdParametroAnalisis)]);
                            }
                            else
                            {
                                cAL_FTParametroAnalisis = new CAL_FTParametroAnalisis
                                {
                                    IdFichaTecnica      = fichaTecnica.IdFichaTecnica,
                                    IdParametroAnalisis = parametroAnalisis.IdParametroAnalisis,
                                    MinValidValue       = PARAM_min,
                                    MaxValidValue       = PARAM_max,
                                    MinAutValue         = 0,
                                    MaxAutValue         = 0,
                                    AccionAutValue      = "",
                                    NoAplica            = bool.Parse(formCollection[string.Format("HID_NA_{0}", parametroAnalisis.IdParametroAnalisis)]),
                                    FechaHoraIns        = DateTime.Now,
                                    IpIns               = RemoteAddr(),
                                    UserIns             = User.Identity.Name
                                };
                                dcSoftwareCalidad.CAL_FTParametroAnalisis.InsertOnSubmit(cAL_FTParametroAnalisis);
                            }

                            dcSoftwareCalidad.SubmitChanges();
                        }
                    }
                }
                fichaTecnica.NotificarEdicion();
                return RedirectToAction("Index", new { errMsg, okMsg = "La ficha técnica ha sido editada" });
            }
            catch
            {
                var rv = fichaTecnica.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            List<CAL_ParametroAnalisis> list = (from X in dcSoftwareCalidad.CAL_ParametroAnalisisProducto
                                                where X.IdProducto == fichaTecnica.IdProducto
                                                orderby X.Orden
                                                select X.CAL_ParametroAnalisis).ToList();
            ViewData["list"]            = list;
            ViewData["errMsg"]          = Request["errMsg"];
            ViewData["okMsg"]           = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Crear                   = CheckPermiso(324),
                Leer                    = CheckPermiso(323),
                Actualizar              = CheckPermiso(325),
                Borrar                  = CheckPermiso(326),
                AdminControlVersiones   = CheckPermiso(331),
                AdminDctos              = CheckPermiso(339),
                AdminFrecuenciaAnalisis = CheckPermiso(385)
            };
            return View("Editar", fichaTecnica);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(326);

            CAL_FT fichaTecnica = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (fichaTecnica == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                fichaTecnica.Habilitado   = false;
                fichaTecnica.UserUpd      = User.Identity.Name;
                fichaTecnica.FechaHoraUpd = DateTime.Now;
                fichaTecnica.IpUpd        = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("La ficha técnica {0} ha sido eliminada", fichaTecnica.Codigo);

                fichaTecnica.NotificarEliminacion();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }

        public ActionResult Print(int id)
        {
            CheckPermisoAndRedirect(323);
            CAL_FT fichaTecnica = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (fichaTecnica == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            return View(fichaTecnica);
        }

        public ActionResult PrintEn(int id)
        {
            CheckPermisoAndRedirect(323);
            CAL_FT fichaTecnica = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (fichaTecnica == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            return View(fichaTecnica);
        }

        public ActionResult Clonar(int id)
        {
            CheckPermisoAndRedirect(324);

            CAL_FT fichaTecnica = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (fichaTecnica == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                dcSoftwareCalidad.CAL_ClonarUnaFichaTecnica(fichaTecnica.IdFichaTecnica, User.Identity.Name, DateTime.Now, RemoteAddr());
                okMsg = String.Format("La ficha técnica {0} ha sido clonada", fichaTecnica.Codigo);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg, okMsg });
        }
    }
}