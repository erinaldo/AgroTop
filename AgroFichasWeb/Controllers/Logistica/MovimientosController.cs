using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class MovimientosController : BaseApplicationController
    {
        //
        // GET: /Movimientos/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public MovimientosController()
        {
            SetCurrentModulo(6);//Logística y Corretaje
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Buscar(int? id, string keyword, int? idRequerimiento)
        {
            CheckPermisoAndRedirect(117);

            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            int key = 0;
            if (!int.TryParse((Request["key"] ?? ""), out key)) { }

            //Verifica si el IdRequerimiento viene por parámetro o por key
            int keyRequest = ResolverKey(idRequerimiento, Request["key"]);
            IQueryable<LOG_Movimiento> items =
                from a in dc.LOG_Movimiento
                join b in dc.LOG_Pedido on a.IdPedido equals b.IdPedido
                join c in dc.LOG_AsignacionPedido on b.IdPedido equals c.IdPedido
                join d in dc.LOG_Requerimiento on c.IdRequerimiento equals d.IdRequerimiento
                where a.Habilitado == true
                && b.Habilitado == true
                && d.IdEstado != 99
                && (keyRequest == 0 || d.IdRequerimiento == keyRequest)
                && (
                    (keyword == "") ||
                    (keyword != "" && a.IdPedido.ToString().Contains(keyword)) ||
                    (keyword != "" && a.NumeroGuia.ToString().Contains(keyword)) ||
                    (keyword != "" && a.PesajeLlegadaKg.ToString().Contains(keyword)) ||
                    (keyword != "" && a.PesajeSalidaKg.ToString().Contains(keyword)) ||
                    (keyword != "" && a.ValorFletePorKgTransportado.ToString().Contains(keyword))
                )
                orderby a.NumeroGuia descending
                select a;

            var model = new MovimientosViewModel();
            model.Columnas = 18;
            model.MostrarCompletar = CheckPermiso(120);
            model.MostrarEditar = CheckPermiso(129);
            model.MostrarEliminar = CheckPermiso(121);

            if (!model.MostrarCompletar)
                model.Columnas--;
            if (!model.MostrarEditar)
                model.Columnas--;
            if (!model.MostrarEliminar)
                model.Columnas--;

            model.IdRequerimiento = (keyRequest == 0 ? null : (int?)keyRequest);
            model.Key = key;

            var pagina = new PaginatedList<LOG_Movimiento>(items, pageIndex, pageSize);
            model.Movimientos = pagina;
            model.SelectListRequerimientos = LogisticaSelectList.SetRequerimientos((keyRequest == 0 ? null : (int?)keyRequest));

            ViewData["keyword"] = keyword;
            return View("index", model);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(129);

            var movimiento = dc.LOG_Movimiento.SingleOrDefault(x => x.IdMovimiento == id && x.Habilitado == true);
            if (movimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha registrado el movimiento de salida"); }

            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == movimiento.IdPedido && x.IdEstado == 5 && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            ViewData["IdPedido"] = pedido.IdPedido;
            ViewData["IdMovimiento"] = movimiento.IdMovimiento;
            return View(movimiento);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(FormCollection formValues)
        {
            CheckPermisoAndRedirect(129);

            int idPedido = int.Parse(formValues["IdPedido"]);
            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == idPedido && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            int idMovimiento = int.Parse(formValues["IdMovimiento"]);
            var movimiento = dc.LOG_Movimiento.SingleOrDefault(x => x.IdMovimiento == idMovimiento && x.Habilitado == true);
            if (movimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el movimiento"); }

            try
            {
                UpdateModel(movimiento, new string[] { "FechaSalida", "PesajeSalidaKg", "FechaLlegada", "PesajeLlegadaKg", "ValorFletePorKgTransportado" });

                string msgerr = "";
                string msgok = "";
                if (movimiento.FechaSalida == null || (movimiento.PesajeSalidaKg == 0))
                    msgerr = "La fecha y el pesaje de salida son requeridos";
                if (movimiento.FechaLlegada == null || (movimiento.PesajeLlegadaKg == 0 || movimiento.PesajeLlegadaKg == null))
                    msgerr = "La fecha y el pesaje de llegada son requeridos";
                if (!string.IsNullOrEmpty(msgerr))
                    return RedirectToAction("editar", new { id = pedido.IdPedido, msgerr = msgerr, msgok = msgok });

                movimiento.DiferenciaPesajesKg = (movimiento.PesajeSalidaKg - movimiento.PesajeLlegadaKg - movimiento.Tolerancia);
                movimiento.Merma = (movimiento.DiferenciaPesajesKg > 5 ? movimiento.DiferenciaPesajesKg : 0);
                movimiento.TotalNeto = (movimiento.PesajeLlegadaKg * movimiento.ValorFletePorKgTransportado);
                movimiento.IVA = (movimiento.TotalNeto * (decimal)0.19);
                movimiento.TotalBruto = (movimiento.TotalNeto + movimiento.IVA);
                movimiento.FechaHoraUpd = DateTime.Now;
                movimiento.IpUpd = RemoteAddr();
                movimiento.UserUpd = User.Identity.Name;
                dc.SubmitChanges();

                return RedirectToAction("index");
            }
            catch
            {
                var rv = movimiento.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["IdPedido"] = pedido.IdPedido;
            ViewData["IdMovimiento"] = movimiento.IdMovimiento;
            return View(movimiento);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(121);

            var movimiento = dc.LOG_Movimiento.SingleOrDefault(x => x.IdMovimiento == id && x.Habilitado == true);
            if (movimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el movimiento"); }

            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == movimiento.IdPedido && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                movimiento.Habilitado = false;
                movimiento.UserUpd = User.Identity.Name;
                movimiento.FechaHoraUpd = DateTime.Now;
                movimiento.IpUpd = RemoteAddr();

                pedido.IdEstado = 2;
                pedido.UserUpd = User.Identity.Name;
                pedido.FechaHoraUpd = DateTime.Now;
                pedido.IpUpd = RemoteAddr();

                var estado = dc.LOG_EstadoPedido.Single(x => x.IdEstado == 2);
                var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                asignacionpedido.Estado = estado.Descripcion;

                dc.SubmitChanges();

                msgok = String.Format("El movimiento N° {0} ha sido eliminado", movimiento.IdMovimiento);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        public ActionResult Exportar(int? id, int? idRequerimiento)
        {
            CheckPermisoAndRedirect(117);

            var idRequerimientoSelect = idRequerimiento ?? 0;

            List<LOG_Movimiento> movimientos =
                (from a in dc.LOG_Movimiento
                 join b in dc.LOG_Pedido on a.IdPedido equals b.IdPedido
                 join c in dc.LOG_AsignacionPedido on b.IdPedido equals c.IdPedido
                 join d in dc.LOG_Requerimiento on c.IdRequerimiento equals d.IdRequerimiento
                 where a.Habilitado == true
                 && b.Habilitado == true
                 && d.IdEstado != 99
                 && (idRequerimientoSelect == 0 || d.IdRequerimiento == idRequerimientoSelect)
                 orderby d.IdRequerimiento descending
                 select a).ToList();

            List<MovimientosExportModel> movimientosAExportar = new List<MovimientosExportModel>();
            foreach (var movimiento in movimientos)
            {
                var requerimiento = (from a in dc.LOG_Movimiento
                                     join b in dc.LOG_Pedido on a.IdPedido equals b.IdPedido
                                     join c in dc.LOG_AsignacionPedido on b.IdPedido equals c.IdPedido
                                     join d in dc.LOG_Requerimiento on c.IdRequerimiento equals d.IdRequerimiento
                                     where a.IdMovimiento == movimiento.IdMovimiento && a.Habilitado == true && b.Habilitado == true && d.IdEstado != 99
                                     select d).Single();

                var transportista = (from a in dc.LOG_Movimiento
                                     join b in dc.LOG_Pedido on a.IdPedido equals b.IdPedido
                                     join c in dc.LOG_AsignacionCamion on b.IdPedido equals c.IdPedido
                                     join d in dc.LOG_Camion on c.IdCamion equals d.IdCamion
                                     join e in dc.LOG_Transportista on d.IdTransportista equals e.IdTransportista
                                     where a.IdMovimiento == movimiento.IdMovimiento && a.Habilitado == true && b.Habilitado == true
                                     select e).Single();

                movimientosAExportar.Add(new MovimientosExportModel()
                {
                    Destino = LogisticaHelper.GetSucursal(movimiento.LOG_Pedido, LogisticaHelper.Proviene.Destino),
                    Diferencia = (movimiento.DiferenciaPesajesKg.HasValue ? movimiento.DiferenciaPesajesKg.Value : -1),
                    Estado = movimiento.LOG_Pedido.LOG_EstadoPedido.Descripcion,
                    GlosaReq = requerimiento.Glosa,
                    KgsLlegada = (movimiento.PesajeLlegadaKg.HasValue ? movimiento.PesajeLlegadaKg.Value : -1),
                    KgsSalida = movimiento.PesajeSalidaKg,
                    Merma = (movimiento.Merma.HasValue ? movimiento.Merma.Value : -1),
                    Neto = (movimiento.TotalNeto.HasValue ? movimiento.TotalNeto.Value : -1),
                    NumGuia = movimiento.NumeroGuia,
                    NumPed = movimiento.IdPedido,
                    NumReq = requerimiento.IdRequerimiento,
                    Precio = movimiento.ValorFletePorKgTransportado,
                    Producto = movimiento.LOG_Pedido.Cultivo.Nombre,
                    Salida = LogisticaHelper.GetSucursal(movimiento.LOG_Pedido, LogisticaHelper.Proviene.Origen),
                    Tolerancia = movimiento.Tolerancia,
                    Transportista = transportista.Nombre,
                    FechaSalida = string.Format("{0:dd/MM/yyyy hh:mm}", movimiento.FechaSalida),
                    FechaLlegada = (movimiento.FechaLlegada.HasValue ? string.Format("{0:dd/MM/yyyy hh:mm}", movimiento.FechaLlegada.Value) : "")
                });
            }

            return new CsvActionResult<MovimientosExportModel>(movimientosAExportar, string.Format("Movimientos Hasta {0:dd-MM-yyyy}.csv", DateTime.Now));
        }

        public ActionResult Index(int? id, int? idRequerimiento)
        {
            CheckPermisoAndRedirect(117);

            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            int key = 0;
            if (!int.TryParse((Request["key"] ?? ""), out key)) { }

            int keyRequest = ResolverKey(idRequerimiento, Request["key"]);
            IQueryable<LOG_Movimiento> items =
                from a in dc.LOG_Movimiento
                join b in dc.LOG_Pedido on a.IdPedido equals b.IdPedido
                join c in dc.LOG_AsignacionPedido on b.IdPedido equals c.IdPedido
                join d in dc.LOG_Requerimiento on c.IdRequerimiento equals d.IdRequerimiento
                where a.Habilitado == true
                && b.Habilitado == true
                && d.IdEstado != 99
                && (keyRequest == 0 || d.IdRequerimiento == keyRequest)
                orderby d.IdRequerimiento descending
                select a;

            var model = new MovimientosViewModel();
            model.Columnas = 18;
            model.MostrarCompletar = CheckPermiso(120);
            model.MostrarEditar = CheckPermiso(129);
            model.MostrarEliminar = CheckPermiso(121);
            model.ErrorMessage = Request["msgerr"];
            model.OKMessage = Request["msgok"];

            if (!model.MostrarCompletar)
                model.Columnas--;
            if (!model.MostrarEditar)
                model.Columnas--;
            if (!model.MostrarEliminar)
                model.Columnas--;

            model.IdRequerimiento = (keyRequest == 0 ? null : (int?)keyRequest);
            model.Key = key;

            var pagina = new PaginatedList<LOG_Movimiento>(items, pageIndex, pageSize);
            model.Movimientos = pagina;
            model.SelectListRequerimientos = LogisticaSelectList.SetRequerimientos((keyRequest == 0 ? null : (int?)keyRequest));

            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult MarcarCompleto(int id)
        {
            CheckPermisoAndRedirect(120);

            var movimiento = dc.LOG_Movimiento.SingleOrDefault(x => x.IdMovimiento == id && x.Habilitado == true);
            if (movimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el movimiento"); }

            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == movimiento.IdPedido && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                movimiento.UserUpd = User.Identity.Name;
                movimiento.FechaHoraUpd = DateTime.Now;
                movimiento.IpUpd = RemoteAddr();

                pedido.IdEstado = 5;
                pedido.UserUpd = User.Identity.Name;
                pedido.FechaHoraUpd = DateTime.Now;
                pedido.IpUpd = RemoteAddr();

                var estado = dc.LOG_EstadoPedido.Single(x => x.IdEstado == 5);
                var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                asignacionpedido.Estado = estado.Descripcion;

                dc.SubmitChanges();

                msgok = String.Format("El pedido N° {0} ha sido marcado como completo", movimiento.IdPedido);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RegistrarEntrada(int id)
        {
            CheckPermisoAndRedirect(119);

            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == id && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            var movimiento = dc.LOG_Movimiento.SingleOrDefault(x => x.IdPedido == pedido.IdPedido && pedido.IdEstado == 3 && x.Habilitado == true);
            if (movimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha registrado el movimiento de salida"); }

            ViewData["IdPedido"] = pedido.IdPedido;
            ViewData["IdMovimiento"] = movimiento.IdMovimiento;
            return View(movimiento);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult RegistrarEntrada(FormCollection formValues)
        {
            CheckPermisoAndRedirect(119);

            int idPedido = int.Parse(formValues["IdPedido"]);
            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == idPedido && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            int idMovimiento = int.Parse(formValues["IdMovimiento"]);
            var movimiento = dc.LOG_Movimiento.SingleOrDefault(x => x.IdMovimiento == idMovimiento && x.Habilitado == true);
            if (movimiento == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el movimiento"); }

            try
            {
                UpdateModel(movimiento, new string[] { "FechaLlegada", "PesajeLlegadaKg" });

                string msgerr = "";
                string msgok = "";
                if (movimiento.FechaLlegada == null || (movimiento.PesajeLlegadaKg == 0 || movimiento.PesajeLlegadaKg == null))
                    msgerr = "La fecha y el pesaje de llegada son requeridos";
                if (!string.IsNullOrEmpty(msgerr))
                    return RedirectToAction("registrarentrada", new { id = pedido.IdPedido, msgerr = msgerr, msgok = msgok });

                pedido.IdEstado = 4;
                var estado = dc.LOG_EstadoPedido.Single(x => x.IdEstado == pedido.IdEstado);
                var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                asignacionpedido.Estado = estado.Descripcion;

                movimiento.DiferenciaPesajesKg = (movimiento.PesajeSalidaKg - movimiento.PesajeLlegadaKg - movimiento.Tolerancia);
                movimiento.Merma = (movimiento.DiferenciaPesajesKg > 5 ? movimiento.DiferenciaPesajesKg : 0);
                movimiento.TotalNeto = (movimiento.PesajeLlegadaKg * movimiento.ValorFletePorKgTransportado);
                movimiento.IVA = (movimiento.TotalNeto * (decimal)0.19);
                movimiento.TotalBruto = (movimiento.TotalNeto + movimiento.IVA);
                movimiento.FechaHoraUpd = DateTime.Now;
                movimiento.IpUpd = RemoteAddr();
                movimiento.UserUpd = User.Identity.Name;
                dc.SubmitChanges();

                #region Notificar: Diferencia por sobre rango (romanas descalibradas)
                if (movimiento.PesajeLlegadaKg.Value > movimiento.PesajeSalidaKg)
                {
                    movimiento.NotificarDiferenciaSobreRango(movimiento.PesajeSalidaKg, movimiento.PesajeLlegadaKg.Value, User.Identity.Name, RemoteAddr());
                }
                #endregion

                #region Notificar: Merma que excede el máximo permitido
                if (movimiento.Merma.Value > 0)
                {
                    movimiento.NotificarMermaExcedeMaximo(movimiento.Merma.Value, User.Identity.Name, RemoteAddr());
                }
                #endregion

                return RedirectToAction("index");
            }
            catch
            {
                var rv = movimiento.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["IdPedido"] = pedido.IdPedido;
            return View(movimiento);
        }

        public ActionResult RegistrarEntradaIndex(int? id, int? idRequerimiento)
        {
            CheckPermisoAndRedirect(119);

            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            int keyRequest = ResolverKey(idRequerimiento, Request["key"]);
            IQueryable<LOG_RegistrarEntrada> items = from x in dc.LOG_RegistrarEntrada
                                                    where (keyRequest == 0 || x.IdRequerimiento == keyRequest)
                                                    orderby x.FechaAsignacion descending
                                                    select x;

            var pagina = new PaginatedList<LOG_RegistrarEntrada>(items, pageIndex, pageSize);
            ViewData["IdRequerimiento"] = idRequerimiento;
            ViewData["key"] = Request["key"];
            ViewData["requerimientosList"] = LogisticaSelectList.SetRequerimientos(keyRequest);
            return View(pagina);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RegistrarSalida(int id)
        {
            CheckPermisoAndRedirect(118);

            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == id && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            var movimiento = new LOG_Movimiento();

            ViewData["IdPedido"] = pedido.IdPedido;
            return View(movimiento);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult RegistrarSalida(LOG_Movimiento movimiento, FormCollection formValues)
        {
            CheckPermisoAndRedirect(118);

            int idPedido = int.Parse(formValues["IdPedido"]);
            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == idPedido && x.Habilitado == true);
            if (pedido == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el pedido"); }

            if (ModelState.IsValid)
            {
                try
                {
                    pedido.IdEstado = 3;
                    var asignacionpedido = dc.LOG_AsignacionPedido.Single(x => x.IdPedido == pedido.IdPedido);
                    var estado = dc.LOG_EstadoPedido.Single(x => x.IdEstado == pedido.IdEstado);
                    asignacionpedido.Estado = estado.Descripcion;

                    var asignacioncamion = dc.LOG_AsignacionCamion.Single(x => x.IdPedido == pedido.IdPedido);
                    movimiento.ValorFletePorKgTransportado = asignacioncamion.ValorFletePorKgTransportado;
                    movimiento.IdMoneda = 1;
                    movimiento.IdPedido = pedido.IdPedido;
                    movimiento.Tolerancia = (movimiento.PesajeSalidaKg * (decimal)0.0015); //0,15%
                    movimiento.Habilitado = true;
                    movimiento.FechaHoraIns = DateTime.Now;
                    movimiento.IpIns = RemoteAddr();
                    movimiento.UserIns = User.Identity.Name;

                    if (movimiento.CCD_NumeroSerieSello == null)
                        movimiento.CCD_NumeroSerieSello = 0;

                    dc.LOG_Movimiento.InsertOnSubmit(movimiento);
                    dc.SubmitChanges();

                    #region Notificar: Camión no apto para carga
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<ul>");
                    var OK = true;
                    if (movimiento.CCD_Limpio == 2)
                    {
                        OK = false;
                        sb.AppendLine("<li>Limpio: No</li>");
                    }
                    if (movimiento.CCD_LibrePlaga == 2)
                    {
                        OK = false;
                        sb.AppendLine("<li>Libre de plaga: No</li>");
                    }
                    if (movimiento.CCD_Seco == 2)
                    {
                        OK = false;
                        sb.AppendLine("<li>Seco: No</li>");
                    }
                    if (movimiento.CCD_ConSello == 2)
                    {
                        OK = false;
                        sb.AppendLine("<li>Con sello: No</li>");
                    }
                    if (movimiento.CCD_CondicionCamion == 2 || movimiento.CCD_CondicionCamion == 3)
                    {
                        OK = false;
                        if (movimiento.CCD_CondicionCamion == 2)
                            sb.AppendLine("<li>Condición general del camión: Regular</li>");
                        if (movimiento.CCD_CondicionCamion == 3)
                            sb.AppendLine("<li>Condición general del camión: Malo</li>");
                    }
                    sb.AppendLine("</ul>");
                    if (!OK)
                    {
                        movimiento.NotificarNoCumpleCondiciones(sb.ToString(), User.Identity.Name, RemoteAddr());
                    }
                    #endregion

                    #region Notificar: Sobrecarga (excede la tara máxima del camión)
                    var camion = dc.LOG_Camion.Single(x => x.IdCamion == asignacioncamion.IdCamion);
                    var diferenciaSobreTaraMaxima = (movimiento.PesajeSalidaKg - camion.TaraMaxima);
                    var porcentajeAdicional = ((diferenciaSobreTaraMaxima / camion.TaraMaxima) * 100);
                    if (porcentajeAdicional > 5)
                    {
                        movimiento.NotificarExcedeTaraMaxima(diferenciaSobreTaraMaxima, porcentajeAdicional, User.Identity.Name, RemoteAddr());
                    }

                    #endregion

                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = movimiento.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["IdPedido"] = pedido.IdPedido;
            return View(movimiento);
        }

        public ActionResult RegistrarSalidaIndex(int? id, int? idRequerimiento)
        {
            CheckPermisoAndRedirect(118);

            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            int keyRequest = ResolverKey(idRequerimiento, Request["key"]);
            IQueryable<LOG_RegistrarSalida> items = from x in dc.LOG_RegistrarSalida
                                                    where (keyRequest == 0 || x.IdRequerimiento == keyRequest)
                                                    orderby x.FechaAsignacion descending
                                                    select x;

            var pagina = new PaginatedList<LOG_RegistrarSalida>(items, pageIndex, pageSize);
            ViewData["IdRequerimiento"] = idRequerimiento;
            ViewData["key"] = Request["key"];
            ViewData["requerimientosList"] = LogisticaSelectList.SetRequerimientos(keyRequest);
            return View(pagina);
        }

        private int ResolverKey(int? idRequerimiento, string keyRequest)
        {
            int key = 0;
            if (!int.TryParse((keyRequest ?? ""), out key)) { }

            if (idRequerimiento.HasValue)
                return idRequerimiento.Value;

            if (key != 0)
                return key;

            return 0;
        }
    }
}
