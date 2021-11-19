using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace AgroFichasWeb.Controllers.Configuracion
{
    public class CTRMantenedorRomanaController : BaseApplicationController
    {
        // GET: CTRMantenedorRomana
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();
        public CTRMantenedorRomanaController()
        {
            SetCurrentModulo(2);
        }
        public ActionResult Index()
        {
            CheckPermisoAndRedirect(2015);
            List<CTR_MantenedorRomana> objList = (from c in dc.CTR_MantenedorRomana where c.Vigente == true select c).ToList();
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(2015),
                              CheckPermiso(2016),
                              CheckPermiso(2017),
                              CheckPermiso(2018));
            return View(objList);
        }
        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(2015);

            var obj = new CTR_MantenedorRomana();

            return View("crear", obj);
        }
        [HttpPost]
        public ActionResult Crear(CTR_MantenedorRomana obj)
        {
            CheckPermisoAndRedirect(2015);

                obj.Vigente = true;
                dc.CTR_MantenedorRomana.InsertOnSubmit(obj);
                dc.SubmitChanges();

            return RedirectToAction("index");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(2015);

            var obj = dc.CTR_MantenedorRomana.SingleOrDefault(x => x.IdMantenedorRomana == id && x.Vigente == true);
            if (obj == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la romana");

            return View("crear", obj);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(2015);

            var obj = dc.CTR_MantenedorRomana.SingleOrDefault(x => x.IdMantenedorRomana == id && x.Vigente == true);
            if (obj == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la romana");
            UpdateModel(obj, new string[] { "Nombre", "EsPlanta", "EsPesajeAutomatico", "RomanaEntrada", "RomanaSalida" });

            dc.SubmitChanges();
            return RedirectToAction("index");

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(2015);

            var obj = dc.CTR_MantenedorRomana.SingleOrDefault(x => x.IdMantenedorRomana == id && x.Vigente == true);
            if (obj == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "No se ha encontrado la romana");

            string msgerr = "";
            string msgok = "";
            try
            {
                obj.Vigente = false;
                dc.SubmitChanges();
                msgok = String.Format("La Romana '{0}' ha sido eliminada", obj.Nombre);
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
            }

            return RedirectToAction("Index", new { msgerr = msgerr, msgok = msgok });
        }
    }
}