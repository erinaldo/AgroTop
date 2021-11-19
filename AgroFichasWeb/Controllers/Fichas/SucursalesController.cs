using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Fichas
{
    public class SucursalesController : BaseApplicationController
    {
        //
        // GET: /Sucursales/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public SucursalesController()
        {
            SetCurrentModulo(1);//Fichas
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(104);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            string key = Request.QueryString["key"] ?? "";

            IQueryable<Sucursal> items = dc.Sucursal.Where(x => x.Habilitada == true && (key == "" || x.Nombre.Contains(key) || x.Comuna.Nombre.Contains(key))).OrderBy(a => a.Nombre);

            var pagina = new PaginatedList<Sucursal>(items, pageIndex, pageSize);

            ViewData["key"] = key;
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(105);

            var sucursal = new Sucursal();

            SetComunas(null);
            return View("crear", sucursal);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(Sucursal sucursal)
        {
            CheckPermisoAndRedirect(105);
            if (ModelState.IsValid)
            {
                try
                {
                    if (sucursal.IDOleotop == null)
                        sucursal.IDOleotop = "";
                    if (sucursal.IDAvenatop == null)
                        sucursal.IDAvenatop = "";
                    if (sucursal.IDGranotop == null)
                        sucursal.IDGranotop = "";
                    if (sucursal.IDSaprosem == null)
                        sucursal.IDSaprosem = "";

                    sucursal.IdSucursal = (dc.Sucursal.Max(x => x.IdSucursal) + 1);
                    sucursal.Habilitada = true;
                    sucursal.FechaHoraIns = DateTime.Now;
                    sucursal.IpIns = RemoteAddr();
                    sucursal.UserIns = User.Identity.Name;
                    dc.Sucursal.InsertOnSubmit(sucursal);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = sucursal.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            SetComunas(sucursal.IdComuna);
            return View("crear", sucursal);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(106);

            var sucursal = dc.Sucursal.SingleOrDefault(x => x.IdSucursal == id && x.Habilitada == true);
            if (sucursal == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la sucursal");

            SetComunas(sucursal.IdComuna);
            return View("crear", sucursal);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(106);

            var sucursal = dc.Sucursal.SingleOrDefault(x => x.IdSucursal == id && x.Habilitada == true);
            if (sucursal == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la sucursal");

            try
            {
                UpdateModel(sucursal, new string[] { "Nombre", "IdComuna", "IDOleotop", "IDAvenatop", "IDGranotop", "IDSaprosem" });

                if (sucursal.IDOleotop == null)
                    sucursal.IDOleotop = "";
                if (sucursal.IDAvenatop == null)
                    sucursal.IDAvenatop = "";
                if (sucursal.IDGranotop == null)
                    sucursal.IDGranotop = "";
                if (sucursal.IDSaprosem == null)
                    sucursal.IDSaprosem = "";

                sucursal.UserUpd = User.Identity.Name;
                sucursal.FechaHoraUpd = DateTime.Now;
                sucursal.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = sucursal.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            SetComunas(sucursal.IdComuna);
            return View("crear", sucursal);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(107);

            var sucursal = dc.Sucursal.SingleOrDefault(x => x.IdSucursal == id && x.Habilitada == true);
            if (sucursal == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la sucursal");

            string msgerr = "";
            string msgok = "";
            try
            {
                sucursal.Habilitada = false;
                sucursal.UserUpd = User.Identity.Name;
                sucursal.FechaHoraUpd = DateTime.Now;
                sucursal.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("La sucursal {0} ha sido eliminada", sucursal.Nombre);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        private void SetComunas(int? IdComuna)
        {
            var selectList = Comuna.SelectList(dc, IdComuna);
            ViewData["comunasList"] = selectList;
        }
    }
}
