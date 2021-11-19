using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Configuracion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Configuracion
{
    [WebsiteAuthorize]
    public class PreciosServicioController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();
        
        public PreciosServicioController()
        {
            SetCurrentModulo(2); //Configuración;

            ViewData["cultivos"] = dc.Cultivo.Where(c => (new int[] { 1, 2, 3, 4, 10, 16 }).Contains(c.IdCultivo)).ToList();
        }


        public ActionResult Index(int? id, int? idCultivo, int? idTipoServicio, DateTime? fecha1, int? idSucursal)
        {
            CheckPermisoAndRedirect(51);

            var FechaSelect = (fecha1.HasValue ? fecha1.Value : DateTime.Now);
            var idCultivoSelect = idCultivo ?? 0;
            var idTipoServicioSelect = idTipoServicio ?? 0;
            var idSucursalSelect = idSucursal ?? 0;

            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            List<PrecioServicio> preciosServicio = (from X in dc.PrecioServicio
                                                    where (idCultivoSelect == 0 || X.IdCultivo == idCultivoSelect) &&
                                                          (idTipoServicioSelect == 0 || X.IdTipoServicio == idTipoServicioSelect) &&
                                                          (X.Fecha <= FechaSelect) &&
                                                          (idSucursalSelect == 0 || X.IdSucursal == idSucursalSelect) &&
                                                          (X.Habilitado == true)
                                                    orderby X.Fecha descending
                                                    select X).ToList();


            //var precios = dc.PrecioServicio.Where(ps => ps.IdCultivo == cultivo).OrderBy(ps => ps.IdSucursal).OrderByDescending(ps => ps.Fecha);
            //var pagina = new PaginatedList<PrecioServicio>(precios, pageIndex, pageSize);
            PrecioServicio ps = new PrecioServicio();
            ps.IdCultivoSelect = idCultivo ?? 0;
            ps.IdTipoServicioSelect = idTipoServicio ?? 0;
            ps.IdSucursalSelect = idSucursal ?? 0;
            ps.Fecha1 = FechaSelect;

            ViewData["precioServicio"] = ps;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(2002),
                                                      CheckPermiso(2001),
                                                      CheckPermiso(2003),
                                                      CheckPermiso(2004));

            return View(preciosServicio);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(2002);
            //var precio = new PrecioServicioViewModel(dc, cultivo, DateTime.Today, true);
            //ViewData["tipoServicio"] = dc.TipoServicio.ToList();

            PrecioServicio precioServicio = new PrecioServicio();
            precioServicio.Fecha = DateTime.Now;


            return View("Crear", precioServicio);
        }

        [HttpPost]
        public ActionResult Crear(PrecioServicio precioServicio)
        {
            CheckPermisoAndRedirect(2002);
            if (ModelState.IsValid)
            {
                try
                {
                    precioServicio.ValorForm = precioServicio.Valor.ToString();
                    precioServicio.Habilitado = true;
                    precioServicio.FechaHoraIns = DateTime.Now;
                    precioServicio.IpIns = RemoteAddr();
                    precioServicio.UserIns = User.Identity.Name;
                    dc.PrecioServicio.InsertOnSubmit(precioServicio);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = precioServicio.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", precioServicio);
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(2003);

            //DateTime fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();
            //var precio = new PrecioServicioViewModel(dc, id, fecha, false);
                        
            PrecioServicio precioServicio = dc.PrecioServicio.SingleOrDefault(X => X.IdPrecioServicio == id);
            if (precioServicio == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el precio de servicio", okMsg = "" }); }

            return View("editar", precioServicio);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(2003);
            var errMsg = "";



            PrecioServicio precioServicio = dc.PrecioServicio.SingleOrDefault(X => X.IdPrecioServicio == id && X.Habilitado == true);
            precioServicio.ValorForm = formValues[string.Format("Valor")];
            if (precioServicio == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el precio de servicio", okMsg = "" });
            }
            if (ModelState.IsValid)
            {
                try
                {

                    UpdateModel(precioServicio, new string[] { "IdSucursal", "IdTipoServicio", "IdCultivo", "Valor", "Fecha" });

                    precioServicio.UserUpd = User.Identity.Name;
                    precioServicio.FechaHoraUpd = DateTime.Now;
                    precioServicio.IpUpd = RemoteAddr();
                    dc.SubmitChanges();

                    return RedirectToAction("index", new { errMsg = errMsg, okMsg = "El precio del servicio se ha editado" });
                }

                catch
                {
                    var rv = precioServicio.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso()
            {
                Leer = CheckPermiso(2001),
                Crear = CheckPermiso(2002),
                Actualizar = CheckPermiso(2003),
                Borrar = CheckPermiso(2004)
            };

            return View("Editar", precioServicio);

        }

        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Editar([Bind(Exclude = "Fecha, Cultivo")]PrecioServicioViewModel model)
        //{
        //    CheckPermisoAndRedirect(53);
        //    model.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

        //    model.Validate(dc, ModelState);
        //    if (ModelState.IsValid)
        //    {
        //        model.Persist(dc, CurrentUser.UserName, RemoteAddr());
        //        return RedirectToAction("Index", new { cultivo = model.IdCultivo });
        //    }
        //    else
        //    {
        //        model.LoadLookups(dc);
        //        return View("PrecioServicio", model);
        //    }
        //}

        public ActionResult Eliminar(int id)
        {
            PrecioServicio precioServicio = dc.PrecioServicio.SingleOrDefault(X => X.IdPrecioServicio == id && X.Habilitado == true);
            if (precioServicio == null) { return RedirectToAction("Index", new { errMsg = "No se ha encontrado el Precio de Servicio", okMsg = "" }); }

            string errMsg = "";
            string okMsg = "";
            
            try
            {
                precioServicio.ValorForm = precioServicio.Valor.ToString();
                precioServicio.Habilitado = false;
                precioServicio.UserUpd = User.Identity.Name;
                precioServicio.FechaHoraUpd = DateTime.Now;
                precioServicio.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                okMsg = String.Format("El Precio de Servicio ha sido eliminado");
            }
            catch (Exception ex)
            {
                var rv = precioServicio.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
                //errMsg = ex.Message;
            }
            
            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

    }
}
