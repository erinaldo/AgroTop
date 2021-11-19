using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class TipoSiembra : ISynSerializable
    {
        public string[] SyncProperties
        {
            get { return new[] { "Nombre", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdTipoSiembra; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("ts");
            element.SetAttributeValue("id", this.IdTipoSiembra);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("a", this.Habilitado);

            return element;
        }
    }
}