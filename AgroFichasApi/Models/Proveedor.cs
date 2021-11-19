using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class Proveedor : ISynSerializable
    {
        public static string XmlTag = "pv";

        public string[] SyncProperties
        {
            get { return new[] { "Nombre", "Email", "Telefono1", "ID", "Habilitado" }; }
            set { }
        }

        public int ID
        {
            get { return IdProveedor; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(Proveedor.XmlTag);
            element.SetAttributeValue("id", this.IdProveedor);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("e", this.Email);
            element.SetAttributeValue("f1", this.Telefono1);
            element.SetAttributeValue("a", this.Habilitado);

            return element;
        }
    }
}