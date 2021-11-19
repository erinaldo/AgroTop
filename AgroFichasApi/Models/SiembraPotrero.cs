using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class SiembraPotrero : ISynSerializable
    {
        public static string XmlTag = "sp";

        public string[] SyncProperties
        {
            get { return new[] { "IdPotrero", "IdTemporada", "IdSiembra", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdSiembraPotrero; }
        }


        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(SiembraPotrero.XmlTag);
            element.SetAttributeValue("id", this.IdSiembraPotrero);
            element.SetAttributeValue("idpo", this.IdPotrero);
            element.SetAttributeValue("idtm", this.IdTemporada);
            element.SetAttributeValue("idsi", this.IdSiembra);
            return element;
        }
    }
}