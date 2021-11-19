using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Configuracion
{
    [WebsiteAuthorize]
    public class TasasCambioController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public TasasCambioController()
        {
            SetCurrentModulo(2); //Configuración
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(55);

            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var items = dc.TasaCambio.OrderByDescending(tc => tc.Fecha);
            var model = new PaginatedList<TasaCambio>(items, pageIndex, pageSize);

            //ViewData
            ViewData["monedas"] = dc.Moneda.ToList();

            return View(model);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(56);
            var tasaCambio = new TasaCambio
            {
                Fecha = DateTime.Today,
                IsNew = true,
                IdMoneda = 2
            };
            tasaCambio.Moneda = dc.Moneda.SingleOrDefault(m => m.IdMoneda == tasaCambio.IdMoneda);

            return View("TasaCambio", tasaCambio);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Crear([Bind(Exclude = "Fecha")]TasaCambio tasaCambio)
        {
            CheckPermisoAndRedirect(56);

            tasaCambio.Fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

            if (ModelState.IsValid)
            {
                try
                {
                    tasaCambio.UserIns = User.Identity.Name;
                    tasaCambio.FechaHoraIns = DateTime.Now;
                    tasaCambio.IpIns = RemoteAddr();

                    dc.TasaCambio.InsertOnSubmit(tasaCambio);

                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = tasaCambio.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                tasaCambio.Moneda = dc.Moneda.SingleOrDefault(m => m.IdMoneda == tasaCambio.IdMoneda);
            }


            return View("TasaCambio", tasaCambio);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(57);

            DateTime fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

            var tasaCambio = dc.TasaCambio.SingleOrDefault(p => p.IdMoneda == id && p.Fecha == fecha);
            if (tasaCambio == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Tasa de Cambio Not Found");

            return View("TasaCambio", tasaCambio);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(57);

            DateTime fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

            var tasaCambio = dc.TasaCambio.SingleOrDefault(p => p.IdMoneda == id && p.Fecha == fecha);
            var fields = new string[] { "Valor" };

            if (TryUpdateModel(tasaCambio, fields))
            {
                try
                {
                    tasaCambio.UserUpd = User.Identity.Name;
                    tasaCambio.FechaHoraUpd = DateTime.Now;
                    tasaCambio.IpUpd = RemoteAddr();

                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = tasaCambio.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("TasaCambio", tasaCambio);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(58);

            string msgok = "";
            string msgerr = "";
            try
            {
                var fecha = DateParser.DateFromRequest("Fecha").GetValueOrDefault();

                var tasaCambio = dc.TasaCambio.SingleOrDefault(ps => ps.IdMoneda == id && ps.Fecha == fecha);
                if (tasaCambio != null)
                    dc.TasaCambio.DeleteOnSubmit(tasaCambio);

                dc.SubmitChanges();

                msgok = "La Tasa de Cambio fue eliminada con éxito";

            }
            catch (Exception ex)
            {
                msgerr = "Ocurrió un error al eliminar la Tasa de Cambio: " + ex.Message;
            }

            return RedirectToAction("Index", new { msgok = msgok, msgerr = msgerr });
        }
    }
}
