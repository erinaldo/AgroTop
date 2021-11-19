using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class EstadoSiembra : ISynSerializable
    {
        public string[] SyncProperties
        {
            get { return new[] { "Nombre", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdEstadoSiembra; }
        }


        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("ess");
            element.SetAttributeValue("id", this.IdEstadoSiembra);
            element.SetAttributeValue("n", this.Nombre);

            return element;
        }
    }
}
