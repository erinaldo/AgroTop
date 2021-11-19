using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class CamionesController : BaseApplicationController
    {
        //
        // GET: /Camiones/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public CamionesController()
        {
            SetCurrentModulo(6);//Logística y Corretaje
        }

        public ActionResult AsignacionCamiones(int? id, int? idRequerimiento)
        {
            CheckPermisoAndRedirect(116);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            int key = 0;
            if (!int.TryParse((Request["key"] ?? ""), out key)) { }
            IQueryable<LOG_Pedido> items = null;

            items = from a in dc.LOG_Pedido
                    join b in dc.LOG_AsignacionPedido on a.IdPedido equals b.IdPedido
                    join c in dc.LOG_Requerimiento on b.IdRequerimiento equals c.IdRequerimiento
                    where a.Habilitado == true
                    && a.IdEstado == 1
                    && c.IdEstado == 1
                    select a;

            ViewData["requerimientosList"] = LogisticaSelectList.SetRequerimientos(null);
            ViewData["IdRequerimiento"] = 0;

            if (idRequerimiento.HasValue)
            {
                items = from a in dc.LOG_Pedido
                        join b in dc.LOG_AsignacionPedido on a.IdPedido equals b.IdPedido
                        join c in dc.LOG_Requerimiento on b.IdRequerimiento equals c.IdRequerimiento
                        where a.Habilitado == true
                        && a.IdEstado == 1
                        && c.IdEstado == 1
                        && c.IdRequerimiento == idRequerimiento.Value
                        select a;

                ViewData["requerimientosList"] = LogisticaSelectList.SetRequerimientos(idRequerimiento);
                ViewData["IdRequerimiento"] = idRequerimiento.Value;
            }

            if (key != 0)
            {
                items = from a in dc.LOG_Pedido
                        join b in dc.LOG_AsignacionPedido on a.IdPedido equals b.IdPedido
                        join c in dc.LOG_Requerimiento on b.IdRequerimiento equals c.IdRequerimiento
                        where a.Habilitado == true
                        && a.IdEstado == 1
                        && c.IdEstado == 1
                        && c.IdRequerimiento == key
                        select a;

                ViewData["requerimientosList"] = LogisticaSelectList.SetRequerimientos(key);
                ViewData["IdRequerimiento"] = key;
            }

            var pagina = new PaginatedList<LOG_Pedido>(items, pageIndex, pageSize);

            ViewData["key"] = key;
            return View(pagina);
        }

        public ActionResult AsignarPaso1(int id, int IdRequerimiento)
        {
            CheckPermisoAndRedirect(116);

            #region Validación de Entrada

            var pedido = LogisticaHelper.GetPedidoHabilitado(id);
            if (pedido == null) { throw new HttpException(404, "No se ha encontrado el pedido"); }

            var requerimiento = LogisticaHelper.GetRequerimientoCreado(IdRequerimiento);
            if (requerimiento == null) { throw new HttpException(404, "No se ha encontrado el requerimiento"); }

            var asignacionpedido = LogisticaHelper.GetPedidoAsignadoExistente(pedido.IdPedido);

            #endregion

            var model = new AsignarCamionViewModel();
            model.IdRequerimiento = requerimiento.IdRequerimiento;
            model.IdPedido = pedido.IdPedido;
            model.Origen = asignacionpedido.Origen;
            model.Destino = asignacionpedido.Destino;
            model.Reasignar = false;
            return View(model);
        }

        [HttpPost]
        public ActionResult AsignarPaso1(AsignarCamionViewModel model)
        {
            CheckPermisoAndRedirect(116);

            var transportista = LogisticaHelper.GetTransportistaHabilitado(model.IdTransportista);
            if (transportista != null)
            {
                return RedirectToAction("AsignarPaso2", new { id = model.IdPedido, IdRequerimiento = model.IdRequerimiento, IdTransportista = model.IdTransportista });
            }
            else
            {
                model.MsgErr = "No se ha encontrado el transportista";
            }

            model.TransportistasList = LogisticaSelectList.SetTransportistas(null);
            return View(model);
        }

        public ActionResult AsignarPaso2(int id, int IdRequerimiento, int IdTransportista)
        {
            CheckPermisoAndRedirect(116);

            #region Validación de Entrada

            var pedido = LogisticaHelper.GetPedidoHabilitado(id);
            if (pedido == null) { throw new HttpException(404, "No se ha encontrado el pedido"); }

            var requerimiento = LogisticaHelper.GetRequerimientoCreado(IdRequerimiento);
            if (requerimiento == null) { throw new HttpException(404, "No se ha encontrado el requerimiento"); }

            var transportista = LogisticaHelper.GetTransportistaHabilitado(IdTransportista);
            if (transportista == null) { throw new HttpException(404, "No se ha encontrado el transportista"); }

            var asignacionpedido = LogisticaHelper.GetPedidoAsignadoExistente(pedido.IdPedido);

            #endregion

            var model = new AsignarCamionViewModel();
            model.IdPedido = pedido.IdPedido;
            model.IdRequerimiento = requerimiento.IdRequerimiento;
            model.IdTransportista = transportista.IdTransportista;
            model.Transportista = transportista;
            model.CamionesList = LogisticaSelectList.SetCamiones(transportista.IdTransportista);
            model.Origen = asignacionpedido.Origen;
            model.Destino = asignacionpedido.Destino;
            model.Reasignar = false;
            return View(model);
        }

        [HttpPost]
        public ActionResult AsignarPaso2(AsignarCamionViewModel model)
        {
            CheckPermisoAndRedirect(116);

            #region Validación de Entrada

            var OK = true;
            var sb = new StringBuilder();

            sb.AppendLine("<ul>");
            var chofer = LogisticaHelper.GetChoferPorTransportistaYHabilitado(model.IdChofer, model.IdTransportista);
            if (chofer == null)
            {
                sb.AppendLine("<li>No se ha encontrado el chofer</li>");
                OK = false;
            }

            var camion = LogisticaHelper.GetCamionPorTransportistaYHabilitado(model.IdCamion, model.IdTransportista);
            if (camion == null)
            {
                sb.AppendLine("<li>No se ha encontrado el camión</li>");
                OK = false;
            }

            if (model.ValorFletePorKgTransportado <= 0)
            {
                sb.AppendLine("<li>El valor flete por kg transportado no es válido</li>");
                OK = false;
            }
            sb.AppendLine("</ul>");

            #endregion

            if (OK)
            {
                var camionesAsignados = dc.LOG_AsignacionCamion.Where(x => x.IdPedido == model.IdPedido);
                foreach (var camionAsignado in camionesAsignados)
                {
                    dc.LOG_AsignacionCamion.DeleteOnSubmit(camionAsignado);
                }
                dc.SubmitChanges();

                var pedido = LogisticaHelper.GetPedidoHabilitadoExistenteParaEdicion(model.IdPedido, dc);
                pedido.IdEstado = 2;
                var aignacioncamion = new LOG_AsignacionCamion()
                {
                    IdCamion = camion.IdCamion,
                    IdChofer = chofer.IdChofer,
                    IdPedido = pedido.IdPedido,
                    ValorFletePorKgTransportado = model.ValorFletePorKgTransportado,
                    UserIns = User.Identity.Name,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr()
                };
                dc.LOG_AsignacionCamion.InsertOnSubmit(aignacioncamion);
                dc.SubmitChanges();

                var asignacionpedido = LogisticaHelper.GetPedidoAsignadoExistenteParaEdicion(model.IdPedido, dc);
                asignacionpedido.Estado = pedido.LOG_EstadoPedido.Descripcion;
                dc.SubmitChanges();

                //Para mostrar mensaje de éxito
                var transportista = LogisticaHelper.GetTransportistaExistente(model.IdTransportista);
                var requerimiento = LogisticaHelper.GetRequerimientoExistente(model.IdRequerimiento);

                var msg = "Se ha asignado el transportista {0} con el chofer {1} y el camión {2} al pedido N° {3} del requerimiento {4} desde {5} hasta {6}.";

                model.MsgOk = string.Format(msg, transportista.Nombre, chofer.Nombre, camion.Patente, pedido.IdPedido, requerimiento.Glosa, LogisticaHelper.GetSucursal(pedido.IdPedido, LogisticaHelper.Proviene.Origen), LogisticaHelper.GetBodega(pedido.IdPedido, LogisticaHelper.Proviene.Destino));

                return RedirectToAction("detalle", "requerimientos", new { msgok = model.MsgOk, id = model.IdRequerimiento });
            }
            else
            {
                model.MsgErr = sb.ToString();
            }

            //Load Lookups
            model.Chofer = LogisticaHelper.GetChoferHabilitado(model.IdChofer);
            model.Transportista = LogisticaHelper.GetTransportistaExistente(model.IdTransportista);
            model.CamionesList = LogisticaSelectList.SetCamiones(model.IdTransportista, model.IdCamion);
            model.ChoferesList = LogisticaSelectList.SetChoferes(model.IdTransportista, model.IdChofer);
            return View(model);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(96);

            var camion = new LOG_Camion();

            ViewData["transportistasList"] = LogisticaSelectList.SetTransportistas(null);
            ViewData["marcasList"] = LogisticaSelectList.SetMarcas(null);
            ViewData["tipoCamionList"] = LogisticaSelectList.SetTipoCamiones(null);
            ViewData["tipoDescargasList"] = LogisticaSelectList.SetTipoDescargas(null);
            return View("crear", camion);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Crear(LOG_Camion camion, FormCollection formValues)
        {
            CheckPermisoAndRedirect(96);

            if (ModelState.IsValid)
            {
                try
                {
                    if (Request.Files.Count > 0)
                    {
                        for (int i = 0; i < Request.Files.Count; i++)
                        {
                            if (Request.Files[i].ContentLength > 0)
                            {
                                var fileExt = Path.GetExtension(Request.Files[i].FileName);
                                if (fileExt != ".jpg" && fileExt != ".jpeg" && fileExt != ".png")
                                {
                                    //var ruleViolations = camion.GetRuleViolations();
                                    throw new Exception("El archivo " + (i + 1) + " debe ser una fotografía con extensión jpg, jpeg o png.");
                                }
                                var fileName = System.Guid.NewGuid().ToString().Replace("-", "");
                                var path = "~/uploads/seguros";
                                var filePath = Path.Combine(Server.MapPath(path), fileName) + fileExt;
                                var absolutePath = string.Format("{0}/{1}{2}", path, fileName, fileExt);

                                Request.Files[i].SaveAs(filePath);

                                switch (i)
                                {
                                    case 0:
                                        camion.SC_Foto1 = absolutePath;
                                        break;
                                    case 1:
                                        camion.SC_Foto2 = absolutePath;
                                        break;
                                    case 2:
                                        camion.SC_Foto3 = absolutePath;
                                        break;
                                    case 3:
                                        camion.SC_Foto4 = absolutePath;
                                        break;
                                }
                            }
                        }
                    }

                    if (camion.SC_FechaVencimientoPoliza.ToString() == "1/1/0001 12:00:00 AM")
                        camion.SC_FechaVencimientoPoliza = DateTime.Now;
                    if (camion.SC_Foto1 == null)
                        camion.SC_Foto1 = "";
                    if (camion.SC_Foto2 == null)
                        camion.SC_Foto2 = "";
                    if (camion.SC_Foto3 == null)
                        camion.SC_Foto3 = "";
                    if (camion.SC_Foto4 == null)
                        camion.SC_Foto4 = "";
                    if (camion.Modelo == null)
                        camion.Modelo = "";

                    camion.Patente = PatenteValidator.formatearPatente(camion.Patente);
                    camion.Habilitado = true;
                    camion.FechaHoraIns = DateTime.Now;
                    camion.IpIns = RemoteAddr();
                    camion.UserIns = User.Identity.Name;
                    dc.LOG_Camion.InsertOnSubmit(camion);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = camion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["transportistasList"] = LogisticaSelectList.SetTransportistas(camion.IdTransportista);
            ViewData["marcasList"] = LogisticaSelectList.SetMarcas(camion.IdMarca);
            ViewData["tipoCamionList"] = LogisticaSelectList.SetTipoCamiones(camion.IdTipoCamion);
            ViewData["tipoDescargasList"] = LogisticaSelectList.SetTipoDescargas(camion.IdTipoDescarga);
            return View("crear", camion);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(97);

            var camion = dc.LOG_Camion.SingleOrDefault(x => x.IdCamion == id && x.Habilitado == true && x.LOG_Transportista.Habilitado == true);
            if (camion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el camión"); }

            ViewData["transportistasList"] = LogisticaSelectList.SetTransportistas(camion.IdTransportista);
            ViewData["marcasList"] = LogisticaSelectList.SetMarcas(camion.IdMarca);
            ViewData["tipoCamionList"] = LogisticaSelectList.SetTipoCamiones(camion.IdTipoCamion);
            ViewData["tipoDescargasList"] = LogisticaSelectList.SetTipoDescargas(camion.IdTipoDescarga);
            return View("crear", camion);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(97);

            var camion = dc.LOG_Camion.SingleOrDefault(x => x.IdCamion == id && x.Habilitado == true && x.LOG_Transportista.Habilitado == true);
            if (camion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el camión"); }

            try
            {
                UpdateModel(camion, new string[] { "IdTransportista", "Patente", "IdMarca", "Modelo", "IdTipoCamion", "IdTipoDescarga", "TaraMaxima", "PatenteCarro", "SC_NumeroPoliza", "SC_FechaVencimientoPoliza" });

                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        if (Request.Files[i].ContentLength > 0)
                        {
                            var fileExt = Path.GetExtension(Request.Files[i].FileName);
                            if (fileExt != ".jpg" && fileExt != ".jpeg" && fileExt != ".png")
                            {
                                //var ruleViolations = camion.GetRuleViolations();
                                throw new Exception("El archivo " + (i + 1) + " debe ser una fotografía con extensión jpg, jpeg o png.");
                            }
                            var fileName = System.Guid.NewGuid().ToString().Replace("-", "");
                            var path = "~/uploads/seguros";
                            var filePath = Path.Combine(Server.MapPath(path), fileName) + fileExt;
                            var absolutePath = string.Format("{0}/{1}{2}", path, fileName, fileExt);

                            Request.Files[i].SaveAs(filePath);

                            switch (i)
                            {
                                case 0:
                                    camion.SC_Foto1 = absolutePath;
                                    break;
                                case 1:
                                    camion.SC_Foto2 = absolutePath;
                                    break;
                                case 2:
                                    camion.SC_Foto3 = absolutePath;
                                    break;
                                case 3:
                                    camion.SC_Foto4 = absolutePath;
                                    break;
                            }
                        }
                    }
                }

                if (camion.SC_Foto1 == null)
                    camion.SC_Foto1 = "";
                if (camion.SC_Foto2 == null)
                    camion.SC_Foto2 = "";
                if (camion.SC_Foto3 == null)
                    camion.SC_Foto3 = "";
                if (camion.SC_Foto4 == null)
                    camion.SC_Foto4 = "";
                if (camion.Modelo == null)
                    camion.Modelo = "";

                camion.Patente = PatenteValidator.formatearPatente(camion.Patente);
                camion.UserUpd = User.Identity.Name;
                camion.FechaHoraUpd = DateTime.Now;
                camion.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = camion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["transportistasList"] = LogisticaSelectList.SetTransportistas(camion.IdTransportista);
            ViewData["marcasList"] = LogisticaSelectList.SetMarcas(camion.IdMarca);
            ViewData["tipoCamionList"] = LogisticaSelectList.SetTipoCamiones(camion.IdTipoCamion);
            ViewData["tipoDescargasList"] = LogisticaSelectList.SetTipoDescargas(camion.IdTipoDescarga);
            return View("crear", camion);
        }

        public ActionResult EditarAsignacionPaso1(int id, int IdRequerimiento)
        {
            CheckPermisoAndRedirect(116);

            #region Validación de Entrada

            var pedido = LogisticaHelper.GetPedidoHabilitado(id);
            if (pedido == null) { throw new HttpException(404, "No se ha encontrado el pedido"); }

            var requerimiento = LogisticaHelper.GetRequerimientoCreado(IdRequerimiento);
            if (requerimiento == null) { throw new HttpException(404, "No se ha encontrado el requerimiento"); }

            var asignacionpedido = LogisticaHelper.GetPedidoAsignadoExistente(pedido.IdPedido);

            #endregion

            var model = new AsignarCamionViewModel();
            model.IdRequerimiento = requerimiento.IdRequerimiento;
            model.IdPedido = pedido.IdPedido;
            model.Origen = asignacionpedido.Origen;
            model.Destino = asignacionpedido.Destino;
            model.Reasignar = true;
            return View("AsignarPaso1", model);
        }

        [HttpPost]
        public ActionResult EditarAsignacionPaso1(AsignarCamionViewModel model)
        {
            CheckPermisoAndRedirect(116);

            var transportista = LogisticaHelper.GetTransportistaHabilitado(model.IdTransportista);
            if (transportista != null)
            {
                return RedirectToAction("EditarAsignacionPaso2", new { id = model.IdPedido, IdRequerimiento = model.IdRequerimiento, IdTransportista = model.IdTransportista });
            }
            else
            {
                model.MsgErr = "No se ha encontrado el transportista";
            }

            model.TransportistasList = LogisticaSelectList.SetTransportistas(null);
            return View("AsignarPaso1", model);
        }

        public ActionResult EditarAsignacionPaso2(int id, int IdRequerimiento, int IdTransportista)
        {
            CheckPermisoAndRedirect(116);

            #region Validación de Entrada

            var pedido = LogisticaHelper.GetPedidoHabilitado(id);
            if (pedido == null) { throw new HttpException(404, "No se ha encontrado el pedido"); }

            var requerimiento = LogisticaHelper.GetRequerimientoCreado(IdRequerimiento);
            if (requerimiento == null) { throw new HttpException(404, "No se ha encontrado el requerimiento"); }

            var transportista = LogisticaHelper.GetTransportistaHabilitado(IdTransportista);
            if (transportista == null) { throw new HttpException(404, "No se ha encontrado el transportista"); }

            var asignacionpedido = LogisticaHelper.GetPedidoAsignadoExistente(pedido.IdPedido);

            #endregion

            var model = new AsignarCamionViewModel();
            model.IdPedido = pedido.IdPedido;
            model.IdRequerimiento = requerimiento.IdRequerimiento;
            model.IdTransportista = transportista.IdTransportista;
            model.Transportista = transportista;
            model.CamionesList = LogisticaSelectList.SetCamiones(transportista.IdTransportista);
            model.Origen = asignacionpedido.Origen;
            model.Destino = asignacionpedido.Destino;
            model.Reasignar = true;
            return View("AsignarPaso2", model);
        }

        [HttpPost]
        public ActionResult EditarAsignacionPaso2(AsignarCamionViewModel model)
        {
            CheckPermisoAndRedirect(116);

            #region Validación de Entrada

            var OK = true;
            var sb = new StringBuilder();

            sb.AppendLine("<ul>");
            var chofer = LogisticaHelper.GetChoferPorTransportistaYHabilitado(model.IdChofer, model.IdTransportista);
            if (chofer == null)
            {
                sb.AppendLine("<li>No se ha encontrado el chofer</li>");
                OK = false;
            }

            var camion = LogisticaHelper.GetCamionPorTransportistaYHabilitado(model.IdCamion, model.IdTransportista);
            if (camion == null)
            {
                sb.AppendLine("<li>No se ha encontrado el camión</li>");
                OK = false;
            }

            if (model.ValorFletePorKgTransportado <= 0)
            {
                sb.AppendLine("<li>El valor flete por kg transportado no es válido</li>");
                OK = false;
            }
            sb.AppendLine("</ul>");

            #endregion

            if (OK)
            {
                var camionesAsignados = dc.LOG_AsignacionCamion.Where(x => x.IdPedido == model.IdPedido);
                foreach (var camionAsignado in camionesAsignados)
                {
                    dc.LOG_AsignacionCamion.DeleteOnSubmit(camionAsignado);
                }
                dc.SubmitChanges();

                var pedido = LogisticaHelper.GetPedidoHabilitadoExistenteParaEdicion(model.IdPedido, dc);
                var aignacioncamion = new LOG_AsignacionCamion()
                {
                    IdCamion = camion.IdCamion,
                    IdChofer = chofer.IdChofer,
                    IdPedido = pedido.IdPedido,
                    ValorFletePorKgTransportado = model.ValorFletePorKgTransportado,
                    UserIns = User.Identity.Name,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr()
                };
                dc.LOG_AsignacionCamion.InsertOnSubmit(aignacioncamion);
                dc.SubmitChanges();

                var transportista = LogisticaHelper.GetTransportistaExistente(model.IdTransportista);
                var requerimiento = LogisticaHelper.GetRequerimientoExistente(model.IdRequerimiento);

                var msg = "Se ha asignado el transportista {0} con el chofer {1} y el camión {2} al pedido N° {3} del requerimiento {4} desde {5} hasta {6}.";

                model.MsgOk = string.Format(msg, transportista.Nombre, chofer.Nombre, camion.Patente, pedido.IdPedido, requerimiento.Glosa, LogisticaHelper.GetSucursal(pedido.IdPedido, LogisticaHelper.Proviene.Origen), LogisticaHelper.GetBodega(pedido.IdPedido, LogisticaHelper.Proviene.Destino));

                return RedirectToAction("detalle", "requerimientos", new { msgok = model.MsgOk, id = model.IdRequerimiento });
            }
            else
            {
                model.MsgErr = sb.ToString();
            }

            //Load Lookups
            model.Chofer = LogisticaHelper.GetChoferHabilitado(model.IdChofer);
            model.Transportista = LogisticaHelper.GetTransportistaExistente(model.IdTransportista);
            model.CamionesList = LogisticaSelectList.SetCamiones(model.IdTransportista, model.IdCamion);
            model.ChoferesList = LogisticaSelectList.SetChoferes(model.IdTransportista, model.IdChofer);
            return View("AsignarPaso2", model);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(98);

            var camion = dc.LOG_Camion.SingleOrDefault(x => x.IdCamion == id && x.Habilitado == true && x.LOG_Transportista.Habilitado == true);
            if (camion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el camión"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                camion.Habilitado = false;
                camion.UserUpd = User.Identity.Name;
                camion.FechaHoraUpd = DateTime.Now;
                camion.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("El camión {0} ha sido eliminado", camion.Patente);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(95);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<LOG_Camion> items = null;
            string key = Request.QueryString["key"] ?? "";
            int idTransportista = 0;
            if (!int.TryParse(Request.QueryString["IdTransportista"], out idTransportista)) { }

            if (idTransportista != 0)
            {
                items = dc.LOG_Camion.Where(x => x.Habilitado == true && x.LOG_Transportista.Habilitado == true && x.IdTransportista == idTransportista).OrderBy(a => a.LOG_Transportista.Nombre);
            }
            else
            {
                items = dc.LOG_Camion.Where(x => x.Habilitado == true && x.LOG_Transportista.Habilitado == true && (key == "" || x.Patente.Contains(key) || (x.PatenteCarro != null && x.PatenteCarro.Contains(key)) || x.LOG_Marca.Nombre.Contains(key) || x.Modelo.Contains(key) || x.LOG_Transportista.Nombre.Contains(key))).OrderBy(a => a.LOG_Transportista.Nombre);
            }

            var pagina = new PaginatedList<LOG_Camion>(items, pageIndex, pageSize);

            ViewData["key"] = key;
            return View(pagina);
        }
    }
}
