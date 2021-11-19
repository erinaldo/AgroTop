using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class TipoFicha : ISynSerializable
    {
        public string[] SyncProperties
        {
            get { return new[] { "Nombre", "Orden", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdTipoFicha; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("tf");
            element.SetAttributeValue("id", this.IdTipoFicha);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("o", this.Orden);

            return element;
        }
    }
}