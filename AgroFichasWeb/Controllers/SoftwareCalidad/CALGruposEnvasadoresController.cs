using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALGruposEnvasadoresController : BaseApplicationController
    {
        //
        // GET: /CALGruposEnvasadores/

        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALGruposEnvasadoresController()
        {
            SetCurrentModulo(10);
        }

        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(313);

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
            List<CAL_GrupoEnvasador> list = new List<CAL_GrupoEnvasador>();
            if (IdPlantaProduccionSelect == 0)
            {
                list = dcSoftwareCalidad.CAL_GrupoEnvasador.Where(X => X.Habilitado == true).ToList();
            }
            else
            {
                list = dcSoftwareCalidad.CAL_GrupoEnvasador.Where(X => X.Habilitado == true && X.IdPlanta == IdPlantaProduccionSelect).ToList();
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"]  = Request["okMsg"];
            ViewData["PlantasProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(314),
                                                      CheckPermiso(313),
                                                      CheckPermiso(315),
                                                      CheckPermiso(316));
            return View(list);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(315);
            CAL_GrupoEnvasador grupoEnvasador = dcSoftwareCalidad.CAL_GrupoEnvasador.SingleOrDefault(X => X.IdGrupoEnvasador == id && X.Habilitado == true);
            if (grupoEnvasador == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el grupo de envasadores", okMsg = "" }); }
            CAL_Turno turno = dcSoftwareCalidad.CAL_Turno.Single(X => X.IdTurno == grupoEnvasador.IdTurno);
            grupoEnvasador.CAL_Turno = turno;
            return View("Crear", grupoEnvasador);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(315);
            CAL_GrupoEnvasador grupoEnvasador = dcSoftwareCalidad.CAL_GrupoEnvasador.SingleOrDefault(X => X.IdGrupoEnvasador == id && X.Habilitado == true);
            if (grupoEnvasador == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el grupo de envasadores", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";

            try
            {
                if (grupoEnvasador.ValidacionEnvasadores(ModelState, System.Web.HttpContext.Current))
                {
                    UpdateModel(grupoEnvasador, new string[] { "Nombre" });

                    string[] ids = formCollection["chkEnvasador"].Split(new char[] { ',' }).ToArray<string>();

                    foreach (var CAL_Envasador in grupoEnvasador.CAL_Envasador)
                        if (ids.SingleOrDefault(idx => idx == CAL_Envasador.UserID.ToString()) == null)
                            dcSoftwareCalidad.CAL_Envasador.DeleteOnSubmit(CAL_Envasador);

                    List<SYS_User> users = new List<SYS_User>();

                    foreach (var idx in ids)
                    {
                        SYS_User sYS_User = dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserID == idx.ToInt(0));
                        if (sYS_User != null)
                        {
                            if (grupoEnvasador.CAL_Envasador.SingleOrDefault(X => X.UserID == int.Parse(idx)) == null)
                            {
                                CAL_Envasador envasador = new CAL_Envasador()
                                {
                                    IdGrupoEnvasador  = grupoEnvasador.IdGrupoEnvasador,
                                    UserID            = sYS_User.UserID,
                                    Habilitado        = true,
                                    UserIns           = User.Identity.Name,
                                    FechaHoraIns      = DateTime.Now,
                                    IpIns = RemoteAddr()
                                };
                                dcSoftwareCalidad.CAL_Envasador.InsertOnSubmit(envasador);
                                dcSoftwareCalidad.SubmitChanges();
                            }
                        }

                        users.Add(sYS_User);
                    }

                    grupoEnvasador.IdTipoGrupoEnvasador = grupoEnvasador.GetTipoEnvasadores(users);
                    grupoEnvasador.UserUpd              = User.Identity.Name;
                    grupoEnvasador.FechaHoraUpd         = DateTime.Now;
                    grupoEnvasador.IpUpd                = RemoteAddr();
                    dcSoftwareCalidad.SubmitChanges();

                    okMsg = string.Format("El grupo de envasadores {0} ha sido editado", grupoEnvasador.Nombre);

                    return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
                }
            }
            catch
            {
                var rv = grupoEnvasador.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            CAL_Turno2 turno = dcSoftwareCalidad.CAL_Turno2.Single(X => X.IdTurno == grupoEnvasador.IdTurno2);
            grupoEnvasador.CAL_Turno2 = turno;
            return View("Crear", grupoEnvasador);
        }
    }
}