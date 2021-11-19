using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class ChoferesController : BaseApplicationController
    {
        //
        // GET: /Choferes/

        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ChoferesController()
        {
            SetCurrentModulo(6);//Logística y Corretaje
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(92);

            var chofer = new LOG_Chofer();

            return View("crear", chofer);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Crear(LOG_Chofer chofer)
        {
            CheckPermisoAndRedirect(92);
            if (ModelState.IsValid)
            {
                try
                {
                    if (chofer.Telefono == null)
                        chofer.Telefono = "";

                    chofer.Licencia = "NO ANUNCIADA";
                    chofer.Habilitado = true;
                    chofer.RUT = Rut.NomarlizarRut(chofer.RUT);
                    chofer.FechaHoraIns = DateTime.Now;
                    chofer.IpIns = RemoteAddr();
                    chofer.UserIns = User.Identity.Name;

                    chofer.Nombre = Comunes.RemoveDiacritics(chofer.Nombre).ToUpper();

                    dc.LOG_Chofer.InsertOnSubmit(chofer);
                    dc.SubmitChanges();
                    return RedirectToAction("index");
                }
                catch
                {
                    var rv = chofer.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View("crear", chofer);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(93);

            var chofer = dc.LOG_Chofer.SingleOrDefault(x => x.IdChofer == id && x.Habilitado == true && x.LOG_Transportista.Habilitado == true);
            if (chofer == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el chofer"); }

            return View("crear", chofer);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(93);

            var chofer = dc.LOG_Chofer.SingleOrDefault(x => x.IdChofer == id && x.Habilitado == true && x.LOG_Transportista.Habilitado == true);
            if (chofer == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el chofer"); }

            try
            {
                UpdateModel(chofer, new string[] { "IdTransportista", "RUT", "Nombre", "Telefono" });

                if (chofer.Telefono == null)
                    chofer.Telefono = "";

                chofer.RUT = Rut.NomarlizarRut(chofer.RUT);
                chofer.UserUpd = User.Identity.Name;
                chofer.FechaHoraUpd = DateTime.Now;
                chofer.IpUpd = RemoteAddr();

                chofer.Nombre = Comunes.RemoveDiacritics(chofer.Nombre).ToUpper();

                dc.SubmitChanges();
                return RedirectToAction("index");
            }
            catch
            {
                var rv = chofer.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            return View("crear", chofer);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(90);

            var chofer = dc.LOG_Chofer.SingleOrDefault(x => x.IdChofer == id && x.Habilitado == true && x.LOG_Transportista.Habilitado == true);
            if (chofer == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado el chofer"); }

            string msgerr = "";
            string msgok = "";
            try
            {
                chofer.Habilitado = false;
                chofer.UserUpd = User.Identity.Name;
                chofer.FechaHoraUpd = DateTime.Now;
                chofer.IpUpd = RemoteAddr();
                dc.SubmitChanges();
                msgok = String.Format("El chofer {0} ha sido eliminado", chofer.Nombre);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(91);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            IQueryable<LOG_Chofer> items = null;
            string key = Request.QueryString["key"] ?? "";
            int IdTransportista = 0;
            if (!int.TryParse(Request.QueryString["IdTransportista"], out IdTransportista)) { }

            if (IdTransportista != 0)
            {
                items = dc.LOG_Chofer.Where(x => x.Habilitado == true && x.LOG_Transportista.Habilitado == true && x.IdTransportista == IdTransportista).OrderBy(a => a.Nombre);
            }
            else
            {
                items = dc.LOG_Chofer.Where(x => x.Habilitado == true && x.LOG_Transportista.Habilitado == true && (key == "" || x.Nombre.Contains(key) || x.RUT.Contains(key) || x.LOG_Transportista.Nombre.Contains(key))).OrderBy(a => a.Nombre);
            }

            var pagina = new PaginatedList<LOG_Chofer>(items, pageIndex, pageSize);

            SetTransportistas(null);

            ViewData["key"] = key;
            return View(pagina);
        }

        private void SetTransportistas(int? IdTransportista)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_Transportista
                where s.Habilitado == true
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdTransportista == IdTransportista && IdTransportista != null),
                    Text = s.Nombre,
                    Value = s.IdTransportista.ToString()
                };
            ViewData["transportistasList"] = selectList;
        }
    }
}
