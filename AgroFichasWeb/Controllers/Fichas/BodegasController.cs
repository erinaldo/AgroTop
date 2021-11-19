using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Fichas
{
    public class BodegasController : BaseApplicationController
    {
        //
        // GET: /Bodegas/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public BodegasController()
        {
            SetCurrentModulo(1);//Fichas
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(108);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<Bodega> items = null;
            string key = Request.QueryString["key"] ?? "";
            int IdSucursal = 0;
            if (!int.TryParse(Request.QueryString["IdSucursal"], out IdSucursal))
            {
                // Previene el error / fallo
            }

            if (IdSucursal != 0)
            {
                items = dc.Bodega.Where(x => x.Habilitada == true && x.Sucursal.Habilitada == true && x.IdSucursal == IdSucursal).OrderBy(a => a.Sucursal.Nombre).ThenBy(a => a.Nombre);
            }
            else
            {
                items = dc.Bodega.Where(x => x.Habilitada == true && x.Sucursal.Habilitada == true && (key == "" || x.Nombre.Contains(key) || x.Sucursal.Nombre.Contains(key) || x.Sucursal.Comuna.Nombre.Contains(key))).OrderBy(a => a.Sucursal.Nombre).ThenBy(a => a.Nombre);
            }

            var pagina = new PaginatedList<Bodega>(items, pageIndex, pageSize);

            SetSucursales(null);

            ViewData["key"] = key;
            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(109);

            var bodega = new Bodega();

            SetSucursales(null);
            return View("crear", bodega);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(Bodega bodega)
        {
            CheckPermisoAndRedirect(109);
            if (ModelState.IsValid)
            {
                try
                {
                    if (bodega.IDAvenatop == null)
                        bodega.IDAvenatop = "";
                    if (bodega.IDGranotop == null)
                        bodega.IDGranotop = "";
                    if (bodega.IDOleotop == null)
                        bodega.IDOleotop = "";
                    if (bodega.IDSaprosem == null)
                        bodega.IDSaprosem = "";

                    bodega.IdBodega = (dc.Bodega.Max(x => x.IdBodega) + 1);
                    bodega.Habilitada = true;
                    bodega.FechaHoraIns = DateTime.Now;
                    bodega.IpIns = RemoteAddr();
                    bodega.UserIns = User.Identity.Name;
                    dc.Bodega.InsertOnSubmit(bodega);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = bodega.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            SetSucursales(bodega.IdSucursal);
            return View("crear", bodega);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(110);

            var bodega = dc.Bodega.SingleOrDefault(x => x.IdBodega == id && x.Habilitada == true && x.Sucursal.Habilitada == true);
            if (bodega == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la bodega");

            SetSucursales(bodega.IdSucursal);
            return View("crear", bodega);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(110);

            var bodega = dc.Bodega.SingleOrDefault(x => x.IdBodega == id && x.Habilitada == true && x.Sucursal.Habilitada == true);
            if (bodega == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la bodega");

            try
            {
                UpdateModel(bodega, new string[] { "Nombre", "IdSucursal", "EsManga", "NombreCorto", "IDOleotop", "IDAvenatop", "IDGranotop", "IDSaprosem" });

                if (bodega.IDAvenatop == null)
                    bodega.IDAvenatop = "";
                if (bodega.IDGranotop == null)
                    bodega.IDGranotop = "";
                if (bodega.IDOleotop == null)
                    bodega.IDOleotop = "";
                if (bodega.IDSaprosem == null)
                    bodega.IDSaprosem = "";

                bodega.UserUpd = User.Identity.Name;
                bodega.FechaHoraUpd = DateTime.Now;
                bodega.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = bodega.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            SetSucursales(bodega.IdSucursal);
            return View("crear", bodega);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(111);

            var bodega = dc.Bodega.SingleOrDefault(x => x.IdBodega == id && x.Habilitada == true && x.Sucursal.Habilitada == true);
            if (bodega == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la bodega");

            string msgerr = "";
            string msgok = "";
            try
            {
                bodega.Habilitada = false;
                bodega.UserUpd = User.Identity.Name;
                bodega.FechaHoraUpd = DateTime.Now;
                bodega.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("La bodega {0} ha sido eliminada", bodega.Nombre);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        private void SetSucursales(int? IdSucursal)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.Sucursal
                where s.Habilitada == true
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdSucursal == IdSucursal && IdSucursal != null),
                    Text = s.Nombre,
                    Value = s.IdSucursal.ToString()
                };
            ViewData["sucursalesList"] = selectList;
        }
    }
}