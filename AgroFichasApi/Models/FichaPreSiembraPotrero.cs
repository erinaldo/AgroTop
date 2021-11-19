using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class FichaPreSiembraPotrero : ISynSerializable
    {
        public static string XmlTag = "fpsp";

        public string[] SyncProperties
        {
            get { return new[] { "IdPotrero", "IdFichaPreSiembra", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdFichaPreSiembraPotrero; }
        }


        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(FichaPreSiembraPotrero.XmlTag);
            element.SetAttributeValue("id", this.IdFichaPreSiembraPotrero);
            element.SetAttributeValue("idpo", this.IdPotrero);
            element.SetAttributeValue("idfi", this.IdFichaPreSiembra);
            return element;
        }
    }
}