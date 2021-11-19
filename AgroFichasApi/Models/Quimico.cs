using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class Quimico : ISynSerializable
    {
        public string[] SyncProperties
        {
            get { return new[] { "IdTipoRecomendacion", "Nombre", "Dosis", "IdUM", "ID", "Habilitado" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdQuimico; }
        }


        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("qu");
            element.SetAttributeValue("id", this.IdQuimico);
            element.SetAttributeValue("idtr", this.IdTipoRecomendacion);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("d", this.Dosis);
            element.SetAttributeValue("um", this.IdUM);
            element.SetAttributeValue("a", this.Habilitado);
            return element;
        }
    }
}