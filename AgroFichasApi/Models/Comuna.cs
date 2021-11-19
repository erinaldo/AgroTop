using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class Comuna : ISynSerializable 
    {
        public string[] SyncProperties
        {
            get { return new[] { "Nombre", "Orden", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdComuna; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("co");
            element.SetAttributeValue("id", this.IdComuna);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("o", this.Orden);

            return element;
        }
    }
}