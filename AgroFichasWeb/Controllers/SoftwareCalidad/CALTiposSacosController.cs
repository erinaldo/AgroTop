using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALTiposSacosController : BaseApplicationController
    {
        //
        // GET: /CALTiposSacos/

        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public CALTiposSacosController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(243);
            List<CAL_TipoSaco> list = dc.CAL_TipoSaco.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(244),
                                                      CheckPermiso(243),
                                                      CheckPermiso(245),
                                                      CheckPermiso(246));
            return View(list);
        }

        public ActionResult Pesos(int? id)
        {
            CheckPermisoAndRedirect(243);
            List<CAL_PesoSaco> list = dc.CAL_PesoSaco.Where(X => X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(244),
                                                      CheckPermiso(243),
                                                      CheckPermiso(245),
                                                      CheckPermiso(246));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(244);
            CAL_TipoSaco tipoSaco = new CAL_TipoSaco();
            return View("crear", tipoSaco);
        }

        [HttpPost]
        public ActionResult Crear(CAL_TipoSaco tipoSaco, FormCollection formValues)
        {
            CheckPermisoAndRedirect(244);
            if (ModelState.IsValid)
            {
                try
                {
                    tipoSaco.Descripcion = tipoSaco.Descripcion.ToUpperInvariant();

                    tipoSaco.Habilitado = true;
                    tipoSaco.FechaHoraIns = DateTime.Now;
                    tipoSaco.IpIns = RemoteAddr();
                    tipoSaco.UserIns = User.Identity.Name;
                    dc.CAL_TipoSaco.InsertOnSubmit(tipoSaco);
                    dc.SubmitChanges();

                    string[] ids = { };

                    if (formValues["chkPesoSaco"] != null && formValues["chkPesoSaco"] != "")
                        ids = formValues["chkPesoSaco"].Split(',');

                    foreach (string idx in ids)
                    {
                        var pesoSaco = new CAL_PesoTipoSaco()
                        {
                            IdTipoSaco = tipoSaco.IdTipoSaco,
                            IdPesoSaco = int.Parse(idx),
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        };
                        tipoSaco.CAL_PesoTipoSaco.Add(pesoSaco);
                    }

                    dc.SubmitChanges();

                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = tipoSaco.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", tipoSaco);
        }

        public ActionResult CrearPeso()
        {
            CheckPermisoAndRedirect(244);
            CAL_PesoSaco pesoSaco = new CAL_PesoSaco();
            return View("CrearPeso", pesoSaco);
        }

        [HttpPost]
        public ActionResult CrearPeso(CAL_PesoSaco pesoSaco, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(244);
            if (ModelState.IsValid)
            {
                try
                {
                    pesoSaco.Habilitado = true;
                    pesoSaco.FechaHoraIns = DateTime.Now;
                    pesoSaco.IpIns = RemoteAddr();
                    pesoSaco.UserIns = User.Identity.Name;
                    dc.CAL_PesoSaco.InsertOnSubmit(pesoSaco);
                    dc.SubmitChanges();
                    return RedirectToAction("Pesos");
                }
                catch
                {
                    var rv = pesoSaco.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("CrearPeso", pesoSaco);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(245);
            var tipoSaco = dc.CAL_TipoSaco.SingleOrDefault(X => X.IdTipoSaco == id && X.Habilitado == true);
            if (tipoSaco == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de saco", okMsg = "" }); }
            return View("crear", tipoSaco);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(245);
            var tipoSaco = dc.CAL_TipoSaco.SingleOrDefault(X => X.IdTipoSaco == id && X.Habilitado == true);
            if (tipoSaco == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de saco", okMsg = "" }); }

            try
            {
                UpdateModel(tipoSaco, new string[] { "Descripcion" });

                tipoSaco.Descripcion = tipoSaco.Descripcion.ToUpperInvariant();
                tipoSaco.UserUpd = User.Identity.Name;
                tipoSaco.FechaHoraUpd = DateTime.Now;
                tipoSaco.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                string[] ids = { };

                if (formValues["chkPesoSaco"] != null && formValues["chkPesoSaco"] != "")
                    ids = formValues["chkPesoSaco"].Split(',');

                foreach (var pesoSaco in tipoSaco.CAL_PesoTipoSaco)
                    if (ids.SingleOrDefault(idx => idx == pesoSaco.IdPesoSaco.ToString()) == null)
                        dc.CAL_PesoTipoSaco.DeleteOnSubmit(pesoSaco);

                foreach (string idx in ids)
                {
                    if (tipoSaco.CAL_PesoTipoSaco.SingleOrDefault(X => X.IdPesoSaco == int.Parse(idx)) == null)
                    {
                        var pesoSaco = new CAL_PesoTipoSaco()
                        {
                            IdTipoSaco = tipoSaco.IdTipoSaco,
                            IdPesoSaco = int.Parse(idx),
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        };
                        tipoSaco.CAL_PesoTipoSaco.Add(pesoSaco);
                    }
                }

                dc.SubmitChanges();

                return RedirectToAction("index");
            }
            catch
            {
                var rv = tipoSaco.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", tipoSaco);
        }

        public ActionResult EditarPeso(int id)
        {
            CheckPermisoAndRedirect(245);
            var pesoSaco = dc.CAL_PesoSaco.SingleOrDefault(X => X.IdPesoSaco == id && X.Habilitado == true);
            if (pesoSaco == null) { return RedirectToAction("Pesos", new { errMsg = "No se ha encontrado el peso", okMsg = "" }); }
            return View("CrearPeso", pesoSaco);
        }

        [HttpPost]
        public ActionResult EditarPeso(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(245);
            var pesoSaco = dc.CAL_PesoSaco.SingleOrDefault(X => X.IdPesoSaco == id && X.Habilitado == true);
            if (pesoSaco == null) { return RedirectToAction("Pesos", new { errMsg = "No se ha encontrado el peso", okMsg = "" }); }

            try
            {
                UpdateModel(pesoSaco, new string[] { "Peso" });

                pesoSaco.UserUpd = User.Identity.Name;
                pesoSaco.FechaHoraUpd = DateTime.Now;
                pesoSaco.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                return RedirectToAction("Pesos");
            }
            catch
            {
                var rv = pesoSaco.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("CrearPeso", pesoSaco);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(246);
            var tipoSaco = dc.CAL_TipoSaco.SingleOrDefault(X => X.IdTipoSaco == id && X.Habilitado == true);
            if (tipoSaco == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el tipo de saco", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                tipoSaco.Habilitado = false;
                tipoSaco.UserUpd = User.Identity.Name;
                tipoSaco.FechaHoraUpd = DateTime.Now;
                tipoSaco.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El tipo de saco {0} ha sido eliminado", tipoSaco.Descripcion);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult EliminarPeso(int id)
        {
            CheckPermisoAndRedirect(246);
            var pesoSaco = dc.CAL_PesoSaco.SingleOrDefault(X => X.IdPesoSaco == id && X.Habilitado == true);
            if (pesoSaco == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el peso", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                pesoSaco.Habilitado = false;
                pesoSaco.UserUpd = User.Identity.Name;
                pesoSaco.FechaHoraUpd = DateTime.Now;
                pesoSaco.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El peso {0} ha sido eliminado", pesoSaco.Peso);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Pesos", new { errMsg = errMsg, okMsg = okMsg });
        }
    }
}
