using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using AgroFichasWeb.ViewModels.Logistica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class PedidosController : BaseApplicationController
    {
        //
        // GET: /Pedidos/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public PedidosController()
        {
            SetCurrentModulo(6);//Logística y Corretaje
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Crear(int id)
        {
            CheckPermisoAndRedirect(113);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == id && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            var pedido = new LOG_Pedido();
            SetCultivos(null);
            SetEnvases(null);
            SetEstados(null);
            SetTipoPedidos(null);

            var bodegasOrigenList = SetBodegas(null);
            var bodegasDestinoList = SetBodegas(null);
            ViewData["bodegasOrigenList"] = bodegasOrigenList;
            ViewData["bodegasDestinoList"] = bodegasDestinoList;
            ViewData["IdRequerimiento"] = requerimiento.IdRequerimiento;
            return View("crear", pedido);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(LOG_Pedido pedido, FormCollection formValues)
        {
            CheckPermisoAndRedirect(113);
            int IdRequerimiento = int.Parse(formValues["IdRequerimiento"]);
            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == IdRequerimiento && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            if (ModelState.IsValid)
            {
                try
                {
                    if (pedido.Observacion == null)
                        pedido.Observacion = "";

                    pedido.Habilitado = true;
                    pedido.FechaHoraIns = DateTime.Now;
                    pedido.IpIns = RemoteAddr();
                    pedido.UserIns = User.Identity.Name;

                    var cultivo = dc.Cultivo.Single(x => x.IdCultivo == pedido.IdCultivo);
                    var estado = dc.LOG_EstadoPedido.Single(x => x.IdEstado == pedido.IdEstado);
                    var asignacionpedido = new LOG_AsignacionPedido()
                    {
                        IdRequerimiento = requerimiento.IdRequerimiento,
                        Cultivo = cultivo.Nombre,
                        CantidadUnitariaKg = pedido.CantidadUnitariaKg,
                        Estado = estado.Descripcion,
                        Origen = "",
                        Destino = "",
                        UserIns = User.Identity.Name,
                        FechaHoraIns = DateTime.Now,
                        IpIns = RemoteAddr()
                    };

                    if (pedido.Origen > 0 & pedido.Destino > 0)
                    {
                        var bodega = dc.Bodega.Single(x => x.IdBodega == pedido.Origen.Value);
                        var bodega1 = dc.Bodega.Single(x => x.IdBodega == pedido.Destino.Value);
                        asignacionpedido.Origen = string.Format("{0} ({1})", bodega.Sucursal.Nombre, bodega.NombreCorto);
                        asignacionpedido.Destino = string.Format("{0} ({1})", bodega1.Sucursal.Nombre, bodega1.NombreCorto);
                    }
                    else
                    {
                        asignacionpedido.Origen = pedido.OtroOrigen;
                        asignacionpedido.Destino = pedido.OtroDestino;
                    }

                    requerimiento.CantidadTotalKg += pedido.CantidadUnitariaKg;
                    pedido.LOG_AsignacionPedido.Add(asignacionpedido);
                    dc.LOG_Pedido.InsertOnSubmit(pedido);
                    dc.SubmitChanges();

                    return RedirectToAction("detalle", "requerimientos", new { id = IdRequerimiento });
                }
                catch
                {
                    var rv = pedido.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else if (requerimiento.GetRuleViolations().Count() > 0)
                        ModelState.AddRuleViolations(requerimiento.GetRuleViolations());
                    else
                        throw new Exception();
                }
            }

            SetCultivos(pedido.IdCultivo);
            SetEnvases(pedido.IdEnvase);
            SetEstados(pedido.IdEstado);
            SetTipoPedidos(pedido.IdTipoPedido);
            var bodegasOrigenList = SetBodegas(pedido.Origen);
            var bodegasDestinoList = SetBodegas(pedido.Destino);
            ViewData["bodegasOrigenList"] = bodegasOrigenList;
            ViewData["bodegasDestinoList"] = bodegasDestinoList;
            ViewData["IdRequerimiento"] = requerimiento.IdRequerimiento;
            return View("crear", pedido);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CrearMultiples(int id)
        {
            CheckPermisoAndRedirect(113);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == id && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            var pedido = new LOG_Pedido();
            SetCultivos(null);
            SetEnvases(null);
            SetEstados(null);
            SetTipoPedidos(null);

            var bodegasOrigenList = SetBodegas(null);
            var bodegasDestinoList = SetBodegas(null);
            ViewData["bodegasOrigenList"] = bodegasOrigenList;
            ViewData["bodegasDestinoList"] = bodegasDestinoList;
            ViewData["IdRequerimiento"] = requerimiento.IdRequerimiento;
            return View("crearmultiples", pedido);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult CrearMultiples(LOG_Pedido pedido, FormCollection formValues)
        {
            CheckPermisoAndRedirect(113);

            int IdRequerimiento = int.Parse(formValues["IdRequerimiento"]);
            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == IdRequerimiento && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            var msgerr = "";
            //Para evitar poner el seleccionador de estados
            pedido.IdEstado = 1; //En espera de camión
            if (ModelState.IsValid)
            {
                try
                {
                    var dividirPedidosEn = 0;
                    if (!int.TryParse(formValues["DivididaEn"], out dividirPedidosEn))
                    {
                        msgerr = "Dividir pedidos en no es válido";
                    }
                    var cantidadUnitariaTotalTn = pedido.CantidadUnitariaKg;
                    var cantidadUnitariaTotalKg = (cantidadUnitariaTotalTn * 1000);
                    var cantidadTotalPedidos = (int)Math.Ceiling(cantidadUnitariaTotalKg / dividirPedidosEn);

                    decimal cantidadUnitariaKg = 0;
                    if (string.IsNullOrEmpty(msgerr))
                    {
                        for (int I = 0; I < cantidadTotalPedidos; I++)
                        {
                            if (cantidadUnitariaTotalKg < dividirPedidosEn)
                                cantidadUnitariaKg = cantidadUnitariaTotalKg;
                            else
                                cantidadUnitariaKg = dividirPedidosEn;

                            var cultivo = dc.Cultivo.Single(x => x.IdCultivo == pedido.IdCultivo);
                            var pedidoIndividual = new LOG_Pedido()
                            {
                                CantidadUnitariaKg = cantidadUnitariaKg,
                                FechaHoraIns = DateTime.Now,
                                Habilitado = true,
                                IdCultivo = cultivo.IdCultivo,
                                IdEnvase = pedido.IdEnvase,
                                IdEstado = pedido.IdEstado,
                                IdTipoPedido = pedido.IdTipoPedido,
                                IpIns = RemoteAddr(),
                                Observacion = string.Format("Pedido {0}/{1}", (I + 1), cantidadTotalPedidos),
                                UserIns = User.Identity.Name,
                            };

                            var estado = dc.LOG_EstadoPedido.Single(x => x.IdEstado == pedido.IdEstado);
                            var asignacionpedidoIndividual = new LOG_AsignacionPedido()
                            {
                                IdRequerimiento = requerimiento.IdRequerimiento,
                                Cultivo = cultivo.Nombre,
                                CantidadUnitariaKg = cantidadUnitariaKg,
                                Estado = estado.Descripcion,
                                UserIns = User.Identity.Name,
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr()
                            };

                            if (pedido.Origen > 0 & pedido.Destino > 0)
                            {
                                var origen = dc.Bodega.Single(x => x.IdBodega == pedido.Origen.Value);
                                var destino = dc.Bodega.Single(x => x.IdBodega == pedido.Destino.Value);
                                pedidoIndividual.Origen = origen.IdBodega;
                                pedidoIndividual.Destino = destino.IdBodega;
                                asignacionpedidoIndividual.Origen = string.Format("{0} ({1})", origen.Sucursal.Nombre, origen.NombreCorto);
                                asignacionpedidoIndividual.Destino = string.Format("{0} ({1})", destino.Sucursal.Nombre, destino.NombreCorto);
                            }
                            else
                            {
                                pedidoIndividual.OtroOrigen = pedido.OtroOrigen;
                                pedidoIndividual.OtroDestino = pedido.OtroDestino;
                                asignacionpedidoIndividual.Origen = pedido.OtroOrigen;
                                asignacionpedidoIndividual.Destino = pedido.OtroDestino;
                            }

                            requerimiento.CantidadTotalKg += cantidadUnitariaKg;
                            pedidoIndividual.LOG_AsignacionPedido.Add(asignacionpedidoIndividual);
                            dc.LOG_Pedido.InsertOnSubmit(pedidoIndividual);
                            dc.SubmitChanges();

                            // Decrementamos el total
                            cantidadUnitariaTotalKg -= cantidadUnitariaKg;
                        }

                        return RedirectToAction("detalle", "requerimientos", new { id = IdRequerimiento });
                    }
                }
                catch
                {
                    var rv = pedido.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else if (requerimiento.GetRuleViolations().Count() > 0)
                        ModelState.AddRuleViolations(requerimiento.GetRuleViolations());
                    else
                        throw new Exception();
                }
            }

            SetCultivos(pedido.IdCultivo);
            SetEnvases(pedido.IdEnvase);
            SetEstados(pedido.IdEstado);
            SetTipoPedidos(pedido.IdTipoPedido);
            var bodegasOrigenList = SetBodegas(pedido.Origen);
            var bodegasDestinoList = SetBodegas(pedido.Destino);
            ViewData["bodegasOrigenList"] = bodegasOrigenList;
            ViewData["bodegasDestinoList"] = bodegasDestinoList;
            ViewData["IdRequerimiento"] = requerimiento.IdRequerimiento;
            ViewData["msgerr"] = msgerr;
            return View("crearmultiples", pedido);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Detalle(int id)
        {
            CheckPermisoAndRedirect(112);

            var pedido = LogisticaHelper.GetPedidoHabilitado(id);
            if (pedido == null) { throw new HttpException(404, "No se ha encontrado el pedido"); }

            DetallePedidoViewModel model = new DetallePedidoViewModel();

            model.Pedido = pedido;
            model.PedidoAsignado = LogisticaHelper.GetPedidoAsignadoExistente(pedido.IdPedido);
            model.Requerimiento = LogisticaHelper.GetRequerimientoExistente(model.PedidoAsignado.IdRequerimiento);
            model.CamionAsignado = LogisticaHelper.GetCamionAsignado(pedido.IdPedido);//Opcional
            model.Movimiento = LogisticaHelper.GetMovimientoPorPedidoYHabilitado(pedido.IdPedido);//Opcional
            model.Controles = LogisticaHelper.GetControles(pedido.IdPedido);//Opcional
            model.Requerimiento = LogisticaHelper.GetRequerimientoExistente(model.PedidoAsignado.IdRequerimiento);

            if (model.Controles.Count > 0 && (model.Controles.Count <= 4 || model.Controles.Count <= 8))
            {
                model.LlegadaPorteriaOrigen = LogisticaHelper.GetEstadoControlCamionPlantaPorControlesYaHechos(model.Controles, 1);
                model.InicioCargaOrigen = LogisticaHelper.GetEstadoControlCamionPlantaPorControlesYaHechos(model.Controles, 2);
                model.TerminoCargaOrigen = LogisticaHelper.GetEstadoControlCamionPlantaPorControlesYaHechos(model.Controles, 3);
                model.SalidaPorteriaOrigen = LogisticaHelper.GetEstadoControlCamionPlantaPorControlesYaHechos(model.Controles, 4);

                if (model.SalidaPorteriaOrigen != null && model.LlegadaPorteriaOrigen != null)
                {
                    model.TiempoTotalCamionPlantaOrigen = model.SalidaPorteriaOrigen.FechaHoraIns - model.LlegadaPorteriaOrigen.FechaHoraIns;
                }
                
                if (model.TerminoCargaOrigen != null && model.InicioCargaOrigen != null)
                {
                    model.TiempoTotalCamionCargaOrigen = model.TerminoCargaOrigen.FechaHoraIns - model.InicioCargaOrigen.FechaHoraIns;
                }
            }

            if (model.Controles.Count > 4 && model.Controles.Count <= 8)
            {
                model.LlegadaPorteriaDestino = LogisticaHelper.GetEstadoControlCamionPlantaPorControlesYaHechos(model.Controles, 5);
                model.InicioCargaDestino = LogisticaHelper.GetEstadoControlCamionPlantaPorControlesYaHechos(model.Controles, 6);
                model.TerminoCargaDestino = LogisticaHelper.GetEstadoControlCamionPlantaPorControlesYaHechos(model.Controles, 7);
                model.SalidaPorteriaDestino = LogisticaHelper.GetEstadoControlCamionPlantaPorControlesYaHechos(model.Controles, 8);

                if (model.SalidaPorteriaDestino != null && model.LlegadaPorteriaDestino != null)
                {
                    model.TiempoTotalCamionPlantaDestino = model.SalidaPorteriaDestino.FechaHoraIns - model.LlegadaPorteriaDestino.FechaHoraIns;
                }

                if (model.TerminoCargaDestino != null && model.InicioCargaDestino != null)
                {
                    model.TiempoTotalCamionCargaDestino = model.TerminoCargaDestino.FechaHoraIns - model.InicioCargaDestino.FechaHoraIns;
                }
            }

            return View("detalle", model);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id, int IdRequerimiento)
        {
            CheckPermisoAndRedirect(114);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == IdRequerimiento && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == id && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            SetCultivos(pedido.IdCultivo);
            SetEnvases(pedido.IdEnvase);
            SetEstados(pedido.IdEstado);
            SetTipoPedidos(pedido.IdTipoPedido);
            var bodegasOrigenList = SetBodegas(pedido.Origen);
            var bodegasDestinoList = SetBodegas(pedido.Destino);
            ViewData["bodegasOrigenList"] = bodegasOrigenList;
            ViewData["bodegasDestinoList"] = bodegasDestinoList;
            ViewData["IdRequerimiento"] = requerimiento.IdRequerimiento;
            return View("crear", pedido);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(114);

            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == id && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
            var pedidoActual = dc.LOG_Pedido.Single(x => x.IdPedido == pedido.IdPedido);
            int idRequerimiento = int.Parse(formValues["IdRequerimiento"]);
            var requerimiento = dc.LOG_Requerimiento.Single(x => x.IdRequerimiento == idRequerimiento);

            try
            {
                UpdateModel(pedido, new string[] { "IdCultivo", "IdTipoPedido", "IdEnvase", "CantidadUnitariaKg", "IdEstado", "Observacion", "OtroDestino", "OtroOrigen", "Origen", "Destino" });

                if (pedido.Observacion == null) { pedido.Observacion = ""; }

                var cultivo = dc.Cultivo.Single(x => x.IdCultivo == pedido.IdCultivo);
                var estado = dc.LOG_EstadoPedido.Single(x => x.IdEstado == pedido.IdEstado);

                asignacionpedido.CantidadUnitariaKg = pedido.CantidadUnitariaKg;
                asignacionpedido.Cultivo = cultivo.Nombre;
                asignacionpedido.Estado = estado.Descripcion;

                asignacionpedido.FechaHoraUpd = DateTime.Now;
                asignacionpedido.IpUpd = RemoteAddr();
                asignacionpedido.UserUpd = User.Identity.Name;

                if (pedido.IdTipoPedido != 3) { pedido.IdEnvase = null; }

                if (requerimiento.IdTipoMovimiento == 1)//Interno
                {
                    pedido.OtroDestino = null;
                    pedido.OtroOrigen = null;
                }
                else if (requerimiento.IdTipoMovimiento == 2)//Externo
                {
                    pedido.Origen = null;
                    pedido.Destino = null;
                }

                if (pedido.Origen > 0 & pedido.Destino > 0)
                {
                    var bodega = dc.Bodega.Single(x => x.IdBodega == pedido.Origen.Value);
                    var bodega1 = dc.Bodega.Single(x => x.IdBodega == pedido.Destino.Value);
                    asignacionpedido.Origen = string.Format("{0} ({1})", bodega.Sucursal.Nombre, bodega.NombreCorto);
                    asignacionpedido.Destino = string.Format("{0} ({1})", bodega1.Sucursal.Nombre, bodega1.NombreCorto);
                }
                else
                {
                    asignacionpedido.Origen = pedido.OtroOrigen;
                    asignacionpedido.Destino = pedido.OtroDestino;
                }

                requerimiento.CantidadTotalKg -= pedidoActual.CantidadUnitariaKg;
                requerimiento.CantidadTotalKg += pedido.CantidadUnitariaKg;
                requerimiento.UserUpd = User.Identity.Name;
                requerimiento.FechaHoraUpd = DateTime.Now;
                requerimiento.IpUpd = RemoteAddr();

                pedido.UserUpd = User.Identity.Name;
                pedido.FechaHoraUpd = DateTime.Now;
                pedido.IpUpd = RemoteAddr();

                //Estados
                if (pedido.IdEstado == 1)//En espera de camión
                {
                    EliminarAsignacionCamiones(pedido.IdPedido);
                    EliminarMovimiento(pedido.IdPedido);
                }
                if (pedido.IdEstado == 2)//En tránsito a planta de origen
                {
                    EliminarMovimiento(pedido.IdPedido);
                }
                if (pedido.IdEstado == 3)//En tránsito a planta de destino
                {
                    EliminarMovimientoEntrada(pedido.IdPedido);
                }

                dc.SubmitChanges();
                return RedirectToAction("detalle", "requerimientos", new { id = requerimiento.IdRequerimiento });
            }
            catch
            {
                var rv = pedido.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            SetCultivos(pedido.IdCultivo);
            SetEnvases(pedido.IdEnvase);
            SetEstados(pedido.IdEstado);
            SetTipoPedidos(pedido.IdTipoPedido);
            var bodegasOrigenList = SetBodegas(pedido.Origen);
            var bodegasDestinoList = SetBodegas(pedido.Destino);
            ViewData["bodegasOrigenList"] = bodegasOrigenList;
            ViewData["bodegasDestinoList"] = bodegasDestinoList;
            ViewData["IdRequerimiento"] = requerimiento.IdRequerimiento;
            return View("crear", pedido);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id, int IdRequerimiento)
        {
            CheckPermisoAndRedirect(115);

            var requerimiento = dc.LOG_Requerimiento.SingleOrDefault(x => x.IdRequerimiento == IdRequerimiento && x.IdEstado == 1);
            if (requerimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el requerimiento"); }

            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == id && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            requerimiento.CantidadTotalKg -= pedido.CantidadUnitariaKg;
            requerimiento.UserUpd = User.Identity.Name;
            requerimiento.FechaHoraUpd = DateTime.Now;
            requerimiento.IpUpd = RemoteAddr();

            var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido && x.IdRequerimiento == requerimiento.IdRequerimiento);
            dc.LOG_AsignacionPedido.DeleteOnSubmit(asignacionpedido);

            string msgerr = "";
            string msgok = "";
            try
            {
                pedido.Habilitado = false;
                pedido.UserUpd = User.Identity.Name;
                pedido.FechaHoraUpd = DateTime.Now;
                pedido.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("El pedido N° {0} ha sido eliminado", pedido.IdPedido);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("detalle", "requerimientos", new { id = requerimiento.IdRequerimiento });
        }

        public ActionResult Index()
        {
            return View();
        }

        #region Functions

        private void EliminarAsignacionCamiones(int IdPedido)
        {
            var asignacionCamiones = dc.LOG_AsignacionCamion.Where(x => x.IdPedido == IdPedido);
            foreach (var asignacioncamion in asignacionCamiones)
            {
                dc.LOG_AsignacionCamion.DeleteOnSubmit(asignacioncamion);
            }

            dc.SubmitChanges();
        }

        private void EliminarMovimiento(int IdPedido)
        {
            var movimientos = dc.LOG_Movimiento.Where(x => x.IdPedido == IdPedido && x.Habilitado == true);
            foreach (var movimiento in movimientos)
            {
                movimiento.Habilitado = false;
            }

            dc.SubmitChanges();
        }

        private void EliminarMovimientoEntrada(int p)
        {
            var movimiento = dc.LOG_Movimiento.Single(x => x.IdPedido == p && x.Habilitado == true);
            movimiento.FechaLlegada = null;
            movimiento.PesajeLlegadaKg = null;
            movimiento.DiferenciaPesajesKg = null;
            movimiento.Merma = null;
            movimiento.TotalNeto = null;
            movimiento.IVA = null;
            movimiento.TotalBruto = null;
            dc.SubmitChanges();
        }

        #endregion

        #region Select Lists

        private IEnumerable<SelectListItem> SetBodegas(int? IdBodega)
        {
            IEnumerable<SelectListItem> selectList =
                from x in dc.Bodega
                where x.Habilitada == true
                orderby x.Sucursal.Nombre
                select new SelectListItem
                {
                    Selected = (x.IdBodega == IdBodega && IdBodega != null),
                    Text = string.Format("{0} - {1}", x.Sucursal.Nombre, x.Nombre),
                    Value = x.IdBodega.ToString()
                };
            return selectList;
        }

        private void SetCultivos(int? IdCultivo)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.Cultivo
                where s.Habilitado == true
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdCultivo == IdCultivo && IdCultivo != null),
                    Text = s.Nombre,
                    Value = s.IdCultivo.ToString()
                };
            ViewData["cultivosList"] = selectList;
        }

        private void SetEnvases(int? IdEnvase)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_Envase
                orderby s.IdEnvase
                select new SelectListItem
                {
                    Selected = (s.IdEnvase == IdEnvase && IdEnvase != null),
                    Text = s.Descripcion,
                    Value = s.IdEnvase.ToString()
                };
            ViewData["envasesList"] = selectList;
        }

        private void SetEstados(int? IdEstado)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_EstadoPedido
                orderby s.IdEstado
                select new SelectListItem
                {
                    Selected = (s.IdEstado == IdEstado && IdEstado != null),
                    Text = s.Descripcion,
                    Value = s.IdEstado.ToString()
                };
            ViewData["estadosList"] = selectList;
        }

        private void SetTipoPedidos(int? IdTipoPedido)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_TipoPedido
                orderby s.Descripcion
                select new SelectListItem
                {
                    Selected = (s.IdTipoPedido == IdTipoPedido && IdTipoPedido != null),
                    Text = s.Descripcion,
                    Value = s.IdTipoPedido.ToString()
                };
            ViewData["tipoPedidosList"] = selectList;
        }

        #endregion
    }
}
