using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers
{
    public class DocumentsController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public FileResult Ficha(int id, string h)
        {
            var ficha = dc.Ficha.SingleOrDefault(f => f.IdFicha == id);
            if (ficha == null || (ficha.Hash != h && ficha.Hash2 != h))
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Ficha Not Found");

            var filepath = Server.MapPath(string.Format("~/App_Data/pdfs/{0}.pdf", ficha.IdFicha));
            var pdf = ficha.GetPdf(filepath);

            if (Request.QueryString["nolog"] == null || Request.QueryString["nolog"] == "0")
            {
                ficha.LecturaMailFicha.Add(new LecturaMailFicha()
                {
                    FechaHoraIns = DateTime.Now,
                    IpIns = HttpContext.Request.ServerVariables["REMOTE_ADDR"],
                    UserIns = "DocumentsController"
                });
                dc.SubmitChanges();
            }
            return File(pdf, "application/pdf", String.Format("Ficha-{0}.pdf", ficha.IdFicha));
        }

        public FileResult FichaPreSiembra(int id, string h)
        {
            var ficha = dc.FichaPreSiembra.SingleOrDefault(f => f.IdFichaPreSiembra == id);
            if (ficha == null || (ficha.Hash != h && ficha.Hash2 != h))
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Ficha Not Found");

            var filepath = Server.MapPath(string.Format("~/App_Data/pdfs/{0}.pdf", ficha.IdFichaPreSiembra));
            var pdf = ficha.GetPdf(filepath);

            if (Request.QueryString["nolog"] == null || Request.QueryString["nolog"] == "0")
            {
                ficha.LecturaMailFichaPreSiembra.Add(new LecturaMailFichaPreSiembra()
                {
                    FechaHoraIns = DateTime.Now,
                    IpIns = HttpContext.Request.ServerVariables["REMOTE_ADDR"],
                    UserIns = "DocumentsController"
                });
                dc.SubmitChanges();
            }
            return File(pdf, "application/pdf", String.Format("FichaPreSiembra-{0}.pdf", ficha.IdFichaPreSiembra));
        }
    }
}
