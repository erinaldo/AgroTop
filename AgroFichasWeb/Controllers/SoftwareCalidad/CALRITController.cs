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
    public class CALRITController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private readonly AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALRITController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALRIT
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(364);


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
            List<CAL_RIT> list = new List<CAL_RIT>();
            if (IdPlantaProduccionSelect == 0)
            {
                list = dcSoftwareCalidad.CAL_RIT.Where(X => X.Habilitado == true).ToList();
            }
            else
            {
                list = dcSoftwareCalidad.CAL_RIT.Where(X => X.Habilitado == true && X.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect).ToList();
            }



            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["plantas"] = plantas;

            ViewData["permisosUsuario"] = new Permiso()
            {
                Crear = CheckPermiso(365),
                Leer = CheckPermiso(364),
                Actualizar = CheckPermiso(366),
                Borrar = CheckPermiso(367),
                VerificarRIT = CheckPermiso(368)
            };
            return View(list);
        }

        public ActionResult Crear(int id)
        {
            CheckPermisoAndRedirect(365);
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            CAL_RIT cAL_RIT = new CAL_RIT();
            cAL_RIT.IdOrdenProduccion = ordenProduccion.IdOrdenProduccion;

            CAL_RITSelloTara cAL_RITSelloTara = new CAL_RITSelloTara();
 

            List<CAL_ParametroRevisarContenedor> cAL_ParametroRevisarContenedorList = dcSoftwareCalidad.CAL_ParametroRevisarContenedor.Where(X => X.Habilitado == true).ToList();
            List<CAL_RITAccionCorrectiva> cAL_RITAccionCorrectivaList = dcSoftwareCalidad.CAL_RITAccionCorrectiva.Where(X => X.Habilitado == true).ToList();

            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["cAL_ParametroRevisarContenedorList"] = cAL_ParametroRevisarContenedorList;
            ViewData["cAL_RITAccionCorrectivaList"] = cAL_RITAccionCorrectivaList;
            ViewData["cAL_RITSelloTara"] = cAL_RITSelloTara;
            ViewData["planta"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == ordenProduccion.IdPlanta).FirstOrDefault();
            return View("Crear", cAL_RIT);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Crear(int id, CAL_RIT cAL_RIT, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(365);
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            }

            List<CAL_ParametroRevisarContenedor> cAL_ParametroRevisarContenedorList = dcSoftwareCalidad.CAL_ParametroRevisarContenedor.ToList();
            List<CAL_RITAccionCorrectiva> cAL_RITAccionCorrectivaList = dcSoftwareCalidad.CAL_RITAccionCorrectiva.Where(X => X.Habilitado == true).ToList();
            CAL_RITSelloTara caL_RITSelloTara = new CAL_RITSelloTara();

            string errMsg = string.Empty;
            string okMsg = string.Empty;

            cAL_RIT.Validate(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    CAL_RITContenedor cAL_RITContenedor = new CAL_RITContenedor()
                    {
                        NContenedor = cAL_RIT.NContenedor.ToUpperInvariant(),
                        Habilitado = true,
                        FechaHoraIns = DateTime.Now,
                        IpIns = RemoteAddr(),
                        UserIns = User.Identity.Name,
                        IdPlanta = ordenProduccion.IdPlanta
                    };
                    dcSoftwareCalidad.CAL_RITContenedor.InsertOnSubmit(cAL_RITContenedor);
                    dcSoftwareCalidad.SubmitChanges();

                    if (string.IsNullOrEmpty(cAL_RIT.Observacion))
                        cAL_RIT.Observacion = string.Empty;

                    cAL_RIT.IdContenedor = cAL_RITContenedor.IdContenedor;
                    cAL_RIT.Habilitado = true;
                    cAL_RIT.FechaHoraIns = DateTime.Now;
                    cAL_RIT.IpIns = RemoteAddr();
                    cAL_RIT.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_RIT.InsertOnSubmit(cAL_RIT);
                    dcSoftwareCalidad.SubmitChanges();

                    if (!string.IsNullOrEmpty(formCollection["NContenedor"]))
                    {
                        CAL_RITSelloTara cAL_RITSelloTara = new CAL_RITSelloTara()
                        {
                            IdRIT = cAL_RIT.IdRIT,
                            IdContenedor = cAL_RITContenedor.IdContenedor,
                            NContenedor = cAL_RITContenedor.NContenedor.ToUpperInvariant(),
                            SelloLinea = formCollection["SelloLinea"].ToUpperInvariant(),
                            Tara = decimal.Parse(formCollection["Tara"]),
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name,
                        };
                        dcSoftwareCalidad.CAL_RITSelloTara.InsertOnSubmit(cAL_RITSelloTara);
                        dcSoftwareCalidad.SubmitChanges();
                    }
                    

                    foreach (CAL_ParametroRevisarContenedor cAL_ParametroRevisarContenedor in cAL_ParametroRevisarContenedorList)
                    {
                        int? ACCIONCORRECTIVA__Nullable = null;
                        if (!string.IsNullOrEmpty((formCollection[string.Format("ACCIONCORRECTIVA__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)])) && int.TryParse(formCollection[string.Format("ACCIONCORRECTIVA__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)], out int ACCIONCORRECTIVA))
                        {
                            ACCIONCORRECTIVA__Nullable = ACCIONCORRECTIVA;
                        }

                        string OBSERVACIONES = formCollection[string.Format("OBSERVACIONES__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)];
                        if (string.IsNullOrEmpty(OBSERVACIONES))
                            OBSERVACIONES = string.Empty;

                        CAL_RITParametroRevisarContenedor cAL_RITParametroRevisarContenedor = new CAL_RITParametroRevisarContenedor()
                        {
                            IdRIT = cAL_RIT.IdRIT,
                            IdParametroRevisarContenedor = cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor,
                            Cumple = bool.Parse(formCollection[string.Format("HIDDEN__CUMPLE__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)]),
                            IdAccionCorrectiva = ACCIONCORRECTIVA__Nullable,
                            Observacion = OBSERVACIONES,
                            UserIns = User.Identity.Name,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                        };
                        dcSoftwareCalidad.CAL_RITParametroRevisarContenedor.InsertOnSubmit(cAL_RITParametroRevisarContenedor);
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha creado el Registro de Inspección" });
                }
                catch
                {
                    var rv = cAL_RIT.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            ViewData["cAL_RITSelloTara"] = caL_RITSelloTara;
            ViewData["ordenProduccion"] = ordenProduccion;
            ViewData["cAL_ParametroRevisarContenedorList"] = cAL_ParametroRevisarContenedorList;
            ViewData["cAL_RITAccionCorrectivaList"] = cAL_RITAccionCorrectivaList;
            ViewData["planta"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == ordenProduccion.IdPlanta).FirstOrDefault();
            return View("Crear", cAL_RIT);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(366);

            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == id && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }

            List<CAL_RITParametroRevisarContenedor> cAL_RITParametroRevisarContenedorList = dcSoftwareCalidad.CAL_RITParametroRevisarContenedor.Where(X => X.IdRIT == cAL_RIT.IdRIT).ToList();
            List<CAL_ParametroRevisarContenedor> cAL_ParametroRevisarContenedorList = dcSoftwareCalidad.CAL_ParametroRevisarContenedor.Where(X => X.Habilitado == true).ToList();
            List<CAL_RITAccionCorrectiva> cAL_RITAccionCorrectivaList = dcSoftwareCalidad.CAL_RITAccionCorrectiva.Where(X => X.Habilitado == true).ToList();
            
            CAL_RITSelloTara cAL_RITSelloTara = dcSoftwareCalidad.CAL_RITSelloTara.SingleOrDefault(x => x.IdRIT == id);
            if (cAL_RITSelloTara != null)
            {
                ViewData["cAL_RITSelloTara"] = cAL_RITSelloTara;
            }
            else 
            {
                CAL_RITSelloTara cAL_RITSelloTara2 = new CAL_RITSelloTara();
                ViewData["cAL_RITSelloTara"] = cAL_RITSelloTara2;
            }

            ViewData["ordenProduccion"] = cAL_RIT.CAL_OrdenProduccion;
            ViewData["cAL_RITParametroRevisarContenedorList"] = cAL_RITParametroRevisarContenedorList;
            ViewData["cAL_ParametroRevisarContenedorList"] = cAL_ParametroRevisarContenedorList;
            ViewData["cAL_RITAccionCorrectivaList"] = cAL_RITAccionCorrectivaList;
            ViewData["planta"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == cAL_RIT.CAL_OrdenProduccion.IdPlanta).FirstOrDefault();

            return View("Editar", cAL_RIT);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(366);
            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == id && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }

            List<CAL_RITParametroRevisarContenedor> cAL_RITParametroRevisarContenedorList = dcSoftwareCalidad.CAL_RITParametroRevisarContenedor.Where(X => X.IdRIT == cAL_RIT.IdRIT).ToList();
            List<CAL_ParametroRevisarContenedor> cAL_ParametroRevisarContenedorList = dcSoftwareCalidad.CAL_ParametroRevisarContenedor.ToList();
            List<CAL_RITAccionCorrectiva> cAL_RITAccionCorrectivaList = dcSoftwareCalidad.CAL_RITAccionCorrectiva.Where(X => X.Habilitado == true).ToList();

            string errMsg = string.Empty;
            string okMsg = string.Empty;

            cAL_RIT.Validate(ModelState, formCollection);

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(cAL_RIT, new string[] { "NContenedor", "Patente", "Observacion", "Aprobado", "Verificado", "SelloLinea", "Tara", "IdPlanta" });


                    if (string.IsNullOrEmpty(cAL_RIT.Observacion))
                        cAL_RIT.Observacion = string.Empty;

                    cAL_RIT.FechaHoraUpd = DateTime.Now;
                    cAL_RIT.IpUpd = RemoteAddr();
                    cAL_RIT.UserUpd = User.Identity.Name;

                    // Actualiza IDRIT,IdContenedor en sello tara
                    CAL_RITSelloTara sellotara = dcSoftwareCalidad.CAL_RITSelloTara.SingleOrDefault(X => X.IdRIT == id);
                    if (sellotara != null)
                    {
                        sellotara.IdContenedor = cAL_RIT.IdContenedor;
                        sellotara.NContenedor = cAL_RIT.CAL_RITContenedor.NContenedor;
                        sellotara.SelloLinea = formCollection["SelloLinea"].ToUpperInvariant();
                        sellotara.Tara = decimal.Parse(formCollection["Tara"]);
                        sellotara.FechaHoraUpd = DateTime.Now;
                        sellotara.IpUpd = RemoteAddr();
                        sellotara.UserUpd = User.Identity.Name;
                    }

                    var contenedor = dcSoftwareCalidad.CAL_RITContenedor.SingleOrDefault(X => X.IdContenedor == cAL_RIT.IdContenedor && X.Habilitado == true);
                    if (contenedor != null) 
                    {
                        contenedor.NContenedor = formCollection["NContenedor"].ToUpperInvariant();
                        contenedor.UserUpd = User.Identity.Name;
                        contenedor.FechaHoraUpd = DateTime.Now;
                        contenedor.IpUpd = RemoteAddr();
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    foreach (CAL_ParametroRevisarContenedor cAL_ParametroRevisarContenedor in cAL_ParametroRevisarContenedorList)
                    {
                        int? ACCIONCORRECTIVA__Nullable = null;
                        if (!string.IsNullOrEmpty((formCollection[string.Format("ACCIONCORRECTIVA__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)])) && int.TryParse(formCollection[string.Format("ACCIONCORRECTIVA__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)], out int ACCIONCORRECTIVA))
                        {
                            ACCIONCORRECTIVA__Nullable = ACCIONCORRECTIVA;
                        }

                        string OBSERVACIONES = formCollection[string.Format("OBSERVACIONES__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)];
                        if (string.IsNullOrEmpty(OBSERVACIONES))
                            OBSERVACIONES = string.Empty;

                        CAL_RITParametroRevisarContenedor cAL_RITParametroRevisarContenedor = cAL_RITParametroRevisarContenedorList.SingleOrDefault(X => X.IdParametroRevisarContenedor == cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor);
                        if (cAL_RITParametroRevisarContenedor != null)
                        {
                            cAL_RITParametroRevisarContenedor.Cumple = bool.Parse(formCollection[string.Format("HIDDEN__CUMPLE__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)]);
                            cAL_RITParametroRevisarContenedor.IdAccionCorrectiva = ACCIONCORRECTIVA__Nullable;
                            cAL_RITParametroRevisarContenedor.Observacion = OBSERVACIONES;
                            cAL_RITParametroRevisarContenedor.UserUpd = User.Identity.Name;
                            cAL_RITParametroRevisarContenedor.FechaHoraUpd = DateTime.Now;
                            cAL_RITParametroRevisarContenedor.IpUpd = RemoteAddr();
                        }
                        else
                        {
                            cAL_RITParametroRevisarContenedor = new CAL_RITParametroRevisarContenedor()
                            {
                                IdRIT = cAL_RIT.IdRIT,
                                IdParametroRevisarContenedor = cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor,
                                Cumple = bool.Parse(formCollection[string.Format("HIDDEN__CUMPLE__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)]),
                                IdAccionCorrectiva = ACCIONCORRECTIVA__Nullable,
                                Observacion = OBSERVACIONES,
                                UserIns = User.Identity.Name,
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                            };
                            dcSoftwareCalidad.CAL_RITParametroRevisarContenedor.InsertOnSubmit(cAL_RITParametroRevisarContenedor);
                        }
                    }

                    dcSoftwareCalidad.SubmitChanges();

                    return RedirectToAction("Index", new { errMsg = "", okMsg = "Se ha editado el Registro de Inspección de Transporte" });
                }
                catch
                {
                    var rv = cAL_RIT.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["ordenProduccion"] = cAL_RIT.CAL_OrdenProduccion;
            ViewData["cAL_RITParametroRevisarContenedorList"] = cAL_RITParametroRevisarContenedorList;
            ViewData["cAL_ParametroRevisarContenedorList"] = cAL_ParametroRevisarContenedorList;
            ViewData["cAL_RITAccionCorrectivaList"] = cAL_RITAccionCorrectivaList;
            ViewData["planta"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == cAL_RIT.CAL_OrdenProduccion.IdPlanta).FirstOrDefault();
            return View("Crear", cAL_RIT);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(367);
            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == id && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cAL_RIT.Habilitado = false;
                cAL_RIT.UserUpd = User.Identity.Name;
                cAL_RIT.FechaHoraUpd = DateTime.Now;
                cAL_RIT.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El registro de inspección de transporte {0} ha sido eliminado", cAL_RIT.IdRIT);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult Imprimir(int id)
        {
            CheckPermisoAndRedirect(364);
            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == id && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }

            RITViewModel rITViewModel = new RITViewModel();
            rITViewModel.RegistroInspeccionTransporte = cAL_RIT;
            rITViewModel.Contenedor = cAL_RIT.CAL_RITContenedor;
            rITViewModel.OrdenProduccion = cAL_RIT.CAL_OrdenProduccion;

            List<CAL_RITParametroRevisarContenedor> cAL_RITParametroRevisarContenedorList = dcSoftwareCalidad.CAL_RITParametroRevisarContenedor.Where(X => X.IdRIT == cAL_RIT.IdRIT).ToList();
            rITViewModel.ParametroRevisarList = cAL_RITParametroRevisarContenedorList;

            return View("Imprimir", rITViewModel);
        }

        public ActionResult Verificar(int id)
        {
            CheckPermisoAndRedirect(368);
            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == id && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                cAL_RIT.Verificado = true;
                cAL_RIT.UserInsVerificacion = User.Identity.Name;
                cAL_RIT.FechaHoraInsVerificacion = DateTime.Now;
                cAL_RIT.IpInsVerificacion = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El registro de inspección de transporte {0} ha sido verificado", cAL_RIT.IdRIT);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult VerOP(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(364);


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
                 list = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                         where X.Habilitado
                         && (X.Autorizado.HasValue && X.Autorizado.Value)
                         && !X.Terminada
                         select X).ToList();
             }
             else
             {
                 list = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                         where X.Habilitado
                         && (X.Autorizado.HasValue && X.Autorizado.Value)
                         && !X.Terminada && X.IdPlanta == IdPlantaProduccionSelect
                         select X).ToList();
             }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso()
            {
                Crear = CheckPermiso(365),
                Leer = CheckPermiso(364),
                Actualizar = CheckPermiso(366),
                Borrar = CheckPermiso(367),
                VerificarRIT = CheckPermiso(368)
            };
            return View("VerOP", list);
        }

        public ActionResult VerRIT(int id)
        {
            CheckPermisoAndRedirect(364);
            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == id && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }

            RITViewModel rITViewModel = new RITViewModel();
            rITViewModel.RegistroInspeccionTransporte = cAL_RIT;
            rITViewModel.Contenedor = cAL_RIT.CAL_RITContenedor;
            rITViewModel.OrdenProduccion = cAL_RIT.CAL_OrdenProduccion;

            List<CAL_RITParametroRevisarContenedor> cAL_RITParametroRevisarContenedorList = dcSoftwareCalidad.CAL_RITParametroRevisarContenedor.Where(X => X.IdRIT == cAL_RIT.IdRIT).ToList();
            rITViewModel.ParametroRevisarList = cAL_RITParametroRevisarContenedorList;

            ViewData["plantaProduccion"] = dcAgroFichas.PlantaProduccion.Where(X => X.IdPlantaProduccion == cAL_RIT.CAL_OrdenProduccion.IdPlanta).FirstOrDefault();

            return View("VerRIT", rITViewModel);
        }

        public ActionResult AgregarSelloTara(int id)
        {
            CheckPermisoAndRedirect(365);
            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == id && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }

            CAL_RITSelloTara ritsellotara = (from X in dcSoftwareCalidad.CAL_RITSelloTara
                                          join y in dcSoftwareCalidad.CAL_RIT on X.IdRIT equals y.IdRIT
                                          where X.IdRIT == id
                                                   && y.Habilitado
                                                   select X).SingleOrDefault();

            if (ritsellotara == null)
            {
                CAL_RITSelloTara sellotara = new CAL_RITSelloTara();
                sellotara.IdRIT = cAL_RIT.IdRIT;
                sellotara.IdContenedor = cAL_RIT.IdContenedor;

                ViewData["cAL_RIT"] = cAL_RIT;
                return View(sellotara);
            }
            else
            {
                CAL_RITSelloTara sellotara = dcSoftwareCalidad.CAL_RITSelloTara.SingleOrDefault(X => X.IdRIT == id);

                ViewData["cAL_RIT"] = cAL_RIT;
                return View(sellotara);
            }                          
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CrearSelloTara(int IdRIT, CAL_RITSelloTara sellotara, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(365);
            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == IdRIT && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(cAL_RIT, new string[] { "SelloLinea", "Tara" });

                    if (cAL_RIT.ValidacionSelloTara(sellotara, ModelState))
                    {
                        // Rescatados del Form
                        sellotara.FechaHoraIns = DateTime.Now;
                        sellotara.IpIns = RemoteAddr();
                        sellotara.UserIns = User.Identity.Name;
                        dcSoftwareCalidad.CAL_RITSelloTara.InsertOnSubmit(sellotara);
                        dcSoftwareCalidad.SubmitChanges();
                        var okMsg = String.Format("El Sello y tara del contenedor {0} se agregó con éxtio", cAL_RIT.CAL_RITContenedor.NContenedor);
                        return RedirectToAction("Index", new { okMsg = okMsg, errMsg = "" });
                    }
                }
                catch
                {
                    var rv = cAL_RIT.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            ViewData["cAL_RIT"] = cAL_RIT;
            return View(sellotara);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditarSelloTara(int IdRIT, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(365);
            CAL_RITSelloTara sellotara = dcSoftwareCalidad.CAL_RITSelloTara.SingleOrDefault(X => X.IdRIT == IdRIT);
            if (sellotara == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el contenedor", okMsg = "" });
            }

            CAL_RIT cAL_RIT = dcSoftwareCalidad.CAL_RIT.SingleOrDefault(X => X.IdRIT == IdRIT && X.Habilitado == true);
            if (cAL_RIT == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el registro de inspección de transporte", okMsg = "" });
            }

            if (ModelState.IsValid)
            {
                try
                {

                    if (cAL_RIT.ValidacionSelloTara(sellotara, ModelState))
                    {
                        // Rescatados del Form
                        sellotara.SelloLinea = string.Format("{0}", formCollection["SelloLinea"]);
                        sellotara.Tara = decimal.Parse(formCollection["Tara"]);
                        sellotara.FechaHoraUpd = DateTime.Now;
                        sellotara.IpUpd = RemoteAddr();
                        sellotara.UserUpd = User.Identity.Name;
                        dcSoftwareCalidad.SubmitChanges();
                        var okMsg = String.Format("El Sello y tara del contenedor {0} se ha editado con éxtio", cAL_RIT.CAL_RITContenedor.NContenedor);
                        return RedirectToAction("Index", new { okMsg = okMsg, errMsg = "" });
                    }
                }
                catch
                {
                    var rv = cAL_RIT.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["cAL_RIT"] = cAL_RIT;
            return View(sellotara);
        }
    }
}