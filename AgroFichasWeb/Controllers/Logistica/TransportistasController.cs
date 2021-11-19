using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    [WebsiteAuthorize]
    public class TransportistasController : BaseApplicationController
    {
        //
        // GET: /Transportistas/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public TransportistasController()
        {
            SetCurrentModulo(6);//Logística y Corretaje
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(88);

            var transportista = new LOG_Transportista();

            SetBancos(null);
            return View("crear", transportista);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(LOG_Transportista transportista)
        {
            CheckPermisoAndRedirect(88);

            if (ModelState.IsValid)
            {
                try
                {
                    if (transportista.Telefono == null)
                        transportista.Telefono = "";
                    if (transportista.NumeroCuenta == null)
                        transportista.NumeroCuenta = "";
                    if (transportista.Observacion == null)
                        transportista.Observacion = "";
                    if (transportista.Email == null)
                        transportista.Email = "";
                    if (transportista.IDOleotop == null)
                        transportista.IDOleotop = "";
                    if (transportista.IDAvenatop == null)
                        transportista.IDAvenatop = "";
                    if (transportista.IDGranotop == null)
                        transportista.IDGranotop = "";
                    if (transportista.IDSaprosem == null)
                        transportista.IDSaprosem = "";

                    transportista.Habilitado = true;
                    transportista.RUT = Rut.NomarlizarRut(transportista.RUT);
                    transportista.FechaHoraIns = DateTime.Now;
                    transportista.IpIns = RemoteAddr();
                    transportista.UserIns = User.Identity.Name;

                    transportista.Nombre = Comunes.RemoveDiacritics(transportista.Nombre).ToUpper();

                    dc.LOG_Transportista.InsertOnSubmit(transportista);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = transportista.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            SetBancos(transportista.IdBanco);
            return View("crear", transportista);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(89);

            var transportista = dc.LOG_Transportista.SingleOrDefault(x => x.IdTransportista == id && x.Habilitado == true);
            if (transportista == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el transportista"); }

            SetBancos(transportista.IdBanco);
            return View("crear", transportista);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(89);

            var transportista = dc.LOG_Transportista.SingleOrDefault(x => x.IdTransportista == id && x.Habilitado == true);
            if (transportista == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el transportista"); }

            try
            {
                UpdateModel(transportista, new string[] { "RUT", "Nombre", "Telefono", "IdBanco", "NumeroCuenta", "IDOleotop", "IDAvenatop", "IDGranotop", "IDSaprosem", "Observacion" });

                if (transportista.Telefono == null)
                    transportista.Telefono = "";
                if (transportista.NumeroCuenta == null)
                    transportista.NumeroCuenta = "";
                if (transportista.Observacion == null)
                    transportista.Observacion = "";
                if (transportista.Email == null)
                    transportista.Email = "";
                if (transportista.IDOleotop == null)
                    transportista.IDOleotop = "";
                if (transportista.IDAvenatop == null)
                    transportista.IDAvenatop = "";
                if (transportista.IDGranotop == null)
                    transportista.IDGranotop = "";
                if (transportista.IDSaprosem == null)
                    transportista.IDSaprosem = "";

                transportista.RUT = Rut.NomarlizarRut(transportista.RUT);
                transportista.UserUpd = User.Identity.Name;
                transportista.FechaHoraUpd = DateTime.Now;
                transportista.IpUpd = RemoteAddr();

                transportista.Nombre = Comunes.RemoveDiacritics(transportista.Nombre).ToUpper();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = transportista.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            SetBancos(transportista.IdBanco);
            return View("crear", transportista);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(90);

            var transportista = dc.LOG_Transportista.SingleOrDefault(x => x.IdTransportista == id && x.Habilitado == true);
            if (transportista == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el transportista"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                transportista.Habilitado = false;
                transportista.UserUpd = User.Identity.Name;
                transportista.FechaHoraUpd = DateTime.Now;
                transportista.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("El transportista {0} ha sido eliminado", transportista.Nombre);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(87);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            string key = Request.QueryString["key"] ?? "";

            IQueryable<LOG_Transportista> items = dc.LOG_Transportista.Where(x => x.Habilitado == true && (key == "" || x.Nombre.Contains(key) || x.RUT.Contains(key))).OrderBy(a => a.Nombre);

            var pagina = new PaginatedList<LOG_Transportista>(items, pageIndex, pageSize);

            ViewData["key"] = key;
            return View(pagina);
        }
        private void SetBancos(int? IdBanco)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.Banco
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdBanco == IdBanco && IdBanco != null),
                    Text = s.Nombre,
                    Value = s.IdBanco.ToString()
                };
            ViewData["bancosList"] = selectList;
        }
    }
}
