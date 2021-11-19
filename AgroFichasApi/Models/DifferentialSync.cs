using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public class DifferentialSync
    {
        public XDocument Response;
        public void FillResponse(List<XElement> results)
        {
            //Create the empty doc
            Response = new XDocument();
            var root = new XElement("data");
            Response.Add(root);

            //Easy
            root.Add(results);
        }
    }
}