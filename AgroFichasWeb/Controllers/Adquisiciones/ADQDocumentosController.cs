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
    public class ADQDocumentosController : BaseApplicationController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public ADQDocumentosController()
        {
            SetCurrentModulo(11);
        }

        // GET: CALFTDoc
        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(450);
            ADQ_SolicitudCompra aDQ_SolicitudCompra = dc.ADQ_SolicitudCompra.SingleOrDefault(X => X.IdSolicitud == id && X.Habilitado == true);
            if (aDQ_SolicitudCompra == null) return RedirectToAction("Index", "ADQOrdenescompra", new { errMsg = "No se ha encontrado la solicitud de órden de compra", okMsg = "" });
            List<ADQ_Documento> list = dc.ADQ_Documento.Where(X => X.IdSolicitud == id && X.Habilitado == true).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["aDQ_SolicitudCompra"] = aDQ_SolicitudCompra;
            return View(list);
        }

        public ActionResult Crear(int id)
        {
            CheckPermisoAndRedirect(450);
            ADQ_SolicitudCompra aDQ_SolicitudCompra = dc.ADQ_SolicitudCompra.SingleOrDefault(X => X.IdSolicitud == id && X.Habilitado == true);
            if (aDQ_SolicitudCompra == null) return RedirectToAction("Index", "ADQOrdenescompra", new { errMsg = "No se ha encontrado la solicitud de órden de compra", okMsg = "" });
            ADQ_Documento aDQ_Documento = new ADQ_Documento();
            aDQ_Documento.IdSolicitud = aDQ_SolicitudCompra.IdSolicitud;
            ViewData["aDQ_SolicitudCompra"] = aDQ_SolicitudCompra;
            return View("Crear", aDQ_Documento);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Crear(int id, ADQ_Documento aDQ_Documento, FormCollection formCollection, HttpPostedFileBase postedFile)
        {
            CheckPermisoAndRedirect(450);
            ADQ_SolicitudCompra aDQ_SolicitudCompra = dc.ADQ_SolicitudCompra.SingleOrDefault(X => X.IdSolicitud == id && X.Habilitado == true);
            if (aDQ_SolicitudCompra == null) return RedirectToAction("Index", "ADQOrdenescompra", new { errMsg = "No se ha encontrado la solicitud de órden de compra", okMsg = "" });

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(aDQ_Documento.Observacion))
                        aDQ_Documento.Observacion = string.Empty;

                    string path = Server.MapPath("~/uploads/adquisiciones");

                    if (postedFile != null)
                    {
                        string fileName = string.Format("{0}.{1}", Guid.NewGuid(), Path.GetExtension(postedFile.FileName).Replace(".", ""));
                        postedFile.SaveAs(string.Format("{0}/{1}", path, fileName));

                        aDQ_Documento.NombreArchivo = fileName;
                    }

                    aDQ_Documento.IdSolicitud = aDQ_SolicitudCompra.IdSolicitud;
                    aDQ_Documento.Habilitado = true;
                    aDQ_Documento.FechaHoraIns = DateTime.Now;
                    aDQ_Documento.IpIns = RemoteAddr();
                    aDQ_Documento.UserIns = User.Identity.Name;
                    dc.ADQ_Documento.InsertOnSubmit(aDQ_Documento);
                    dc.SubmitChanges();
                    return RedirectToAction("Index", new { id = id });
                }
                catch
                {
                    var rv = aDQ_Documento.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["aDQ_SolicitudCompra"] = aDQ_SolicitudCompra;
            return View("Crear", aDQ_Documento);
        }

        //public ActionResult Editar(int id, int IdDoc)
        //{
        //    CheckPermisoAndRedirect(339);
        //    CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
        //    if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

        //    CAL_FTDoc cAL_FTDoc = dcSoftwareCalidad.CAL_FTDoc.SingleOrDefault(X => X.IdDoc == IdDoc && X.Habilitado == true);
        //    if (cAL_FTDoc == null) return RedirectToAction("Index", "CALFTDoc", new { errMsg = "No se ha encontrado el Documento de la ficha técnica", okMsg = "" });

        //    cAL_FTDoc.IdFichaTecnica = cAL_FT.IdFichaTecnica;
        //    ViewData["cAL_FT"] = cAL_FT;

        //    return View("Crear", cAL_FTDoc);
        //}

        //[HttpPost, ValidateInput(false)]
        //public ActionResult Editar(int id, int IdDoc, FormCollection formCollection, HttpPostedFileBase postedFile)
        //{
        //    CheckPermisoAndRedirect(339);
        //    CAL_FT cAL_FT = dcSoftwareCalidad.CAL_FT.SingleOrDefault(X => X.IdFichaTecnica == id && X.Habilitado == true);
        //    if (cAL_FT == null) return RedirectToAction("Index", "CALFichasTecnicas", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

        //    CAL_FTDoc cAL_FTDoc = dcSoftwareCalidad.CAL_FTDoc.SingleOrDefault(X => X.IdDoc == IdDoc && X.Habilitado == true);
        //    if (cAL_FTDoc == null) return RedirectToAction("Index", "CALFTDoc", new { errMsg = "No se ha encontrado el Documento de la ficha técnica", okMsg = "" });

        //    try
        //    {
        //        UpdateModel(cAL_FTDoc, new string[] { "PostedFile", "Observacion", "EditarArchivo" });
        //        if (string.IsNullOrEmpty(cAL_FTDoc.Observacion))
        //            cAL_FTDoc.Observacion = string.Empty;

        //        string path = Server.MapPath("~/uploads/softwarecalidad");

        //        if (postedFile != null)
        //        {
        //            string fileName = string.Format("{0}.{1}", Guid.NewGuid(), Path.GetExtension(postedFile.FileName).Replace(".", ""));
        //            postedFile.SaveAs(string.Format("{0}/{1}", path, fileName));

        //            cAL_FTDoc.NombreArchivo = fileName;
        //        }

        //        cAL_FTDoc.UserUpd = User.Identity.Name;
        //        cAL_FTDoc.FechaHoraUpd = DateTime.Now;
        //        cAL_FTDoc.IpUpd = RemoteAddr();
        //        dcSoftwareCalidad.SubmitChanges();
        //        return RedirectToAction("Index", new { id = id });
        //    }
        //    catch
        //    {
        //        var rv = cAL_FTDoc.GetRuleViolations();
        //        if (rv.Count() > 0)
        //            ModelState.AddRuleViolations(rv);
        //        else
        //            throw;
        //    }

        //    ViewData["cAL_FT"] = cAL_FT;
        //    return View("Crear", cAL_FTDoc);
        //}

        public ActionResult Eliminar(int id, int IdDocumento)
        {
            CheckPermisoAndRedirect(339);
            ADQ_SolicitudCompra aDQ_SolicitudCompra = dc.ADQ_SolicitudCompra.SingleOrDefault(X => X.IdSolicitud == id && X.Habilitado == true);
            if (aDQ_SolicitudCompra == null) return RedirectToAction("Index", "ADQOrdenescompra", new { errMsg = "No se ha encontrado la solicitud de orden de producción", okMsg = "" });

            ADQ_Documento aDQ_Documento = dc.ADQ_Documento.SingleOrDefault(X => X.IdDocumento == IdDocumento);
            if (aDQ_Documento == null) return RedirectToAction("Index", "ADQDocumentos", new { errMsg = "No se ha encontrado el documento", okMsg = "" });

            string errMsg = "";
            string okMsg = "";

            try
            {
                aDQ_Documento.Habilitado = false;
                aDQ_Documento.UserUpd = User.Identity.Name;
                aDQ_Documento.FechaHoraUpd = DateTime.Now;
                aDQ_Documento.IpUpd = RemoteAddr();
                dc.SubmitChanges();
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