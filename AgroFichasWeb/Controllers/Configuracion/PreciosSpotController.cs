using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Configuracion
{
    [WebsiteAuthorize]
    public class PreciosSpotController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();
        
        public PreciosSpotController()
        {
            SetCurrentModulo(2); //Configuración;

            ViewData["cultivos"] = dc.Cultivo.Where(c => (new int[] { 1, 2, 3, 4, 10, 16 }).Contains(c.IdCultivo)).ToList();
        }


        public ActionResult Index(int? id, int cultivo)
        {
            CheckPermisoAndRedirect(51);

            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var precios = dc.vwPrecioSpot.Where(ps => ps.IdCultivo == cultivo).OrderBy(ps => ps.Sucursal).OrderByDescending(ps => ps.Fecha);
            var pagina = new PaginatedList<vwPrecioSpot>(precios, pageIndex, pageSize);

            ViewData["cultivo"] = dc.Cultivo.Single(c => c.IdCultivo == cultivo);
            ViewData["monedas"] = dc.Moneda.ToList();

            return View(pagina);
        }

        public ActionResult Crear(int cultivo)
        {
            CheckPermisoAndRedirect(52);
            var precio = new PrecioSpotViewModel(dc, cultivo, DateTime.Today, true);
        
            return View("PrecioSpot", precio);
        }

        [HttpPost]
        public ActionResult Crear([Bind(Exclude = "Fecha, Cultivo")]PrecioSpotViewModel model)
        {
            CheckPermisoAndRedirect(52);
            model.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

            model.Validate(dc, ModelState);
            if (ModelState.IsValid)
            {
                model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("Index", new { cultivo = model.IdCultivo });
            }
            else
            {
                model.LoadLookups(dc);
                return View("PrecioSpot", model);
            }
        }


        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(53);

            DateTime fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

            var precio = new PrecioSpotViewModel(dc, id, fecha, false);

            return View("PrecioSpot", precio);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Editar([Bind(Exclude = "Fecha, Cultivo")]PrecioSpotViewModel model)
        {
            CheckPermisoAndRedirect(53);
            model.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

            model.Validate(dc, ModelState);
            if (ModelState.IsValid)
            {
                model.Persist(dc, CurrentUser.UserName, RemoteAddr());
                return RedirectToAction("Index", new { cultivo = model.IdCultivo });
            }
            else
            {
                model.LoadLookups(dc);
                return View("PrecioSpot", model);
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(54);

            string msgok = "";
            string msgerr = "";
            try
            {
                var fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

                var precioCLP = dc.PrecioSpot.SingleOrDefault(ps => ps.IdCultivo == id && ps.IdMoneda == 1 && ps.Fecha == fecha);
                if (precioCLP != null)
                    dc.PrecioSpot.DeleteOnSubmit(precioCLP);

                var precioUSD = dc.PrecioSpot.SingleOrDefault(ps => ps.IdCultivo == id && ps.IdMoneda == 2 && ps.Fecha == fecha);
                if (precioUSD != null)
                    dc.PrecioSpot.DeleteOnSubmit(precioUSD);


                dc.SubmitChanges();

                msgok = "El precio spot fue eliminado con éxito";
               
            }
            catch (Exception ex)
            {
                msgerr = "Ocurrió un error al eliminar el precio spot: " + ex.Message;
            }

            return RedirectToAction("Index", new { cultivo = id,  msgok = msgok, msgerr = msgerr });
        }

    }
}
