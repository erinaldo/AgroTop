using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class ImportanciaSeguimiento : ISynSerializable
    {
        public string[] SyncProperties
        {
            get { return new[] { "Nombre", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdImportanciaSeguimiento; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("ims");
            element.SetAttributeValue("id", this.IdImportanciaSeguimiento);
            element.SetAttributeValue("n", this.Nombre);

            return element;
        }
    }
}