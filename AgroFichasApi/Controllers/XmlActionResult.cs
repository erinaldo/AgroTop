using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Controllers
{
    public class XmlActionResult : ActionResult
    {
        private readonly XDocument _document;

        public Formatting Formatting { get; set; }
        public string MimeType { get; set; }

        public XmlActionResult(XDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            _document = document;

            // Default values
            MimeType = "text/xml";
            Formatting = Formatting.Indented;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var ut = new UTF8Encoding(false);

            context.HttpContext.Response.Clear();
            context.HttpContext.Response.ContentType = MimeType;
            context.HttpContext.Request.ContentEncoding =ut;

            using (var writer = new XmlTextWriter(context.HttpContext.Response.OutputStream, ut) { Formatting = Formatting })
                _document.WriteTo(writer);
        }
    }
}