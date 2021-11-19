using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using AgroFichasWeb.ViewModels.Recepciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;


namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class RecepcionesController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public RecepcionesController()
        {
            SetCurrentModulo(3); //Recepciones
        }

        #region Index

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "idCultivoContrato", Request.QueryString["idCultivoContrato"] ?? "" },
                { "idSucursal", Request.QueryString["idSucursal"] ?? "" },
                { "idEstadoIngreso", Request.QueryString["idEstadoIngreso"] ?? "" },
                { "idTipoSevicio", Request.QueryString["idTipoSevicio"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        private string ConvertFromBarcodeToID(string barcode)
        {
            int id = ProcesoIngreso.IdFromBarcode(barcode);
            if (id > 0)
                return id.ToString();
            else
                return barcode;
        }

        public ActionResult Index(int? pageIndex, int? idTemporada, int? idEmpresa, int? idCultivo, int? idSucursal, int? idEstadoIngreso, string key = "")
        {
            CheckPermisoAndRedirect(24);

            key = ConvertFromBarcodeToID(key);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var idCultivoSelect = idCultivo ?? 0;
            //var idCultivoContratoSelect = idCultivoContrato ?? 0;
            var idSucursalSelect = idSucursal ?? 0;
            var idEstadoIngresoSelect = idEstadoIngreso ?? 0;

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<ProcesoIngreso> items;
            if (adminTodos)
            {
                items = from pi in dc.ProcesoIngreso
                        join cc in dc.CultivoContrato on pi.IdCultivoContrato equals cc.IdCultivoContrato
                        where pi.IdTemporada == temporada.IdTemporada
                           && (idEmpresaSelect == 0 || pi.IdEmpresa == idEmpresaSelect)
                           && (idCultivoSelect == 0 || cc.IdCultivo == idCultivoSelect)
                           && (idSucursalSelect == 0 || pi.IdSucursal == idSucursalSelect)
                           && ((idEstadoIngresoSelect == 0 && pi.IdEstado != 99) || pi.IdEstado == idEstadoIngresoSelect)
                           && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key)
                        orderby pi.FechaHoraLlegada descending
                        select pi;
            }
            else
            {
                items = from pi in dc.ProcesoIngreso
                        join ua in dc.UsuarioAgricultor on pi.IdAgricultor equals ua.IdAgricultor
                        join su in dc.SYS_User on ua.UserID equals su.UserID
                        join cc in dc.CultivoContrato on pi.IdCultivoContrato equals cc.IdCultivoContrato
                        where pi.IdTemporada == temporada.IdTemporada
                           && (idEmpresaSelect == 0 || pi.IdEmpresa == idEmpresaSelect)
                           && (idCultivoSelect == 0 || cc.IdCultivo == idCultivoSelect)
                           && (idSucursalSelect == 0 || pi.IdSucursal == idSucursalSelect)
                           && ((idEstadoIngresoSelect == 0 && pi.IdEstado != 99) || pi.IdEstado == idEstadoIngresoSelect)
                           && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key)
                           && ua.UserID == CurrentUser.UserID
                        orderby pi.FechaHoraLlegada descending
                        select pi;
            }

            //Diccionario de rutas para el paginador
            var model = new PaginatedList<ProcesoIngreso>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "idCultivo", Request.QueryString["idCultivo"] ?? "" },
                { "idSucursal", Request.QueryString["idSucursal"] ?? "" },
                { "idEstadoIngreso", Request.QueryString["idEstadoIngreso"] ?? "" },
            };

            //ViewData
            ViewData["totalBrulto"] = items.Sum(i => i.PesoBruto) ?? 0;
            ViewData["totalNormal"] = items.Sum(i => i.PesoNormal) ?? 0;
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["idCultivo"] = idCultivoSelect;
            ViewData["cultivo"] = dc.Cultivo.ToList();
            ViewData["idSucursal"] = idSucursalSelect;
            ViewData["sucursales"] = dc.Sucursal.ToList();
            ViewData["idEstadoIngreso"] = idEstadoIngresoSelect;
            ViewData["estadosIngreso"] = dc.EstadoProcesoIngreso.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;
            ViewData["cultivos"] = dc.Cultivo.Where(c => (new int[] { 1, 2, 3, 4, 16, 10}).Contains(c.IdCultivo)).ToList();
            SetCultivoContrato(idCultivoSelect);

            if (key != "" && model.Count == 1)
                return RedirectToAction("detalleingreso", new { id = model.First().IdProcesoIngreso });

            return View(model);
        }

        public ActionResult IndexExport(int idTemporada, int? idEmpresa, int? idCultivo, int? idSucursal, int? idEstadoIngreso, string key = "")
        {
            CheckPermisoAndRedirect(24);

            var idEmpresaSelect = idEmpresa ?? 0;
            var idCultivoContratoSelect = idCultivo ?? 0;
            var idSucursalSelect = idSucursal ?? 0;
            var idEstadoIngresoSelect = idEstadoIngreso ?? 0;

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<RecepcionesExportModel> items;
            if (adminTodos)
            {
                items = from pi in dc.ProcesoIngreso
                        join cc in dc.CultivoContrato on pi.IdCultivoContrato equals cc.IdCultivoContrato
                        where pi.IdTemporada == idTemporada
                           && (idEmpresaSelect == 0 || pi.IdEmpresa == idEmpresaSelect)
                           && (idCultivoContratoSelect == 0 || cc.IdCultivo == idCultivoContratoSelect)
                           && (idSucursalSelect == 0 || pi.IdSucursal == idSucursalSelect)
                           && ((idEstadoIngresoSelect == 0 && pi.IdEstado != 99) || pi.IdEstado == idEstadoIngresoSelect)
                           && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key)
                        orderby pi.FechaHoraLlegada descending
                        select RecepcionesExportModel.FromProcesoIngreso(pi);
            }
            else
            {
                items = from pi in dc.ProcesoIngreso
                        join ua in dc.UsuarioAgricultor on pi.IdAgricultor equals ua.IdAgricultor
                        join su in dc.SYS_User on ua.UserID equals su.UserID
                        join cc in dc.CultivoContrato on pi.IdCultivoContrato equals cc.IdCultivoContrato
                        where pi.IdTemporada == idTemporada
                           && (idEmpresaSelect == 0 || pi.IdEmpresa == idEmpresaSelect)
                           && (idCultivoContratoSelect == 0 || cc.IdCultivo == idCultivoContratoSelect)
                           && (idSucursalSelect == 0 || pi.IdSucursal == idSucursalSelect)
                           && ((idEstadoIngresoSelect == 0 && pi.IdEstado != 99) || pi.IdEstado == idEstadoIngresoSelect)
                           && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key)
                           && ua.UserID == CurrentUser.UserID
                        orderby pi.FechaHoraLlegada descending
                        select RecepcionesExportModel.FromProcesoIngreso(pi);
            }

            return new CsvActionResult<RecepcionesExportModel>(items.ToList(), "Recepciones.csv", 1, ';', null);
        }

        public ActionResult Crear(int idCultivo)
        {
            CheckPermisoAndRedirect(49);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, null, out temporada);

            var model = new Ingreso1StepViewModel(dc, idCultivo, temporada.IdTemporada, CurrentUser.UserID);

            ViewData["cultivos"] = dc.Cultivo.Where(c => (new int[] { 1, 2, 3, 4, 16, 10 }).Contains(c.IdCultivo)).ToList();
            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            return View("Ingreso1Step", model);
        }

        [HttpPost]
        public ActionResult Crear([Bind(Exclude = "FechaLlegada")]Ingreso1StepViewModel model)
        {
            CheckPermisoAndRedirect(49);

            model.FechaLlegada = DateParser.DateFromRequest("FechaLlegada").GetValueOrDefault();

            model.Validate(dc, ModelState);
            if (ModelState.IsValid)
            {
                var ingreso = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("index", new { msgok = "El ingreso fue creado con éxito", idTemporada = model.IdTemporada });
            }
            else
            {
                model.LoadLookups(dc, CurrentUser.UserID);

                ViewData["cultivos"] = dc.Cultivo.Where(c => (new int[] { 1, 2, 3, 4, 16, 10 }).Contains(c.IdCultivo)).ToList();

                Temporada temporada;
                List<Temporada> temporadas = ResolveTemporadas(dc, model.IdTemporada, out temporada);
                ViewData["temporada"] = temporada;
                ViewData["temporadas"] = temporadas;

                return View("Ingreso1Step", model);
            }
        }

        public ActionResult DetalleIngreso(int id)
        {
            CheckPermisoAndRedirect(24);
            var model = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);
            return View(model);
        }

        

        public ActionResult PrintIngreso(int id, int copias = 1)
        {
            CheckPermisoAndRedirect(24);
            var model = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);
            ViewData["copias"] = copias;
            ViewData["hideUsers"] = "1";
            return View("DetalleIngresoPrintable", model);
        }
        public ActionResult PrintIngresoAnalista(int id, int copias = 1)
        {
            CheckPermisoAndRedirect(24);
            var model = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);
            ViewData["copias"] = copias;
            ViewData["hideUsers"] = "1";
            return View("DetalleIngresoPrintableAnalista", model);
        }

        public ActionResult PrintIngresoFinal(int idCultivo, int id, int copias = 1)
        {
            CheckPermisoAndRedirect(24);
            var model = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);
            ViewData["precioSpotCLP"] = dc.PrecioSpot.Where(ps => ps.IdCultivo == model.CultivoContrato.IdCultivo && ps.IdSucursal == model.IdSucursal && ps.IdMoneda == 1 && ps.Fecha <= model.FechaHoraLlegada).OrderByDescending(ps => ps.IdPrecioSpot).ToList();
            ViewData["precioSpotUSD"] = dc.PrecioSpot.Where(ps => ps.IdCultivo == model.CultivoContrato.IdCultivo && ps.IdSucursal == model.IdSucursal && ps.IdMoneda == 2 && ps.Fecha <= model.FechaHoraLlegada).OrderByDescending(ps => ps.IdPrecioSpot).ToList();
            ViewData["analista"] = dc.SYS_User.Single(u => u.UserName == model.UserAnalisis);
            ViewData["responsable"] = dc.SYS_User.Single(u => u.UserName == model.UserVerifica);
            ViewData["tipoServicio"] = model.IdTipoServicio;
            ViewData["copias"] = copias;
            ViewData["hideUsers"] = "1";
            if(idCultivo == 10)
                return View("DetalleIngresoPrintableMaiz", model);
            else
                return View("DetalleIngresoPrintable", model);
        }

        public ActionResult Anular(int id)
        {
            CheckPermisoAndRedirect(25);

            var routeValues = new RouteValueDictionary();
            try
            {
                dc.AnularProcesoIngreso(id, SYS_User.Current().UserName, RemoteAddr());
                routeValues.Add("msgok", "El Ingreso fue anulado con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible anular el ingreso";
                if (ex.Message.Contains("FK_"))
                    msgerr += " porque tiene al menos un *** asociado";
                else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            return RedirectToAction("Index", IndexRouteValues(routeValues));
        }

        public ActionResult Editar(int id, int idAgricultor)
        {
            CheckPermisoAndSucursalRedirect(72, dc.ProcesoIngreso.Single(p => p.IdProcesoIngreso == id).IdSucursal, "recepcion");

            var model = new EditarProcesoIngresoViewModel(dc, id, idAgricultor);

            ViewData["idTipoServicio"] = model.IdTipoServicio;
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View(model);
        }

        [HttpPost]
        public ActionResult Editar(EditarProcesoIngresoViewModel model)
        {
            CheckPermisoAndSucursalRedirect(72, dc.ProcesoIngreso.Single(p => p.IdProcesoIngreso == model.IdProcesoIngreso).IdSucursal, "recepcion");

            model.LoadLookups(dc);
            model.Validate(dc, ModelState);

            if (ModelState.IsValid)
            {
                var ingreso = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("detalleingreso", new { id = ingreso.IdProcesoIngreso });
            }
            else
            {
                ViewData["indexRouteValues"] = IndexRouteValues(null);
                return View("editar", model);
            }
        }

        private void SetCultivoContrato(int? IdCultivoContrato)
        {
            var selectList = dc.CultivoContratoEmpresaParaFiltro.ToList();
            var js = new JavaScriptSerializer();
            ViewData["cultivoContratoList"] = js.Serialize(selectList);
            ViewData["idCultivoContratoSelect"] = IdCultivoContrato;
        }

        #endregion

        #region Crear Agricultor

        /*
         * CREAR AGRICULTOR
         * *******************************************************************/

        public ActionResult CrearAgricultor()
        {
            CheckPermisoAndRedirect(45);
            var agricultor = new Agricultor()
            {
                IsNew = true
            };
            return View("Agricultor", agricultor);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult CrearAgricultor(Agricultor agricultor)
        {
            CheckPermisoAndRedirect(45);

            var relacionados = new List<AgricultorRelacionadoViewModel>();
            UpdateModel(relacionados, "Relacionados");

            if (ModelState.IsValid)
            {
                try
                {
                    if (agricultor.Email == null)
                        agricultor.Email = "";
                    if (agricultor.Fono1 == null)
                        agricultor.Fono1 = "";
                    if (agricultor.Fono2 == null)
                        agricultor.Fono2 = "";

                    agricultor.PasswordDefined = false;
                    agricultor.Rut = Rut.NomarlizarRut(agricultor.Rut);
                    agricultor.UserIns = User.Identity.Name;
                    agricultor.FechaHoraIns = DateTime.Now;
                    agricultor.IpIns = RemoteAddr();
                    agricultor.MobileTag = "";
                    agricultor.Origen = 0;
                    agricultor.IDOleotop = "";
                    agricultor.IDAvenatop = "";
                    agricultor.IDGranotop = "";
                    agricultor.IsNew = true;
                    agricultor.IdProveedor = 1;
                    agricultor.Habilitado = true;
                    dc.Agricultor.InsertOnSubmit(agricultor);
                    dc.SubmitChanges();

                    return View("AgricultorCreado", agricultor);
                }
                catch
                {
                    var rv = agricultor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            return View("Agricultor", agricultor);
        }


        #endregion

        #region Registrar Llegada

        /*
         * REGISTRAR LLEGADA 
         * *******************************************************************/

        public ActionResult LlegadaPaso1(int? idTemporada)
        {
            CheckPermisoAndRedirect(23);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            return View();
        }

        [HttpPost]
        public ActionResult LlegadaPaso2(int idAgricultor, int idTemporada)
        {
            CheckPermisoAndRedirect(23);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var model = new LlegadaViewModel(dc, idAgricultor, temporada.IdTemporada, CurrentUser.UserID);

            
            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;

            return View(model);
        }

        [HttpPost]
        public ActionResult LlegadaPaso3(LlegadaViewModel model)
        {
            CheckPermisoAndRedirect(23);

            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                var ingreso = model.Persist(dc, RemoteAddr());
                return RedirectToAction("llegadafin", new { id = ingreso.IdProcesoIngreso });
            }
            else
            {
                model.LoadLookups(dc, CurrentUser.UserID);

                Temporada temporada;
                List<Temporada> temporadas = ResolveTemporadas(dc, model.IdTemporada, out temporada);

                ViewData["temporada"] = temporada;
                ViewData["temporadas"] = temporadas;

                return View("LlegadaPaso2", model);
            }
        }

        public ActionResult LlegadaFin(int id)
        {
            CheckPermisoAndRedirect(23);
            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);

            return View(ingreso);
        }

        #endregion

        #region Registrar Toma Muestra

        /*
         * REGISTRAR TOMA MUESTRA
         * *******************************************************************/

        private RouteValueDictionary TomaMuestraRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult TomaMuestra1(int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(26);

            key = ConvertFromBarcodeToID(key);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var ingresos = from us in dc.SucursalUsuario
                           join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                           join pi in dc.ProcesoIngreso on su.IdSucursal equals pi.IdSucursal
                           where us.UserID == CurrentUser.UserID
                             && us.ZoneToken == "recepcion"
                             && su.Habilitada
                             && pi.IdTemporada == temporada.IdTemporada
                             && pi.IdEstado == 1
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                           orderby pi.IdProcesoIngreso
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("tomamuestra2", new { id = model.First().IdProcesoIngreso });

            return View(model);
        }

        public ActionResult TomaMuestra2(int id)
        {
            CheckPermisoAndRedirect(26);

            var model = new MuestraViewModel(dc, id);

            ViewData["indexRouteValues"] = TomaMuestraRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            return View(model);
        }

        [HttpPost]
        public ActionResult TomaMuestra2(MuestraViewModel model)
        {
            CheckPermisoAndRedirect(26);

            model.Validate(ModelState);
            model.LoadLookups(dc);
            if (ModelState.IsValid)
            {
                var ingreso = model.Persist(dc, RemoteAddr());
                return RedirectToAction("tomamuestra1", new { idTemporada = model.ProcesoIngreso.IdTemporada });
            }
            else
            {
                ViewData["indexRouteValues"] = TomaMuestraRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                return View("tomamuestra2", model);
            }
        }

        #endregion

        #region Registrar Ingreso a Laboratorio

        /*
         * REGISTRAR INGRESO A LABORATORIO
         * *******************************************************************/

        private RouteValueDictionary LaboratorioRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Laboratorio1(int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(27);

            key = ConvertFromBarcodeToID(key);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var ingresos = from us in dc.SucursalUsuario
                           join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                           join pi in dc.ProcesoIngreso on su.IdSucursal equals pi.IdSucursal
                           where us.UserID == CurrentUser.UserID
                             && us.ZoneToken == "recepcion"
                             && su.Habilitada
                             && pi.IdTemporada == temporada.IdTemporada
                             && pi.IdEstado == 2
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                           orderby pi.IdProcesoIngreso
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("laboratorio2", new { id = model.First().IdProcesoIngreso });

            return View(model);
        }

        public ActionResult Laboratorio2(int id)
        {
            CheckPermisoAndRedirect(27);

            var model = new LaboratorioViewModel(dc, id);

            ViewData["indexRouteValues"] = LaboratorioRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            return View(model);
        }

        [HttpPost]
        public ActionResult Laboratorio2(LaboratorioViewModel model)
        {
            CheckPermisoAndRedirect(27);

            model.Validate(ModelState);
            model.LoadLookups(dc);

            if (ModelState.IsValid)
            {
                var ingreso = model.Persist(dc, RemoteAddr());
                return RedirectToAction("laboratorio1", new { idTemporada = model.ProcesoIngreso.IdTemporada });
            }
            else
            {
                ViewData["indexRouteValues"] = LaboratorioRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                return View("laboratorio2", model);
            }
        }

        #endregion

        #region Registrar Análisis

        /*
         * REGISTRAR ANÁLISIS
         * *******************************************************************/

        private RouteValueDictionary AnalisisRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Analisis1(int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(28);

            key = ConvertFromBarcodeToID(key);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var ingresos = from us in dc.SucursalUsuario
                           join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                           join pi in dc.ProcesoIngreso on su.IdSucursal equals pi.IdSucursal
                           where us.UserID == CurrentUser.UserID
                             && us.ZoneToken == "recepcion"
                             && su.Habilitada
                             && pi.IdTemporada == temporada.IdTemporada
                             && pi.IdEstado == 3
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                           orderby pi.IdProcesoIngreso
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("analisis2", new { id = model.First().IdProcesoIngreso });

            return View(model);
        }

        //Ingreso de analisis desde listado
        public ActionResult Analisis2(int id)
        {
            CheckPermisoAndRedirect(28);

            var model = new AnalisisViewModel(dc, id);

            ViewData["hideAnalisis"] = "1";
            ViewData["indexRouteValues"] = AnalisisRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            return View(model);
        }

        [HttpPost]
        public ActionResult Analisis2(AnalisisViewModel model)
        {
            CheckPermisoAndRedirect(new int[] { 28, 60 });

            model.Validate(dc, ModelState);
            model.LoadLookups(dc);

            if (ModelState.IsValid)
            {
                bool notificarAlertas = false;
                var ingreso = model.Persist(dc, RemoteAddr(), out notificarAlertas);
                var msgerr = "";
                var msgok = "";
                if (ingreso.Autorizado == ProcesoIngreso.ANALISIS_ENREVISION)
                {
                    string msg = "";
                    if (ingreso.NotificarEnRevisionLaboratorio(ControllerContext, out msg))
                        msgok = msg;
                    else
                        msgerr = msg;
                }
                else if (ingreso.Autorizado == ProcesoIngreso.ANALISIS_AUTORIZADO && notificarAlertas)
                {
                    string msg = "";
                    if (ingreso.NotificarAlertaLaboratorio(ControllerContext, out msg))
                        msgok = msg;
                    else
                        msgerr = msg;
                }
                return RedirectToAction("analisisfin", new { id = model.IdProcesoIngreso, msgerr = msgerr, msgok = msgok, idTemporada = model.ProcesoIngreso.IdTemporada });
            }
            else
            {
                ViewData["indexRouteValues"] = AnalisisRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                ViewData["hideAnalisis"] = "1";
                return View("analisis2", model);
            }
        }

        public ActionResult AnalisisFin(int id)
        {
            CheckPermisoAndRedirect(new int[] { 28, 60 });
            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);

            return View(ingreso);
        }

        public ActionResult EditarAnalisis(int id)
        {
            CheckPermisoAndSucursalRedirect(60, dc.ProcesoIngreso.Single(p => p.IdProcesoIngreso == id).IdSucursal, "recepcion");

            var model = new AnalisisViewModel(dc, id);

            ViewData["hideAnalisis"] = "1";
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("Analisis2", model);
        }

        #endregion

        #region Verificación Análisis

        /*
         * VERIFICACION ANALISIS
         * *******************************************************************/

        private RouteValueDictionary VerificacionAnalisisRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "verificado", Request.QueryString["verificado"] },
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult VerificacionAnalisis1(int? idTemporada, int? selectVerificado, string key = "")
        {
            CheckPermisoAndRedirect(1042);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            bool verificado = selectVerificado == 0 || selectVerificado == null ? false : true;
            selectVerificado = selectVerificado ?? 0;
            var ingresos = from pi in dc.ProcesoIngreso
                           join cc in dc.CultivoContrato on pi.IdCultivoContrato equals cc.IdCultivoContrato
                           where pi.IdTemporada == temporada.IdTemporada && cc.IdCultivo == 10
                             && (pi.IdEstado == 4 || pi.IdEstado == 6 || pi.IdEstado == 7 || pi.IdEstado == 8 || pi.IdEstado == 10 || pi.IdEstado == 11)
                             && pi.Verificado == verificado
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                           orderby pi.FechaHoraAnalisis descending
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada},
                { "notificado", selectVerificado }
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["verificado"] = selectVerificado;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada},
                { "notificado", selectVerificado }
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("VerificacionAnalisis2", new { id = model.First().IdProcesoIngreso });

            return View(model);
        }

        public ActionResult VerificacionAnalisis2(int id)
        {
            CheckPermisoAndRedirect(1043);

            var model = new VerificacionAnalisisViewModel(dc, id);

            ViewData["indexRouteValues"] = VerificacionAnalisisRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            return View(model);
        }

        [HttpPost]
        public ActionResult VerificacionAnalisis2(VerificacionAnalisisViewModel model)
        {
            CheckPermisoAndRedirect(1043);

            model.Validate(ModelState);
            model.LoadLookups(dc);

            if (ModelState.IsValid)
            {


                var baseMsg = String.Format("El ingreso ha sido {0} con éxito", "VERIFICADO") /*ingreso.Autorizado == ProcesoIngreso.ANALISIS_AUTORIZADO ? "autorizado" : "rechazado")*/;
                var msgerr = "";
                var msgok = "";
                //if (ingreso.Autorizado == ProcesoIngreso.ANALISIS_RECHAZADO)
                //{
                var ingreso = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                string msg = "";
                //if (ingreso.NotificarMaiz(ControllerContext, CurrentUser.UserName, CurrentUser.FullName, out msg))
                //{ msgok = baseMsg + ". " + msg; }
                //else
                //{

                //    msgerr = baseMsg + ". " + msg;
                //}
                //}}
                //else
                //{
                //    msgok = baseMsg;
                //}

                return RedirectToAction("VerificacionAnalisis1", new { msgerr = msgerr, msgok = msgok, idTemporada = ingreso.IdTemporada });
            }
            else
            {
                ViewData["indexRouteValues"] = VerificacionAnalisisRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                return View("VerificacionAnalisis2", model);
            }
        }

        #endregion

        #region NotificacionMaiz

        /*
         * NOTIFICACION MAIZ
         * *******************************************************************/

        private RouteValueDictionary NotificacionMaizRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "notificado", Request.QueryString["notificado"] },
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult NotificacionMaiz1(int? idTemporada, int? selectNotificado, string key = "")
        {
            CheckPermisoAndRedirect(1040);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);
            
            bool notificado = selectNotificado == 0 || selectNotificado == null ? false : true;
            selectNotificado = selectNotificado ?? 0;
            var ingresos = from pi in dc.ProcesoIngreso
                           join cc in dc.CultivoContrato on pi.IdCultivoContrato equals cc.IdCultivoContrato
                           where pi.IdTemporada == temporada.IdTemporada && cc.IdCultivo == 10
                             && pi.Verificado == true
                             && pi.Notificado == notificado
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                           orderby pi.FechaHoraAnalisis descending
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada},
                { "notificado", selectNotificado }
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["notificado"] = selectNotificado;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada},
                { "notificado", selectNotificado }
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("NotificacionMaiz2", new { id = model.First().IdProcesoIngreso });

            return View(model);
        }

        public ActionResult NotificacionMaiz2(int id)
        {
            CheckPermisoAndRedirect(1041);

            var model = new NotificarMaizViewModel(dc, id);

            ViewData["indexRouteValues"] = NotificacionMaizRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            return View(model);
        }

        [HttpPost]
        public ActionResult NotificacionMaiz2(NotificarMaizViewModel model)
        {
            CheckPermisoAndRedirect(1041);

            model.Validate(ModelState);
            model.LoadLookups(dc);

            if (ModelState.IsValid)
            {
                

                var baseMsg = String.Format("El ingreso ha sido {0} con éxito", "NOTIFICADO") /*ingreso.Autorizado == ProcesoIngreso.ANALISIS_AUTORIZADO ? "autorizado" : "rechazado")*/;
                var msgerr = "";
                var msgok = "";
                //if (ingreso.Autorizado == ProcesoIngreso.ANALISIS_RECHAZADO)
                //{
                var ingreso = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                string msg = "";
                if (ingreso.NotificarMaiz(ControllerContext, CurrentUser.UserName, CurrentUser.FullName, out msg))
                { msgok = baseMsg + ". " + msg; }
                else
                {
                    
                    msgerr = baseMsg + ". " + msg;
                }
                //}}
                //else
                //{
                //    msgok = baseMsg;
                //}

                return RedirectToAction("NotificacionMaiz1", new { msgerr = msgerr, msgok = msgok, idTemporada = ingreso.IdTemporada });
            }
            else
            {
                ViewData["indexRouteValues"] = NotificacionMaizRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                return View("NotificacionMaiz2", model);
            }
        }

        #endregion

        #region Autorizar

        /*
         * AUTORIZAR
         * *******************************************************************/

        private RouteValueDictionary AutorizarRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Autorizar1(int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(30);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var ingresos = from pi in dc.ProcesoIngreso
                           where pi.IdTemporada == temporada.IdTemporada
                             && pi.IdEstado == 5
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                           orderby pi.IdProcesoIngreso
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("autorizar2", new { id = model.First().IdProcesoIngreso });

            return View(model);
        }

        public ActionResult Autorizar2(int id)
        {
            CheckPermisoAndRedirect(30);

            var model = new AutorizarViewModel(dc, id);

            ViewData["indexRouteValues"] = AutorizarRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            return View(model);
        }

        [HttpPost]
        public ActionResult Autorizar2(AutorizarViewModel model)
        {
            CheckPermisoAndRedirect(30);

            model.Validate(ModelState);
            model.LoadLookups(dc);

            if (ModelState.IsValid)
            {
                var ingreso = model.Persist(dc, CurrentUser.UserName, RemoteAddr());

                var baseMsg = String.Format("El ingreso ha sido {0} con éxito", ingreso.Autorizado == ProcesoIngreso.ANALISIS_AUTORIZADO ? "autorizado" : "rechazado");
                var msgerr = "";
                var msgok = "";
                if (ingreso.Autorizado == ProcesoIngreso.ANALISIS_RECHAZADO)
                {
                    string msg = "";
                    if (ingreso.NotificarRechazoFinalLaboratorio(ControllerContext, out msg))
                        msgok = baseMsg + ". " + msg;
                    else
                        msgerr = baseMsg + ". " + msg;
                }
                else
                {
                    msgok = baseMsg;
                }

                return RedirectToAction("autorizar1", new { msgerr = msgerr, msgok = msgok, idTemporada = ingreso.IdTemporada });
            }
            else
            {
                ViewData["indexRouteValues"] = AutorizarRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                return View("autorizar2", model);
            }
        }

        #endregion

        #region Registrar Peso Inicial

        /*
         * REGISTRAR Peso Inicial
         * *******************************************************************/

        private RouteValueDictionary PesoInicialRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult PesoInicial1(int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(31);

            key = ConvertFromBarcodeToID(key);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var ingresos = from us in dc.SucursalUsuario
                           join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                           join pi in dc.ProcesoIngreso on su.IdSucursal equals pi.IdSucursal
                           where us.UserID == CurrentUser.UserID
                             && us.ZoneToken == "recepcion"
                             && su.Habilitada
                             && pi.IdTemporada == temporada.IdTemporada
                             && pi.IdEstado == 4
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                           orderby pi.IdProcesoIngreso
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("pesoinicial2", new { id = model.First().IdProcesoIngreso });

            ViewData["puntosDescarga"] = dc.PuntoDescarga.Where(p => p.Habilitada).ToList();
            ViewData["indexRouteValues"] = PesoInicialRouteValues(new RouteValueDictionary() { { "idTemporada", temporada.IdTemporada } });

            return View(model);
        }

        public ActionResult PesoInicial2(int id)
        {
            CheckPermisoAndRedirect(31);

            var model = new PesoInicialViewModel(dc, id);
            ViewBag.Romana = (from c in dc.CTR_MantenedorRomana where (c.EsPlanta == false && c.Nombre == model.ProcesoIngreso.Sucursal.Nombre && c.Vigente == true) select c).SingleOrDefault();
            ViewData["indexRouteValues"] = PesoInicialRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            
            return View(model);
        }

        [HttpPost]
        public ActionResult PesoInicial2(PesoInicialViewModel model)
        {
            CheckPermisoAndRedirect(new int[] { 31, 61 });

            model.Validate(ModelState);
            model.LoadLookups(dc);
            if (ModelState.IsValid)
            {
                var ingreso = model.Persist(dc, RemoteAddr());
                return RedirectToAction("pesoinicialfin", new { id = model.IdProcesoIngreso });
            }
            else
            {
                ViewData["indexRouteValues"] = PesoInicialRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                return View("pesoinicial2", model);
            }
        }

        public ActionResult PesoInicialFin(int id)
        {
            CheckPermisoAndRedirect(new int[] { 31, 61 });
            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);

            return View(ingreso);
        }

        [HttpPost]
        public ActionResult AsignarPuntoDescarga(AsignarPuntoDescargaViewModel model)
        {
            CheckPermisoAndRedirect(31);

            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == model.IdProcesoIngreso);
            ingreso.IdPuntoDescarga = model.IdPuntoDescarga;
            ingreso.FechaHoraPuntoDescarga = DateTime.Now;

            dc.SubmitChanges();

            return RedirectToAction("pesoinicial1", PesoInicialRouteValues(new RouteValueDictionary() { { "idTemporada", ingreso.IdTemporada } }));
        }

        [HttpPost]
        public ActionResult EliminarPuntoDescarga(int IdProcesoIngreso)
        {
            CheckPermisoAndRedirect(31);

            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == IdProcesoIngreso);
            ingreso.IdPuntoDescarga = null;
            ingreso.FechaHoraPuntoDescarga = null;

            dc.SubmitChanges();

            return RedirectToAction("pesoinicial1", PesoInicialRouteValues(new RouteValueDictionary() { { "idTemporada", ingreso.IdTemporada } }));
        }

        public ActionResult EditarPesoInicial(int id)
        {
            CheckPermisoAndSucursalRedirect(61, dc.ProcesoIngreso.Single(p => p.IdProcesoIngreso == id).IdSucursal, "recepcion");

            var model = new PesoInicialViewModel(dc, id);
            ViewBag.Romana = (from c in dc.CTR_MantenedorRomana where (c.EsPlanta == false && c.Nombre == model.ProcesoIngreso.Sucursal.Nombre && c.Vigente == true) select c).SingleOrDefault();
            ViewData["indexRouteValues"] = PesoInicialRouteValues(null);
            return View("PesoInicial2", model);
        }


        #endregion

        #region Registrar Peso Final

        /*
         * REGISTRAR Peso Final
         * *******************************************************************/

        private RouteValueDictionary PesoFinalRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult PesoFinal1(int? idTemporada, string key = "")
        {
            CheckPermisoAndRedirect(32);

            key = ConvertFromBarcodeToID(key);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var ingresos = from us in dc.SucursalUsuario
                           join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                           join pi in dc.ProcesoIngreso on su.IdSucursal equals pi.IdSucursal
                           where us.UserID == CurrentUser.UserID
                             && us.ZoneToken == "recepcion"
                             && su.Habilitada
                             && pi.IdTemporada == temporada.IdTemporada
                             && pi.IdEstado == 6
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                           orderby pi.IdProcesoIngreso
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "key", key },
                { "idTemporada", temporada.IdTemporada}
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("pesofinal2", new { id = model.First().IdProcesoIngreso });

            return View(model);
        }

        public ActionResult PesoFinal2(int id)
        {
            CheckPermisoAndRedirect(32);

            var model = new PesoFinalViewModel(dc, id);
            ViewBag.Romana = (from c in dc.CTR_MantenedorRomana where (c.EsPlanta == false && c.Nombre == model.ProcesoIngreso.Sucursal.Nombre && c.Vigente == true) select c).SingleOrDefault();
            ViewData["indexRouteValues"] = PesoFinalRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            return View(model);
        }

        [HttpPost]
        public ActionResult PesoFinal2(PesoFinalViewModel model)
        {
            CheckPermisoAndRedirect(new int[] { 32, 61 });
            model.Validate(dc, ModelState);
            model.LoadLookups(dc);

            if (ModelState.IsValid)
            {
                var ingreso = model.Persist(dc, RemoteAddr());

                return RedirectToAction("pesofinalfin", new { id = model.IdProcesoIngreso });
            }
            else
            {
                ViewData["indexRouteValues"] = PesoFinalRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                return View("pesofinal2", model);
            }
        }

        public ActionResult PesoFinalFin(int id)
        {
            CheckPermisoAndRedirect(new int[] { 32, 61 });
            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);

            return View(ingreso);
        }

        public ActionResult EditarPesoFinal(int id)
        {
            CheckPermisoAndSucursalRedirect(61, dc.ProcesoIngreso.Single(p => p.IdProcesoIngreso == id).IdSucursal, "recepcion");

            var model = new PesoFinalViewModel(dc, id);

            ViewData["indexRouteValues"] = PesoFinalRouteValues(null);
            return View("PesoFinal2", model);
        }
        #endregion

        #region Cerrar Ingreso

        /*
         * CERRAR Ingreso
         * *******************************************************************/

        private RouteValueDictionary CerrarRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        public ActionResult Cerrar1(int? idTemporada, int? idCultivoContrato, string key = "")
        {
            CheckPermisoAndRedirect(33);

            var idCultivoContratoSelect = idCultivoContrato ?? 0;
            key = ConvertFromBarcodeToID(key);

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            var ingresos = from us in dc.SucursalUsuario
                           join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                           join pi in dc.ProcesoIngreso on su.IdSucursal equals pi.IdSucursal
                           where us.UserID == CurrentUser.UserID
                             && us.ZoneToken == "recepcion"
                             && su.Habilitada
                             && pi.IdTemporada == temporada.IdTemporada
                             && pi.IdEstado == 7
                             && (key == "" || pi.Agricultor.Nombre.Contains(key) || pi.IdProcesoIngreso.ToString() == key || pi.Patente.Contains(key))
                             && (idCultivoContratoSelect == 0 || pi.IdCultivoContrato == idCultivoContratoSelect)
                           orderby pi.IdProcesoIngreso
                           select pi;

            var model = new PaginatedList<ProcesoIngreso>(ingresos, 1, 1000000);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "idTemporada", temporada.IdTemporada},
                { "idCultivoContrato", idCultivoContrato },
                { "key", key }
            };

            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["key"] = key;
            ViewData["BaseRouteValues"] = new RouteValueDictionary()
            {
                { "idTemporada", temporada.IdTemporada},
                { "idCultivoContrato", idCultivoContrato },
                { "key", key }
            };

            if (key != "" && model.Count == 1)
                return RedirectToAction("cerrar2", new { id = model.First().IdProcesoIngreso });

            var selectList = (from x in dc.CultivoContrato orderby x.Nombre select new { IdCultivoContrato = x.IdCultivoContrato, Nombre = x.Nombre }).ToList();
            var js = new JavaScriptSerializer();
            ViewData["cultivoContratoList"] = js.Serialize(selectList);
            ViewData["idCultivoContratoSelect"] = idCultivoContrato;

            return View(model);
        }

        public ActionResult Cerrar2(int id)
        {
            CheckPermisoAndRedirect(33);

            var model = new CerrarViewModel(dc, id);

            //PrecioServicio precioServicio = traePreciosServicios(dc, model);

            //ViewData["preciosServicios"] = precioServicio;
            ViewData["hideConveniosPrecios"] = "1";
            ViewData["indexRouteValues"] = CerrarRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
            return View(model);
        }

        [HttpPost]
        public ActionResult Cerrar2(CerrarViewModel model)
        {
            CheckPermisoAndRedirect(33);

            model.LoadLookups(dc);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                //calcularDescuentos(model);
                var ingreso = model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                
                return RedirectToAction("cerrarfin", new { id = model.IdProcesoIngreso });
            }
            else
            {
                ViewData["hideConveniosPrecios"] = "1";
                ViewData["indexRouteValues"] = CerrarRouteValues(new RouteValueDictionary() { { "idTemporada", model.ProcesoIngreso.IdTemporada } });
                return View("cerrar2", model);
            }
        }

        public ActionResult CerrarFin(int id)
        {
            CheckPermisoAndRedirect(33);
            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);
            return View(ingreso);
        }

        public ActionResult AnularCierre(int id)
        {
            CheckPermisoAndRedirect(62);

            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);
            if (!ingreso.EsCierreEditable())
                throw new Exception("No es posible anular este cierre de ingreso. Su estado no lo permite: " + ingreso.EstadoProcesoIngreso.Nombre);

            dc.AnularCierreProcesoIngreso(ingreso.IdProcesoIngreso);

            return RedirectToAction("anularcierrefin", new { id = ingreso.IdProcesoIngreso });
        }

        public ActionResult AnularCierreFin(int id)
        {
            CheckPermisoAndRedirect(62);
            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);

            return View(ingreso);
        }

        #endregion

        #region Análisis Ingreso

        /*
         * ANALISIS Ingreso
         * *******************************************************************/

        public ActionResult ExplainStandarizacion(int id)
        {
            CheckPermisoAndRedirect(24);

            var ingreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == id);
            var result = ingreso.ExplainPesoNormal(dc);

            ViewData["ingreso"] = ingreso;
            return View(result);
        }


        #endregion

        #region Cola Ingreso

        /*
         * COLA Ingreso
         * *******************************************************************/

        public ActionResult ColaIndex()
        {
            CheckPermisoAndRedirect(35);

            var temporada = Temporada.TemporadaActiva();
            ViewData["temporada"] = temporada;

            var sucursales = (from us in dc.SucursalUsuario
                              join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                              where us.UserID == CurrentUser.UserID
                                  && us.ZoneToken == "recepcion"
                                  && su.Habilitada
                              select su).ToList();

            return View(sucursales);
        }

        public ActionResult ColaEstado(int id, int idSucursal)
        {
            CheckPermisoAndRedirect(35);

            var temporada = Temporada.TemporadaActiva();
            var cola = new ColaViewModel(dc, id, idSucursal, temporada.IdTemporada);

            ViewData["temporada"] = temporada;

            return View(cola);
        }

        public ActionResult ColaDescarga(int idSucursal)
        {
            CheckPermisoAndRedirect(35);

            var temporada = Temporada.TemporadaActiva();
            var cola = new ColaDescargaViewModel(dc, idSucursal, temporada.IdTemporada);

            ViewData["temporada"] = temporada;

            return View(cola);
        }
        #endregion

        #region Generar Pdf
        public ActionResult GenerarPdf(int id)
        {
            ProcesoIngreso proceso = dc.ProcesoIngreso.SingleOrDefault(X => X.IdProcesoIngreso == id);

            TicketPdf ticketPdf = new TicketPdf
            {
                Imprimir = true,
                RazonSocial = proceso.Empresa.RazonSocial,
                Comuna = proceso.Sucursal.Comuna.Nombre,
                Actividad = proceso.Empresa.Actividad,
                Ciudad = proceso.Sucursal.Comuna.Provincia.Nombre,
                Rut = proceso.Empresa.Rut,
                Fono = "NO ESPECIFICA",
                Direccion = proceso.Empresa.Direccion,
                Email = proceso.Empresa.Email,
                Patente = proceso.Patente,
                TipoCamion = "NO ESPECIFICA",
                Empresa = proceso.Empresa.Nombre,
                FechaEntrada = (DateTime)proceso.FechaHoraLlegada,
                FechaSalida = null,
                Chofer = proceso.Chofer,
                Transportista = "NO ESPECIFICA",
                TipoOperacion = "NO ESPECIFICA",
                NumeroGuia = proceso.NumeroGuia.ToString(),
                Observacion = proceso.ObservacionesPesoFinal,
                PesoFinal = proceso.PesoFinal,
                PesoInicial = proceso.PesoInicial

            };

            FileStreamResult fileStream = (FileStreamResult)ticketPdf.TicketMain();

            return fileStream;
        }
        #endregion
        //public PrecioServicio traePreciosServicios(AgroFichasDBDataContext dc, CerrarViewModel model)
        //{
        //    try
        //    {
        //        var procesoIngreso = dc.ProcesoIngreso.FirstOrDefault(pi => pi.IdProcesoIngreso == model.IdProcesoIngreso);

        //        List<TipoServicio> tipoServicio = new List<TipoServicio>();

        //        tipoServicio = dc.TipoServicio.ToList();
        //        ppss.Item = new List<PrecioServicio.PrecioServicioList>();

        //        foreach (TipoServicio ts in tipoServicio)
        //        {
        //            PrecioServicio servicio = dc.PrecioServicio.Where(ps => ps.IdCultivo == procesoIngreso.CultivoContrato.IdCultivo && ps.IdSucursal == procesoIngreso.IdSucursal && ps.Fecha <= procesoIngreso.FechaHoraLlegada && ps.IdTipoServicio == ts.IdTipoServicio && ps.Habilitado == true).OrderByDescending(ps => ps.Fecha).FirstOrDefault();

        //            if (servicio != null)
        //            {
        //                ppss.Item.Add(new PrecioServicio.PrecioServicioList()
        //                {
        //                    IdTipoServicio = ts.IdTipoServicio,
        //                    TipoServicio = ts.Nombre,
        //                    Valor = servicio.Valor > 0 ? servicio.Valor : 0,
        //                    Fecha = servicio.Fecha,
        //                    PesoBruto = procesoIngreso.PesoBruto,
        //                    TotalDescuento = 0//Math.Round((servicio.Valor > 0 ? servicio.Valor : 0) * Convert.ToDecimal(procesoIngreso.PesoBruto), 0)
        //                });
        //            }
        //            else
        //            {
        //                ppss.Item.Add(new PrecioServicio.PrecioServicioList()
        //                {
        //                    IdTipoServicio = ts.IdTipoServicio,
        //                    TipoServicio = ts.Nombre,
        //                    Valor = 0,
        //                    Fecha = DateTime.Now,
        //                    PesoBruto = procesoIngreso.PesoBruto,
        //                    TotalDescuento = 0
        //                });
        //            }
        //        }
        //    }catch(Exception ex)
        //    {
        //        throw ex;
        //    }

        //    return ppss;
        //}

        public void calcularDescuentos(CerrarViewModel model)
        {
            try
            {
                var procesoIngreso = dc.ProcesoIngreso.FirstOrDefault(pi => pi.IdProcesoIngreso == model.IdProcesoIngreso);
                List<DescuentoServicio> descuentoServicio = dc.DescuentoServicio.Where(ds => ds.IdProcesoIngreso == procesoIngreso.IdProcesoIngreso).OrderByDescending(ds => ds.IdTipoServicio).ToList();

                List<PrecioServicio> servicoSecado = dc.PrecioServicio.Where(ps => ps.IdCultivo == procesoIngreso.CultivoContrato.IdCultivo && ps.IdSucursal == procesoIngreso.IdSucursal && ps.Fecha <= procesoIngreso.FechaHoraLlegada && ps.IdTipoServicio == 1 && ps.Habilitado == true).OrderByDescending(ps => ps.IdPrecioServicio).ToList();
                List<PrecioServicio> servicioAnalisis = dc.PrecioServicio.Where(ps => ps.IdCultivo == procesoIngreso.CultivoContrato.IdCultivo && ps.IdSucursal == procesoIngreso.IdSucursal && ps.Fecha <= procesoIngreso.FechaHoraLlegada && ps.IdTipoServicio == 2 && ps.Habilitado == true).OrderByDescending(ps => ps.IdPrecioServicio).ToList();
                List<PrecioServicio> servicioLimpieza = dc.PrecioServicio.Where(ps => ps.IdCultivo == procesoIngreso.CultivoContrato.IdCultivo && ps.IdSucursal == procesoIngreso.IdSucursal && ps.Fecha <= procesoIngreso.FechaHoraLlegada && ps.IdTipoServicio == 3 && ps.Habilitado == true).OrderByDescending(ps => ps.IdPrecioServicio).ToList();


                var valorServicioSecado = servicoSecado.FirstOrDefault(sec => sec.IdTipoServicio == 1).Valor;//dc.PrecioServicio.FirstOrDefault(ps => ps.IdTipoServicio == 1 && ps.IdSucursal == procesoIngreso.IdSucursal && ps.Fecha <= procesoIngreso.FechaHoraLlegada);//.OrderByDescending(ps => ps.IdPrecioServicio).ToList();
                var valorServicioAnalisis = servicioAnalisis.FirstOrDefault(sec => sec.IdTipoServicio == 2).Valor;//dc.PrecioServicio.FirstOrDefault(ps => ps.IdTipoServicio == 2 && ps.IdSucursal == procesoIngreso.IdSucursal && ps.Fecha <= procesoIngreso.FechaHoraLlegada);//.OrderByDescending(ps => ps.IdPrecioServicio).ToList();
                var valorServicioLimpieza = servicioLimpieza.FirstOrDefault(sec => sec.IdTipoServicio == 3).Valor;//dc.PrecioServicio.FirstOrDefault(ps => ps.IdTipoServicio == 3 && ps.IdSucursal == procesoIngreso.IdSucursal && ps.Fecha <= procesoIngreso.FechaHoraLlegada);//.OrderByDescending(ps => ps.IdPrecioServicio).ToList();


                var valorHum = model.ProcesoIngreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 62).Valor.Value;
                var valorGP = model.ProcesoIngreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 63).Valor.Value;
                var valorImp = model.ProcesoIngreso.ValorAnalisis.SingleOrDefault(va => va.IdParametroAnalisis == 64).Valor.Value;

                #region calculo secado
                var totalServicioSecado = 0M;
                if (valorHum > 14.5M)
                {
                    var difHumedad = Math.Round(valorHum - 14.5M, 2);
                    var secado = Math.Round(difHumedad * valorServicioSecado, 2);
                    totalServicioSecado = Math.Round(secado * procesoIngreso.PesoBruto.Value, 0);
                }
                #endregion

                #region Cálculo Analisis
                var totalServicioAnalisis = Math.Round(valorServicioAnalisis * procesoIngreso.PesoBruto.Value, 0);
                #endregion

                #region Cálculo Analisis
                var totalServicioLimpieza = 0M;
                if ((valorImp + valorGP) > 5)
                {
                    totalServicioLimpieza = Math.Round(valorServicioLimpieza * procesoIngreso.PesoBruto.Value, 0);
                }
                #endregion

                Descuento descuento = new Descuento();

                descuento.IdTemporada = procesoIngreso.IdTemporada;
                descuento.IdAgricultor = procesoIngreso.IdAgricultor;
                descuento.IdTipoDescuento = 5;
                descuento.Monto = totalServicioSecado + totalServicioAnalisis + totalServicioLimpieza;
                descuento.IdMoneda = 1;
                descuento.Institucion = "2020-2021 GRANOTOP";
                descuento.Fecha = DateTime.Now;
                descuento.FechaVencimiento = DateTime.Now;
                descuento.Comentarios = "Granotop, Ingreso N° " + procesoIngreso.IdProcesoIngreso + " con Peso Neto " + procesoIngreso.PesoBruto.Value.ToString("#,##0");
                    //" ,\nServicios de análisis " + valorServicioAnalisis.ToString("#,##0.00") + " CLP/Kg, Total " + totalServicioAnalisis.ToString("#,##0.00") +  
                    //" ,\nServicio de secado " + valorServicioSecado.ToString("#,##0.00") + "  CLP/Kg, Total " + totalServicioSecado.ToString("#,##0.00") + ", Valor Hum " + valorHum.ToString("#,##0.00") + 
                    //" ,\nServicio de limpieza " + valorServicioLimpieza.ToString("#,##0.00") + "  CLP/Kg, Total " + totalServicioLimpieza.ToString("#,##0.00") + ", Valor GP " + valorGP.ToString("#,##0.00") + " y Valor Imp " + valorImp.ToString("#,##0.00");
                descuento.UserIns = CurrentUser.UserName;
                descuento.FechaHoraIns = DateTime.Now;
                descuento.IpIns = RemoteAddr();

                if (!(totalServicioAnalisis + totalServicioLimpieza + totalServicioSecado <= 0))
                {
                    dc.Descuento.InsertOnSubmit(descuento);
                    dc.SubmitChanges();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
