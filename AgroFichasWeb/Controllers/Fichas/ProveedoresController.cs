using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class ProveedoresController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ProveedoresController()
        {
            SetCurrentModulo(1); //Fichas;
        }

        public ActionResult Index(int? id, string msg)
        {
            CheckPermisoAndRedirect(6);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;


            var items = dc.Proveedor.OrderBy(a => a.Nombre);
            var pagina = new PaginatedList<Proveedor>(items, pageIndex, pageSize);

            if (msg != null)
                ModelState.AddRuleViolations(new List<RuleViolation>() { new RuleViolation(msg, "Eliminar") });

            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(6);
            var Proveedor = new Proveedor
            {
                Habilitado = true
            };
            return View("Proveedor", Proveedor);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(Proveedor proveedor)
        {
            CheckPermisoAndRedirect(6);
            if (ModelState.IsValid)
            {
                try
                {
                    proveedor.UserIns = User.Identity.Name;
                    proveedor.FechaHoraIns = DateTime.Now;
                    proveedor.IpIns = RemoteAddr();
                    proveedor.MobileTag = "";
                    dc.Proveedor.InsertOnSubmit(proveedor);
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = proveedor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            return View("Proveedor", proveedor);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(6);
            var proveedor = dc.Proveedor.SingleOrDefault(p => p.IdProveedor == id);
            if (proveedor == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Proveedor Not Found");

            return View("Proveedor", proveedor);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(6);
            var proveedor = dc.Proveedor.SingleOrDefault(p => p.IdProveedor == id);

            try
            {
                UpdateModel(proveedor, new string[] { "Nombre", "Email", "Telefono1", "Habilitado" });

                proveedor.UserUpd = User.Identity.Name;
                proveedor.FechaHoraUpd = DateTime.Now;
                proveedor.IpUpd = RemoteAddr();

                dc.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var rv = proveedor.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("Proveedor", proveedor);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(6);
            var proveedor = dc.Proveedor.SingleOrDefault(p => p.IdProveedor == id);

            string msg = "";
            try
            {
                if (proveedor != null)
                {
                    dc.Proveedor.DeleteOnSubmit(proveedor);
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK_Agricultor_Proveedor"))
                    msg = "No es posible elimnar al proveedor porque tiene al menos un agricultor asociado";
                else
                    msg = ex.Message;
            }

            return RedirectToAction("Index", new { msg = msg });
        }

        public ActionResult Agricultores(int id)
        {
            CheckPermisoAndRedirect(6);

            var data = dc.SelectAgricultoresProveedor(id).ToList();

            return View(data);
        }

        [HttpPost]
        public ActionResult Agricultores(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(6);

            var proveedor = dc.Proveedor.Single(p => p.IdProveedor == id);

            string[] ids = { };

            if (formValues["ia"] != null && formValues["ia"] != "")
                ids = formValues["ia"].Split(',');

            //Para cada agricultor que tenía el proveedor revisamos si sigue en la lista. Si no está en la lista nueva lo asignamos al proveedor por defecto
            foreach (var agricultor in proveedor.Agricultor)
                if (ids.SingleOrDefault(idx => idx == agricultor.IdAgricultor.ToString()) == null)
                    agricultor.IdProveedor = 1;

            //Para cada agricultor de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
            foreach (string idx in ids)
            {
                if (proveedor.Agricultor.SingleOrDefault(c => c.IdAgricultor == int.Parse(idx)) == null)
                {
                    var agricultor = dc.Agricultor.Single(ag => ag.IdAgricultor == int.Parse(idx));
                    agricultor.IdProveedor = proveedor.IdProveedor;
                }
            }

            dc.SubmitChanges();
            return RedirectToAction("Index");
        }

    }
}
