using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Fichas
{
    [WebsiteAuthorize]
    public class FichasClientController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public FichasClientController()
        {
            SetCurrentModulo(1); //Fichas;
        }

        public ActionResult Index()
        {
            CheckPermisoAndRedirect(1);

            IEnumerable<Agricultor> model;
            if (SYS_User.Current().HasPermiso(5)) //Puede verlos a todos
            {
                model = dc.Agricultor.Where(a => a.Habilitado);
            }
            else //Sólo los con acceso
            {
                model = (from ag in dc.Agricultor
                         join us in dc.UsuarioAgricultor on ag.IdAgricultor equals us.IdAgricultor
                         where ag.Habilitado
                            && us.UserID ==  SYS_User.Current().UserID
                         orderby ag.Nombre
                         select ag);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult GetAgricultor(int id)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == id && a.Habilitado);

                if (agricultor == null || !SYS_User.Current().TieneAccesoAAgricultor(agricultor.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Agricultor Not Found");

                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Id = agricultor.IdAgricultor,
                        Nombre = agricultor.Nombre,
                        Rut = agricultor.Rut,
                        Email = agricultor.Email,
                        Fono1 = agricultor.Fono1,
                        Fono2 = agricultor.Fono2,

                        UserIns = agricultor.UserIns,
                        FechaHoraIns = agricultor.FechaHoraIns,
                        IpIns = agricultor.IpIns,

                        UserUpd = agricultor.UserUpd,
                        FechaHoraUpd = agricultor.FechaHoraUpd,
                        IpUpd = agricultor.IpUpd
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer el agricultor:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult CreateAgricultor(Agricultor agricultor)
        {
            try
            {
                CheckPermisoAndRedirect(1);
                var rv = agricultor.GetRuleViolations();

                if (rv.Count() == 0)
                {
                    agricultor.SetDefaults();

                    agricultor.Habilitado = true;
                    agricultor.IdProveedor = 1;
                    agricultor.MobileTag = "";
                    agricultor.IsNew = true;
                    agricultor.UserIns = User.Identity.Name;
                    agricultor.FechaHoraIns = DateTime.UtcNow;
                    agricultor.IpIns = RemoteAddr();

                    dc.Agricultor.InsertOnSubmit(agricultor);
                    dc.SubmitChanges();

                    if (!CurrentUser.HasPermiso(5))
                    {
                        agricultor.UsuarioAgricultor.Add(new UsuarioAgricultor()
                        {
                            UserID = CurrentUser.UserID,
                            MobileTag = "",
                            UserIns = "api",
                            FechaHoraIns = DateTime.Now,
                            IpIns = "127.0.0.1"
                        });
                        dc.SubmitChanges();
                    }

                    var view = RenderRazorViewToString("ItemRow", agricultor);

                    return Json(new { ok = true, msg = "El agricultor fue creado con éxito", view = view, id = agricultor.IdAgricultor });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error inesperado al crear el ejecutante. Nombre: {0}. {1}", agricultor.Nombre, ex.ToString());

                string msg = "Ocurrió un error al intentar crear el agricultor";
                msg += ": " + ex.Message;


                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult EditAgricultor(int id)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var agricultor = dc.Agricultor.SingleOrDefault(e => e.IdAgricultor == id);
                if (agricultor == null || !SYS_User.Current().TieneAccesoAAgricultor(agricultor.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Agricultor Not Found");

                var fields = new string[] { "Nombre", "Rut", "Email", "Fono1", "Fono2" };

                UpdateModel(agricultor, fields);

                var rv = agricultor.GetRuleViolations();
                if (rv.Count() == 0)
                {
                    agricultor.SetDefaults();

                    agricultor.UserUpd = User.Identity.Name;
                    agricultor.FechaHoraUpd = DateTime.UtcNow;
                    agricultor.IpUpd = RemoteAddr();

                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRow", agricultor);
                    return Json(new { ok = true, msg = "El agricultor fue actualizado con éxito", view = view, id = agricultor.IdAgricultor });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }

            }
            catch (Exception ex)
            {
                string msg = "Ocurrió un error al intentar actualizar el agricultor";
                msg += ": " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        public ActionResult Agricultor(int id)
        {
            CheckPermisoAndRedirect(1);

            var model = dc.Agricultor.SingleOrDefault(e => e.IdAgricultor == id);
            if (model == null || !SYS_User.Current().TieneAccesoAAgricultor(model.IdAgricultor))
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Agricultor Not Found");

            return View(model);
        }

        [HttpPost]
        public ActionResult GetPredio(int id)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var predio = dc.Predio.SingleOrDefault(a => a.IdPredio == id && a.Agricultor.Habilitado);

                if (predio == null || !SYS_User.Current().TieneAccesoAAgricultor(predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Predio Not Found");

                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Id = predio.IdPredio,
                        Nombre = predio.Nombre,
                        IdComuna = predio.IdComuna,

                        UserIns = predio.UserIns,
                        FechaHoraIns = predio.FechaHoraIns,
                        IpIns = predio.IpIns,

                        UserUpd = predio.UserUpd,
                        FechaHoraUpd = predio.FechaHoraUpd,
                        IpUpd = predio.IpUpd
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer el predio:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult CreatePredio(Predio predio)
        {
            try
            {
                CheckPermisoAndRedirect(1);
                var rv = predio.GetRuleViolations();

                if (rv.Count() == 0)
                {
                    predio.SetDefaults();

                    predio.Habilitado = true;
                    predio.MobileTag = "";
                    predio.IDOleotop = "";
                    predio.IDAvenatop = "";
                    predio.IDGranotop = "";
                    predio.UserIns = User.Identity.Name;
                    predio.FechaHoraIns = DateTime.UtcNow;
                    predio.IpIns = RemoteAddr();

                    dc.Predio.InsertOnSubmit(predio);
                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowPredio", predio);

                    return Json(new { ok = true, msg = "El predio fue creado con éxito", view = view, id = predio.IdPredio });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error inesperado al crear el ejecutante. Nombre: {0}. {1}", agricultor.Nombre, ex.ToString());

                string msg = "Ocurrió un error al intentar crear el predio";
                msg += ": " + ex.Message;


                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult EditPredio(int idPredio)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var predio = dc.Predio.SingleOrDefault(p => p.IdPredio == idPredio && p.Agricultor.Habilitado);
                if (predio == null || !SYS_User.Current().TieneAccesoAAgricultor(predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Predio Not Found");

                var fields = new string[] { "Nombre", "IdComuna" };

                UpdateModel(predio, fields);

                var rv = predio.GetRuleViolations();
                if (rv.Count() == 0)
                {
                    predio.SetDefaults();

                    predio.UserUpd = User.Identity.Name;
                    predio.FechaHoraUpd = DateTime.UtcNow;
                    predio.IpUpd = RemoteAddr();

                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowPredio", predio);
                    return Json(new { ok = true, msg = "El predio fue actualizado con éxito", view = view, id = predio.IdPredio });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }

            }
            catch (Exception ex)
            {
                string msg = "Ocurrió un error al intentar actualizar el predio";
                msg += ": " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult GetIntencionSiembra(int id)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var intencionSiembra = dc.IntencionSiembra.SingleOrDefault(a => a.IdIntencionSiembra == id && a.Agricultor.Habilitado);

                if (intencionSiembra == null || !SYS_User.Current().TieneAccesoAAgricultor(intencionSiembra.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "IntencionSiembra Not Found");

                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Id = intencionSiembra.IdIntencionSiembra,
                        IdCultivo = intencionSiembra.IdCultivo,
                        IdComuna = intencionSiembra.IdComuna,
                        IdAgricultor = intencionSiembra.IdAgricultor,
                        Cantidad = intencionSiembra.Cantidad,
                        Superficie = intencionSiembra.Superficie,
                        PuntoEntrega = intencionSiembra.PuntoEntrega,
                        Observaciones = intencionSiembra.Observaciones,

                        UserIns = intencionSiembra.UserIns,
                        FechaHoraIns = intencionSiembra.FechaHoraIns,
                        IpIns = intencionSiembra.IpIns,

                        UserUpd = intencionSiembra.UserUpd,
                        FechaHoraUpd = intencionSiembra.FechaHoraUpd,
                        IpUpd = intencionSiembra.IpUpd
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer la Intención de Siembra:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult CreateIntencionSiembra(IntencionSiembra intencionSiembra)
        {
            try
            {
                CheckPermisoAndRedirect(1);
                var rv = intencionSiembra.GetRuleViolations();

                if (rv.Count() == 0)
                {
                    intencionSiembra.SetDefaults();

                    intencionSiembra.IdTemporada = Temporada.TemporadaActivaFichas().IdTemporada;
                    intencionSiembra.MobileTag = "";
                    intencionSiembra.UserIns = User.Identity.Name;
                    intencionSiembra.FechaHoraIns = DateTime.UtcNow;
                    intencionSiembra.IpIns = RemoteAddr();

                    dc.IntencionSiembra.InsertOnSubmit(intencionSiembra);
                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowIntencionSiembra", intencionSiembra);

                    return Json(new { ok = true, msg = "La Intención de Siembra fue creada con éxito", view = view, id = intencionSiembra.IdIntencionSiembra });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error inesperado al crear el ejecutante. Nombre: {0}. {1}", agricultor.Nombre, ex.ToString());

                string msg = "Ocurrió un error al intentar crear la Intención de Siembra";
                msg += ": " + ex.Message;


                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult EditIntencionSiembra(int idIntencionSiembra)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var intencionSiembra = dc.IntencionSiembra.SingleOrDefault(p => p.IdIntencionSiembra == idIntencionSiembra && p.Agricultor.Habilitado);
                if (intencionSiembra == null || !SYS_User.Current().TieneAccesoAAgricultor(intencionSiembra.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "IntencionSiembra Not Found");

                var fields = new string[] { "IdCultivo", "IdComuna", "Superficie", "Cantidad", "PuntoEntrega", "Observaciones" };

                UpdateModel(intencionSiembra, fields);

                var rv = intencionSiembra.GetRuleViolations();
                if (rv.Count() == 0)
                {
                    intencionSiembra.SetDefaults();

                    intencionSiembra.UserUpd = User.Identity.Name;
                    intencionSiembra.FechaHoraUpd = DateTime.UtcNow;
                    intencionSiembra.IpUpd = RemoteAddr();

                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowIntencionSiembra", intencionSiembra);
                    return Json(new { ok = true, msg = "La Intención de Siembra fue actualizada con éxito", view = view, id = intencionSiembra.IdIntencionSiembra });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }

            }
            catch (Exception ex)
            {
                string msg = "Ocurrió un error al intentar actualizar la Intención de Siembra";
                msg += ": " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        public ActionResult Predio(int id)
        {
            CheckPermisoAndRedirect(1);

            var model = dc.Predio.SingleOrDefault(e => e.IdPredio == id && e.Agricultor.Habilitado);
            if (model == null || !SYS_User.Current().TieneAccesoAAgricultor(model.IdAgricultor))
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Predio Not Found");

            return View(model);
        }

        [HttpPost]
        public ActionResult GetPotrero(int id)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var potrero = dc.Potrero.SingleOrDefault(a => a.IdPotrero == id && a.Predio.Agricultor.Habilitado);

                if (potrero == null || !SYS_User.Current().TieneAccesoAAgricultor(potrero.Predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Potrero Not Found");

                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Id = potrero.IdPotrero,
                        IdPredio = potrero.IdPredio,
                        Nombre = potrero.Nombre,
                        Superficie = potrero.Superficie,

                        UserIns = potrero.UserIns,
                        FechaHoraIns = potrero.FechaHoraIns,
                        IpIns = potrero.IpIns,

                        UserUpd = potrero.UserUpd,
                        FechaHoraUpd = potrero.FechaHoraUpd,
                        IpUpd = potrero.IpUpd
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer el potrero:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult CreatePotrero(Potrero potrero)
        {
            try
            {
                CheckPermisoAndRedirect(1);
                var rv = potrero.GetRuleViolations();

                if (rv.Count() == 0)
                {
                    potrero.SetDefaults();

                    potrero.Habilitado = true;
                    potrero.MobileTag = "";
                    potrero.UserIns = User.Identity.Name;
                    potrero.FechaHoraIns = DateTime.UtcNow;
                    potrero.IpIns = RemoteAddr();

                    dc.Potrero.InsertOnSubmit(potrero);
                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowPotrero", potrero);

                    return Json(new { ok = true, msg = "El potrero fue creado con éxito", view = view, id = potrero.IdPotrero });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error inesperado al crear el ejecutante. Nombre: {0}. {1}", agricultor.Nombre, ex.ToString());

                string msg = "Ocurrió un error al intentar crear el potrero";
                msg += ": " + ex.Message;


                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult EditPotrero(int idPotrero)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var potrero = dc.Potrero.SingleOrDefault(p => p.IdPotrero == idPotrero && p.Predio.Agricultor.Habilitado);
                if (potrero == null || !SYS_User.Current().TieneAccesoAAgricultor(potrero.Predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Potrero Not Found");

                var fields = new string[] { "Nombre", "Superficie" };

                UpdateModel(potrero, fields);

                var rv = potrero.GetRuleViolations();
                if (rv.Count() == 0)
                {
                    potrero.SetDefaults();

                    potrero.UserUpd = User.Identity.Name;
                    potrero.FechaHoraUpd = DateTime.UtcNow;
                    potrero.IpUpd = RemoteAddr();

                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowPotrero", potrero);
                    return Json(new { ok = true, msg = "El potrero fue actualizado con éxito", view = view, id = potrero.IdPotrero });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }

            }
            catch (Exception ex)
            {
                string msg = "Ocurrió un error al intentar actualizar el potrero";
                msg += ": " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult GetSiembra(int id)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var siembra = dc.Siembra.SingleOrDefault(s => s.IdSiembra == id && s.Predio.Agricultor.Habilitado);

                if (siembra == null || !SYS_User.Current().TieneAccesoAAgricultor(siembra.Predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Siembra Not Found");

                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Id = siembra.IdSiembra,
                        IdPredio = siembra.IdPredio,
                        IdVariedad = siembra.IdVariedad,
                        FechaSiembra = siembra.FechaSiembra.ToString("dd/MM/yyyy"),
                        Dosis = siembra.Dosis,
                        IdCultivoAnterior = siembra.IdCultivoAnterior,
                        IdTipoSiembra = siembra.IdTipoSiembra,
                        RendimientoEstimado = siembra.RendimientoEstimado,
                        FechaCosechaEstimada = siembra.FechaCosechaEstimada,

                        Potreros = SelectorPotreroViewModel.ForSiembraPotrero(dc, siembra),

                        UserIns = siembra.UserIns,
                        FechaHoraIns = siembra.FechaHoraIns,
                        IpIns = siembra.IpIns,

                        UserUpd = siembra.UserUpd,
                        FechaHoraUpd = siembra.FechaHoraUpd,
                        IpUpd = siembra.IpUpd
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer la siembra:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        public ActionResult GetPotrerosParaSiembra(int id, int idTemporada)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Potreros = SelectorPotreroViewModel.ForSiembraPotrero(dc, id, idTemporada, 0)
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer los potreros:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult CreateSiembra([Bind(Exclude = "FechaSiembra, FechaCosechaEstimada")]Siembra siembra)
        {
            try
            {
                CheckPermisoAndRedirect(1);

                siembra.FechaSiembra = DateParser.DateFromRequest("FechaSiembra").GetValueOrDefault();
                siembra.FechaCosechaEstimada = DateParser.DateFromRequest("FechaCosechaEstimada");

                var potreros = new List<SelectorPotreroViewModel>();
                UpdateModel(potreros, "SubItems");

                var rv = siembra.GetRuleViolations(potreros);

                if (rv.Count() == 0)
                {
                    siembra.SetDefaults();

                    siembra.MobileTag = "";
                    siembra.UserIns = User.Identity.Name;
                    siembra.FechaHoraIns = DateTime.UtcNow;
                    siembra.IpIns = RemoteAddr();

                    dc.Siembra.InsertOnSubmit(siembra);

                    //Potreros
                    //Nuevos
                    foreach (var potrero in potreros.Where(p => p.Seleccionado))
                    {
                        siembra.SiembraPotrero.Add(new SiembraPotrero()
                        {
                            IdPotrero = potrero.IdPotrero,
                            IdTemporada = siembra.IdTemporada,
                            MobileTag = "",
                            FechaHoraIns = DateTime.Now,
                            UserIns = User.Identity.Name,
                            IpIns = RemoteAddr()
                        });
                    }

                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowSiembra", siembra);

                    return Json(new { ok = true, msg = "La siembra fue creada con éxito", view = view, id = siembra.IdSiembra });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error inesperado al crear el ejecutante. Nombre: {0}. {1}", agricultor.Nombre, ex.ToString());

                string msg = "Ocurrió un error al intentar crear la siembra";
                msg += ": " + ex.Message;


                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult EditSiembra(int idSiembra)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var siembra = dc.Siembra.SingleOrDefault(p => p.IdSiembra == idSiembra && p.Predio.Agricultor.Habilitado);
                if (siembra == null || !SYS_User.Current().TieneAccesoAAgricultor(siembra.Predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Siembra Not Found");

                var fields = new string[] { "IdVariedad", "Dosis", "IdtipoSiembra", "IdCultivoAnterior", "RendimientoEstimado" };

                UpdateModel(siembra, fields);

                siembra.FechaSiembra = DateParser.DateFromRequest("FechaSiembra").GetValueOrDefault();
                siembra.FechaCosechaEstimada = DateParser.DateFromRequest("FechaCosechaEstimada");

                var potreros = new List<SelectorPotreroViewModel>();
                UpdateModel(potreros, "SubItems");

                var rv = siembra.GetRuleViolations(potreros);
                if (rv.Count() == 0)
                {
                    siembra.SetDefaults();

                    siembra.UserUpd = User.Identity.Name;
                    siembra.FechaHoraUpd = DateTime.UtcNow;
                    siembra.IpUpd = RemoteAddr();
                    
                    //Potreros
                    //Elimiandos
                    var eliminados = siembra.SiembraPotrero.Where(exs => !(from potrero in potreros.Where(p => p.Seleccionado) select potrero.IdPotrero).Contains(exs.IdPotrero));
                    dc.SiembraPotrero.DeleteAllOnSubmit(eliminados);

                    //Nuevos
                    var existentes = siembra.SiembraPotrero;
                    foreach (var potrero in potreros.Where(p => p.Seleccionado))
                    {
                        var existente = existentes.SingleOrDefault(r => r.IdPotrero == potrero.IdPotrero);
                        if (existente == null)
                        {
                            siembra.SiembraPotrero.Add(new SiembraPotrero()
                            {
                                IdPotrero = potrero.IdPotrero,
                                IdTemporada = siembra.IdTemporada,
                                MobileTag = "",
                                FechaHoraIns = DateTime.Now,
                                UserIns = User.Identity.Name,
                                IpIns = RemoteAddr()
                            });
                        }
                    }

                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowSiembra", siembra);
                    return Json(new { ok = true, msg = "La siembra fue actualizada con éxito", view = view, id = siembra.IdSiembra });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }

            }
            catch (Exception ex)
            {
                string msg = "Ocurrió un error al intentar actualizar la siembra";
                msg += ": " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult GetFichaPreSiembra(int id)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var ficha = dc.FichaPreSiembra.SingleOrDefault(f => f.IdFichaPreSiembra == id && f.Predio.Agricultor.Habilitado);

                if (ficha == null || !SYS_User.Current().TieneAccesoAAgricultor(ficha.Predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "FichaPreSiembra Not Found");

                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Id = ficha.IdFichaPreSiembra,
                        IdPredio = ficha.IdPredio,
                        Fecha = ficha.Fecha.ToString("dd/MM/yyyy"),
                        Observaciones = ficha.Observaciones,
                        IdEstadoSiembra = ficha.IdEstadoSiembra,
                        IdImportanciaSeguimiento = ficha.IdImportanciaSeguimiento,

                        Potreros = SelectorPotreroViewModel.ForPreSiembraPotrero(dc, ficha),
                        Recomendaciones = (from r in ficha.RecomendacionPreSiembra
                                           select new
                                           {
                                               Id = r.IdRecomendacionPreSiembra,
                                               IdQuimico = r.IdQuimico,
                                               Dosis = r.Dosis,
                                               IdUM = r.IdUM,
                                               FechaAplicacion = (r.FechaAplicacion.HasValue ? r.FechaAplicacion.Value.ToString("dd/MM/yyyy") : ""),
                                               FerN = r.FerN,
                                               FerP2O5 = r.FerP2O5,
                                               FerKO2 = r.FerKO2,
                                               FerMgO = r.FerMgO,
                                               FerS = r.FerS,
                                               FerB = r.FerB,
                                               FerZn = r.FerZn,
                                               FerCaO = r.FerCaO,
                                               IdTipoRecomendacion = r.Quimico.IdTipoRecomendacion,
                                               NombreQuimico = r.Quimico.Nombre,
                                               NombreTipoRecomendacion = r.Quimico.TipoRecomendacion.Nombre,
                                               NombreUM = r.UM.Nombre
                                           }).ToList(),

                        Fotos = (from f in ficha.FotoFichaPreSiembra

                                 select new FotoFichaViewModel
                                 {
                                     Id = f.IdFotoFichaPreSiembra,
                                     FotoUrl = f.FotoUrl,
                                     Observaciones = f.Observaciones,
                                     FileName = f.FileName
                                 }).ToList(),

                        UserIns = ficha.UserIns,
                        FechaHoraIns = ficha.FechaHoraIns,
                        IpIns = ficha.IpIns,

                        UserUpd = ficha.UserUpd,
                        FechaHoraUpd = ficha.FechaHoraUpd,
                        IpUpd = ficha.IpUpd
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer la siembra:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        public ActionResult GetPotrerosParaPreSiembra(int id, int idTemporada)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Potreros = SelectorPotreroViewModel.ForPreSiembraPotrero(dc, id, idTemporada, 0)
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer los potreros:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult CreateFichaPreSiembra([Bind(Exclude = "Fecha")]FichaPreSiembra ficha)
        {
            try
            {
                CheckPermisoAndRedirect(1);

                ficha.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

                var potreros = new List<SelectorPotreroViewModel>();
                UpdateModel(potreros, "SubItems");

                var fotos = new List<FotoFichaViewModel>();
                UpdateModel(fotos, "Fotos");

                var recomendaciones = new List<RecomendacionViewModel>();
                UpdateModel(recomendaciones, "Recomendaciones");

                var rv = ficha.GetRuleViolations(potreros);

                if (rv.Count() == 0)
                {
                    ficha.SetDefaults();

                    ficha.MobileTag = "";
                    ficha.UserIns = User.Identity.Name;
                    ficha.FechaHoraIns = DateTime.UtcNow;
                    ficha.IpIns = RemoteAddr();

                    dc.FichaPreSiembra.InsertOnSubmit(ficha);

                    //Potreros
                    //Nuevos
                    foreach (var potrero in potreros.Where(p => p.Seleccionado))
                    {
                        ficha.FichaPreSiembraPotrero.Add(new FichaPreSiembraPotrero()
                        {
                            IdPotrero = potrero.IdPotrero,
                            MobileTag = "",
                            FechaHoraIns = DateTime.Now,
                            UserIns = User.Identity.Name,
                            IpIns = RemoteAddr()
                        });
                    }

                    //Fotos
                    //Nuevas
                    foreach (var foto in fotos)
                    {
                        ficha.FotoFichaPreSiembra.Add(new FotoFichaPreSiembra()
                        {
                            FileName = foto.FileName,
                            Observaciones = foto.Observaciones ?? "",
                            MobileTag = "",
                            FechaHoraIns = DateTime.Now,
                            UserIns = User.Identity.Name,
                            IpIns = RemoteAddr()
                        });
                    }

                    //Recomendaciones
                    //Nuevas
                    foreach (var rec in recomendaciones)
                    {
                        DateTime? fechaAplicacion = DateParser.DateFromString(rec.FechaAplicacion);
                        ficha.RecomendacionPreSiembra.Add(new RecomendacionPreSiembra()
                        {
                            Dosis = rec.Dosis,
                            FechaAplicacion = fechaAplicacion,
                            FerB = rec.FerB,
                            FerCaO = rec.FerCaO,
                            FerKO2 = rec.FerKO2,
                            FerMgO = rec.FerMgO,
                            FerN = rec.FerN,
                            FerP2O5 = rec.FerP2O5,
                            FerS = rec.FerS,
                            FerZn = rec.FerZn,
                            IdQuimico = rec.IdQuimico,
                            IdUM = rec.IdUM,
                            MobileTag = "",
                            FechaHoraIns = DateTime.Now,
                            UserIns = User.Identity.Name,
                            IpIns = RemoteAddr()
                        });
                    }

                    dc.SubmitChanges();

                    ficha.NotifyCreator();
                    var view = RenderRazorViewToString("ItemRowPreSiembra", ficha);

                    return Json(new { ok = true, msg = "La ficha fue creada con éxito", view = view, id = ficha.IdFichaPreSiembra });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error inesperado al crear el ejecutante. Nombre: {0}. {1}", agricultor.Nombre, ex.ToString());

                string msg = "Ocurrió un error al intentar crear la ficha";
                msg += ": " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult EditFichaPreSiembra(int idFichaPreSiembra)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var ficha = dc.FichaPreSiembra.SingleOrDefault(p => p.IdFichaPreSiembra == idFichaPreSiembra && p.Predio.Agricultor.Habilitado);
                if (ficha == null || !SYS_User.Current().TieneAccesoAAgricultor(ficha.Predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Ficha PreSiembra Not Found");

                var fields = new string[] { "Observaciones", "IdEstadoSiembra", "IdImportanciaSeguimiento" };

                UpdateModel(ficha, fields);

                ficha.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

                var potreros = new List<SelectorPotreroViewModel>();
                UpdateModel(potreros, "SubItems");

                var fotos = new List<FotoFichaViewModel>();
                UpdateModel(fotos, "Fotos");

                var recomendaciones = new List<RecomendacionViewModel>();
                UpdateModel(recomendaciones, "Recomendaciones");

                var rv = ficha.GetRuleViolations(potreros);
                if (rv.Count() == 0)
                {
                    ficha.SetDefaults();

                    ficha.UserUpd = User.Identity.Name;
                    ficha.FechaHoraUpd = DateTime.UtcNow;
                    ficha.IpUpd = RemoteAddr();

                    //Potreros
                    //Elimiandos
                    var eliminados = ficha.FichaPreSiembraPotrero.Where(exs => !(from potrero in potreros.Where(p => p.Seleccionado) select potrero.IdPotrero).Contains(exs.IdPotrero));
                    dc.FichaPreSiembraPotrero.DeleteAllOnSubmit(eliminados);

                    //Nuevos
                    var existentes = ficha.FichaPreSiembraPotrero;
                    foreach (var potrero in potreros.Where(p => p.Seleccionado))
                    {
                        var existente = existentes.SingleOrDefault(r => r.IdPotrero == potrero.IdPotrero);
                        if (existente == null)
                        {
                            ficha.FichaPreSiembraPotrero.Add(new FichaPreSiembraPotrero()
                            {
                                IdPotrero = potrero.IdPotrero,
                                MobileTag = "",
                                FechaHoraIns = DateTime.Now,
                                UserIns = User.Identity.Name,
                                IpIns = RemoteAddr()
                            });
                        }
                    }

                    //Fotos
                    var fotosEliminadas = ficha.FotoFichaPreSiembra.Where(ffps => !(from foto in fotos select foto.Id).Contains(ffps.IdFotoFichaPreSiembra));
                    dc.FotoFichaPreSiembra.DeleteAllOnSubmit(fotosEliminadas);

                    //Nuevas y Editadas
                    var fotosExistentes = ficha.FotoFichaPreSiembra;
                    foreach (var foto in fotos)
                    {
                        var existente = fotosExistentes.SingleOrDefault(r => r.IdFotoFichaPreSiembra == foto.Id);
                        if (existente == null)
                        {
                            ficha.FotoFichaPreSiembra.Add(new FotoFichaPreSiembra()
                            {
                                FileName = foto.FileName,
                                Observaciones = foto.Observaciones ?? "",
                                MobileTag = "",
                                FechaHoraIns = DateTime.Now,
                                UserIns = User.Identity.Name,
                                IpIns = RemoteAddr()
                            });
                        }
                        else
                        {
                            if (existente.FileName != foto.FileName || existente.Observaciones != foto.Observaciones)
                            {
                                existente.FileName = foto.FileName;
                                existente.Observaciones = foto.Observaciones ?? "";
                                existente.FechaHoraIns = DateTime.Now;
                                existente.UserIns = User.Identity.Name;
                            }
                        }
                    }

                    //Recomendaciones
                    var recsEliminadas = ficha.RecomendacionPreSiembra.Where(rps => !(from rec in recomendaciones select rec.Id).Contains(rps.IdRecomendacionPreSiembra));
                    dc.RecomendacionPreSiembra.DeleteAllOnSubmit(recsEliminadas);

                    //Nuevas y Editadas
                    var recsExistentes = ficha.RecomendacionPreSiembra;
                    foreach (var rec in recomendaciones)
                    {
                        var existente = recsExistentes.SingleOrDefault(r => r.IdRecomendacionPreSiembra == rec.Id);
                        DateTime? fechaAplicacion = DateParser.DateFromString(rec.FechaAplicacion);
                        if (existente == null)
                        {
                            ficha.RecomendacionPreSiembra.Add(new RecomendacionPreSiembra()
                            {
                                Dosis = rec.Dosis,
                                FechaAplicacion = fechaAplicacion,
                                FerB = rec.FerB,
                                FerCaO =rec.FerCaO,
                                FerKO2 = rec.FerKO2,
                                FerMgO = rec.FerMgO,
                                FerN = rec.FerN,
                                FerP2O5 = rec.FerP2O5,
                                FerS = rec.FerS,
                                FerZn = rec.FerZn,
                                IdQuimico = rec.IdQuimico,
                                IdUM = rec.IdUM,
                                MobileTag = "",
                                FechaHoraIns = DateTime.Now,
                                UserIns = User.Identity.Name,
                                IpIns = RemoteAddr()
                            });
                        }
                        else
                        {
                            if (existente.Dosis != rec.Dosis || 
                                existente.FechaAplicacion != fechaAplicacion ||
                                existente.FerB != rec.FerB ||
                                existente.FerCaO != rec.FerCaO ||
                                existente.FerKO2 != rec.FerKO2 ||
                                existente.FerMgO != rec.FerMgO ||
                                existente.FerN != rec.FerN ||
                                existente.FerP2O5 != rec.FerP2O5 ||
                                existente.FerS != rec.FerS ||
                                existente.FerZn != rec.FerZn ||
                                existente.IdQuimico != rec.IdQuimico ||
                                existente.IdUM != rec.IdUM)
                            {
                                existente.Dosis = rec.Dosis;
                                existente.FechaAplicacion = fechaAplicacion;
                                existente.FerB = rec.FerB;
                                existente.FerCaO = rec.FerCaO;
                                existente.FerKO2 = rec.FerKO2;
                                existente.FerMgO = rec.FerMgO;
                                existente.FerN = rec.FerN;
                                existente.FerP2O5 = rec.FerP2O5;
                                existente.FerS = rec.FerS;
                                existente.FerZn = rec.FerZn;
                                existente.IdQuimico = rec.IdQuimico;
                                existente.IdUM = rec.IdUM;
                                existente.FechaHoraUpd = DateTime.Now;
                                existente.UserUpd = User.Identity.Name;
                                existente.IpUpd = RemoteAddr();
                            }
                        }
                    }


                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowPreSiembra", ficha);
                    return Json(new { ok = true, msg = "La ficha fue actualizada con éxito", view = view, id = ficha.IdFichaPreSiembra });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }

            }
            catch (Exception ex)
            {
                string msg = "Ocurrió un error al intentar actualizar la ficha";
                msg += ": " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult UploadFoto()
        {
            try
            {
                var file = Request.Files[0];
                if (!file.FileName.EndsWith(".jpg") && !file.FileName.EndsWith(".jpeg"))
                {
                    throw new Exception("El archivo debe estar en formato JPG.");
                }

                var fileName = $"{System.Guid.NewGuid()}.jpg";

                var baseFolder = ConfigurationManager.AppSettings["FotosFolder"];
                var folder = "default";
                if (fileName.Length >= 3)
                {
                    folder = Path.Combine(baseFolder, fileName.Substring(0, 1), fileName.Substring(0, 2));
                }

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                file.SaveAs(Path.Combine(folder, fileName));

                return Json(new { ok = true, msg = "La foto fue creada con éxito.", filename = fileName, fotourl = FotoFicha.FotoUrlForFileName(fileName) });
            }
            catch (Exception ex)
            {
                string msg = "Ocurrió un error al intentar guardar la foto";
                msg += ": " + ex.Message;


                return Json(new { ok = false, msg = msg });
            }

        }

        public ActionResult GetFicha(int id)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var ficha = dc.Ficha.SingleOrDefault(f => f.IdFicha == id && f.Predio.Agricultor.Habilitado);

                if (ficha == null || !SYS_User.Current().TieneAccesoAAgricultor(ficha.Predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Ficha Not Found");

                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Id = ficha.IdFicha,
                        IdTipoFicha = ficha.IdTipoFicha,
                        IdPredio = ficha.IdPredio,
                        Fecha = ficha.Fecha.ToString("dd/MM/yyyy"),
                        Observaciones = ficha.Observaciones,

                        IdEstadoSiembra = ficha.IdEstadoSiembra,
                        IdImportanciaSeguimiento = ficha.IdImportanciaSeguimiento,

                        Potreros = SelectorPotreroViewModel.ForFichaPotrero(dc, ficha),
                        Recomendaciones = (from r in ficha.Recomendacion
                                           select new
                                           {
                                               Id = r.IdRecomendacion,
                                               IdQuimico = r.IdQuimico,
                                               Dosis = r.Dosis,
                                               IdUM = r.IdUM,
                                               FechaAplicacion = (r.FechaAplicacion.HasValue ? r.FechaAplicacion.Value.ToString("dd/MM/yyyy") : ""),
                                               FerN = r.FerN,
                                               FerP2O5 = r.FerP2O5,
                                               FerKO2 = r.FerKO2,
                                               FerMgO = r.FerMgO,
                                               FerS = r.FerS,
                                               FerB = r.FerB,
                                               FerZn = r.FerZn,
                                               FerCaO = r.FerCaO,
                                               IdTipoRecomendacion = r.Quimico.IdTipoRecomendacion,
                                               NombreQuimico = r.Quimico.Nombre,
                                               NombreTipoRecomendacion = r.Quimico.TipoRecomendacion.Nombre,
                                               NombreUM = r.UM.Nombre
                                           }).ToList(),

                        Fotos = (from f in ficha.FotoFicha

                                 select new FotoFichaViewModel
                                 {
                                     Id = f.IdFotoFicha,
                                     FotoUrl = f.FotoUrl,
                                     Observaciones = f.Observaciones,
                                     FileName = f.FileName
                                 }).ToList(),

                        UserIns = ficha.UserIns,
                        FechaHoraIns = ficha.FechaHoraIns,
                        IpIns = ficha.IpIns,

                        UserUpd = ficha.UserUpd,
                        FechaHoraUpd = ficha.FechaHoraUpd,
                        IpUpd = ficha.IpUpd
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer la ficha:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        public ActionResult GetPotrerosParaFicha(int id, int idTemporada)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                return Json(new
                {
                    ok = true,
                    msg = "",
                    item = new
                    {
                        Potreros = SelectorPotreroViewModel.ForFichaPotrero(dc, id, idTemporada, 0)
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error al leer los potreros:  " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult CreateFicha([Bind(Exclude = "Fecha")]Ficha ficha)
        {
            try
            {
                CheckPermisoAndRedirect(1);

                ficha.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

                var potreros = new List<SelectorPotreroViewModel>();
                UpdateModel(potreros, "SubItems");

                var fotos = new List<FotoFichaViewModel>();
                UpdateModel(fotos, "Fotos");

                var recomendaciones = new List<RecomendacionViewModel>();
                UpdateModel(recomendaciones, "Recomendaciones");


                var rv = ficha.GetRuleViolations(potreros);

                if (rv.Count() == 0)
                {
                    ficha.SetDefaults();

                    ficha.MobileTag = "";
                    ficha.UserIns = User.Identity.Name;
                    ficha.FechaHoraIns = DateTime.UtcNow;
                    ficha.IpIns = RemoteAddr();

                    dc.Ficha.InsertOnSubmit(ficha);

                    //Potreros
                    //Nuevos
                    foreach (var potrero in potreros.Where(p => p.Seleccionado))
                    {
                        ficha.FichaPotrero.Add(new FichaPotrero()
                        {
                            IdPotrero = potrero.IdPotrero,
                            MobileTag = "",
                            FechaHoraIns = DateTime.Now,
                            UserIns = User.Identity.Name,
                            IpIns = RemoteAddr()
                        });
                    }

                    //Fotos
                    //Nuevas
                    foreach (var foto in fotos)
                    {
                        ficha.FotoFicha.Add(new FotoFicha()
                        {
                            FileName = foto.FileName,
                            Observaciones = foto.Observaciones ?? "",
                            MobileTag = "",
                            FechaHoraIns = DateTime.Now,
                            UserIns = User.Identity.Name,
                            IpIns = RemoteAddr()
                        });
                    }

                    //Recomendaciones
                    //Nuevas
                    foreach (var rec in recomendaciones)
                    {
                        DateTime? fechaAplicacion = DateParser.DateFromString(rec.FechaAplicacion);
                        ficha.Recomendacion.Add(new Recomendacion()
                        {
                            Dosis = rec.Dosis,
                            FechaAplicacion = fechaAplicacion,
                            FerB = rec.FerB,
                            FerCaO = rec.FerCaO,
                            FerKO2 = rec.FerKO2,
                            FerMgO = rec.FerMgO,
                            FerN = rec.FerN,
                            FerP2O5 = rec.FerP2O5,
                            FerS = rec.FerS,
                            FerZn = rec.FerZn,
                            IdQuimico = rec.IdQuimico,
                            IdUM = rec.IdUM,
                            MobileTag = "",
                            FechaHoraIns = DateTime.Now,
                            UserIns = User.Identity.Name,
                            IpIns = RemoteAddr()
                        });
                    }

                    dc.SubmitChanges();

                    ficha.NotifyCreator();
                    var view = RenderRazorViewToString("ItemRowFicha", ficha);

                    return Json(new { ok = true, msg = "La ficha fue creada con éxito", view = view, id = ficha.IdFicha });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error inesperado al crear el ejecutante. Nombre: {0}. {1}", agricultor.Nombre, ex.ToString());

                string msg = "Ocurrió un error al intentar crear la ficha";
                msg += ": " + ex.Message;


                return Json(new { ok = false, msg = msg });
            }
        }

        [HttpPost]
        public ActionResult EditFicha(int idFicha)
        {
            CheckPermisoAndRedirect(1);

            try
            {
                var ficha = dc.Ficha.SingleOrDefault(p => p.IdFicha == idFicha && p.Predio.Agricultor.Habilitado);
                if (ficha == null || !SYS_User.Current().TieneAccesoAAgricultor(ficha.Predio.IdAgricultor))
                    throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Ficha Not Found");

                var fields = new string[] { "IdTipoFicha", "Observaciones", "IdEstadoSiembra", "IdImportanciaSeguimiento" };
                UpdateModel(ficha, fields);

                ficha.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

                var potreros = new List<SelectorPotreroViewModel>();
                UpdateModel(potreros, "SubItems");

                var fotos = new List<FotoFichaViewModel>();
                UpdateModel(fotos, "Fotos");

                var recomendaciones = new List<RecomendacionViewModel>();
                UpdateModel(recomendaciones, "Recomendaciones");

                var rv = ficha.GetRuleViolations(potreros);
                if (rv.Count() == 0)
                {
                    ficha.SetDefaults();

                    ficha.UserUpd = User.Identity.Name;
                    ficha.FechaHoraUpd = DateTime.UtcNow;
                    ficha.IpUpd = RemoteAddr();

                    //Potreros
                    //Elimiandos
                    var eliminados = ficha.FichaPotrero.Where(exs => !(from potrero in potreros.Where(p => p.Seleccionado) select potrero.IdPotrero).Contains(exs.IdPotrero));
                    dc.FichaPotrero.DeleteAllOnSubmit(eliminados);

                    //Nuevos
                    var existentes = ficha.FichaPotrero;
                    foreach (var potrero in potreros.Where(p => p.Seleccionado))
                    {
                        var existente = existentes.SingleOrDefault(r => r.IdPotrero == potrero.IdPotrero);
                        if (existente == null)
                        {
                            ficha.FichaPotrero.Add(new FichaPotrero()
                            {
                                IdPotrero = potrero.IdPotrero,
                                MobileTag = "",
                                FechaHoraIns = DateTime.Now,
                                UserIns = User.Identity.Name,
                                IpIns = RemoteAddr()
                            });
                        }
                    }

                    //Fotos
                    var fotosEliminadas = ficha.FotoFicha.Where(ffps => !(from foto in fotos select foto.Id).Contains(ffps.IdFotoFicha));
                    dc.FotoFicha.DeleteAllOnSubmit(fotosEliminadas);

                    //Nuevas y Editadas
                    var fotosExistentes = ficha.FotoFicha;
                    foreach (var foto in fotos)
                    {
                        var existente = fotosExistentes.SingleOrDefault(r => r.IdFotoFicha == foto.Id);
                        if (existente == null)
                        {
                            ficha.FotoFicha.Add(new FotoFicha()
                            {
                                FileName = foto.FileName,
                                Observaciones = foto.Observaciones ?? "",
                                MobileTag = "",
                                FechaHoraIns = DateTime.Now,
                                UserIns = User.Identity.Name,
                                IpIns = RemoteAddr()
                            });
                        }
                        else
                        {
                            if (existente.FileName != foto.FileName || existente.Observaciones != foto.Observaciones)
                            {
                                existente.FileName = foto.FileName;
                                existente.Observaciones = foto.Observaciones ?? "";
                                existente.FechaHoraIns = DateTime.Now;
                                existente.UserIns = User.Identity.Name;
                            }
                        }
                    }

                    //Recomendaciones
                    var recsEliminadas = ficha.Recomendacion.Where(rps => !(from rec in recomendaciones select rec.Id).Contains(rps.IdRecomendacion));
                    dc.Recomendacion.DeleteAllOnSubmit(recsEliminadas);

                    //Nuevas y Editadas
                    var recsExistentes = ficha.Recomendacion;
                    foreach (var rec in recomendaciones)
                    {
                        var existente = recsExistentes.SingleOrDefault(r => r.IdRecomendacion == rec.Id);
                        DateTime? fechaAplicacion = DateParser.DateFromString(rec.FechaAplicacion);
                        if (existente == null)
                        {
                            ficha.Recomendacion.Add(new Recomendacion()
                            {
                                Dosis = rec.Dosis,
                                FechaAplicacion = fechaAplicacion,
                                FerB = rec.FerB,
                                FerCaO = rec.FerCaO,
                                FerKO2 = rec.FerKO2,
                                FerMgO = rec.FerMgO,
                                FerN = rec.FerN,
                                FerP2O5 = rec.FerP2O5,
                                FerS = rec.FerS,
                                FerZn = rec.FerZn,
                                IdQuimico = rec.IdQuimico,
                                IdUM = rec.IdUM,
                                MobileTag = "",
                                FechaHoraIns = DateTime.Now,
                                UserIns = User.Identity.Name,
                                IpIns = RemoteAddr()
                            });
                        }
                        else
                        {
                            if (existente.Dosis != rec.Dosis ||
                                existente.FechaAplicacion != fechaAplicacion ||
                                existente.FerB != rec.FerB ||
                                existente.FerCaO != rec.FerCaO ||
                                existente.FerKO2 != rec.FerKO2 ||
                                existente.FerMgO != rec.FerMgO ||
                                existente.FerN != rec.FerN ||
                                existente.FerP2O5 != rec.FerP2O5 ||
                                existente.FerS != rec.FerS ||
                                existente.FerZn != rec.FerZn ||
                                existente.IdQuimico != rec.IdQuimico ||
                                existente.IdUM != rec.IdUM)
                            {
                                existente.Dosis = rec.Dosis;
                                existente.FechaAplicacion = fechaAplicacion;
                                existente.FerB = rec.FerB;
                                existente.FerCaO = rec.FerCaO;
                                existente.FerKO2 = rec.FerKO2;
                                existente.FerMgO = rec.FerMgO;
                                existente.FerN = rec.FerN;
                                existente.FerP2O5 = rec.FerP2O5;
                                existente.FerS = rec.FerS;
                                existente.FerZn = rec.FerZn;
                                existente.IdQuimico = rec.IdQuimico;
                                existente.IdUM = rec.IdUM;
                                existente.FechaHoraUpd = DateTime.Now;
                                existente.UserUpd = User.Identity.Name;
                                existente.IpUpd = RemoteAddr();
                            }
                        }
                    }


                    dc.SubmitChanges();

                    var view = RenderRazorViewToString("ItemRowFicha", ficha);
                    return Json(new { ok = true, msg = "La ficha fue actualizada con éxito", view = view, id = ficha.IdFicha });
                }
                else
                {
                    return Json(new { ok = false, msg = "Corrige lo siguiente:", errors = rv });
                }

            }
            catch (Exception ex)
            {
                string msg = "Ocurrió un error al intentar actualizar la ficha";
                msg += ": " + ex.Message;

                return Json(new { ok = false, msg = msg });
            }
        }

    }
}
