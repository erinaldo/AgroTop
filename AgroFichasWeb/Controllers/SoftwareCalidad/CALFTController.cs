using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFTController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFT
        public ActionResult Asociar(int id)
        {
            CheckPermisoAndRedirect(297);
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion == null) return RedirectToAction("Index", new { errMsg = "No se ha encontrado la orden de producción", okMsg = "" });
            List<CAL_DetalleOrdenProduccion> list = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(X => X.IdOrdenProduccion == ordenProduccion.IdOrdenProduccion).ToList();
            ViewData["ordenProduccion"] = ordenProduccion;
            return View("Asociar", list);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(297);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null) return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });
            return View("Editar", detalleOrdenProduccion);
        }

        [HttpPost]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(297);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null) return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });

            try
            {
                UpdateModel(detalleOrdenProduccion, new string[] { "IdFichaTecnica" });

                detalleOrdenProduccion.UserUpd = User.Identity.Name;
                detalleOrdenProduccion.FechaHoraUpd = DateTime.Now;
                detalleOrdenProduccion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Asociar", new { id = detalleOrdenProduccion.IdOrdenProduccion });
            }
            catch
            {
                var rv = detalleOrdenProduccion.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Editar", detalleOrdenProduccion);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(297);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null) return RedirectToAction("Index", new { errMsg = "No se ha encontrado el detalle de la orden de producción", okMsg = "" });

            string errMsg = "";
            string okMsg = "";

            try
            {
                detalleOrdenProduccion.IdFichaTecnica = null;
                detalleOrdenProduccion.UserUpd = User.Identity.Name;
                detalleOrdenProduccion.FechaHoraUpd = DateTime.Now;
                detalleOrdenProduccion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = string.Format("Se desasoció la ficha técnica al producto {0}", detalleOrdenProduccion.CAL_Subproducto.Nombre);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Asociar", new { id = detalleOrdenProduccion.IdOrdenProduccion, errMsg = errMsg, okMsg = okMsg });
        }
    }
}