using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class TipoFichaCultivo : ISynSerializable
    {
        public string[] SyncProperties
        {
            get { return new[] { "IdCultivo", "IdTipoFicha", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdTipoFichaCultivo; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("tfc");
            element.SetAttributeValue("id", this.IdTipoFicha);
            element.SetAttributeValue("tf", this.IdTipoFicha);
            element.SetAttributeValue("tc", this.IdCultivo);

            return element;
        }

    }
}