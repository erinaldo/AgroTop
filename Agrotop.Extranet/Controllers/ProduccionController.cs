using Agrotop.Extranet.Controllers.Filters;
using Agrotop.Extranet.Models;
using Agrotop.Extranet.Models.PlataformaProductiva;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Extranet.Controllers
{
    [ExtranetAuthorize]
    public class ProduccionController : BaseController
    {
        //
        // GET: /Produccion/

        public ActionResult Index(int? id)
        {
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            Temporada temporada = null;
            if (id.HasValue)
                temporada = temporadas.SingleOrDefault(t => t.IdTemporada == id.Value);

            if (temporada == null)
                temporada = temporadas.Single(t => t.Activa);

            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;

            ViewData["predios"] = dc.PRO_Predio.Where(p => p.IdAgricultor == agricultor.IdAgricultor && p.IdTemporada == temporada.IdTemporada && p.Habilitado == true);

            return View();
        }

        public ActionResult CrearPredio(int idTemporada)
        {
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == idTemporada);

            IEnumerable<SelectListItem> selectList =
                from s in dc.Comuna
                orderby s.Nombre
                select new SelectListItem
                {
                    //Selected = (s.Id == selectedStateId),
                    Text = s.Nombre,
                    Value = s.IdComuna.ToString()
                };

            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;
            ViewData["selectList"] = selectList;

            return View();
        }

        [HttpPost]
        public ActionResult CrearPredio(PRO_Predio model)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            if (ModelState.IsValid)
            {
                model.IdAgricultor = user.IdAgricultor;
                model.Habilitado = true;
                model.UserIns = agricultor.Rut;
                model.FechaHoraIns = DateTime.Now;
                model.IpIns = RemoteAddr();

                dc.PRO_Predio.InsertOnSubmit(model);
                dc.SubmitChanges();

                return RedirectToAction("index", new { IdTemporada = model.IdTemporada });
            }

            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == model.IdTemporada);

            IEnumerable<SelectListItem> selectList =
                from s in dc.Comuna
                orderby s.Nombre
                select new SelectListItem
                {
                    //Selected = (s.Id == selectedStateId),
                    Text = s.Nombre,
                    Value = s.IdComuna.ToString()
                };

            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;
            ViewData["selectList"] = selectList;

            return View();
        }

        [HttpGet]
        public ActionResult EliminarPredio(int? id)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            var predio = (from p in dc.PRO_Predio
                          where p.IdPredio == id
                             && p.IdAgricultor == agricultor.IdAgricultor
                             && p.Habilitado == true
                          select p).SingleOrDefault();

            if (predio != null)
            {
                predio.Habilitado = false;
                predio.UserUpd = agricultor.Rut;
                predio.FechaHoraUpd = DateTime.Now;
                predio.IpUpd = RemoteAddr();

                dc.SubmitChanges();

                return RedirectToAction("index", new { IdTemporada = predio.IdTemporada });
            }
            else
            {
                return RedirectToAction("index");
            }
        }

        public ActionResult Predio(int id, int idTemporada)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == idTemporada);

            var predio = (from p in dc.PRO_Predio
                          where p.IdPredio == id
                             && p.IdAgricultor == agricultor.IdAgricultor
                             && p.Habilitado == true
                          select p).SingleOrDefault();

            IEnumerable<SelectListItem> selectList =
                from s in dc.Comuna
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdComuna == predio.IdComuna),
                    Text = s.Nombre,
                    Value = s.IdComuna.ToString()
                };

            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;
            ViewData["selectList"] = selectList;

            return View(predio);
        }

        [HttpPost]
        public ActionResult Predio(PRO_Predio model)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            if (ModelState.IsValid)
            {
                {
                    var predio = (from p in dc.PRO_Predio
                                  where p.IdPredio == model.IdPredio
                                     && p.IdAgricultor == agricultor.IdAgricultor
                                     && p.Habilitado == true
                                  select p).Single();

                    predio.IdComuna = model.IdComuna;
                    predio.Nombre = model.Nombre;
                    predio.UserUpd = agricultor.Rut;
                    predio.FechaHoraUpd = DateTime.Now;
                    predio.IpUpd = RemoteAddr();

                    dc.SubmitChanges();

                    return RedirectToAction("index", new { IdTemporada = model.IdTemporada });
                }
            }
            // Different scope
            {
                var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
                Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == model.IdTemporada);

                var predio = (from p in dc.PRO_Predio
                              where p.IdPredio == model.IdPredio
                                 && p.IdAgricultor == agricultor.IdAgricultor
                                 && p.Habilitado == true
                              select p).SingleOrDefault();

                IEnumerable<SelectListItem> selectList =
                    from s in dc.Comuna
                    orderby s.Nombre
                    select new SelectListItem
                    {
                        Selected = (s.IdComuna == predio.IdComuna),
                        Text = s.Nombre,
                        Value = s.IdComuna.ToString()
                    };

                ViewData["temporadas"] = temporadas;
                ViewData["agricultor"] = agricultor;
                ViewData["temporada"] = temporada;
                ViewData["selectList"] = selectList;

                return View(predio);
            }
        }

        public ActionResult CrearPotrero(int idPredio, int idTemporada)
        {
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == idPredio && p.Habilitado == true);

            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == idTemporada);

            IEnumerable<SelectListItem> selectList = (from s in dc.Cultivo
                                                      orderby s.Nombre
                                                      select new SelectListItem
                                                      {
                                                          //Selected = (s.Id == selectedStateId),
                                                          Text = s.Nombre,
                                                          Value = s.IdCultivo.ToString()
                                                      });

            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;
            ViewData["predio"] = predio;
            ViewData["selectList"] = selectList;

            return View();
        }

        [HttpPost]
        public ActionResult CrearPotrero(PRO_Potrero model)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == model.IdPredio && p.Habilitado == true);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            if (ModelState.IsValid)
            {
                model.Habilitado = true;
                model.UserIns = agricultor.Rut;
                model.FechaHoraIns = DateTime.Now;
                model.IpIns = RemoteAddr();

                dc.PRO_Potrero.InsertOnSubmit(model);
                dc.SubmitChanges();

                return RedirectToAction("predio", new { id = model.IdPredio, IdTemporada = temporada.IdTemporada });
            }

            IEnumerable<SelectListItem> selectList = (from s in dc.Cultivo
                                                      orderby s.Nombre
                                                      select new SelectListItem
                                                      {
                                                          //Selected = (s.Id == selectedStateId),
                                                          Text = s.Nombre,
                                                          Value = s.IdCultivo.ToString()
                                                      });

            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;
            ViewData["predio"] = predio;
            ViewData["selectList"] = selectList;

            return View();
        }

        [HttpGet]
        public ActionResult EliminarPotrero(int? id)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            var potrero = (from p in dc.PRO_Potrero
                           where p.IdPotrero == id
                              && p.PRO_Predio.IdAgricultor == agricultor.IdAgricultor
                              && p.Habilitado == true
                           select p).SingleOrDefault();

            if (potrero != null)
            {
                potrero.Habilitado = false;
                potrero.UserUpd = agricultor.Rut;
                potrero.FechaHoraUpd = DateTime.Now;
                potrero.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                return RedirectToAction("predio", new { id = potrero.IdPredio, IdTemporada = potrero.PRO_Predio.IdTemporada });
            }
            else
            {
                return RedirectToAction("index");
            }
        }

        public ActionResult Potrero(int id, int idTemporada)
        {
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == idTemporada);

            //TODO: 1 filter id agricultor
            var potrero = (from p in dc.PRO_Potrero
                           where p.IdPotrero == id
                              && p.PRO_Predio.IdAgricultor == agricultor.IdAgricultor
                              && p.Habilitado == true
                           select p).SingleOrDefault();

            IEnumerable<SelectListItem> selectList = (from s in dc.Cultivo
                                                      orderby s.Nombre
                                                      select new SelectListItem
                                                      {
                                                          Selected = (s.IdCultivo == potrero.IdCultivo),
                                                          Text = s.Nombre,
                                                          Value = s.IdCultivo.ToString()
                                                      });

            Dictionary<int, string> mesesList = new Dictionary<int, string>();
            mesesList.Add(1, "1 mes");
            mesesList.Add(2, "2 meses");
            mesesList.Add(3, "3 meses");
            mesesList.Add(4, "4 meses");
            mesesList.Add(5, "5 meses");
            mesesList.Add(6, "6 meses");
            mesesList.Add(7, "7 meses");
            mesesList.Add(8, "8 meses");
            mesesList.Add(9, "9 meses");
            mesesList.Add(10, "10 meses");
            mesesList.Add(11, "11 meses");
            mesesList.Add(12, "12 meses");

            IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                           select new SelectListItem
                                                           {
                                                               Selected = (s.Key == potrero.CostoFinanciero),
                                                               Text = s.Value,
                                                               Value = s.Key.ToString()
                                                           });

            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;
            ViewData["selectList"] = selectList;
            ViewData["mesesSelectList"] = mesesSelectList;
            ViewData["evaluacionesSuelo"] = (from X in dc.PRO_EvaluacionSuelo
                                             where X.IdPotrero == potrero.IdPotrero
                                             && X.Habilitado
                                             select X).ToList();
            ViewData["parametroAnalisis"] = (from X in dc.PRO_ParametroAnalisis
                                             orderby X.Orden
                                             select X).ToList();
            ViewData["labores"] = (from X in dc.PRO_MecanizacionLabor
                                   orderby X.Descripcion
                                   select X).ToList();
            ViewData["laboresRealizadas"] = (from X in dc.PRO_Actividad
                                             where X.IdPotrero == potrero.IdPotrero
                                             && X.IdTipoActividad == 1
                                             && X.Habilitado
                                             select X).ToList();
            ViewData["insumos"] = (from X in dc.PRO_Insumo
                                   orderby X.Descripcion
                                   select X).ToList();
            ViewData["insumosAplicados"] = (from X in dc.PRO_Actividad
                                            where X.IdPotrero == potrero.IdPotrero
                                            && X.IdTipoActividad == 2
                                            && X.Habilitado
                                            select X).ToList();
            ViewData["fletes"] = (from X in dc.PRO_Flete
                                  orderby X.Descripcion
                                  select X).ToList();
            ViewData["fletesRealizados"] = (from X in dc.PRO_Actividad
                                            where X.IdPotrero == potrero.IdPotrero
                                            && X.IdTipoActividad == 3
                                            && X.Habilitado
                                            select X).ToList();
            ViewData["manoObras"] = (from X in dc.PRO_ManoObra
                                     orderby X.Descripcion
                                     select X).ToList();
            ViewData["manoObrasRealizadas"] = (from X in dc.PRO_Actividad
                                               where X.IdPotrero == potrero.IdPotrero
                                               && X.IdTipoActividad == 4
                                               && X.Habilitado
                                               select X).ToList();
            return View(potrero);
        }

        [HttpPost]
        public ActionResult Potrero(PRO_Potrero model)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == model.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            if (ModelState.IsValid)
            {
                {
                    var potrero = (from p in dc.PRO_Potrero
                                   where p.IdPotrero == model.IdPotrero
                                      && p.PRO_Predio.IdAgricultor == agricultor.IdAgricultor
                                      && p.Habilitado == true
                                   select p).Single();

                    potrero.IdCultivo = model.IdCultivo;
                    potrero.Nombre = model.Nombre;
                    potrero.Superficie = model.Superficie;
                    potrero.CostoFinanciero = model.CostoFinanciero;
                    potrero.CostoArriendo = model.CostoArriendo;
                    potrero.UserUpd = agricultor.Rut;
                    potrero.FechaHoraUpd = DateTime.Now;
                    potrero.IpUpd = RemoteAddr();
                    dc.SubmitChanges();

                    #region 1. EVALUACIÓN DE SUELO

                    if (!string.IsNullOrEmpty(Request["HID__ES__AGREGAR__EVALUACION__SUELO"]) && bool.TryParse(Request["HID__ES__AGREGAR__EVALUACION__SUELO"], out bool isAgregar))
                    {
                        if (isAgregar)
                        {
                            if (!string.IsNullOrEmpty(Request["ES__FECHA"]) && DateTime.TryParse(Request["ES__FECHA"], out DateTime dateTime)) { }
                            else
                            {
                                dateTime = DateTime.Now;
                            }

                            PRO_EvaluacionSuelo evaluacionSuelo = new PRO_EvaluacionSuelo();
                            evaluacionSuelo.IdPotrero = potrero.IdPotrero;
                            evaluacionSuelo.Fecha = dateTime;
                            evaluacionSuelo.Laboratorio = Request["ES__LABORATORIO"];
                            evaluacionSuelo.Habilitado = true;
                            evaluacionSuelo.UserIns = agricultor.Rut;
                            evaluacionSuelo.FechaHoraIns = DateTime.Now;
                            evaluacionSuelo.IpIns = RemoteAddr();
                            dc.PRO_EvaluacionSuelo.InsertOnSubmit(evaluacionSuelo);
                            dc.SubmitChanges();

                            var parametrosAnalisis = (from X in dc.PRO_ParametroAnalisis
                                                      orderby X.Orden
                                                      select X).ToList();

                            foreach (var parametroAnalisis in parametrosAnalisis)
                            {
                                PRO_ResultadoEvaluacionSuelo resultadoEvaluacionSuelo = new PRO_ResultadoEvaluacionSuelo();
                                if (!string.IsNullOrEmpty(Request[string.Format("ES__PARAMETRO__ANALISIS__{0}", parametroAnalisis.IdParametroAnalisis)]))
                                {
                                    decimal ES__VALOR__ANALISIS = Utils.ParseDecimal(Request[string.Format("ES__PARAMETRO__ANALISIS__{0}", parametroAnalisis.IdParametroAnalisis)]);
                                    resultadoEvaluacionSuelo.Valor = ES__VALOR__ANALISIS;
                                }
                                else
                                {
                                    resultadoEvaluacionSuelo.Valor = 0;
                                }
                                resultadoEvaluacionSuelo.IdEvaluacionSuelo = evaluacionSuelo.IdEvaluacionSuelo;
                                resultadoEvaluacionSuelo.IdParametroAnalisis = parametroAnalisis.IdParametroAnalisis;
                                resultadoEvaluacionSuelo.UserIns = agricultor.Rut;
                                resultadoEvaluacionSuelo.FechaHoraIns = DateTime.Now;
                                resultadoEvaluacionSuelo.IpIns = RemoteAddr();
                                dc.PRO_ResultadoEvaluacionSuelo.InsertOnSubmit(resultadoEvaluacionSuelo);
                                dc.SubmitChanges();
                            }
                        }
                    }

                    #endregion

                    #region 2. MECANIZACIÓN Y LABORES

                    if (!string.IsNullOrEmpty(Request["HID__MYL__AGREGAR__MECANIZACION__LABORES"]) && bool.TryParse(Request["HID__MYL__AGREGAR__MECANIZACION__LABORES"], out isAgregar))
                    {
                        if (isAgregar)
                        {
                            Actividad actividad = model.ParseLaborRequest(HttpContext);
                            if (actividad != null)
                            {
                                PRO_Actividad actividad1 = new PRO_Actividad();
                                actividad1.IdPotrero = potrero.IdPotrero;
                                actividad1.IdTipoActividad = 1;
                                actividad1.Descripcion = actividad.Descripcion;
                                actividad1.DescripcionAgricultor = actividad.DescripcionAgricultor;
                                actividad1.Mes = actividad.Mes;
                                actividad1.Cantidad = actividad.Cantidad;
                                actividad1.Unidad = actividad.Unidad;
                                actividad1.ValorUnitario = actividad.ValorUnitario;
                                actividad1.ValorItem = actividad.ValorItem;
                                actividad1.Habilitado = true;
                                actividad1.UserIns = agricultor.Rut;
                                actividad1.FechaHoraIns = DateTime.Now;
                                actividad1.IpIns = RemoteAddr();
                                dc.PRO_Actividad.InsertOnSubmit(actividad1);
                                dc.SubmitChanges();
                            }
                            else
                            {
                                return RedirectToAction("index", new { msgerr = "Ha ocurrido un error mientras se guardaba su labor" });
                            }
                        }
                    }

                    #endregion

                    #region 3. INSUMOS

                    if (!string.IsNullOrEmpty(Request["HID__IN__AGREGAR__INSUMO"]) && bool.TryParse(Request["HID__IN__AGREGAR__INSUMO"], out isAgregar))
                    {
                        if (isAgregar)
                        {
                            Actividad actividad = model.ParseInsumoRequest(HttpContext);
                            if (actividad != null)
                            {
                                PRO_Actividad actividad1 = new PRO_Actividad();
                                actividad1.IdPotrero = potrero.IdPotrero;
                                actividad1.IdTipoActividad = 2;
                                actividad1.Descripcion = actividad.Descripcion;
                                actividad1.DescripcionAgricultor = actividad.DescripcionAgricultor;
                                actividad1.Mes = actividad.Mes;
                                actividad1.Cantidad = actividad.Cantidad;
                                actividad1.Unidad = actividad.Unidad;
                                actividad1.ValorUnitario = actividad.ValorUnitario;
                                actividad1.ValorItem = actividad.ValorItem;
                                actividad1.Habilitado = true;
                                actividad1.UserIns = agricultor.Rut;
                                actividad1.FechaHoraIns = DateTime.Now;
                                actividad1.IpIns = RemoteAddr();
                                dc.PRO_Actividad.InsertOnSubmit(actividad1);
                                dc.SubmitChanges();
                            }
                            else
                            {
                                return RedirectToAction("index", new { msgerr = "Ha ocurrido un error mientras se guardaba su insumo" });
                            }
                        }
                    }

                    #endregion

                    #region 4. FLETES

                    if (!string.IsNullOrEmpty(Request["HID__FL__AGREGAR__FLETE"]) && bool.TryParse(Request["HID__FL__AGREGAR__FLETE"], out isAgregar))
                    {
                        if (isAgregar)
                        {
                            Actividad actividad = model.ParseFleteRequest(HttpContext);
                            if (actividad != null)
                            {
                                PRO_Actividad actividad1 = new PRO_Actividad();
                                actividad1.IdPotrero = potrero.IdPotrero;
                                actividad1.IdTipoActividad = 3;
                                actividad1.Descripcion = actividad.Descripcion;
                                actividad1.DescripcionAgricultor = actividad.DescripcionAgricultor;
                                actividad1.Mes = actividad.Mes;
                                actividad1.Cantidad = actividad.Cantidad;
                                actividad1.Unidad = actividad.Unidad;
                                actividad1.ValorUnitario = actividad.ValorUnitario;
                                actividad1.ValorItem = actividad.ValorItem;
                                actividad1.Habilitado = true;
                                actividad1.UserIns = agricultor.Rut;
                                actividad1.FechaHoraIns = DateTime.Now;
                                actividad1.IpIns = RemoteAddr();
                                dc.PRO_Actividad.InsertOnSubmit(actividad1);
                                dc.SubmitChanges();
                            }
                            else
                            {
                                return RedirectToAction("index", new { msgerr = "Ha ocurrido un error mientras se guardaba su flete" });
                            }
                        }
                    }

                    #endregion

                    #region 5. MANO DE OBRA

                    if (!string.IsNullOrEmpty(Request["HID__MO__AGREGAR__MANO__DE__OBRA"]) && bool.TryParse(Request["HID__MO__AGREGAR__MANO__DE__OBRA"], out isAgregar))
                    {
                        if (isAgregar)
                        {
                            Actividad actividad = model.ParseManoObraRequest(HttpContext);
                            if (actividad != null)
                            {
                                PRO_Actividad actividad1 = new PRO_Actividad();
                                actividad1.IdPotrero = potrero.IdPotrero;
                                actividad1.IdTipoActividad = 4;
                                actividad1.Descripcion = actividad.Descripcion;
                                actividad1.DescripcionAgricultor = actividad.DescripcionAgricultor;
                                actividad1.Mes = actividad.Mes;
                                actividad1.Cantidad = actividad.Cantidad;
                                actividad1.Unidad = actividad.Unidad;
                                actividad1.ValorUnitario = actividad.ValorUnitario;
                                actividad1.ValorItem = actividad.ValorItem;
                                actividad1.Habilitado = true;
                                actividad1.UserIns = agricultor.Rut;
                                actividad1.FechaHoraIns = DateTime.Now;
                                actividad1.IpIns = RemoteAddr();
                                dc.PRO_Actividad.InsertOnSubmit(actividad1);
                                dc.SubmitChanges();
                            }
                            else
                            {
                                return RedirectToAction("index", new { msgerr = "Ha ocurrido un error mientras se guardaba su mano de obra" });
                            }
                        }
                    }

                    #endregion

                    return RedirectToAction("predio", new { id = model.IdPredio, IdTemporada = temporada.IdTemporada });
                }
            }
            // Model not valid
            // Different scope
            {
                var potrero = (from p in dc.PRO_Potrero
                               where p.IdPotrero == model.IdPotrero
                                  && p.PRO_Predio.IdAgricultor == agricultor.IdAgricultor
                                  && p.Habilitado == true
                               select p).SingleOrDefault();

                IEnumerable<SelectListItem> selectList = (from s in dc.Cultivo
                                                          orderby s.Nombre
                                                          select new SelectListItem
                                                          {
                                                              Selected = (s.IdCultivo == potrero.IdCultivo),
                                                              Text = s.Nombre,
                                                              Value = s.IdCultivo.ToString()
                                                          });

                Dictionary<int, string> mesesList = new Dictionary<int, string>();
                mesesList.Add(1, "1 mes");
                mesesList.Add(2, "2 meses");
                mesesList.Add(3, "3 meses");
                mesesList.Add(4, "4 meses");
                mesesList.Add(5, "5 meses");
                mesesList.Add(6, "6 meses");
                mesesList.Add(7, "7 meses");
                mesesList.Add(8, "8 meses");
                mesesList.Add(9, "9 meses");
                mesesList.Add(10, "10 meses");
                mesesList.Add(11, "11 meses");
                mesesList.Add(12, "12 meses");

                IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                               select new SelectListItem
                                                               {
                                                                   Selected = (s.Key == potrero.CostoFinanciero),
                                                                   Text = s.Value,
                                                                   Value = s.Key.ToString()
                                                               });

                ViewData["temporadas"] = temporadas;
                ViewData["agricultor"] = agricultor;
                ViewData["temporada"] = temporada;
                ViewData["selectList"] = selectList;
                ViewData["mesesSelectList"] = mesesSelectList;
                ViewData["evaluacionesSuelo"] = (from X in dc.PRO_EvaluacionSuelo
                                                 where X.IdPotrero == potrero.IdPotrero
                                                 && X.Habilitado
                                                 select X).ToList();
                ViewData["parametroAnalisis"] = (from X in dc.PRO_ParametroAnalisis
                                                 orderby X.Orden
                                                 select X).ToList();
                ViewData["labores"] = (from X in dc.PRO_MecanizacionLabor
                                       orderby X.Descripcion
                                       select X).ToList();
                ViewData["laboresRealizadas"] = (from X in dc.PRO_Actividad
                                                 where X.IdPotrero == potrero.IdPotrero
                                                 && X.IdTipoActividad == 1
                                                 && X.Habilitado
                                                 select X).ToList();
                ViewData["insumos"] = (from X in dc.PRO_Insumo
                                       orderby X.Descripcion
                                       select X).ToList();
                ViewData["insumosAplicados"] = (from X in dc.PRO_Actividad
                                                where X.IdPotrero == potrero.IdPotrero
                                                && X.IdTipoActividad == 2
                                                && X.Habilitado
                                                select X).ToList();
                ViewData["fletes"] = (from X in dc.PRO_Flete
                                      orderby X.Descripcion
                                      select X).ToList();
                ViewData["fletesRealizados"] = (from X in dc.PRO_Actividad
                                                where X.IdPotrero == potrero.IdPotrero
                                                && X.IdTipoActividad == 3
                                                && X.Habilitado
                                                select X).ToList();
                ViewData["manoObras"] = (from X in dc.PRO_ManoObra
                                         orderby X.Descripcion
                                         select X).ToList();
                ViewData["manoObrasRealizadas"] = (from X in dc.PRO_Actividad
                                                   where X.IdPotrero == potrero.IdPotrero
                                                   && X.IdTipoActividad == 4
                                                   && X.Habilitado
                                                   select X).ToList();
                return View(potrero);
            }
        }

        [HttpGet]
        public ActionResult EliminarEvaluacionSuelo(int? id)
        {
            var evaluacionSuelo = dc.PRO_EvaluacionSuelo.SingleOrDefault(X => X.IdEvaluacionSuelo == id && X.Habilitado == true);
            if (evaluacionSuelo == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == evaluacionSuelo.PRO_Potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            evaluacionSuelo.Habilitado = false;
            evaluacionSuelo.UserUpd = agricultor.Rut;
            evaluacionSuelo.FechaHoraUpd = DateTime.Now;
            evaluacionSuelo.IpUpd = RemoteAddr();
            dc.SubmitChanges();

            return RedirectToAction("predio", new { id = predio.IdPredio, IdTemporada = temporada.IdTemporada });
        }

        [HttpGet]
        public ActionResult EliminarMecanizacionLabores(int? id)
        {
            var actividad = dc.PRO_Actividad.SingleOrDefault(X => X.IdActividad == id && X.Habilitado == true);
            if (actividad == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == actividad.PRO_Potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            actividad.Habilitado = false;
            actividad.UserUpd = agricultor.Rut;
            actividad.FechaHoraUpd = DateTime.Now;
            actividad.IpUpd = RemoteAddr();
            dc.SubmitChanges();

            return RedirectToAction("predio", new { id = predio.IdPredio, IdTemporada = temporada.IdTemporada });
        }

        [HttpGet]
        public ActionResult EliminarInsumo(int? id)
        {
            var actividad = dc.PRO_Actividad.SingleOrDefault(X => X.IdActividad == id && X.Habilitado == true);
            if (actividad == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == actividad.PRO_Potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            actividad.Habilitado = false;
            actividad.UserUpd = agricultor.Rut;
            actividad.FechaHoraUpd = DateTime.Now;
            actividad.IpUpd = RemoteAddr();
            dc.SubmitChanges();

            return RedirectToAction("predio", new { id = predio.IdPredio, IdTemporada = temporada.IdTemporada });
        }

        [HttpGet]
        public ActionResult EliminarFlete(int? id)
        {
            var actividad = dc.PRO_Actividad.SingleOrDefault(X => X.IdActividad == id && X.Habilitado == true);
            if (actividad == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == actividad.PRO_Potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            actividad.Habilitado = false;
            actividad.UserUpd = agricultor.Rut;
            actividad.FechaHoraUpd = DateTime.Now;
            actividad.IpUpd = RemoteAddr();
            dc.SubmitChanges();

            return RedirectToAction("predio", new { id = predio.IdPredio, IdTemporada = temporada.IdTemporada });
        }

        [HttpGet]
        public ActionResult EliminarManoObra(int? id)
        {
            var actividad = dc.PRO_Actividad.SingleOrDefault(X => X.IdActividad == id && X.Habilitado == true);
            if (actividad == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == actividad.PRO_Potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            actividad.Habilitado = false;
            actividad.UserUpd = agricultor.Rut;
            actividad.FechaHoraUpd = DateTime.Now;
            actividad.IpUpd = RemoteAddr();
            dc.SubmitChanges();

            return RedirectToAction("predio", new { id = predio.IdPredio, IdTemporada = temporada.IdTemporada });
        }

        [HttpGet]
        public ActionResult ResumenCostos(int? id)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = (from p in dc.PRO_Potrero
                           where p.IdPotrero == id
                              && p.PRO_Predio.IdAgricultor == agricultor.IdAgricultor
                              && p.Habilitado == true
                           select p).SingleOrDefault();

            if (potrero != null)
            {
                var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
                var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
                Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

                ViewData["laboresRealizadas"] = (from X in dc.PRO_Actividad
                                                 where X.IdPotrero == potrero.IdPotrero
                                                 && X.IdTipoActividad == 1
                                                 && X.Habilitado
                                                 select X).ToList();
                ViewData["insumosAplicados"] = (from X in dc.PRO_Actividad
                                                where X.IdPotrero == potrero.IdPotrero
                                                && X.IdTipoActividad == 2
                                                && X.Habilitado
                                                select X).ToList();
                ViewData["fletesRealizados"] = (from X in dc.PRO_Actividad
                                                where X.IdPotrero == potrero.IdPotrero
                                                && X.IdTipoActividad == 3
                                                && X.Habilitado
                                                select X).ToList();
                ViewData["manoObrasRealizadas"] = (from X in dc.PRO_Actividad
                                                   where X.IdPotrero == potrero.IdPotrero
                                                   && X.IdTipoActividad == 4
                                                   && X.Habilitado
                                                   select X).ToList();
                ViewData["temporadas"] = temporadas;
                ViewData["agricultor"] = agricultor;
                ViewData["temporada"] = temporada;
                return View("ResumenCostos", potrero);
            }
            else
            {
                return RedirectToAction("index");
            }
        }

        [HttpGet]
        public ActionResult EditarMecanizacionLabores(int id)
        {
            var actividad = dc.PRO_Actividad.SingleOrDefault(X => X.IdActividad == id && X.Habilitado == true);
            if (actividad == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = dc.PRO_Potrero.Single(p => p.IdPotrero == actividad.IdPotrero);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            IEnumerable<SelectListItem> selectList = (from s in dc.PRO_MecanizacionLabor
                                                      orderby s.Descripcion
                                                      select new SelectListItem
                                                      {
                                                          Selected = (s.Descripcion == actividad.Descripcion),
                                                          Text = s.Descripcion,
                                                          Value = s.Descripcion
                                                      });
            Dictionary<string, string> mesesList = new Dictionary<string, string>();
            mesesList.Add(string.Format("enero {0}",      DateTime.Now.Year), string.Format("enero {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("febrero {0}",    DateTime.Now.Year), string.Format("febrero {0}",    DateTime.Now.Year));
            mesesList.Add(string.Format("marzo {0}",      DateTime.Now.Year), string.Format("marzo {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("abril {0}",      DateTime.Now.Year), string.Format("abril {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("mayo {0}",       DateTime.Now.Year), string.Format("mayo {0}",       DateTime.Now.Year));
            mesesList.Add(string.Format("junio {0}",      DateTime.Now.Year), string.Format("junio {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("julio {0}",      DateTime.Now.Year), string.Format("julio {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("agosto {0}",     DateTime.Now.Year), string.Format("agosto {0}",     DateTime.Now.Year));
            mesesList.Add(string.Format("septiembre {0}", DateTime.Now.Year), string.Format("septiembre {0}", DateTime.Now.Year));
            mesesList.Add(string.Format("octubre {0}",    DateTime.Now.Year), string.Format("octubre {0}",    DateTime.Now.Year));
            mesesList.Add(string.Format("noviembre {0}",  DateTime.Now.Year), string.Format("noviembre {0}",  DateTime.Now.Year));
            mesesList.Add(string.Format("diciembre {0}",  DateTime.Now.Year), string.Format("diciembre {0}",  DateTime.Now.Year));
            IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                           select new SelectListItem
                                                           {
                                                               Selected = (s.Key == actividad.Mes),
                                                               Text = s.Value,
                                                               Value = s.Key.ToString()
                                                           });
            ViewData["selectList"] = selectList;
            ViewData["mesesSelectList"] = mesesSelectList;
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["agricultor"] = agricultor;
            ViewData["predio"] = predio;
            return View("EditarMecanizacionLabores", actividad);
        }

        [HttpPost]
        public ActionResult EditarMecanizacionLabores(PRO_Actividad model)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = dc.PRO_Potrero.Single(p => p.IdPotrero == model.IdPotrero);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            if (ModelState.IsValid)
            {
                {
                    PRO_Actividad actividad = (from p in dc.PRO_Actividad
                                               where p.IdActividad == model.IdActividad
                                               && p.Habilitado == true
                                               select p).Single();

                    actividad.Descripcion           = model.Descripcion;
                    actividad.DescripcionAgricultor = model.DescripcionAgricultor;
                    actividad.Mes                   = model.Mes;
                    actividad.Cantidad              = model.Cantidad;
                    actividad.Unidad                = model.Unidad;
                    actividad.ValorUnitario         = model.ValorUnitario;
                    actividad.ValorItem             = model.ValorItem;
                    actividad.UserUpd               = agricultor.Rut;
                    actividad.FechaHoraUpd          = DateTime.Now;
                    actividad.IpUpd                 = RemoteAddr();
                    dc.SubmitChanges();

                    return RedirectToAction("predio", new { id = potrero.IdPredio, IdTemporada = temporada.IdTemporada });
                }
            }
            {
                PRO_Actividad actividad = (from p in dc.PRO_Actividad
                                           where p.IdActividad == model.IdActividad
                                           && p.Habilitado == true
                                           select p).Single();
                IEnumerable<SelectListItem> selectList = (from s in dc.PRO_MecanizacionLabor
                                                          orderby s.Descripcion
                                                          select new SelectListItem
                                                          {
                                                              Selected = (s.Descripcion == actividad.Descripcion),
                                                              Text = s.Descripcion,
                                                              Value = s.Descripcion
                                                          });
                Dictionary<string, string> mesesList = new Dictionary<string, string>();
                mesesList.Add(string.Format("enero {0}",      DateTime.Now.Year), string.Format("enero {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("febrero {0}",    DateTime.Now.Year), string.Format("febrero {0}",    DateTime.Now.Year));
                mesesList.Add(string.Format("marzo {0}",      DateTime.Now.Year), string.Format("marzo {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("abril {0}",      DateTime.Now.Year), string.Format("abril {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("mayo {0}",       DateTime.Now.Year), string.Format("mayo {0}",       DateTime.Now.Year));
                mesesList.Add(string.Format("junio {0}",      DateTime.Now.Year), string.Format("junio {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("julio {0}",      DateTime.Now.Year), string.Format("julio {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("agosto {0}",     DateTime.Now.Year), string.Format("agosto {0}",     DateTime.Now.Year));
                mesesList.Add(string.Format("septiembre {0}", DateTime.Now.Year), string.Format("septiembre {0}", DateTime.Now.Year));
                mesesList.Add(string.Format("octubre {0}",    DateTime.Now.Year), string.Format("octubre {0}",    DateTime.Now.Year));
                mesesList.Add(string.Format("noviembre {0}",  DateTime.Now.Year), string.Format("noviembre {0}",  DateTime.Now.Year));
                mesesList.Add(string.Format("diciembre {0}",  DateTime.Now.Year), string.Format("diciembre {0}",  DateTime.Now.Year));
                IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                               select new SelectListItem
                                                               {
                                                                   Selected = (s.Key == actividad.Mes),
                                                                   Text = s.Value,
                                                                   Value = s.Key.ToString()
                                                               });
                ViewData["selectList"]      = selectList;
                ViewData["mesesSelectList"] = mesesSelectList;
                ViewData["temporadas"]      = temporadas;
                ViewData["temporada"]       = temporada;
                ViewData["agricultor"]      = agricultor;
                ViewData["predio"]          = predio;
                return View("EditarMecanizacionLabores", actividad);
            }
        }

        [HttpGet]
        public ActionResult EditarInsumo(int id)
        {
            var actividad = dc.PRO_Actividad.SingleOrDefault(X => X.IdActividad == id && X.Habilitado == true);
            if (actividad == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = dc.PRO_Potrero.Single(p => p.IdPotrero == actividad.IdPotrero);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            IEnumerable<SelectListItem> selectList = (from s in dc.PRO_Insumo
                                                      orderby s.Descripcion
                                                      select new SelectListItem
                                                      {
                                                          Selected = (s.Descripcion == actividad.Descripcion),
                                                          Text = s.Descripcion,
                                                          Value = s.Descripcion
                                                      });
            Dictionary<string, string> mesesList = new Dictionary<string, string>();
            mesesList.Add(string.Format("enero {0}",      DateTime.Now.Year), string.Format("enero {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("febrero {0}",    DateTime.Now.Year), string.Format("febrero {0}",    DateTime.Now.Year));
            mesesList.Add(string.Format("marzo {0}",      DateTime.Now.Year), string.Format("marzo {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("abril {0}",      DateTime.Now.Year), string.Format("abril {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("mayo {0}",       DateTime.Now.Year), string.Format("mayo {0}",       DateTime.Now.Year));
            mesesList.Add(string.Format("junio {0}",      DateTime.Now.Year), string.Format("junio {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("julio {0}",      DateTime.Now.Year), string.Format("julio {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("agosto {0}",     DateTime.Now.Year), string.Format("agosto {0}",     DateTime.Now.Year));
            mesesList.Add(string.Format("septiembre {0}", DateTime.Now.Year), string.Format("septiembre {0}", DateTime.Now.Year));
            mesesList.Add(string.Format("octubre {0}",    DateTime.Now.Year), string.Format("octubre {0}",    DateTime.Now.Year));
            mesesList.Add(string.Format("noviembre {0}",  DateTime.Now.Year), string.Format("noviembre {0}",  DateTime.Now.Year));
            mesesList.Add(string.Format("diciembre {0}",  DateTime.Now.Year), string.Format("diciembre {0}",  DateTime.Now.Year));
            IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                           select new SelectListItem
                                                           {
                                                               Selected = (s.Key == actividad.Mes),
                                                               Text = s.Value,
                                                               Value = s.Key.ToString()
                                                           });
            ViewData["selectList"] = selectList;
            ViewData["mesesSelectList"] = mesesSelectList;
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["agricultor"] = agricultor;
            ViewData["predio"] = predio;
            return View("EditarInsumo", actividad);
        }

        [HttpPost]
        public ActionResult EditarInsumo(PRO_Actividad model)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = dc.PRO_Potrero.Single(p => p.IdPotrero == model.IdPotrero);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            if (ModelState.IsValid)
            {
                {
                    PRO_Actividad actividad = (from p in dc.PRO_Actividad
                                               where p.IdActividad == model.IdActividad
                                               && p.Habilitado == true
                                               select p).Single();

                    actividad.Descripcion           = model.Descripcion;
                    actividad.DescripcionAgricultor = model.DescripcionAgricultor;
                    actividad.Mes                   = model.Mes;
                    actividad.Cantidad              = model.Cantidad;
                    actividad.Unidad                = model.Unidad;
                    actividad.ValorUnitario         = model.ValorUnitario;
                    actividad.ValorItem             = model.ValorItem;
                    actividad.UserUpd               = agricultor.Rut;
                    actividad.FechaHoraUpd          = DateTime.Now;
                    actividad.IpUpd                 = RemoteAddr();
                    dc.SubmitChanges();

                    return RedirectToAction("predio", new { id = potrero.IdPredio, IdTemporada = temporada.IdTemporada });
                }
            }
            {
                PRO_Actividad actividad = (from p in dc.PRO_Actividad
                                           where p.IdActividad == model.IdActividad
                                           && p.Habilitado == true
                                           select p).Single();
                IEnumerable<SelectListItem> selectList = (from s in dc.PRO_Insumo
                                                          orderby s.Descripcion
                                                          select new SelectListItem
                                                          {
                                                              Selected = (s.Descripcion == actividad.Descripcion),
                                                              Text = s.Descripcion,
                                                              Value = s.Descripcion
                                                          });
                Dictionary<string, string> mesesList = new Dictionary<string, string>();
                mesesList.Add(string.Format("enero {0}",      DateTime.Now.Year), string.Format("enero {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("febrero {0}",    DateTime.Now.Year), string.Format("febrero {0}",    DateTime.Now.Year));
                mesesList.Add(string.Format("marzo {0}",      DateTime.Now.Year), string.Format("marzo {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("abril {0}",      DateTime.Now.Year), string.Format("abril {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("mayo {0}",       DateTime.Now.Year), string.Format("mayo {0}",       DateTime.Now.Year));
                mesesList.Add(string.Format("junio {0}",      DateTime.Now.Year), string.Format("junio {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("julio {0}",      DateTime.Now.Year), string.Format("julio {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("agosto {0}",     DateTime.Now.Year), string.Format("agosto {0}",     DateTime.Now.Year));
                mesesList.Add(string.Format("septiembre {0}", DateTime.Now.Year), string.Format("septiembre {0}", DateTime.Now.Year));
                mesesList.Add(string.Format("octubre {0}",    DateTime.Now.Year), string.Format("octubre {0}",    DateTime.Now.Year));
                mesesList.Add(string.Format("noviembre {0}",  DateTime.Now.Year), string.Format("noviembre {0}",  DateTime.Now.Year));
                mesesList.Add(string.Format("diciembre {0}",  DateTime.Now.Year), string.Format("diciembre {0}",  DateTime.Now.Year));
                IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                               select new SelectListItem
                                                               {
                                                                   Selected = (s.Key == actividad.Mes),
                                                                   Text = s.Value,
                                                                   Value = s.Key.ToString()
                                                               });
                ViewData["selectList"]      = selectList;
                ViewData["mesesSelectList"] = mesesSelectList;
                ViewData["temporadas"]      = temporadas;
                ViewData["temporada"]       = temporada;
                ViewData["agricultor"]      = agricultor;
                ViewData["predio"]          = predio;
                return View("EditarInsumo", actividad);
            }
        }

        [HttpGet]
        public ActionResult EditarFlete(int id)
        {
            var actividad = dc.PRO_Actividad.SingleOrDefault(X => X.IdActividad == id && X.Habilitado == true);
            if (actividad == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = dc.PRO_Potrero.Single(p => p.IdPotrero == actividad.IdPotrero);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            IEnumerable<SelectListItem> selectList = (from s in dc.PRO_Flete
                                                      orderby s.Descripcion
                                                      select new SelectListItem
                                                      {
                                                          Selected = (s.Descripcion == actividad.Descripcion),
                                                          Text = s.Descripcion,
                                                          Value = s.Descripcion
                                                      });
            Dictionary<string, string> mesesList = new Dictionary<string, string>();
            mesesList.Add(string.Format("enero {0}",      DateTime.Now.Year), string.Format("enero {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("febrero {0}",    DateTime.Now.Year), string.Format("febrero {0}",    DateTime.Now.Year));
            mesesList.Add(string.Format("marzo {0}",      DateTime.Now.Year), string.Format("marzo {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("abril {0}",      DateTime.Now.Year), string.Format("abril {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("mayo {0}",       DateTime.Now.Year), string.Format("mayo {0}",       DateTime.Now.Year));
            mesesList.Add(string.Format("junio {0}",      DateTime.Now.Year), string.Format("junio {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("julio {0}",      DateTime.Now.Year), string.Format("julio {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("agosto {0}",     DateTime.Now.Year), string.Format("agosto {0}",     DateTime.Now.Year));
            mesesList.Add(string.Format("septiembre {0}", DateTime.Now.Year), string.Format("septiembre {0}", DateTime.Now.Year));
            mesesList.Add(string.Format("octubre {0}",    DateTime.Now.Year), string.Format("octubre {0}",    DateTime.Now.Year));
            mesesList.Add(string.Format("noviembre {0}",  DateTime.Now.Year), string.Format("noviembre {0}",  DateTime.Now.Year));
            mesesList.Add(string.Format("diciembre {0}",  DateTime.Now.Year), string.Format("diciembre {0}",  DateTime.Now.Year));
            IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                           select new SelectListItem
                                                           {
                                                               Selected = (s.Key == actividad.Mes),
                                                               Text = s.Value,
                                                               Value = s.Key.ToString()
                                                           });
            ViewData["selectList"] = selectList;
            ViewData["mesesSelectList"] = mesesSelectList;
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["agricultor"] = agricultor;
            ViewData["predio"] = predio;
            return View("EditarFlete", actividad);
        }

        [HttpPost]
        public ActionResult EditarFlete(PRO_Actividad model)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = dc.PRO_Potrero.Single(p => p.IdPotrero == model.IdPotrero);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            if (ModelState.IsValid)
            {
                {
                    PRO_Actividad actividad = (from p in dc.PRO_Actividad
                                               where p.IdActividad == model.IdActividad
                                               && p.Habilitado == true
                                               select p).Single();

                    actividad.Descripcion           = model.Descripcion;
                    actividad.DescripcionAgricultor = model.DescripcionAgricultor;
                    actividad.Mes                   = model.Mes;
                    actividad.Cantidad              = model.Cantidad;
                    actividad.Unidad                = model.Unidad;
                    actividad.ValorUnitario         = model.ValorUnitario;
                    actividad.ValorItem             = model.ValorItem;
                    actividad.UserUpd               = agricultor.Rut;
                    actividad.FechaHoraUpd          = DateTime.Now;
                    actividad.IpUpd                 = RemoteAddr();
                    dc.SubmitChanges();

                    return RedirectToAction("predio", new { id = potrero.IdPredio, IdTemporada = temporada.IdTemporada });
                }
            }
            {
                PRO_Actividad actividad = (from p in dc.PRO_Actividad
                                           where p.IdActividad == model.IdActividad
                                           && p.Habilitado == true
                                           select p).Single();
                IEnumerable<SelectListItem> selectList = (from s in dc.PRO_Flete
                                                          orderby s.Descripcion
                                                          select new SelectListItem
                                                          {
                                                              Selected = (s.Descripcion == actividad.Descripcion),
                                                              Text = s.Descripcion,
                                                              Value = s.Descripcion
                                                          });
                Dictionary<string, string> mesesList = new Dictionary<string, string>();
                mesesList.Add(string.Format("enero {0}",      DateTime.Now.Year), string.Format("enero {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("febrero {0}",    DateTime.Now.Year), string.Format("febrero {0}",    DateTime.Now.Year));
                mesesList.Add(string.Format("marzo {0}",      DateTime.Now.Year), string.Format("marzo {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("abril {0}",      DateTime.Now.Year), string.Format("abril {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("mayo {0}",       DateTime.Now.Year), string.Format("mayo {0}",       DateTime.Now.Year));
                mesesList.Add(string.Format("junio {0}",      DateTime.Now.Year), string.Format("junio {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("julio {0}",      DateTime.Now.Year), string.Format("julio {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("agosto {0}",     DateTime.Now.Year), string.Format("agosto {0}",     DateTime.Now.Year));
                mesesList.Add(string.Format("septiembre {0}", DateTime.Now.Year), string.Format("septiembre {0}", DateTime.Now.Year));
                mesesList.Add(string.Format("octubre {0}",    DateTime.Now.Year), string.Format("octubre {0}",    DateTime.Now.Year));
                mesesList.Add(string.Format("noviembre {0}",  DateTime.Now.Year), string.Format("noviembre {0}",  DateTime.Now.Year));
                mesesList.Add(string.Format("diciembre {0}",  DateTime.Now.Year), string.Format("diciembre {0}",  DateTime.Now.Year));
                IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                               select new SelectListItem
                                                               {
                                                                   Selected = (s.Key == actividad.Mes),
                                                                   Text = s.Value,
                                                                   Value = s.Key.ToString()
                                                               });
                ViewData["selectList"]      = selectList;
                ViewData["mesesSelectList"] = mesesSelectList;
                ViewData["temporadas"]      = temporadas;
                ViewData["temporada"]       = temporada;
                ViewData["agricultor"]      = agricultor;
                ViewData["predio"]          = predio;
                return View("EditarFlete", actividad);
            }
        }

        [HttpGet]
        public ActionResult EditarManoObra(int id)
        {
            var actividad = dc.PRO_Actividad.SingleOrDefault(X => X.IdActividad == id && X.Habilitado == true);
            if (actividad == null) { return RedirectToAction("index"); }

            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = dc.PRO_Potrero.Single(p => p.IdPotrero == actividad.IdPotrero);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            IEnumerable<SelectListItem> selectList = (from s in dc.PRO_ManoObra
                                                      orderby s.Descripcion
                                                      select new SelectListItem
                                                      {
                                                          Selected = (s.Descripcion == actividad.Descripcion),
                                                          Text = s.Descripcion,
                                                          Value = s.Descripcion
                                                      });
            Dictionary<string, string> mesesList = new Dictionary<string, string>();
            mesesList.Add(string.Format("enero {0}",      DateTime.Now.Year), string.Format("enero {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("febrero {0}",    DateTime.Now.Year), string.Format("febrero {0}",    DateTime.Now.Year));
            mesesList.Add(string.Format("marzo {0}",      DateTime.Now.Year), string.Format("marzo {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("abril {0}",      DateTime.Now.Year), string.Format("abril {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("mayo {0}",       DateTime.Now.Year), string.Format("mayo {0}",       DateTime.Now.Year));
            mesesList.Add(string.Format("junio {0}",      DateTime.Now.Year), string.Format("junio {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("julio {0}",      DateTime.Now.Year), string.Format("julio {0}",      DateTime.Now.Year));
            mesesList.Add(string.Format("agosto {0}",     DateTime.Now.Year), string.Format("agosto {0}",     DateTime.Now.Year));
            mesesList.Add(string.Format("septiembre {0}", DateTime.Now.Year), string.Format("septiembre {0}", DateTime.Now.Year));
            mesesList.Add(string.Format("octubre {0}",    DateTime.Now.Year), string.Format("octubre {0}",    DateTime.Now.Year));
            mesesList.Add(string.Format("noviembre {0}",  DateTime.Now.Year), string.Format("noviembre {0}",  DateTime.Now.Year));
            mesesList.Add(string.Format("diciembre {0}",  DateTime.Now.Year), string.Format("diciembre {0}",  DateTime.Now.Year));
            IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                           select new SelectListItem
                                                           {
                                                               Selected = (s.Key == actividad.Mes),
                                                               Text = s.Value,
                                                               Value = s.Key.ToString()
                                                           });
            ViewData["selectList"] = selectList;
            ViewData["mesesSelectList"] = mesesSelectList;
            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["agricultor"] = agricultor;
            ViewData["predio"] = predio;
            return View("EditarManoObra", actividad);
        }

        [HttpPost]
        public ActionResult EditarManoObra(PRO_Actividad model)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var potrero = dc.PRO_Potrero.Single(p => p.IdPotrero == model.IdPotrero);
            var predio = dc.PRO_Predio.Single(p => p.IdPredio == potrero.IdPredio);
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            Temporada temporada = temporadas.SingleOrDefault(t => t.IdTemporada == predio.IdTemporada);

            if (ModelState.IsValid)
            {
                {
                    PRO_Actividad actividad = (from p in dc.PRO_Actividad
                                               where p.IdActividad == model.IdActividad
                                               && p.Habilitado == true
                                               select p).Single();

                    actividad.Descripcion           = model.Descripcion;
                    actividad.DescripcionAgricultor = model.DescripcionAgricultor;
                    actividad.Mes                   = model.Mes;
                    actividad.Cantidad              = model.Cantidad;
                    actividad.Unidad                = model.Unidad;
                    actividad.ValorUnitario         = model.ValorUnitario;
                    actividad.ValorItem             = model.ValorItem;
                    actividad.UserUpd               = agricultor.Rut;
                    actividad.FechaHoraUpd          = DateTime.Now;
                    actividad.IpUpd                 = RemoteAddr();
                    dc.SubmitChanges();

                    return RedirectToAction("predio", new { id = potrero.IdPredio, IdTemporada = temporada.IdTemporada });
                }
            }
            {
                PRO_Actividad actividad = (from p in dc.PRO_Actividad
                                           where p.IdActividad == model.IdActividad
                                           && p.Habilitado == true
                                           select p).Single();
                IEnumerable<SelectListItem> selectList = (from s in dc.PRO_ManoObra
                                                          orderby s.Descripcion
                                                          select new SelectListItem
                                                          {
                                                              Selected = (s.Descripcion == actividad.Descripcion),
                                                              Text = s.Descripcion,
                                                              Value = s.Descripcion
                                                          });
                Dictionary<string, string> mesesList = new Dictionary<string, string>();
                mesesList.Add(string.Format("enero {0}",      DateTime.Now.Year), string.Format("enero {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("febrero {0}",    DateTime.Now.Year), string.Format("febrero {0}",    DateTime.Now.Year));
                mesesList.Add(string.Format("marzo {0}",      DateTime.Now.Year), string.Format("marzo {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("abril {0}",      DateTime.Now.Year), string.Format("abril {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("mayo {0}",       DateTime.Now.Year), string.Format("mayo {0}",       DateTime.Now.Year));
                mesesList.Add(string.Format("junio {0}",      DateTime.Now.Year), string.Format("junio {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("julio {0}",      DateTime.Now.Year), string.Format("julio {0}",      DateTime.Now.Year));
                mesesList.Add(string.Format("agosto {0}",     DateTime.Now.Year), string.Format("agosto {0}",     DateTime.Now.Year));
                mesesList.Add(string.Format("septiembre {0}", DateTime.Now.Year), string.Format("septiembre {0}", DateTime.Now.Year));
                mesesList.Add(string.Format("octubre {0}",    DateTime.Now.Year), string.Format("octubre {0}",    DateTime.Now.Year));
                mesesList.Add(string.Format("noviembre {0}",  DateTime.Now.Year), string.Format("noviembre {0}",  DateTime.Now.Year));
                mesesList.Add(string.Format("diciembre {0}",  DateTime.Now.Year), string.Format("diciembre {0}",  DateTime.Now.Year));
                IEnumerable<SelectListItem> mesesSelectList = (from s in mesesList
                                                               select new SelectListItem
                                                               {
                                                                   Selected = (s.Key == actividad.Mes),
                                                                   Text = s.Value,
                                                                   Value = s.Key.ToString()
                                                               });
                ViewData["selectList"]      = selectList;
                ViewData["mesesSelectList"] = mesesSelectList;
                ViewData["temporadas"]      = temporadas;
                ViewData["temporada"]       = temporada;
                ViewData["agricultor"]      = agricultor;
                ViewData["predio"]          = predio;
                return View("EditarManoObra", actividad);
            }
        }
    }
}