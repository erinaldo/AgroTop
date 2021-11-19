using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class Variedad : ISynSerializable
    {

        public string[] SyncProperties
        {
            get { return new[] { "IdCultivo", "Nombre", "ID", "Habilitado" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdVariedad; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("va");
            element.SetAttributeValue("id", this.IdVariedad);
            element.SetAttributeValue("idcu", this.IdCultivo);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("a", this.Habilitado);

            return element;
        }
    }
}