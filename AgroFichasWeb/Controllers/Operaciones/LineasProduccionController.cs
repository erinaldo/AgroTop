using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class LineasProduccionController : BaseApplicationController
    {
        //
        // GET: /LineasProduccion/

        OperacionesDBDataContext dc = new OperacionesDBDataContext();

        public LineasProduccionController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(178);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var model = new LineaProduccionViewModel();

            var lineasProduccion = dc.OPR_LineaProduccion.Where(X => X.Habilitado == true);
            foreach (var lineaProduccion in lineasProduccion)
            {
                var equiposPorLineaProduccion = new EquiposPorLineaProduccion()
                {
                    LineaProduccion = lineaProduccion,
                    Equipos = new List<Models.OPR_Equipo>()
                };
                var equipos = dc.OPR_GetLineaProduccionPorEquipo(lineaProduccion.IdLineaProduccion).ToList();
                if (equipos.Count > 0)
                {
                    foreach (var equipo in equipos)
                    {
                        equiposPorLineaProduccion.Equipos.Add(new OPR_Equipo()
                        {
                            IdEquipo = equipo.IdEquipo,
                            Descripcion = equipo.Descripcion
                        });
                    }
                }
                model.ListaEquiposPorLineaProduccion.Add(equiposPorLineaProduccion);
            }

            model.MensajeError        = Request["MensajeError"];
            model.MensajeExito        = Request["MensajeExito"];
            model.PuedeCrear          = CheckPermiso(179);
            model.PuedeEditar         = CheckPermiso(180);
            model.PuedeEliminar       = CheckPermiso(181);
            model.PuedeAsociarEquipos = CheckPermiso(191);
            model.PuedeVerEquipos     = CheckPermiso(182);

            return View(model);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(179);

            var lineaProduccion = new OPR_LineaProduccion();
            return View("crear", lineaProduccion);
        }

        [HttpPost]
        public ActionResult Crear(OPR_LineaProduccion lineaProduccion)
        {
            CheckPermisoAndRedirect(179);
            if (ModelState.IsValid)
            {
                try
                {
                    lineaProduccion.Habilitado = true;
                    lineaProduccion.FechaHoraIns = DateTime.Now;
                    lineaProduccion.IpIns = RemoteAddr();
                    lineaProduccion.UserIns = User.Identity.Name;
                    dc.OPR_LineaProduccion.InsertOnSubmit(lineaProduccion);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = lineaProduccion.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", lineaProduccion);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(180);

            var lineaProduccion = dc.OPR_LineaProduccion.SingleOrDefault(x => x.IdLineaProduccion == id && x.Habilitado == true);
            if (lineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la línea de producción"); }

            return View("crear", lineaProduccion);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(180);

            var lineaProduccion = dc.OPR_LineaProduccion.SingleOrDefault(x => x.IdLineaProduccion == id && x.Habilitado == true);
            if (lineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la línea de producción"); }

            try
            {
                UpdateModel(lineaProduccion, new string[] { "IdLineaProduccion", "Descripcion" });

                lineaProduccion.UserUpd = User.Identity.Name;
                lineaProduccion.FechaHoraUpd = DateTime.Now;
                lineaProduccion.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = lineaProduccion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }
            return View("crear", lineaProduccion);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(181);

            var lineaProduccion = dc.OPR_LineaProduccion.SingleOrDefault(x => x.IdLineaProduccion == id && x.Habilitado == true);
            if (lineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la línea de producción"); }

            string MensajeError = "";
            string MensajeExito = "";

            try
            {
                lineaProduccion.Habilitado = false;
                lineaProduccion.UserUpd = User.Identity.Name;
                lineaProduccion.FechaHoraUpd = DateTime.Now;
                lineaProduccion.IpUpd = RemoteAddr();
                dc.SubmitChanges();

                MensajeExito = String.Format("La línea de producción {0} ha sido eliminada", lineaProduccion.Descripcion);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            return RedirectToAction("Index", new { MensajeError = MensajeError, MensajeExito = MensajeExito });
        }

        public ActionResult EquiposAsociadosPorLineaProduccion(int id)
        {
            CheckPermisoAndRedirect(191);
            var lineaProduccion = dc.OPR_LineaProduccion.SingleOrDefault(x => x.IdLineaProduccion == id && x.Habilitado == true);
            if (lineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la línea de producción"); }

            var model = new LineaProduccionViewModel();
            model.MensajeError = Request["MensajeError"];
            model.MensajeExito = Request["MensajeExito"];
            model.LineaProduccion = lineaProduccion;
            model.EquiposAsociadosPorLineaProduccion = dc.OPR_GetEquiposAsociadosPorLineaProduccion(lineaProduccion.IdLineaProduccion).ToList();

            return View(model);
        }

        public ActionResult AsociarEquipos(int id)
        {
            CheckPermisoAndRedirect(191);
            var lineaProduccion = dc.OPR_LineaProduccion.SingleOrDefault(x => x.IdLineaProduccion == id && x.Habilitado == true);
            var equipos = dc.OPR_GetEquiposNoAsociadosPorLineaProduccion(lineaProduccion.IdLineaProduccion).ToList();
            var model = new LineaProduccionViewModel();
            model.LineaProduccion = lineaProduccion ?? throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la línea de producción");
            model.MensajeError = Request["MensajeError"];
            model.MensajeExito = Request["MensajeExito"];
            ViewData["equiposList"] = new SelectList(equipos, "IdEquipo", "Descripcion");
            return View(model);
        }

        [HttpPost]
        public ActionResult AsociarEquipos(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(191);
            var lineaProduccion = dc.OPR_LineaProduccion.SingleOrDefault(x => x.IdLineaProduccion == id && x.Habilitado == true);
            if (lineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la línea de producción"); }

            string MensajeError = "";
            string MensajeExito = "";

            if (string.IsNullOrEmpty(formValues["IdEquipos"]))
            {
                MensajeError = "Debes seleccionar al menos un equipo de la lista";
                return RedirectToAction("AsociarEquipos", new { id = lineaProduccion.IdLineaProduccion, MensajeError = MensajeError, MensajeExito = MensajeExito, });
            }
            else
            {
                string[] IdEquipos = formValues["IdEquipos"].Split(new char[] { ',' });
                for (int J = 0; J < IdEquipos.Length; J++)
                {
                    if (!int.TryParse(IdEquipos[J], out int idEquipo))
                        throw new HttpException(404, "Las unidades no son válidas");

                    var equipo = (from X in dc.OPR_Equipo
                                  where X.IdEquipo == idEquipo
                                  && X.Habilitado == true
                                  && ((from A in dc.OPR_EquipoPorLineaProduccion
                                       where A.IdEquipo == idEquipo
                                       && A.IdLineaProduccion == lineaProduccion.IdLineaProduccion
                                       select A).SingleOrDefault() == null)
                                  select X).SingleOrDefault();
                    if (equipo != null)
                    {
                        var equipoPorLineaProduccion = new OPR_EquipoPorLineaProduccion();
                        equipoPorLineaProduccion.IdEquipo = equipo.IdEquipo;
                        equipoPorLineaProduccion.IdLineaProduccion = lineaProduccion.IdLineaProduccion;
                        equipoPorLineaProduccion.UserIns = CurrentUser.UserIns;
                        equipoPorLineaProduccion.FechaHoraIns = DateTime.Now;
                        equipoPorLineaProduccion.IpIns = RemoteAddr();
                        dc.OPR_EquipoPorLineaProduccion.InsertOnSubmit(equipoPorLineaProduccion);
                        dc.SubmitChanges();
                    }
                }

                return RedirectToAction("EquiposAsociadosPorLineaProduccion", new { id = lineaProduccion.IdLineaProduccion });
            }
        }

        public ActionResult DesasociarEquipo(int id, int idEquipo)
        {
            CheckPermisoAndRedirect(191);
            var lineaProduccion = dc.OPR_LineaProduccion.SingleOrDefault(x => x.IdLineaProduccion == id && x.Habilitado == true);
            if (lineaProduccion == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la línea de producción"); }

            var equipo = dc.OPR_Equipo.SingleOrDefault(x => x.IdEquipo == idEquipo && x.Habilitado == true);
            if (equipo == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el equipo"); }

            string MensajeError = "";
            string MensajeExito = "";

            var equipoPorLineaProduccion = dc.OPR_EquipoPorLineaProduccion.SingleOrDefault(X => X.IdLineaProduccion == lineaProduccion.IdLineaProduccion && X.IdEquipo == equipo.IdEquipo);
            if (equipoPorLineaProduccion == null)
            {
                MensajeError = string.Format("El equipo {0} no está asociado a esta línea de producción", equipo.Descripcion);
            }
            else
            {
                dc.OPR_EquipoPorLineaProduccion.DeleteOnSubmit(equipoPorLineaProduccion);
                dc.SubmitChanges();
                MensajeExito = String.Format("El equipo {0} ha sido desasociado", equipo.Descripcion);
            }

            return RedirectToAction("EquiposAsociadosPorLineaProduccion", new { id = lineaProduccion.IdLineaProduccion, MensajeError = MensajeError, MensajeExito = MensajeExito, });
        }
    }
}