using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class SaldosCtaCteController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public SaldosCtaCteController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" }, 
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        private void SetLookupLists()
        {
            ViewData["empresas"] = dc.Empresa.ToList();
        }

        public ActionResult Index(int? pageIndex, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(40);

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from c in dc.SaldoCtaCte
                        where (idEmpresaSelect == 0 || c.IdEmpresa == idEmpresaSelect)
                           && (key == "" || 
                               c.Agricultor.Nombre.Contains(key) ||
                               c.Empresa.Nombre.Contains(key))
                        orderby c.Agricultor.Nombre
                        select c;

            var model = new PaginatedList<SaldoCtaCte>(items, pageIndex, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key }, 
                { "pageIndex", model.PageIndex },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" }
            };

            //ViewData
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["key"] = key;
            
            return View(model);
        }

        public ActionResult IndexExport(int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(40);

            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from c in dc.SaldoCtaCte
                        where (idEmpresaSelect == 0 || c.IdEmpresa == idEmpresaSelect)
                           && (key == "" ||
                               c.Agricultor.Nombre.Contains(key) ||
                               c.Empresa.Nombre.Contains(key))
                        orderby c.Agricultor.Nombre
                        select SaldosCtaCteExportModel.FromSaldoCtaCte(c);

            return new CsvActionResult<SaldosCtaCteExportModel>(items.ToList(), "SaldosCtaCte.csv", 1, ';', null);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(41);
            var saldo = new SaldoCtaCte
            {
            };

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("SaldoCtaCte", saldo);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(SaldoCtaCte saldo)
        {
            CheckPermisoAndRedirect(41);

            if (ModelState.IsValid)
            {
                try
                {
                    if (saldo.Comentarios == null)
                        saldo.Comentarios = "";

                    saldo.UserIns = User.Identity.Name;
                    saldo.FechaHoraIns = DateTime.Now;
                    saldo.IpIns = RemoteAddr();
                    
                    dc.SaldoCtaCte.InsertOnSubmit(saldo);
                    
                    dc.SubmitChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = saldo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                saldo.Agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == saldo.IdAgricultor);
            }

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("SaldoCtaCte", saldo);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(42);

            var saldo = dc.SaldoCtaCte.SingleOrDefault(p => p.IdSaldoCtaCte == id);
            if (saldo == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Saldo Not Found");

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("SaldoCtaCte", saldo);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(42);

            var saldo = dc.SaldoCtaCte.SingleOrDefault(p => p.IdSaldoCtaCte == id);
            var fields = new string[] { "IdAgricultor", "IdEmpresa", "MontoCtaCte", "MontoDocumentado", "Comentarios" };

            if (TryUpdateModel(saldo, fields))
            {
                try
                {
                    if (saldo.Comentarios == null)
                        saldo.Comentarios = "";

                    saldo.UserUpd = User.Identity.Name;
                    saldo.FechaHoraUpd = DateTime.Now;
                    saldo.IpUpd = RemoteAddr();

                    dc.SubmitChanges();
                    return RedirectToAction("Index", IndexRouteValues(null));
                }
                catch
                {
                    var rv = saldo.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                if (saldo.Agricultor == null)
                    saldo.Agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == saldo.IdAgricultor);
            }

            SetLookupLists();
            ViewData["indexRouteValues"] = IndexRouteValues(null);

            return View("SaldoCtaCte", saldo);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(43);
            var saldo = dc.SaldoCtaCte.SingleOrDefault(p => p.IdSaldoCtaCte == id);

            var routeValues = new RouteValueDictionary();
            try
            {
                if (saldo != null)
                {
                    dc.SaldoCtaCte.DeleteOnSubmit(saldo);
                    dc.SubmitChanges();
                }
                routeValues.Add("msgok", "El saldo de cuenta corriente fue eliminado con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible eliminar el saldo de cuenta corriente";
                if (ex.Message.Contains("***"))
                    msgerr += " porque tiene al menos un *** asociado";
                else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            return RedirectToAction("Index", IndexRouteValues(routeValues));
        }

        public ActionResult Importar()
        {
            CheckPermisoAndRedirect(44);
            var model = new ImportarSaldosCtaCteViewModel();

            SetLookupLists();
            return View(model);
        }

        [HttpPost]
        public ActionResult Importar(ImportarSaldosCtaCteViewModel model)
        {
            CheckPermisoAndRedirect(44);

            SetLookupLists();

            if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
            {
                var fileExt = Path.GetExtension(Request.Files[0].FileName);
                var filePath = Path.Combine(Server.MapPath("~/App_Data/uploads"), System.Guid.NewGuid().ToString()) + fileExt;
 
                Request.Files[0].SaveAs(filePath);
                model.FilePath = filePath;
                ModelState.Remove("FilePath");

                model.Load();
            }
          
            if (ModelState.IsValid)
            {
                return View("ImportarConfirmar", model);
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult ImportarFinal(ImportarSaldosCtaCteViewModel model)
        {
            CheckPermisoAndRedirect(44);

            SetLookupLists();

            model.Load();
            model.Persist(User.Identity.Name, RemoteAddr());

            return View();
        }
    }
}
