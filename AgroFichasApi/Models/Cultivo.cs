using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class Cultivo : ISynSerializable
    {
        public string[] SyncProperties
        {
            get { return new[] { "Nombre", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdCultivo; }
        }


        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("cu");
            element.SetAttributeValue("id", this.IdCultivo);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("a", this.Habilitado);

            return element;
        }
    }
}