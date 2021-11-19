using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALFTDocController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public CALFTDocController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALFTDoc
        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(339);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            List<CAL_FTDoc> list = dcSoftwareCalidad.CAL_FTDoc.Where(X => X.IdFichaTecnica == id && X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["cAL_FT"] = cAL_FT;
            return View(list);
        }

        public ActionResult Crear(int id)
        {
            CheckPermisoAndRedirect(339);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });
            CAL_FTDoc cAL_FTDoc = new CAL_FTDoc();
            cAL_FTDoc.IdFichaTecnica = cAL_FT.IdFichaTecnica;
            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", cAL_FTDoc);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Crear(int id, CAL_FTDoc cAL_FTDoc, FormCollection formCollection, HttpPostedFileBase postedFile)
        {
            CheckPermisoAndRedirect(339);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(cAL_FTDoc.Observacion))
                        cAL_FTDoc.Observacion = string.Empty;

                    string path = Server.MapPath("~/uploads/softwarecalidad");

                    if (postedFile != null)
                    {
                        string fileName = string.Format("{0}.{1}", Guid.NewGuid(), Path.GetExtension(postedFile.FileName).Replace(".", ""));
                        postedFile.SaveAs(string.Format("{0}/{1}", path, fileName));

                        cAL_FTDoc.NombreArchivo = fileName;
                    }

                    cAL_FTDoc.IdFichaTecnica = cAL_FT.IdFichaTecnica;
                    cAL_FTDoc.Habilitado = true;
                    cAL_FTDoc.FechaHoraIns = DateTime.Now;
                    cAL_FTDoc.IpIns = RemoteAddr();
                    cAL_FTDoc.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_FTDoc.InsertOnSubmit(cAL_FTDoc);
                    dcSoftwareCalidad.SubmitChanges();
                    return RedirectToAction("Index", new { id = id });
                }
                catch
                {
                    var rv = cAL_FTDoc.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", cAL_FTDoc);
        }

        public ActionResult Editar(int id, int IdDoc)
        {
            CheckPermisoAndRedirect(339);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            CAL_FTDoc cAL_FTDoc = dcSoftwareCalidad.CAL_FTDoc.SingleOrDefault(X => X.IdDoc == IdDoc && X.Habilitado == true);
            if (cAL_FTDoc == null) return RedirectToAction("Index", "CALFTDoc", new { errMsg = "No se ha encontrado el Documento de la ficha técnica", okMsg = "" });

            cAL_FTDoc.IdFichaTecnica = cAL_FT.IdFichaTecnica;
            ViewData["cAL_FT"] = cAL_FT;

            return View("Crear", cAL_FTDoc);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Editar(int id, int IdDoc, FormCollection formCollection, HttpPostedFileBase postedFile)
        {
            CheckPermisoAndRedirect(339);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            CAL_FTDoc cAL_FTDoc = dcSoftwareCalidad.CAL_FTDoc.SingleOrDefault(X => X.IdDoc == IdDoc && X.Habilitado == true);
            if (cAL_FTDoc == null) return RedirectToAction("Index", "CALFTDoc", new { errMsg = "No se ha encontrado el Documento de la ficha técnica", okMsg = "" });

            try
            {
                UpdateModel(cAL_FTDoc, new string[] { "PostedFile", "Observacion", "EditarArchivo" });
                if (string.IsNullOrEmpty(cAL_FTDoc.Observacion))
                    cAL_FTDoc.Observacion = string.Empty;

                string path = Server.MapPath("~/uploads/softwarecalidad");

                if (postedFile != null)
                {
                    string fileName = string.Format("{0}.{1}", Guid.NewGuid(), Path.GetExtension(postedFile.FileName).Replace(".", ""));
                    postedFile.SaveAs(string.Format("{0}/{1}", path, fileName));

                    cAL_FTDoc.NombreArchivo = fileName;
                }

                cAL_FTDoc.UserUpd = User.Identity.Name;
                cAL_FTDoc.FechaHoraUpd = DateTime.Now;
                cAL_FTDoc.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                return RedirectToAction("Index", new { id = id });
            }
            catch
            {
                var rv = cAL_FTDoc.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["cAL_FT"] = cAL_FT;
            return View("Crear", cAL_FTDoc);
        }

        public ActionResult Eliminar(int id, int IdDoc)
        {
            CheckPermisoAndRedirect(339);
            CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
            if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

            CAL_FTDoc cAL_FTDoc = dcSoftwareCalidad.CAL_FTDoc.SingleOrDefault(X => X.IdDoc == IdDoc);
            if (cAL_FTDoc == null) return RedirectToAction("Index", "CALFTDoc", new { errMsg = "No se ha encontrado el documento", okMsg = "" });

            string errMsg = "";
            string okMsg = "";

            try
            {
                cAL_FTDoc.Habilitado = false;
                cAL_FTDoc.UserUpd = User.Identity.Name;
                cAL_FTDoc.FechaHoraUpd = DateTime.Now;
                cAL_FTDoc.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = "El documento ha sido eliminado";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { id = id, errMsg = errMsg, okMsg = okMsg });
        }


    }
}