using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Response;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.ControlTiempo
{
    [WebsiteAuthorize]
    public class CTPlanificacionSemanalController : BaseApplicationController
    {
        //
        // GET: /CTPlanificacionSemanal/

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();


        #region Index
        public CTPlanificacionSemanalController()
        {
            SetCurrentModulo(9);
        }
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(272);

            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = from us in dc.PlantaUsuario
                                                  join pl in dc.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
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


            List<CTR_GetSemanasDisponiblesResult> items = dc.CTR_GetSemanasDisponibles(IdPlantaProduccionSelect).ToList();


            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(273),
                                                      CheckPermiso(272),
                                                      CheckPermiso(274),
                                                      CheckPermiso(275));
            return View(items);
        }
        #endregion

        #region Modulo Test
        public ActionResult Test()
        {
            CheckPermisoAndRedirect(273);
            CTR_PlanificacionSemanal planificacionSemanal = new CTR_PlanificacionSemanal();
            ViewData["UserID"] = CurrentUser.UserID;
            return View(planificacionSemanal);
        }
        [HttpPost]
        public ActionResult Test(CTR_PlanificacionSemanal planificacionSemanal, string LINEAS_AUX, string tipo_planificacion)
        {
            CheckPermisoAndRedirect(273);
            int tipo_documento = 0;
            if (tipo_planificacion.Equals("OV")) tipo_documento = 1;
            if (tipo_planificacion.Equals("FR")) tipo_documento = 2;
            if (tipo_planificacion.Equals("TI")) tipo_documento = 3;
            planificacionSemanal.IdEnvase = 1;
            //if (ModelState.IsValid)
            //{
            try
            {
                if (planificacionSemanal.OC < 0)
                    planificacionSemanal.OC = 0;
                if (planificacionSemanal.LC < 0)
                    planificacionSemanal.LC = 0;
                if (string.IsNullOrEmpty(planificacionSemanal.Lote))
                    planificacionSemanal.Lote = "";
                if (string.IsNullOrEmpty(planificacionSemanal.DUS))
                    planificacionSemanal.DUS = "";
                if (string.IsNullOrEmpty(planificacionSemanal.Reserva))
                    planificacionSemanal.Reserva = "";
                planificacionSemanal.Habilitado = true;
                planificacionSemanal.FechaHoraIns = DateTime.Now;
                planificacionSemanal.IpIns = RemoteAddr();
                planificacionSemanal.UserIns = User.Identity.Name;
                dc.CTR_PlanificacionSemanal.InsertOnSubmit(planificacionSemanal);
                dc.SubmitChanges();

                planificacionSemanal.NotificarCreacionPlanificacionSemanal();

                CTR_PlanificacionSemanal obj = (from c in dc.CTR_PlanificacionSemanal select c).MaxBy(c => c.IdPlanificacionSemanal);



                DetallePlanificacion(obj, LINEAS_AUX, tipo_documento);


                return RedirectToAction("index");
            }
            catch
            {
                var rv = planificacionSemanal.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }
            //}
            ViewData["UserID"] = CurrentUser.UserID;
            return View("test", planificacionSemanal);
        }
        #endregion

        #region Modulo crear
        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(273);
            CTR_PlanificacionSemanal planificacionSemanal = new CTR_PlanificacionSemanal();
            ViewData["UserID"] = CurrentUser.UserID;
            return View(planificacionSemanal);
        }

        [HttpPost]
        public ActionResult Crear(CTR_PlanificacionSemanal planificacionSemanal, string LINEAS_AUX, string tipo_planificacion)
        {
            CheckPermisoAndRedirect(273);
            int tipo_documento = 0;
            if (tipo_planificacion.Equals("OV")) tipo_documento = 1;
            if (tipo_planificacion.Equals("FR")) tipo_documento = 2;
            if (tipo_planificacion.Equals("TI")) tipo_documento = 3;
            planificacionSemanal.IdEnvase = 1;
            //if (ModelState.IsValid)
            //{
            try
            {
                if (planificacionSemanal.OC < 0)
                    planificacionSemanal.OC = 0;
                if (planificacionSemanal.LC < 0)
                    planificacionSemanal.LC = 0;
                if (string.IsNullOrEmpty(planificacionSemanal.Lote))
                    planificacionSemanal.Lote = "";
                if (string.IsNullOrEmpty(planificacionSemanal.DUS))
                    planificacionSemanal.DUS = "";
                if (string.IsNullOrEmpty(planificacionSemanal.Reserva))
                    planificacionSemanal.Reserva = "";
                planificacionSemanal.Habilitado = true;
                planificacionSemanal.FechaHoraIns = DateTime.Now;
                planificacionSemanal.IpIns = RemoteAddr();
                planificacionSemanal.UserIns = User.Identity.Name;
                dc.CTR_PlanificacionSemanal.InsertOnSubmit(planificacionSemanal);
                dc.SubmitChanges();

                planificacionSemanal.NotificarCreacionPlanificacionSemanal();

                CTR_PlanificacionSemanal obj = (from c in dc.CTR_PlanificacionSemanal select c).MaxBy(c => c.IdPlanificacionSemanal);


                DetallePlanificacion(obj, LINEAS_AUX, tipo_documento);


                return RedirectToAction("index");
            }
            catch
            {
                var rv = planificacionSemanal.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }
            //}
            ViewData["UserID"] = CurrentUser.UserID;
            return View("crear", planificacionSemanal);
        }

        #endregion

        #region Modulo editar

        public ActionResult EditarPlanificacion(int id)
        {
            CheckPermisoAndRedirect(274);

            var planificacionSemanal = dc.CTR_PlanificacionSemanal.SingleOrDefault(X => X.IdPlanificacionSemanal == id);
            if (planificacionSemanal == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la planificación semanal", okMsg = "" });
            }

            ViewData["permisosUsuario"] = new Permiso()
            {
                Crear = CheckPermiso(273),
                Leer = CheckPermiso(272),
                Actualizar = CheckPermiso(274),
                Borrar = CheckPermiso(275),
                VerificarLC = CheckPermiso(282)
            };
            ViewData["UserID"] = CurrentUser.UserID;
            ViewBag.Detalle = (from c in dc.CTR_PlanificacionSemanal_Detalle where c.IdPlanificacionSemanal == planificacionSemanal.IdPlanificacionSemanal && (c.TipoDocumento == 1 || c.TipoDocumento == 2) select c).ToList();
            return View("EditarPlanificacion", planificacionSemanal);
        }


        [HttpPost]
        public ActionResult EditarPlanificacion(int id, FormCollection formCollection, string LINEAS_AUX, string tipo_planificacion)
        {
            CheckPermisoAndRedirect(274);
            var planificacionSemanal = dc.CTR_PlanificacionSemanal.SingleOrDefault(X => X.IdPlanificacionSemanal == id);
            if (planificacionSemanal == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la planificación semanal", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            if (!string.IsNullOrEmpty(LINEAS_AUX))
            {
                int tipo_documento = 0;
                if (tipo_planificacion.Equals("OV")) tipo_documento = 1;
                if (tipo_planificacion.Equals("FR")) tipo_documento = 2;
                if (tipo_planificacion.Equals("TI")) tipo_documento = 3;
                DetallePlanificacion(planificacionSemanal, LINEAS_AUX, tipo_documento);
            }


            try
            {
                UpdateModel(planificacionSemanal, new string[] { "Año", "Semana", "IdPlantaProduccion", "IdEmpresa", "IdProducto", "IdEnvase", "IdCliente", "Destino", "OC", "LC", "Lunes", "FechaLunes", "Martes", "FechaMartes", "Miercoles", "FechaMiercoles", "Jueves", "FechaJueves", "Viernes", "FechaViernes", "Sabado", "FechaSabado", "Domingo", "FechaDomingo", "PaisCodigo", "Lote", "DUS", "Reserva", "EditarLC", "IdTransportista" });

                if (planificacionSemanal.EditarLC)
                {
                    // Edita L/C
                    UpdateModel(planificacionSemanal, new string[] { "LineaCreditoRechazada", "ObservacionLineaCreditoRechazada" });
                }

                if (planificacionSemanal.OC < 0)
                    planificacionSemanal.OC = 0;
                if (planificacionSemanal.LC < 0)
                    planificacionSemanal.LC = 0;
                if (string.IsNullOrEmpty(planificacionSemanal.Lote))
                    planificacionSemanal.Lote = "";
                if (string.IsNullOrEmpty(planificacionSemanal.DUS))
                    planificacionSemanal.DUS = "";
                if (string.IsNullOrEmpty(planificacionSemanal.Reserva))
                    planificacionSemanal.Reserva = "";

                planificacionSemanal.UserUpd = User.Identity.Name;
                planificacionSemanal.FechaHoraUpd = DateTime.Now;
                planificacionSemanal.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                planificacionSemanal.NotificarEdicionPlanificacionSemanal();

                okMsg = string.Format("El planificación de {0} ha sido editada", planificacionSemanal.Cliente.RazonSocial);

                return RedirectToAction("Ver", new { yy = planificacionSemanal.Año, wk = planificacionSemanal.Semana, errMsg = errMsg, okMsg = okMsg });
            }
            catch
            {
                var rv = planificacionSemanal.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }
            ViewData["UserID"] = CurrentUser.UserID;
            ViewBag.Detalle = (from c in dc.CTR_PlanificacionSemanal_Detalle where c.IdPlanificacionSemanal == planificacionSemanal.IdPlanificacionSemanal && (c.TipoDocumento == 1 || c.TipoDocumento == 2) select c).ToList();
            return View("EditarPlanificacion", planificacionSemanal);
        }


        #endregion

        #region Modulo eliminar

        public ActionResult Eliminar(int yy, int wk)
        {
            CheckPermisoAndRedirect(275);
            List<CTR_PlanificacionSemanal> items = dc.CTR_PlanificacionSemanal.Where(X => X.FechaLunes.Year == yy && X.Semana == wk && X.Habilitado == true).ToList();

            string errMsg = "";
            string okMsg = "";

            try
            {
                foreach (CTR_PlanificacionSemanal planificacionSemanal in items)
                {
                    planificacionSemanal.Habilitado = false;
                    planificacionSemanal.UserUpd = User.Identity.Name;
                    planificacionSemanal.FechaHoraUpd = DateTime.Now;
                    planificacionSemanal.IpUpd = RemoteAddr();
                    dc.SubmitChanges();
                }

                okMsg = String.Format("El planificación semanal de la semana {0} del año {1} ha sido eliminado", wk, yy);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            ViewData["yy"] = yy;
            ViewData["wk"] = wk;
            ViewData["errMsg"] = errMsg;
            ViewData["okMsg"] = okMsg;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(273),
                                                      CheckPermiso(272),
                                                      CheckPermiso(274),
                                                      CheckPermiso(275));

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult EliminarPlanificacion(int id)
        {
            CheckPermisoAndRedirect(275);

            var planificacionSemanal = dc.CTR_PlanificacionSemanal.SingleOrDefault(X => X.IdPlanificacionSemanal == id);
            if (planificacionSemanal == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado la planificación semanal", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                planificacionSemanal.Habilitado = false;
                planificacionSemanal.UserUpd = User.Identity.Name;
                planificacionSemanal.FechaHoraUpd = DateTime.Now;
                planificacionSemanal.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                okMsg = string.Format("El planificación de {0} ha sido eliminada", planificacionSemanal.Cliente.RazonSocial);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Ver", new { yy = planificacionSemanal.Año, wk = planificacionSemanal.Semana, errMsg = errMsg, okMsg = okMsg });
        }

        #endregion

        #region Informes

        public ActionResult CrearPDF(int yy, int wk, int? IdEmpresa, DateTime? fDia)
        {
            CheckPermisoAndRedirect(272);

            ViewData["yy"] = yy;
            ViewData["wk"] = wk;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(273),
                                                      CheckPermiso(272),
                                                      CheckPermiso(274),
                                                      CheckPermiso(275));

            CTR_PlanificacionSemanal planificacionSemanal = new CTR_PlanificacionSemanal();
            planificacionSemanal.Año = yy;
            planificacionSemanal.Semana = wk;
            if (int.TryParse(Request["IdEmpresa"], out int resultIdEmpresa))
                planificacionSemanal.IdEmpresa = resultIdEmpresa;
            if (DateTime.TryParse(Request["fDia"], out DateTime resultfDia))
                planificacionSemanal.fDia = resultfDia;

            return View(planificacionSemanal);
        }

        [HttpPost]
        public ActionResult CrearPDF(int yy, int wk, int? IdEmpresa, DateTime? fDia, CTR_PlanificacionSemanal planificacionSemanal, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(272);

            List<CTR_PlanificacionSemanal> planificaciones = dc.CTR_PlanificacionSemanal.Where(X => X.FechaLunes.Year == planificacionSemanal.Año && X.Semana == wk && X.Habilitado == true && (planificacionSemanal.IdEmpresa == 0 || X.IdEmpresa == planificacionSemanal.IdEmpresa)).ToList();

            string errMsg = "";
            string okMsg = "";

            if (planificaciones.Count == 0)
            {
                errMsg = "No hay planificacionse para creadas para el filtro seleccionado";
                return RedirectToAction("CrearPDF", new { yy = planificacionSemanal.Año, wk = planificacionSemanal.Semana, IdEmpresa = planificacionSemanal.IdEmpresa, fDia = planificacionSemanal.fDia, errMsg = errMsg, okMsg = okMsg });
            }

            PlanificacionPdf planificacionPdf = planificacionSemanal.CrearPdfPlanificacion();
            if (planificacionPdf == null)
                throw new Exception("No se pudo crear el PDF");

            var pdf = Server.MapPath(string.Format("~/App_Data/pdfs/controltiempo/planificaciones/{0}.pdf", string.Format("{0}", planificacionPdf.Guid)));

            return File(pdf, "application/pdf", planificacionPdf.Titulo);
        }

        #endregion

        #region Ver

        public ActionResult Ver(int yy, int wk, int? IdEmpresa, int? IdProducto, int? IdEnvase, int? IdCliente, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(272);

            var IdEmpresaSelect = IdEmpresa ?? 0;
            var IdProductoSelect = IdProducto ?? 0;
            var IdEnvaseSelect = IdEnvase ?? 0;
            var IdClienteSelect = IdCliente ?? 0;
            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            List<CTR_PlanificacionSemanal> items = (from X in dc.CTR_PlanificacionSemanal
                                                    where (IdEmpresaSelect == 0 || X.IdEmpresa == IdEmpresaSelect)
                                                    && (IdProductoSelect == 0 || X.IdProducto == IdProductoSelect)
                                                    && (IdEnvaseSelect == 0 || X.IdEnvase == IdEnvaseSelect)
                                                    && (IdClienteSelect == 0 || X.IdCliente == IdClienteSelect)
                                                    && X.IdPlantaProduccion == IdPlantaProduccionSelect
                                                    && X.FechaLunes.Year == yy
                                                    && X.Semana == wk
                                                    && X.Habilitado == true
                                                    select X).ToList();

            CTR_ControlTiempo controlTiempo = new CTR_ControlTiempo();
            controlTiempo.IdEmpresa = IdEmpresaSelect;
            controlTiempo.IdProducto = IdProductoSelect;
            controlTiempo.IdEnvase = IdEnvaseSelect;
            controlTiempo.IdCliente = IdClienteSelect;
            controlTiempo.IdPlantaProduccion = IdPlantaProduccionSelect;

            ViewData["controlTiempo"] = controlTiempo;
            ViewData["yy"] = yy;
            ViewData["wk"] = wk;
            ViewData["UserID"] = CurrentUser.UserID;
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Crear = CheckPermiso(273),
                Leer = CheckPermiso(272),
                Actualizar = CheckPermiso(274),
                Borrar = CheckPermiso(275),
                VerificarLC = CheckPermiso(282)
            };
            return View(items);
        }

        #endregion

        #region Detalle planificación

        private bool DetallePlanificacion(CTR_PlanificacionSemanal obj, string LINEAS_AUX, int tipo_documento)
        {
            bool resultado = false;
            try
            {
                if (string.IsNullOrEmpty(LINEAS_AUX))
                {
                    CTR_PlanificacionSemanal_Detalle obj_detalle = new CTR_PlanificacionSemanal_Detalle()
                    {
                        IdCliente = obj.IdCliente,
                        IdEmpresa = obj.IdEmpresa,
                        IdPlanificacionSemanal = obj.IdPlanificacionSemanal,
                        IdPlanta = obj.IdPlantaProduccion,
                        IdProducto = obj.IdProducto,
                        TipoDocumento = tipo_documento,
                        NumeroDocumento = null,
                        Codigo = null,
                        NombreProducto = null,
                        Saldo = null
                    };
                    dc.CTR_PlanificacionSemanal_Detalle.InsertOnSubmit(obj_detalle);
                    dc.SubmitChanges();
                }
                else
                {
                    string[] lineas = LINEAS_AUX.Split('-');

                    foreach (var linea in lineas)
                    {
                        string[] contenidos = linea.Split(',');
                        CTR_PlanificacionSemanal_Detalle obj_detalle = new CTR_PlanificacionSemanal_Detalle()
                        {
                            IdCliente = obj.IdCliente,
                            IdEmpresa = obj.IdEmpresa,
                            IdPlanificacionSemanal = obj.IdPlanificacionSemanal,
                            IdPlanta = obj.IdPlantaProduccion,
                            TipoDocumento = tipo_documento,
                            NumeroDocumento = contenidos[1],
                            Codigo = contenidos[2],
                            NombreProducto = contenidos[3],
                            Saldo = Convert.ToDecimal(contenidos[4]),
                            IdProducto = null,
                            WhsCode = contenidos[6],
                            Linea = Convert.ToInt32(contenidos[7])
                        };
                        dc.CTR_PlanificacionSemanal_Detalle.InsertOnSubmit(obj_detalle);
                        dc.SubmitChanges();

                    }
                }

                resultado = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return resultado;
        }

        public ActionResult EliminarDetalle(int IdDetalle, int IdPlanificacion)
        {

            CTR_PlanificacionSemanal_Detalle obj = (from c in dc.CTR_PlanificacionSemanal_Detalle where c.IdPlanificacionDetalle == IdDetalle select c).FirstOrDefault();

            var verificacion = (from c in dc.CTR_ControlTiempo_Detalle where c.IdPlanificacionDetalle == IdDetalle select c).ToList();
            if(verificacion.Count() > 0) return Json(new Response { IsSuccess = false, Message = "No se ha podido eliminar detalle de planificación, este se encuentra ligado al detalle del camión.", Id = IdPlanificacion }, JsonRequestBehavior.AllowGet);

            dc.CTR_PlanificacionSemanal_Detalle.DeleteOnSubmit(obj);
            dc.SubmitChanges();
            return Json(new Response { IsSuccess = true, Id = IdPlanificacion }, JsonRequestBehavior.AllowGet);
        }

        #endregion

          #region Get

        public ActionResult GetCliente(int IdCliente, int IdEmpresa)
        {
            Cliente cliente = dc.Cliente.Where(X => X.IdCliente == IdCliente).FirstOrDefault();

            string codigoSAPCliente = "";
            string codigoEmpresa = "";

            if (IdEmpresa == 1)
            {
                codigoSAPCliente = cliente.IDOleotop;
                codigoEmpresa = "[IDOleotop]";
            }
            if (IdEmpresa == 2)
            {
                codigoSAPCliente = cliente.IDAvenatop;
                codigoEmpresa = "[IDAvenatop]";
            }
            if (IdEmpresa == 3)
            {
                codigoSAPCliente = cliente.IDGranotop;
                codigoEmpresa = "[IDGranotop]";
            }
            if (IdEmpresa == 4)
            {
                codigoSAPCliente = cliente.IDSaprosem;
                codigoEmpresa = "[IDSaprosem]";
            }
            if (IdEmpresa == 5)
            {
                codigoSAPCliente = cliente.IDICI;
                codigoEmpresa = "[IDICI]";
            }
            return Json(new Response { IsSuccess = true, CodigoSAPCliente = codigoSAPCliente, CodigoEmpresa = codigoEmpresa, IdEmpresa = IdEmpresa }, JsonRequestBehavior.AllowGet);
        }

        #endregion























    }
}