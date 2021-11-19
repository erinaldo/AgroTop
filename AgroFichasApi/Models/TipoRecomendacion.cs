using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class TipoRecomendacion : ISynSerializable
    {
        public string[] SyncProperties
        {
            get { return new[] { "Nombre", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdTipoRecomendacion; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("tr");
            element.SetAttributeValue("id", this.IdTipoRecomendacion);
            element.SetAttributeValue("n", this.Nombre);

            return element;
        }
    }
}